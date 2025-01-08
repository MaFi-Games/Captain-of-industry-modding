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
using static ProgramableNetwork.DataBands;

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
        public IDataBand DataBand { get; set; }

        public Antena(EntityId id, AntenaProto proto, TileTransform transform, EntityContext context, IEntityMaintenanceProvidersFactory maintenanceProvidersFactory)
            : base(id, proto, transform, context)
        {
            Prototype = proto;
            ErrorMessage = "";
            m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
            m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
            m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
            m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);

            DataBand = new UnkownnDataBandType(Context, Context.ProtosDb.Get<DataBandProto>(DataBand_Unknown).ValueOrThrow("Unknown signal not found"));
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
            data.SetString("databand_type", DataBand.Prototype.Id.Value);
        }

        public void ApplyConfig(EntityConfigData data)
        {
            string id = data.GetString("databand_type").ValueOrNull ?? DataBands.DataBand_Unknown.Value;
            Proto.ID dataBandType = new Proto.ID(id);
            DataBandProto dataBandProto = Context.ProtosDb.Get<DataBandProto>(dataBandType).ValueOrNull;
            DataBand = dataBandProto.Constructor(Context, dataBandProto);
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
            if (reader.TryStartClassDeserialization(out Antena value, null))
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

            if (DataBand == null)
            {
                DataBand = new UnkownnDataBandType(Context, Context.ProtosDb.Get<DataBandProto>(DataBand_Unknown).ValueOrThrow("Unknown signal not found"));
            }
            else
            {
                Log.Info($"Loaded DataBand type: {DataBand.GetType().FullName}");
                DataBand = (DataBand as UnloadedDataBand).Deserialize(Context, saveVersion);
                DataBand.Context = Context;
                DataBand.initContext();
                Log.Info($"Deserialized DataBand type: {DataBand.GetType().FullName}");
            }
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

            DataBand.Serialize(writer);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            m_protoId = new StaticEntityProto.ID(reader.ReadString());
            int version = reader.ReadInt();

            ErrorMessage = reader.ReadString();
            CustomTitle = Option<string>.Deserialize(reader);

            GeneralPriority = reader.ReadInt();
            m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

            DataBand = reader.ReadDataBand();

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

            DataBand.Update();
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
