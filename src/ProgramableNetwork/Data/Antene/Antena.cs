using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Ports.Io;
using System;
using Mafi.Serialization;
using System.Collections.Generic;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Base;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Ports;
using System.Linq;
using Mafi.Collections;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Economy;
using Mafi.Core.Maintenance;
using Mafi.Core.Products;
using Mafi.Core.Entities.Static;
using Mafi.Unity.UserInterface;

namespace ProgramableNetwork
{
    [GenerateSerializer(false, null, 0)]
    public class Antena : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
        IUnityConsumingEntity, IComputingConsumingEntity, IElectricityConsumingEntity, IMaintainedEntity
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((Antena)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((Antena)obj).DeserializeData(reader);
        };

        public Option<string> CustomTitle { get; set; }

        public Antena(EntityId id, AntenaProto proto, TileTransform transform, EntityContext context, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ErrorMessage = "";
            m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
            m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
            m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);

            // TODO
            // 1) List of broadcasted frequencies on FM (later will be added more of them)
            // 2) FM: 85.5 - 108.0 kHz (steps by half, stored as 0-44 int)
            //        selector will be [+][-]
            // 3) only selected frequencies will be broadcasted to different antenas
            // 4) signal to tower will be set from controller or from broadcasting tower
            // 5) the signal will be valid for 200 ms, then it will send noise
            // 6) the controller will read the signal value
            // 7) Dictionary of received signals on FM (later will be added more of them)
        }

        [DoNotSave(0, null)]
        private AntenaProto m_proto;
        [DoNotSave(0, null)]
        private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

        [DoNotSave(0, null)]
        public new AntenaProto Prototype
        {
            get
            {
                return m_proto;
            }
            protected set
            {
                m_proto = value;
                m_protoId = m_proto.Id;
                base.Prototype = value;
            }
        }

        [DoNotSave(0, null)]
        public override bool CanBePaused => true;

        public void AddToConfig(EntityConfigData data)
        {
        }

        public void ApplyConfig(EntityConfigData data)
        { 
        }

        public static void Serialize(Antena value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static Antena Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out Antena value, (Func<BlobReader, Type, Antena>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        [InitAfterLoad(InitPriority.Normal)]
        [OnlyForSaveCompatibility(null)]
        private void initContexts(int saveVersion)
        {
            Log.Info($"Initialize context after load");

            Prototype = Context.ProtosDb.Get<AntenaProto>(m_protoId).ValueOrThrow("Invalid antene proto: " + m_protoId);
            m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
        }

        [DoNotSave(0, null)]
        private readonly int SerializerVersion = 0;
        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteString(m_protoId.Value);
            writer.WriteInt(SerializerVersion);

            writer.WriteString(ErrorMessage ?? "");
            Option<string>.Serialize(CustomTitle, writer);

            writer.WriteInt(GeneralPriority);
            writer.WriteGeneric(m_maintenanceConsumer);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new Mafi.Core.Entities.Static.StaticEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();

            ErrorMessage = reader.ReadString();
            CustomTitle = Option<string>.Deserialize(reader);

            GeneralPriority = reader.ReadInt();
            m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
        }

        [DoNotSave(0, null)]
        public Upoints MonthlyUnityConsumed => 0.Upoints();

        [DoNotSave(0, null)]
        public Upoints MaxMonthlyUnityConsumed => 0.Upoints();

        public Proto.ID UpointsCategoryId => IdsCore.UpointsCategories.Boost;

        [DoNotSave(0, null)]
        public Option<UnityConsumer> UnityConsumer => m_unityConsumer;
        [DoNotSave(0, null)]
        private UnityConsumer m_unityConsumer;

        [DoNotSave(0, null)]
        public int CurrentInstruction { get; private set; }

        [DoNotSave(0, null)]
        public Electricity PowerRequired { get; private set; } = Electricity.Zero;

        [DoNotSave(0, null)]
        public Option<IElectricityConsumerReadonly> ElectricityConsumer => ((IElectricityConsumerReadonly)m_electricConsumer).SomeOption();
        [DoNotSave(0, null)]
        private IElectricityConsumer m_electricConsumer;

        [DoNotSave(0, null)]
        public Computing ComputingRequired { get; private set; } = Computing.Zero;
        [DoNotSave(0, null)]
        public Option<IComputingConsumerReadonly> ComputingConsumer => ((IComputingConsumerReadonly)m_computingConsumer).SomeOption();

        [DoNotSave(0, null)]
        private IComputingConsumer m_computingConsumer;

        public MaintenanceCosts MaintenanceCosts { get; private set; }

        [DoNotSave(0, null)]
        public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
        [DoNotSave(0, null)]
        private IEntityMaintenanceProvider m_maintenanceConsumer;
        [DoNotSave(0, null)]
        public bool IsIdleForMaintenance => m_maintenanceConsumer.Status.IsBroken;

        [DoNotSave(0, null)]
        public string ErrorMessage { get; private set; }

        [DoNotSave(0, null)]
        public bool IsDebug { get; private set; }

        [DoNotSave(0, null)]
        public bool WaitForUser { get; private set; }

        public void SimUpdate()
        {
            if (IsNotEnabled && IsNotPaused)
            {
                return;
            }

            if (!m_electricConsumer.TryConsume())
            {
                return;
            }

            if (IsPaused || m_maintenanceConsumer.Status.IsBroken)
            {
                CurrentInstruction = 0;
                PowerRequired = Electricity.Zero;
                if (!IsPaused)
                    Maintenance.SetCurrentMaintenanceTo(Percent.Zero);
                return;
            }

            var requiredRunningPower = new List<Module>()
                .ToArray()
                .Where(m => m.IsNotPaused())
                .Select(m => m.Prototype.UsedPower.Value)
                .Sum().Kw();

            var requiredComputingPower = new Computing(new List<Module>()
                .ToArray()
                .Where(m => m.IsNotPaused())
                .Select(m => m.Prototype.UsedComputing.Value)
                .Sum());

            PowerRequired = Prototype.IddlePower + requiredRunningPower;

            ComputingRequired = requiredComputingPower;

            if (m_electricConsumer.CanConsume())
            {
                // TODO update signals
            }
        }

        public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
        {
            return Quantity.Zero; // TODO keep displayed content
        }

        [DoNotSave()]
        public int GeneralPriority { get; set; }

        [DoNotSave()]
        public bool IsGeneralPriorityVisible => true;

        [DoNotSave()]
        public bool IsCargoAffectedByGeneralPriority => false;
    }
}
