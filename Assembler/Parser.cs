using System;
using System.IO;


namespace Nand2Tetris_Assembler
{
    class Parser
    {
        public string currentLine { get; private set; }
        private StreamReader stream;
        enum InstructionType { AInstruction, CInstruction, LInstruction };
        private InstructionType currentInstrType;
        private SymbolTable symbolTable;
        
        // these are the public API requested by the project
        public int nextCommandNum { get; private set; }
        public string dest { get; private set; }
        public string comp { get; private set; }
        public string jump { get; private set; }
        public string address { get; private set; }


        public Parser(StreamReader fileStream)
        {
            currentLine = "";
            stream = fileStream;
            nextCommandNum = 0;
            symbolTable = new SymbolTable();
        }

        public bool hasMoreCommands
        {
            get
            {
                return !(stream.EndOfStream);
            }
        }

        public void advance()
        {
            if(hasMoreCommands)
            {
                do
                {
                    currentLine = stream.ReadLine();
                    currentLine = Clean(currentLine);
                } while (String.IsNullOrEmpty(currentLine));

                if (currentLine[0] == '@')
                {
                    currentInstrType = InstructionType.AInstruction;
                    nextCommandNum++;
                }
                else if (currentLine[0] == '(')
                {
                    currentInstrType = InstructionType.LInstruction;
                }
                else
                {
                    currentInstrType = InstructionType.CInstruction;
                    nextCommandNum++;
                }
            }
        }

        private string Clean(string line)
        {
            int length = line.Length;
            char[] chars = line.ToCharArray();
            int index = 0;
            bool lastWasSlash = false;
            for (int i = 0; i < length; i++)
            {
                char c = chars[i];

                // code to strip out comments
                if (c == '/')
                {
                    if (lastWasSlash)
                    {
                        index--;
                        break;
                    }
                    else
                    {
                        lastWasSlash = true;
                    }
                }
                else
                {
                    lastWasSlash = false;               
                }

                // if the character is not white space, add it to the character array and increase the index.  If it is, skip it
                if (!char.IsWhiteSpace(c))
                {
                    chars[index] = c;
                    index++;
                }
            }
            
            return new string(chars, 0, index);
        }


        // public command to parse either type of instruction
        public void Parse(string line)
        {
            if (currentInstrType == InstructionType.AInstruction)
                ParseAInstruction(line);
            else if (currentInstrType == InstructionType.LInstruction)
                ParseLInstruction(line);
            else
                ParseCInstruction(line);
        }     

        // this command parses C-instructions and populates the dest, comp, and jump fields (address field made empty)
        private void ParseCInstruction(string line)
        {
            address = "";
            string s = "";
            // split the dest argument from everything else by looking for '='
            string[] temp = line.Split('=');
            if(temp.Length == 1)
            {
                dest = "NULL";
                s = temp[0];
            }
            else if(temp.Length == 2)
            {
                dest = temp[0];
                s = temp[1];
            }

            // split the comp and jump arguments by looking for ';'
            temp = s.Split(';');
            if(temp.Length == 1)
            {
                jump = "NULL";
                comp = temp[0];
            }
            else if(temp.Length == 2)
            {
                jump = temp[1];
                comp = temp[0];
            }
        }

        // parses A-instructions and populates the address field; empties dest, comp, and jump fields
        private void ParseAInstruction(string line)
        {
            dest = ""; comp = ""; jump = "";
            address = line.Trim(new char[] {'@'});
        }

        private void ParseLInstruction(string line)
        {
            dest = ""; comp = ""; jump = ""; address = "";
            string symbol = line.Trim(new char[] { '(', ')' });

            if (!symbolTable.Contains(symbol))
            {
                symbolTable.AddEntry(symbol, nextCommandNum);
                Console.WriteLine("Encountered label: " + line + "; adding " + symbol + " to symbol table as instruction " + nextCommandNum);
                Console.WriteLine();
            }
        }

        public void ParseLabels()
        {
            while(!stream.EndOfStream)
            {
                this.advance();
                if(currentInstrType == InstructionType.LInstruction)
                {
                    Parse(currentLine);
                }
            }
            stream.BaseStream.Position = 0;
            stream = new StreamReader(stream.BaseStream);
            nextCommandNum = 0;
        }

        public int ParseAddress()
        {
            int intAddr;
            if(!int.TryParse(address, out intAddr))
            {
                if (symbolTable.Contains(address))
                    intAddr = symbolTable.GetAddress(address);
                else
                    intAddr = symbolTable.AddEntryNextMemory(address);
            }

            return intAddr;
        }
    }
}
