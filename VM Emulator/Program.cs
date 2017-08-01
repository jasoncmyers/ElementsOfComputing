using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nand2Tetris_VM
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inFileNames; 
            string outFileName;
                        
            if(args.Length == 0)
            {
                Console.WriteLine("Please specify a file or directory name");
                return;
            }

            // If the parameter is a single file, we process only it; if a directory, all .vm files in that
            // directory are processed. Regardless, the output is a single .asm file
            if (File.Exists(args[0]))
            {
                inFileNames = new string[1] { args[0] };
                outFileName = Path.ChangeExtension(inFileNames[0], ".asm");
            }
            else if(Directory.Exists(args[0]))
            {
                inFileNames = Directory.GetFiles(args[0], "*.vm");
                outFileName = args[0] + ".asm";
            }
            else
            {
                Console.WriteLine("Could not find file or directory " + args[0]);
                return;
            }
           
            using (var outFile = new StreamWriter(outFileName))
            {
                CodeWriter writer = new CodeWriter(outFile, inFileNames[0]);
                writer.writeInit();
                foreach (string inFileName in inFileNames)
                {
                    using (var inFile = new StreamReader(inFileName))
                    {
                        Parser parser = new Parser(inFile);
                        writer.setStaticVarPrefix(inFileName);
                        while (parser.hasMoreCommands)
                        {
                            parser.Advance();
                            writer.WriteCommand(parser.commandType, parser.arg1, parser.arg2);
                        }
                    }
                }
            }
        }
    }
}
