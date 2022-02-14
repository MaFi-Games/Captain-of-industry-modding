using System.Collections.Generic;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Entities.Validators;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Zippers;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Simulation;
using Mafi.Serialization;

namespace ExampleMod.ReferenceCode {
	/// <summary>
	/// This is an exact code of balancer entity implemented by COI devs. The original class is <see cref="Zipper"/>.
	/// </summary>
	/// <remarks>
	/// Note that for correct functionality this class needs to implement serialization which is normally
	/// auto-generated but we were not yet able to provide the generator. You might need to make very careful manual
	/// changes to <see cref="Balancer.SerializeData"/> and <see cref="Balancer.DeserializeData"/>.
	/// </remarks>
	[GenerateSerializer]
	public partial class Balancer : LayoutEntity, IUniversalPortsHelperFriend, IElectricityConsumingEntity {

		private static readonly Duration DELAY = 10.Ticks();

		new public readonly ZipperProto Prototype;

		public override bool CanBePaused => true;

		private readonly IAssetTransactionManager m_assetTransactionManager;
		private readonly ISimLoopEvents m_simLoopEvents;

		/// <summary>
		/// Buffers used to simulate products movement in the zipper.
		/// </summary>
		private readonly Queueue<ZipBuffProduct> m_buffer = new();

		private readonly UniversalPortsHelper m_portsHelper;
		public override ImmutableArray<IoPort> Ports => m_portsHelper.PortsGeneric;

		// Queue for input products. Every port may be there at most once, except for priority ports.
		private readonly Queueue<KeyValuePair<UniversalPort, ProductQuantity>> m_inputQueue = new();

		public bool IsEmpty => m_buffer.IsEmpty && m_inputQueue.IsEmpty;

		/// <summary>
		/// Whether to prioritize inputs at selected priority ports.
		/// </summary>
		public bool PrioritizeInput { get; private set; }

		/// <summary>
		/// Whether to prioritize outputs at selected priority ports.
		/// </summary>
		public bool PrioritizeOutput { get; private set; }

		/// <summary>
		/// When set, all connected inputs will accept same amounts of products.
		/// </summary>
		public bool ForceEvenInputs => m_inputCounts.HasValue;
		private Option<QuantityLarge[]> m_inputCounts;
		[DoNotSave]
		private int? m_minPortIndexCache;

		/// <summary>
		/// When set, all connected outputs will receive the same amounts of products.
		/// </summary>
		public bool ForceEvenOutputs => m_outputCounts.HasValue;
		private Option<QuantityLarge[]> m_outputCounts;

		Electricity IElectricityConsumingEntity.PowerRequired => Prototype.RequiredPower;
		private readonly IElectricityConsumer m_electricityConsumer;

		[DoNotSave]
		private ImmutableArray<int> m_inputPortsIndicesCache;
		[DoNotSave]
		private ImmutableArray<int> m_outputPriorityPortsIndicesCache;
		[DoNotSave]
		private ImmutableArray<int> m_outputPortsIndicesCache;

		private int m_lastUsedPortIndex = 0;

		public ImmutableArray<char> PriorityPortNames => m_priorityPortNames;
		private ImmutableArray<char> m_priorityPortNames = ImmutableArray.Create('B', 'E');  // TODO: better default.


		public Balancer(
			EntityId id,
			ZipperProto proto,
			TileTransform transform,
			UniversalPortsHelperFactory portsHelperFactory,
			IConstructionManager constructionManager,
			IAssetTransactionManager assetTransactionManager,
			ISimLoopEvents simLoopEvents,
			IElectricityConsumerFactory electricityConsumerFactory
		) : base(id, proto, transform, simLoopEvents, constructionManager) {
			Prototype = proto;
			m_assetTransactionManager = assetTransactionManager;
			m_simLoopEvents = simLoopEvents;

			// TODO: OnlyForSaveCompatibility
			m_electricityConsumer = electricityConsumerFactory.CreateConsumer(this);
			if (Prototype.RequiredPower.IsZero) {
				(m_electricityConsumer as ElectricityConsumer)?.RemoveSelf();
			}

			m_portsHelper = portsHelperFactory.CreateInstance(this, IoPortType.Any);

			foreach (UniversalPort p in m_portsHelper.Ports) {
				p.ConnectionChanged.Add(this, portConnectionChanged);
			}
		}


		private void portConnectionChanged(IoPort ourPort, IoPort theirPort) {
			m_outputPriorityPortsIndicesCache = default(ImmutableArray<int>);
			m_outputPortsIndicesCache = default(ImmutableArray<int>);
			m_inputPortsIndicesCache = default(ImmutableArray<int>);

			if (ourPort.IsNotConnected) {
				return; // We only care about newly connected ports.
			}

			if (m_inputCounts.HasValue && ourPort.IsConnectedAsInput) {
				// New input connected, reset counts.
				QuantityLarge[] arr = m_inputCounts.Value;
				for (int i = 0; i < arr.Length; i++) {
					arr[i] = QuantityLarge.Zero;
				}
				m_minPortIndexCache = null;
			}

			if (m_outputCounts.HasValue && ourPort.IsConnectedAsOutput) {
				// New output connected, reset counts.
				QuantityLarge[] arr = m_outputCounts.Value;
				for (int i = 0; i < arr.Length; i++) {
					arr[i] = QuantityLarge.Zero;
				}
			}
		}

		protected override void SimUpdate() {
			if (IsNotEnabled) {
				return;
			}

			processInputProducts();

			// Always try to send, even if disabled. Only receiving is affected by enabled state.
			while (m_buffer.IsNotEmpty) {
				// Note: We may release more than one product per tick.
				if (tryReleaseFirstProduct() == false) {
					break;
				}
			}
		}

		public Quantity GetTotalStoredQuantity() {
			return new Quantity(
				m_buffer.Sum(x => x.ProductQuantity.Quantity.Value)
				+ m_inputQueue.Sum(x => x.Value.Quantity.Value));
		}

		private bool tryReleaseFirstProduct() {
			Assert.That(m_buffer).IsNotEmpty();

			ZipBuffProduct bp = m_buffer.Dequeue();
			Quantity remainingQuantity = bp.ProductQuantity.Quantity;

			if (PrioritizeOutput) {
				// This allows priority products to exit buffer earlier than others.
				if (m_simLoopEvents.CurrentStep + DELAY >= bp.ReleaseAtStep) {
					if (m_outputPriorityPortsIndicesCache.IsNotValid) {
						m_outputPriorityPortsIndicesCache =
							m_portsHelper.Ports.GetIndices(x => isPriorityPort(x) && x.IsConnectedAsOutput);
					}

					if (trySendToPortIndices(m_outputPriorityPortsIndicesCache, bp.ProductQuantity.Product,
							ref remainingQuantity)) {
						Assert.That(remainingQuantity).IsZero();
						return true;
					}
				}
			}

			if (m_simLoopEvents.CurrentStep >= bp.ReleaseAtStep) {
				if (m_outputPortsIndicesCache.IsNotValid) {
					m_outputPortsIndicesCache = m_portsHelper.Ports.GetIndices(x => x.IsConnectedAsOutput);
				}

				if (trySendToPortIndices(m_outputPortsIndicesCache, bp.ProductQuantity.Product,
						ref remainingQuantity)) {
					Assert.That(remainingQuantity).IsZero();
					return true;
				}
			}

			Assert.That(remainingQuantity).IsPositive();
			// Return product which was not sent to any port.
			m_buffer.EnqueueFirst(new ZipBuffProduct(
				new ProductQuantity(bp.ProductQuantity.Product, remainingQuantity), bp.ReleaseAtStep));
			return false;
		}

		private bool trySendToPortIndices(
			ImmutableArray<int> portIndices,
			ProductProto product,
			ref Quantity remainingQuantity
		) {
			Assert.That(portIndices).IsNotDefaultStruct();
			Assert.That(remainingQuantity).IsPositive();

			ImmutableArray<UniversalPort> ports = m_portsHelper.Ports;

			if (portIndices.Length <= 1) {
				// We have one or none outputs.
				if (portIndices.IsEmpty) {
					// TODO: Show notification.
					return false;
				} else {
					UniversalPort port = ports[portIndices.First];
					Assert.That(port.IsConnectedAsOutput).IsTrue();
					remainingQuantity = port.SendAsMuchAs(new ProductQuantity(product, remainingQuantity));
					if (remainingQuantity.IsNotPositive) {
						return true;
					}
				}
			} else {
				if (m_outputCounts.HasValue) {
					// We have multiple outputs that should be kept even.
					// Find a port with the least items sent and send our products there.
					QuantityLarge[] counts = m_outputCounts.Value;

					QuantityLarge minQ = counts[portIndices.First];
					int minPortI = portIndices.First;

					for (int i = 1; i < portIndices.Length; i++) {
						int portI = portIndices[i];
						QuantityLarge q = counts[portIndices[i]];
						if (q < minQ) {
							minQ = q;
							minPortI = portI;
						}
					}

					UniversalPort port = ports[minPortI];
					Quantity originalQuantity = remainingQuantity;
					remainingQuantity = port.SendAsMuchAs(new ProductQuantity(product, remainingQuantity));
					Quantity sentQuantity = originalQuantity - remainingQuantity;
					Assert.That(sentQuantity).IsNotNegative();
					counts[minPortI] += sentQuantity;

					// In case of priority outputs we attempt only one send operation per tick. This simplifies the code
					// and it is unlikely that it will cause any noticeable issues.
					return remainingQuantity.IsNotPositive;
				} else {
					// We have multiple outputs with no restrictions, just try to evenly distribute but skip ports that
					// are not accepting.
					for (int i = 0; i < portIndices.Length; i++) {
						m_lastUsedPortIndex = (m_lastUsedPortIndex + 1) % portIndices.Length;
						int portIndex = portIndices[m_lastUsedPortIndex];
						UniversalPort port = ports[portIndex];
						Assert.That(port.IsConnectedAsOutput).IsTrue();
						remainingQuantity = port.SendAsMuchAs(new ProductQuantity(product, remainingQuantity));
						if (remainingQuantity.IsNotPositive) {
							return true;
						}
					}
				}
			}

			return false;
		}


		Quantity IUniversalPortsFriend.ReceiveAsMuchAs(ProductQuantity pq, UniversalPort sourcePort) {
			if (IsNotEnabled) {
				return pq.Quantity; // Do not accept when disabled.
			}

			int maxInputsFromPort = PrioritizeInput && isPriorityPort(sourcePort) ? 2 : 1;
			int foundInputs = 0;

			foreach (KeyValuePair<UniversalPort, ProductQuantity> kvp in m_inputQueue) {
				if (kvp.Key == sourcePort) {
					foundInputs += 1;
					if (foundInputs >= maxInputsFromPort) {
						return pq.Quantity; // Port already has product in max inputs in the queue.
					}
				}
			}

			m_inputQueue.Enqueue(Make.Kvp(sourcePort, pq));
			return Quantity.Zero;
		}

		private void processInputProducts() {
			if (m_inputQueue.IsEmpty) {
				return; // No inputs to process.
			}

			if (m_electricityConsumer.CanConsume() == false) {
				return;
			}

			if (m_buffer.Count > 1 && m_simLoopEvents.CurrentStep > m_buffer.Peek().ReleaseAtStep) {
				return; // Buffer is full.
			}

			if (m_inputCounts.HasValue) {
				// Input count enforcement is turned on.
				Assert.That(PrioritizeInput).IsFalse("Priorities and even inputs are not supported at the same time.");

				QuantityLarge[] counts = m_inputCounts.Value;

				// We cache min port index to avoid this linear search every tick when there are products waiting at
				// wrong ports. This cache is cleared every time we successfully process a product.
				if (m_minPortIndexCache == null) {
					if (m_inputPortsIndicesCache.IsNotValid) {
						m_inputPortsIndicesCache = m_portsHelper.Ports.GetIndices(x => x.IsConnectedAsInput);
					}
					Assert.That(m_inputPortsIndicesCache).IsNotEmpty();

					ImmutableArray<int> portIndices = m_inputPortsIndicesCache;

					QuantityLarge minQ = counts[portIndices.First];
					int minPortI = portIndices.First;

					for (int i = 1; i < portIndices.Length; i++) {
						int portI = portIndices[i];
						QuantityLarge q = counts[portIndices[i]];
						if (q < minQ) {
							minQ = q;
							minPortI = portI;
						}
					}

					m_minPortIndexCache = minPortI;
				}

				// Only allow to accept from the port with minimum received products.
				UniversalPort minPort = m_portsHelper.Ports[m_minPortIndexCache.Value];
				for (int i = 0; i < m_inputQueue.Count; i++) {
					KeyValuePair<UniversalPort, ProductQuantity> kvp = m_inputQueue[i];
					if (kvp.Key == minPort) {
						m_buffer.Enqueue(new ZipBuffProduct(kvp.Value, m_simLoopEvents.CurrentStep + DELAY));
						m_inputQueue.RemoveAt(i);
						counts[m_minPortIndexCache.Value] += kvp.Value.Quantity;
						m_minPortIndexCache = null;
						return;
					}
				}
			} else {
				if (PrioritizeInput) {
					for (int i = 0; i < m_inputQueue.Count; i++) {
						KeyValuePair<UniversalPort, ProductQuantity> kvp = m_inputQueue[i];
						if (isPriorityPort(kvp.Key)) {
							m_buffer.Enqueue(new ZipBuffProduct(kvp.Value, m_simLoopEvents.CurrentStep + DELAY));
							m_inputQueue.RemoveAt(i);
							return;
						}
					}
				}

				ProductQuantity pq = m_inputQueue.Dequeue().Value;
				m_buffer.Enqueue(new ZipBuffProduct(pq, m_simLoopEvents.CurrentStep + DELAY));
			}
		}

		/// <summary>
		/// Returns whether all requested ports are present on this zipper.
		/// </summary>
		public bool TrySetPriorityPortNames(ImmutableArray<char> priorityPortNames) {
			m_priorityPortNames = priorityPortNames;
			m_outputPriorityPortsIndicesCache = default(ImmutableArray<int>);
			return priorityPortNames.All(x => m_portsHelper.Ports.Any(p => p.Name == x));
		}

		public void SetPrioritizeInput(bool prioritizeInput) {
			PrioritizeInput = prioritizeInput;
			if (prioritizeInput) {
				Assert.That(m_priorityPortNames).IsNotEmpty("No priority port names set.");
				SetForceEvenInputs(false);
			}
		}

		private bool isPriorityPort(IoPort port) {
			return m_priorityPortNames.Contains(port.Name);
		}

		public void SetPrioritizeOutput(bool prioritizeOutput) {
			PrioritizeOutput = prioritizeOutput;
			if (prioritizeOutput) {
				Assert.That(m_priorityPortNames).IsNotEmpty("No priority port names set.");
				SetForceEvenOutputs(false);
			}
		}

		public void SetForceEvenInputs(bool forceEvenInputs) {
			if (ForceEvenInputs == forceEvenInputs) {
				return;
			}

			if (forceEvenInputs) {
				m_inputCounts = new QuantityLarge[Ports.Length];
				SetPrioritizeInput(false);
			} else {
				m_inputCounts = Option<QuantityLarge[]>.None;
			}
			m_minPortIndexCache = null;
		}

		public void SetForceEvenOutputs(bool forceEvenOutputs) {
			if (ForceEvenOutputs == forceEvenOutputs) {
				return;
			}

			if (forceEvenOutputs) {
				m_outputCounts = new QuantityLarge[Ports.Length];
				SetPrioritizeOutput(false);
			} else {
				m_outputCounts = Option<QuantityLarge[]>.None;
			}
		}

		public override IEntityAddRequest GetAddRequest(EntityAddReason reasonToAdd) {
			return new LayoutEntityAddRequest(this, reasonToAdd);
		}

		protected override void OnDestroy() {
			foreach (UniversalPort p in m_portsHelper.Ports) {
				p.ConnectionChanged.Remove(this, portConnectionChanged);
			}

			foreach (ZipBuffProduct buffer in m_buffer) {
				m_assetTransactionManager.AddClearedProduct(buffer.ProductQuantity);
			}
			m_buffer.Clear();

			foreach (KeyValuePair<UniversalPort, ProductQuantity> kvp in m_inputQueue) {
				m_assetTransactionManager.AddClearedProduct(kvp.Value);
			}
			m_inputQueue.Clear();

			base.OnDestroy();
		}

	}

	[GenerateSerializer]
	public partial struct ZipBuffProduct {

		public readonly ProductQuantity ProductQuantity;
		public readonly SimStep ReleaseAtStep;

		public ZipBuffProduct(ProductQuantity productQuantity, SimStep releaseAtStep) {
			ProductQuantity = productQuantity;
			ReleaseAtStep = releaseAtStep;
		}

	}
}
