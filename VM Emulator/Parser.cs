using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Nand2Tetris_VM
{
    class Parser
    {

        private StreamReader stream;
        private string currentLine;
        public string arg1 { get; private set; }
        public int arg2 { get; private set; }
        public CommandType commandType { get; private set; }
        

        public Parser(StreamReader fileStream)
        {
            stream = fileStream;
            currentLine = "";
        }

        public bool hasMoreCommands
        {
            get
            {
                return !(stream.EndOfStream);
            }
        }

        public void Advance()
        {
            if(hasMoreCommands)
            {

                do
                {
                    currentLine = stream.ReadLine();
                    currentLine = CleanLine(currentLine);
                } while (String.IsNullOrWhiteSpace(currentLine));
                ParseCommandArgs(currentLine);
            }
            else
            {
                Console.Error.WriteLine("ERROR: Advance() called with no more commands");
            }
        }

        private string CleanLine(string line)
        {
            int length = line.Length;
            char[] chars = line.ToCharArray();
            int newLength = 0;
            bool lastWasSlash = false;
            bool lastWasWhiteSpace = false;
            for(int i =0; i < length; i++)
            {
                char c = chars[i];

                // strip out comments, prefixed by "//"
                if (c == '/')
                {
                    // if it is the second slash, back the index up one and break the loop; we're done with this line; otherwise, set lastWasSlash true
                    if (lastWasSlash)
                    {
                        newLength--;
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

                // add valid characters back into the string; reduce whitespace to single spaces
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasWhiteSpace)
                    {
                        lastWasWhiteSpace = true;
                        chars[newLength] = ' ';
                        newLength++;
                    }
                }
                else
                {
                    lastWasWhiteSpace = false;
                    chars[newLength] = c;
                    newLength++;
                }
            }

            return new string(chars, 0, newLength);
        }

        private void ParseCommandArgs(string line)
        {
            string[] args = line.Split(' ');
            commandType = CommandType.C_NULL;
            arg1 = "";
            arg2 = 0;
            if(args.Length > 0)
            {
                commandType = ParseCommandType(args[0]);
                if(commandType == CommandType.C_ARITHMETIC)
                {
                    arg1 = args[0].ToLower();
                    return;
                }
            }
            if(args.Length > 1)
            {
                arg1 = args[1];
            }
            if(args.Length > 2)
            {
                int t = 0;
                if (int.TryParse(args[2], out t)) { arg2 = t; }
                else { arg2 = 0; }
            }
        }

        private CommandType ParseCommandType(string command)
        {
            switch (command.ToLower())
            {
                case "add":
                case "sub":
                case "neg":
                case "eq":
                case "gt":
                case "lt":
                case "and":
                case "or":
                case "not":
                    return CommandType.C_ARITHMETIC;

                case "push":
                    return CommandType.C_PUSH;

                case "pop":
                    return CommandType.C_POP;

                case "label":
                    return CommandType.C_LABEL;

                case "goto":
                    return CommandType.C_GOTO;

                case "if-goto":
                    return CommandType.C_IF;

                case "function":
                    return CommandType.C_FUNCTION;

                case "call":
                    return CommandType.C_CALL;

                case "return":
                    return CommandType.C_RETURN;

                default:
                    return CommandType.C_NULL;
            }
        }
            
    }
}
