using Mafi.Serialization;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Core.Mods;
using Mafi.Core.Research;

namespace ProgramableNetwork
{
    public class ComputerModuleProto : Proto
    {

        [DebuggerStepThrough]
        [DebuggerDisplay("{Value,nq}")]
        [ManuallyWrittenSerialization]
        public new readonly struct ID : IEquatable<ID>, IComparable<ID>
        {
            //
            // Souhrn:
            //     Underlying string value of this Id.
            public readonly string Value;

            public ID(string value)
            {
                Value = value;
            }

            public static bool operator ==(ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public override bool Equals(object other)
            {
                if (other is ID)
                {
                    ID other2 = (ID)other;
                    return Equals(other2);
                }

                return false;
            }

            public bool Equals(ID other)
            {
                return string.Equals(Value, other.Value, StringComparison.Ordinal);
            }

            public int CompareTo(ID other)
            {
                return string.CompareOrdinal(Value, other.Value);
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }

            public static void Serialize(ID value, BlobWriter writer)
            {
                writer.WriteString(value.Value);
            }

            public static ID Deserialize(BlobReader reader)
            {
                return new ID(reader.ReadString());
            }

            public static implicit operator Proto.ID(ID id)
            {
                return new Proto.ID(id.Value);
            }
        }

        public new ID Id { get; }
        public Action<ComputerModule> Action { get; }
        public bool IsInputModule { get; }
        public bool IsOutputModule { get; }
        public int UsedSlots { get; }
        public List<ModuleConnectorProto> Inputs { get; }
        public List<ModuleConnectorProto> Outputs { get; }
        public List<ModuleConnectorProto> Displays { get; }

        public ComputerModuleProto(ID id, Str strings, IEnumerable<Tag> tags, Action<ComputerModule> action, bool isInputModule, bool isOutputModule, int usedSlots,
            List<ModuleConnectorProto> m_inputs, List<ModuleConnectorProto> m_outputs, List<ModuleConnectorProto> m_displays) : base(id, strings, tags)
        {
            Action = action;
            IsInputModule = isInputModule;
            IsOutputModule = isOutputModule;
            UsedSlots = usedSlots;
            Inputs = m_inputs;
            Outputs = m_outputs;
            Displays = m_displays;
            SetAvailability(false);
        }

        public class Builder
        {
            private readonly List<Tag> m_tags;
            private ProtoRegistrator m_registrator;
            private readonly ID m_id;
            private readonly string m_name;
            private readonly string m_description;
            private Action<ComputerModule> m_action;
            private bool m_isOutputModule = false;
            private bool m_isInputModule = false;
            private int m_usedSlots = 1;
            private readonly List<ModuleConnectorProto> m_inputs;
            private readonly List<ModuleConnectorProto> m_outputs;
            private readonly List<ModuleConnectorProto> m_displays;

            public Builder(ProtoRegistrator registrator, string id, string name, string description)
            {
                m_registrator = registrator;
                m_id = new ID("ProgramableNetwork_Module_" + id);
                m_name = name;
                m_description = description;
                m_tags = new List<Tag>();
            }

            public ComputerModuleProto BuildAndAdd()
            {
                return BuildAndAdd(NewIds.Research.ProgramableNetwork_Stage1);
            }

            public ComputerModuleProto BuildAndAdd(ResearchNodeProto.ID researchStage)
            {
                Research.AddModule(m_id, researchStage);
                return m_registrator.PrototypesDb.Add(new ComputerModuleProto(
                    m_id,
                    CreateStr(m_id, m_name, m_description),
                    m_tags,
                    m_action,
                    m_isInputModule,
                    m_isOutputModule,
                    m_usedSlots,
                    m_inputs,
                    m_outputs,
                    m_displays
                ));
            }

            public Builder AddTag(Tag tag)
            {
                m_tags.Add(tag);
                return this;
            }

            public Builder AddInput(string id, string name, params InstructionProto.InputType[] types)
            {
                m_inputs.Add(new ModuleConnectorProto(m_id.Input(id, name), types));
                return this;
            }

            public Builder AddOutput(string id, string name, params InstructionProto.InputType[] types)
            {
                m_outputs.Add(new ModuleConnectorProto(m_id.Output(id, name), types));
                return this;
            }

            public Builder AddDisplay(string id, string name, params InstructionProto.InputType[] types)
            {
                m_displays.Add(new ModuleConnectorProto(m_id.Display(id, name), types));
                return this;
            }
        }
    }

    public static class ComputerModuleProtoExtensions
    {
        public static ComputerModuleProto.Builder Start(this ProtoRegistrator registrator, string id, string name, string description = "")
        {
            return new ComputerModuleProto.Builder(registrator, id, name, description);
        }

        public static Proto.Str Input(this ComputerModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__input__" + name), text, description);
        }

        public static Proto.Str Output(this ComputerModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__output__" + name), text, description);
        }

        public static Proto.Str Display(this ComputerModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__display__" + name), text, description);
        }
    }
}
