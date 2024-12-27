using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public partial class NewTr
    {
        public static readonly LocStr2 InvalidPointerType = Loc.Str2("ProgramableNetwork_InvalidPointerType",
            "Invalid pointer type: expected='{0}' != got:'{1}'", "");

        public static readonly LocStr2 IndexOutOfRange = Loc.Str2("ProgramableNetwork_IndexOutOfRange",
            "Index out of range: 0 <= {0} < {1}", "");

        public static readonly LocStr1 EmptyInput = Loc.Str1("ProgramableNetwork_EmptyInput",
            "Input '{0}' is not set", "");

        public static readonly LocStr2 EmptyInputIndexed = Loc.Str2("ProgramableNetwork_EmptyInputIndexed",
            "Input '{0}' at index '{1}' is not set", "");

        public static readonly LocStr2 TypeOr = Loc.Str2("ProgramableNetwork_TypeOr",
            "{0}' or '{1}", "used in empty example argument");

        public static readonly LocStr CanNotPause = Loc.Str("ProgramableNetwork_CanNotPause",
            "Selected entity can not be paused", "");

        public static readonly LocStr HasNoStorage = Loc.Str("ProgramableNetwork_HasNoStorage",
            "Selected entity has no storage", "");

        public static readonly LocStr WatchDogStop = Loc.Str("ProgramableNetwork_WatchDogStop",
            "Instruction exceeded processable time", "");

        public static readonly LocStr UnknownError = Loc.Str("ProgramableNetwork_UnknownError",
            "Contact captain DeznekCZ, there went something wrong", "");

        public static readonly LocStr1 ModulesCount = Loc.Str1("ProgramableNetwork_ModulesCount",
            "Count of modules: {0}", "");

        public static readonly LocStr InvalidInstruction = Loc.Str("ProgramableNetwork_InvalidInstruction",
            "Invalid instruction", "");

        public static readonly LocStr1 Variable = Loc.Str1("ProgramableNetwork_Variable",
            "Variable picked: {0}", "");

        public static readonly LocStr True = Loc.Str("ProgramableNetwork_True",
            "True", "");

        public static readonly LocStr False = Loc.Str("ProgramableNetwork_False",
            "False", "");

        public static readonly LocStr End = Loc.Str("ProgramableNetwork_End",
            "End", "");

        public static readonly LocStr DivisionByZero = Loc.Str("ProgramableNetwork_DivisionByZero",
            "Division by zero", "This may happen during operation: A / B");

        public static readonly LocStr CableIsNotConnected = Loc.Str("ProgramableNetwork_CableIsNotConnected",
            "Cable is not connected", "");

        //public static readonly Dictionary<InstructionProto.InputType, LocStr> PointerTypes
        //    = new Dictionary<InstructionProto.InputType, LocStr>()
        //    {
        //        { InstructionProto.InputType.None,               Loc.Str("ProgramableNetwork_PointerType_None"         , "None", "") },
        //        { InstructionProto.InputType.Any,                Loc.Str("ProgramableNetwork_PointerType_Any"          , "Any", "") },
        //        { InstructionProto.InputType.Variable,           Loc.Str("ProgramableNetwork_PointerType_Variable"     , "Variable", "") },
        //        { InstructionProto.InputType.Instruction,        Loc.Str("ProgramableNetwork_PointerType_Instruction"  , "Instruction", "") },
        //        { InstructionProto.InputType.Boolean,            Loc.Str("ProgramableNetwork_PointerType_Boolean"      , "Boolean", "") },
        //        { InstructionProto.InputType.Integer,            Loc.Str("ProgramableNetwork_PointerType_Integer"      , "Integer", "") },
        //        { InstructionProto.InputType.Entity,             Loc.Str("ProgramableNetwork_PointerType_Entity"       , "Entity", "") },
        //        { InstructionProto.InputType.StaticEntity,       Loc.Str("ProgramableNetwork_PointerType_StaticEntity" , "Static", "") },
        //        { InstructionProto.InputType.DynamicEntity,      Loc.Str("ProgramableNetwork_PointerType_DynamicEntity", "Dynamic", "") },
        //        { InstructionProto.InputType.Product,            Loc.Str("ProgramableNetwork_PointerType_Product"      , "Product", "") },
        //
        //    };

        public static readonly Dictionary<bool, LocStr> Boolean
            = new Dictionary<bool, LocStr>()
            {
                { true,  True },
                { false, False },
            };

        public partial class Tools
        {
            public static readonly LocStr Remove = Loc.Str("ProgramableNetwork_Tool_Delete",
                "Remove", "");
            public static readonly LocStr Add = Loc.Str("ProgramableNetwork_Tool_Add",
                "Add", "");
            public static readonly LocStr Copy = Loc.Str("ProgramableNetwork_Tool_Copy",
                "Copy", "");
            public static readonly LocStr Paste = Loc.Str("ProgramableNetwork_Tool_Paste",
                "Paste", "");
            public static readonly LocStr Up = Loc.Str("ProgramableNetwork_Tool_Up",
                "Up", "");
            public static readonly LocStr Down = Loc.Str("ProgramableNetwork_Tool_Down",
                "Down", "");
            public static readonly LocStr Pick = Loc.Str("ProgramableNetwork_Tool_Pick",
                "Pick", "");
            public static readonly LocStr Apply = Loc.Str("ProgramableNetwork_Tool_Apply",
                "Apply", "");
            public static readonly LocStr Edit = Loc.Str("ProgramableNetwork_Tool_Edit",
                "Edit", "");
        }
    }
}
