using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    [GenerateSerializer(false, null, 0)]
    public class Module : IEntity, IEntityWithCloneableConfig
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((Module)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((Module)obj).DeserializeData(reader);
        };
        public static void Serialize(Module value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static Module Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out Module obj))
            {
                reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
            }

            return obj;
        }

        private ModuleProto m_proto;
        private string m_protoId;
        private ModuleLayout m_layout;

        public Module(ModuleProto prototype, EntityContext context, Controller entity)
        {
            this.Id = DateTime.UtcNow.Ticks;
            Prototype = prototype;
            Context = context;
            Controller = entity;
            NumberData = new Dict<string, int>();
            StringData = new Dict<string, string>();
            InputModules = new Dict<string, ModuleConnector>();
        }

        public Controller Controller { get; set; }

        public ModuleStatus Status { get; private set; }
        public string Error { get; private set; } = "";
        public long Id { get; private set; }

        public ModuleProto Prototype {
            get => m_proto;
            set
            {
                m_protoId = value.Id.Value;
                m_proto = value;
            }
        }

        public EntityContext Context { get; set; }
        public ModuleLayout Layout => m_layout = m_layout ?? new ModuleLayout(Prototype);

        public void Execute()
        {
            try
            {
                Error = "";
                Prototype.Action(this);
                Status = ModuleStatus.Running;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
                Status = ModuleStatus.Error;
            }
        }

        /// <summary>
        /// Resets all volatile data
        /// </summary>
        public void Reset()
        {
            try
            {
                Error = "";
                Prototype.Reset(this);
                Status = ModuleStatus.Running;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
                Status = ModuleStatus.Error;
            }
        }

        [DoNotSave(0, null)]
        public Dict<string, ModuleConnector> InputModules { get; private set; } // todo get by module id, cached

        [DoNotSave(0, null)]
        private readonly int SerializerVersion = 0;
        protected void SerializeData(BlobWriter writer)
        {
            if (m_protoId == null)
                throw new NullReferenceException($"Prototype was not set valid");

            writer.WriteLong(Id);
            writer.WriteString(m_protoId);
            writer.WriteInt(SerializerVersion);
            writer.WriteBool(IsPaused);
            Dict<string, int>.Serialize(NumberData, writer);
            Dict<string, string>.Serialize(StringData, writer);
            Dict<string, ModuleConnector>.Serialize(InputModules, writer);
        }

        protected void DeserializeData(BlobReader reader)
        {
            Id = reader.ReadLong();
            m_protoId = reader.ReadString();
            int version = reader.ReadInt();
            IsPaused = reader.ReadBool();
            NumberData = Dict<string, int>.Deserialize(reader);
            StringData = Dict<string, string>.Deserialize(reader);
            InputModules = Dict<string, ModuleConnector>.Deserialize(reader);
        }

        [InitAfterLoad(InitPriority.Normal)]
        [OnlyForSaveCompatibility(null)]
        internal void initContexts(int saveVersion)
        {
            var Prototype = Context.ProtosDb.Get<ModuleProto>(new ModuleProto.ID(m_protoId));
            if (Prototype.HasValue)
            {
                this.Prototype = Prototype.Value;
            }
            else
            {
                var alternative = Deprecation.GetAlternative(new ModuleProto.ID(m_protoId));
                this.Prototype = Context.ProtosDb.Get<ModuleProto>(alternative).ValueOrThrow("Invalid module proto: " + m_protoId);
            }
        }

        public void AddToConfig(EntityConfigData data)
        {
            // clone proto id, position and cofigured fields
            // the position on grid is hold by computer
        }

        public void ApplyConfig(EntityConfigData data)
        {
            // applies prototype and accessible fields
        }

        public void UpdateIsEnabled()
        {
            //throw new NotImplementedException();
        }

        public void UpdateIsBroken()
        {
            //throw new NotImplementedException();
        }

        public void UpdateProperties()
        {
            //throw new NotImplementedException();
        }

        public void SetPaused(bool isPaused)
        {
            //throw new NotImplementedException();
        }

        public void AddObserver(IEntityObserver observer)
        {
            //throw new NotImplementedException();
        }

        public void RemoveObserver(IEntityObserver observer)
        {
            //throw new NotImplementedException();
        }

        [DoNotSave(0, null)]
        protected Dict<string, int> NumberData { get; private set; }
        [DoNotSave(0, null)]
        protected Dict<string, string> StringData { get; private set; }


        [DoNotSave(0, null)]
        public OutputData Output => new OutputData(this);

        public class OutputData
        {
            private Module module;

            internal OutputData(Module module)
            {
                this.module = module;
            }

            public int this[string name, int defaultValue = 0]
            {
                get => module.NumberData.TryGetValue("out__" + name, out int data)
                    ? data : defaultValue;
                set => module.NumberData["out__" + name] = value;
            }
        }


        [DoNotSave(0, null)]
        public InputData Input => new InputData(this);

        public class InputData
        {
            private Module module;

            internal InputData(Module module)
            {
                this.module = module;
            }

            public int this[string name, int defaultValue = 0]
            {
                get => module.NumberData.TryGetValue("in__" + name, out int data)
                    ? data : defaultValue;
                set => module.NumberData["in__" + name] = value;
            }
        }

        [DoNotSave(0, null)]
        public FieldData Field => new FieldData(this);

        public class FieldData
        {
            private Module module;

            internal FieldData(Module module)
            {
                this.module = module;
            }

            public string this[string name, string defaultValue]
            {
                get => module.StringData.TryGetValue("field__" + name, out string data)
                    ? data : defaultValue;
            }

            public int this[string name, int defaultValue]
            {
                get => module.NumberData.TryGetValue("field__" + name, out int data)
                    ? data : defaultValue;
            }

            public object this[string name]
            {
                set
                {
                    if (value is int i)
                        module.NumberData["field__" + name] = i;
                    if (value is string s)
                        module.StringData["field__" + name] = s;
                }
            }
        }

        [DoNotSave(0, null)]
        public DisplayData Display => new DisplayData(this);

        public class DisplayData
        {
            private Module module;

            internal DisplayData(Module module)
            {
                this.module = module;
            }

            public string this[string name, string defaultValue]
            {
                get => module.StringData.TryGetValue("display__" + name, out string data)
                    ? data : defaultValue;
            }

            public string this[string name]
            {
                set
                {
                    module.StringData["display__" + name] = value;
                }
            }
        }


        [DoNotSave(0, null)]
        public StatusData StatusIn => new StatusData(this, "in__");


        [DoNotSave(0, null)]
        public StatusData StatusOut => new StatusData(this, "out__");

        public LocStrFormatted DefaultTitle => throw new NotImplementedException();

        EntityId IEntity.Id => new EntityId((int)(Id&0xFFFFFFFF));

        EntityProto IEntity.Prototype => Prototype;

        public bool IsEnabled => true;

        public bool IsPaused { get; set; }

        public bool CanBePaused => false;

        public bool IsDestroyed => false;

        public class StatusData
        {
            private readonly Module module;
            private readonly string direction;

            internal StatusData(Module module, string direction)
            {
                this.module = module;
                this.direction = direction;
            }

            public ModuleStatus this[string name]
            {
                get => module.NumberData.TryGetValue("status__" + direction + name, out int data)
                    ? (ModuleStatus)data : ModuleStatus.Init;
                set => module.NumberData["status__" + direction + name] = (int)value;
            }
        }
    }
}