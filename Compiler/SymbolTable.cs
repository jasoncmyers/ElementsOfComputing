using System.Collections.Generic;
using System;

namespace Nand2Tetris_Compiler
{
    public enum VariableCategory { STATIC, FIELD, ARG, VAR, NONE };

    class SymbolTable
    {

        Dictionary<string, SymbolTableEntry> classScope; 
        Dictionary<string, SymbolTableEntry> subroutineScope;
        int staticIndex, fieldIndex, argIndex, varIndex;

        struct SymbolTableEntry
        {
            public string type;
            public VariableCategory kind;
            public int index;

            public SymbolTableEntry(string type, VariableCategory kind, int index)
            {
                this.type = type;
                this.kind = kind;
                this.index = index;
            }
        }

        public SymbolTable()
        {
            classScope = new Dictionary<string, SymbolTableEntry>();
            subroutineScope = new Dictionary<string, SymbolTableEntry>();
            staticIndex = 0;
            fieldIndex = 0;
            argIndex = 0;
            varIndex = 0;
        }

        public void StartSubroutine()
        {
            subroutineScope = new Dictionary<string, SymbolTableEntry>();
            argIndex = 0;
            varIndex = 0;
        }

        public void Define(string name, string type, VariableCategory kind)
        {
            switch (kind)
            {
                case VariableCategory.ARG:
                    subroutineScope.Add(name, new SymbolTableEntry(type, kind, argIndex++));
                    break;
                case VariableCategory.FIELD:
                    classScope.Add(name, new SymbolTableEntry(type, kind, fieldIndex++));
                    break;
                case VariableCategory.STATIC:
                    classScope.Add(name, new SymbolTableEntry(type, kind, staticIndex++));
                    break;
                case VariableCategory.VAR:
                    subroutineScope.Add(name, new SymbolTableEntry(type, kind, varIndex++));
                    break;
            }
        }

        public int VarCount(VariableCategory kind)
        {
            switch (kind)
            {
                case VariableCategory.ARG:
                    return argIndex;
                case VariableCategory.FIELD:
                    return fieldIndex;
                case VariableCategory.STATIC:
                    return staticIndex;
                case VariableCategory.VAR:
                    return varIndex;
                default:
                    return 0;
            }
        }

        public VariableCategory KindOf(string name)
        {
            if(subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].kind;
            }
            else if(classScope.ContainsKey(name))
            {
                return classScope[name].kind;
            }
            else
            {
                return VariableCategory.NONE;
            }
        }

        public string TypeOf(string name)
        {
            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].type;
            }
            else if (classScope.ContainsKey(name))
            {
                return classScope[name].type;
            }
            else
            {
                return "Undefined";
            }
        }

        public int IndexOf(string name)
        {
            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].index;
            }
            else if (classScope.ContainsKey(name))
            {
                return classScope[name].index;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Debugging tool for listing all defined symbols in the table
        /// </summary>
        public void RollCall()
        {
            Console.WriteLine("\n** Class scope **");
            if(classScope.Count == 0)
            {
                Console.WriteLine("<empty>");
            }
            foreach (KeyValuePair<string, SymbolTableEntry> st in classScope)
            {
                Console.WriteLine(st.Key + " - " + st.Value.type + " " + st.Value.kind.ToString() + " " + st.Value.index);
            }
            
            Console.WriteLine("\n** Subroutine scope **");
            if (subroutineScope.Count == 0)
            {
                Console.WriteLine("<empty>");
            }
            foreach (KeyValuePair<string, SymbolTableEntry> st in subroutineScope)
            {
                Console.WriteLine(st.Key + " - " + st.Value.type + " " + st.Value.kind.ToString() + " " + st.Value.index);
            }

            Console.WriteLine("\n");
        }








    }
}
