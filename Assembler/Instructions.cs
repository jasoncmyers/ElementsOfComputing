using System;
using System.Collections.Generic;


namespace Nand2Tetris_Assembler
{
    static class Instructions
    {
        // This contains the possible values for jump/comparison instructions and their corresponding binary strings
        private static Dictionary<string, string> JumpValues = new Dictionary<string, string>
        {
            {"NULL", "000" },
            {"JGT",  "001" },
            {"JEQ",  "010" },
            {"JGE",  "011" },
            {"JLT",  "100" },
            {"JNE",  "101" },
            {"JLE",  "110" },
            {"JMP",  "111" }
        };

        // These are the various register combinations and corresponding binary
        private static Dictionary<string, string> DestValues = new Dictionary<string, string>
        {
            {"NULL", "000" },
            {"M", "001" },
            {"D", "010" },
            {"MD", "011" },
            {"A", "100" },
            {"AM", "101" },
            {"AD", "110" },
            {"AMD", "111" }
        };

        // This contains the possible computations the Hack architecture can perform and their binary equivalents
        private static Dictionary<string, string> CompValues = new Dictionary<string, string>
        {
            {"0",   "0101010" },
            {"1",   "0111111" },
            {"-1",  "0111010" },
            {"D",   "0001100" },
            {"A",   "0110000" },
            {"!D",  "0001101" },
            {"!A",  "0110001" },
            {"-D",  "0001111" },
            {"-A",  "0110011" },
            {"D+1", "0011111" },
            {"A+1", "0110111" },
            {"D-1", "0001110" },
            {"A-1", "0110010" },
            {"D+A", "0000010" },
            {"D-A", "0010011" },
            {"A-D", "0000111" },
            {"D&A", "0000000" },
            {"D|A", "0010101" },
            {"M",   "1110000" },
            {"!M",  "1110001" },
            {"-M",  "1110011" },
            {"M+1", "1110111" },
            {"M-1", "1110010" },
            {"D+M", "1000010" },
            {"D-M", "1010011" },
            {"M-D", "1000111" },
            {"D&M", "1000000" },
            {"D|M", "1010101" }
        };

        // This form of MakeBinCommand() produces binary commands from C-instructions 
        // It uses the above dictionaries to look up the 3 portions of the instruction
        public static string MakeBinCommand(string dest, string comp, string jump)
        {
            // C-instructions all begin with "111"
            string bin = "111";
            if (CompValues.ContainsKey(comp))
                bin += CompValues[comp];
            else
                return "ERROR, unknown 'comp' instruction";

            if (DestValues.ContainsKey(dest))
                bin += DestValues[dest];
            else
                return "ERROR, unknown 'dest' instruction";

            if (JumpValues.ContainsKey(jump))
                bin += JumpValues[jump];
            else
                return "ERROR, unknown 'jump' instruction";

            return bin;
        }

        // and this form of MakeBinCommand() produces binary commands from A-instructions
        // A-instructions all begin with a zero, followed by a 15-bit memory address.
        public static string MakeBinCommand(int address)
        {
            string bin = Convert.ToString(address, 2);
            for(int i = bin.Length; i < 16; i++)
            {
                bin = "0" + bin;
            }
            return bin;
        }
    }
}
