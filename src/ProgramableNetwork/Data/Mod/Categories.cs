using Mafi.Core.Prototypes;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace ProgramableNetwork
{
    public class Category
    {
        public static List<Category> Categories(ProtosDb protos, Controller controller)
        {
            return protos.All<ModuleProto>()
                .Where(m => m.AllowedDevices.Contains(controller.Prototype.Id))
                .SelectMany(m => m.Categories)
                .Distinct()
                .OrderBy(m => m.Name)
                .ToList();
        }

        public Category(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Category cat && cat.Id == Id;
        }

        public override string ToString()
        {
            return $"Category(Id:{Id}, Name:{Name})";
        }

        //// known types
        public static Category Display { get; } = new Category(id: "display", name: "Display modules");
        public static Category Connection { get; } = new Category(id: "connection", name: "Connection modules");
        public static Category Command { get; } = new Category(id: "command", name: "Command modules (write)");
        public static Category Arithmetic { get; } = new Category(id: "arithmetic", name: "Arithmetic modules");
        public static Category Boolean { get; } = new Category(id: "boolean", name: "Boolean modules");
        public static Category Decision { get; } = new Category(id: "decision", name: "Decision modules");
        public static Category Control { get; } = new Category(id: "control", name: "Control modules");
        public static Category Stats { get; } = new Category(id: "stats", name: "Stats modules");
        public static Category Antene { get; } = new Category(id: "antene", name: "Antena modules");
        public static Category AnteneFM { get; } = new Category(id: "antene_fm", name: "FM modules");
    }
}