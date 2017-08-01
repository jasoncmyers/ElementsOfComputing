using System.Collections.Generic;

namespace Nand2Tetris_Assembler
{

    class SymbolTable
    {

        private Dictionary<string, int> symbols;
        private int nextOpenMemory;

        public SymbolTable()
        {
            // create a symbol table initialized with the pre-defined symbols
            symbols = new Dictionary<string, int>
            {
                {"SP",     0 },
                {"LCL",    1 },
                {"ARG",    2 },
                {"THIS",   3 },
                {"THAT",   4 },
                {"R0",     0 },
                {"R1",     1 },
                {"R2",     2 },
                {"R3",     3 },
                {"R4",     4 },
                {"R5",     5 },
                {"R6",     6 },
                {"R7",     7 },
                {"R8",     8 },
                {"R9",     9 },
                {"R10",    10 },
                {"R11",    11 },
                {"R12",    12 },
                {"R13",    13 },
                {"R14",    14 },
                {"R15",    15 },
                {"SCREEN", 16384 },
                {"KBD",    24576 }
            };

            nextOpenMemory = 16;    // 16 is the first assignable memory location
        }

        public void AddEntry(string symbol, int address)
        {
            symbols.Add(symbol, address);
        }

        public int AddEntryNextMemory(string symbol)
        {
            symbols.Add(symbol, nextOpenMemory);
            return nextOpenMemory++;
        }

        public bool Contains(string symbol)
        {
            return (symbols.ContainsKey(symbol));
        }

        public int GetAddress(string symbol)
        {
            return symbols[symbol];
        }

    }
}
