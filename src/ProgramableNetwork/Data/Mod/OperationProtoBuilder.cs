using Mafi.Core.Entities;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class OperationProtoBuilder
    {
        private readonly ProtoRegistrator registrator;
        private readonly InstructionProto.ID id;
        private readonly string name;
        private readonly List<InstructionInput> inputs = new List<InstructionInput>();
        private readonly List<Tag> tags = new List<Tag>();

        private Action<Program> runtime = _ => { };
        private int instructionCost = 1;
        private int instructionLevel = 1;
        private Action<UiBuilder, StackContainer> customUI = (_0,_1) => { };
        private string descShort = "";
        private Func<Entity, bool> entityFilter = _ => true;

        public OperationProtoBuilder(ProtoRegistrator registrator, InstructionProto.ID id, string name)
        {
            this.registrator = registrator;
            this.id = id;
            this.name = name;
        }

        public InstructionProto BuildAndAdd()
        {
            return registrator.PrototypesDb.Add(new InstructionProto(
                   id: id,
                   strings: Proto.CreateStr(id, name, descShort),
                   runtime: runtime,
                   inputs: inputs.ToArray(),
                   customUI: customUI,
                   entityFilter: entityFilter,
                   instructionLevel: instructionLevel,
                   instructionCost: instructionCost,
                   tags: tags
               ));
        }

        public OperationProtoBuilder Description(string description)
        {
            descShort = description;
            return this;
        }

        public OperationProtoBuilder Level(int level)
        {
            instructionLevel = level;
            return this;
        }

        public OperationProtoBuilder Cost(int cost)
        {
            instructionCost = cost;
            return this;
        }

        public OperationProtoBuilder Runtime(Action<Program> runtime)
        {
            this.runtime = runtime;
            return this;
        }

        public OperationProtoBuilder UIExtension(Action<UiBuilder, StackContainer> customUI)
        {
            this.customUI = customUI;
            return this;
        }

        public OperationProtoBuilder AddInput(string id, string name, params InstructionProto.InputType[] types)
        {
            this.inputs.Add(new InstructionInput(this.id.Input(id, name, ""), types));
            return this;
        }

        public OperationProtoBuilder AddInput(string id, string name, string description, params InstructionProto.InputType[] types)
        {
            this.inputs.Add(new InstructionInput(this.id.Input(id, name, description), types));
            return this;
        }

        public OperationProtoBuilder EntityFilter(Func<Entity, bool> filter)
        {
            this.entityFilter = filter;
            return this;
        }
    }
}
