using System;
using System.IO;

namespace Nand2Tetris_Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader file;
            StreamWriter outFile;
            string s;       // general purpose temp string


            if (args.Length > 0 && File.Exists(args[0]))
            {
                s = args[0];
            }
            else
            {
                Console.WriteLine("Specified file not found");
                return;
            }

            using (file = new StreamReader(s))
            using (outFile = new StreamWriter(Path.ChangeExtension(s, ".hack")))
            {

                // initialize the parser and populate its symbol table
                Parser parser = new Parser(file);
                parser.ParseLabels();


                while (parser.hasMoreCommands)
                {
                    parser.advance();
                    s = parser.currentLine;
                    parser.Parse(s);
                    //Console.WriteLine("Instruction " + (parser.nextCommandNum - 1) + ": " + s);
                    //Console.WriteLine("dest = " + parser.dest + "; comp = " + parser.comp + "; jump = " + parser.jump + "; address = " + parser.address);
                    string binInstruct = "";
                    if (parser.address == "" && parser.comp != "")
                        binInstruct = Instructions.MakeBinCommand(parser.dest, parser.comp, parser.jump);
                    else if (parser.address != "")
                        binInstruct = Instructions.MakeBinCommand(parser.ParseAddress());
                    //Console.WriteLine("binary = " + binInstruct);
                    //Console.WriteLine();

                    if (binInstruct != "")
                        outFile.WriteLine(binInstruct);
                }
            }
        }
    }
}
