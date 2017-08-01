using System;
using System.Collections.Generic;
using System.IO;


namespace Nand2Tetris_Compiler
{
    // At this stage, this is just a basic wrapper for unit testing the tokenizer and compilation engine modules.
    class Program
    {
        static void Main(string[] args)
        {
            
            string[] inFileNames;

            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a .jack file to process");
                return;
            }
            else if(File.Exists(args[0]))
            {
                if(Path.GetExtension(args[0]) != ".jack")
                {
                    Console.WriteLine("File must be a .jack file");
                    return;
                } 
                else
                {
                    inFileNames = new string[1] { args[0] };
                }
            }
            else if(Directory.Exists(args[0]))
            {
                inFileNames = Directory.GetFiles(args[0], "*.jack");
            }
            else
            {
                Console.WriteLine("File " + args[0] + " could not be found");
                return;

            }

            /*
            using (var infile = new StreamReader(inFileName))
            using (var tokenFile = new StreamWriter(tokenFileName))
            {
                JackTokenizer tokenizer = new JackTokenizer(infile);
                XMLWriter xml = new XMLWriter(tokenFile);

                tokenizer.ReadFileIntoTokenQueue();

                xml.OpenTag("tokens");

                while (tokenizer.HasMoreTokens)
                {
                    tokenizer.Advance();
                    xml.OpenCloseTag(tokenizer.TokenType.ToString(), tokenizer.TokenValue);
                }

                xml.CloseTag();
            }
            */
            
            foreach(string inFN in inFileNames)
            {
                using (var infile = new StreamReader(inFN))
                using (var outfile = new StreamWriter(Path.ChangeExtension(inFN, ".vm")))
                {
                    var compiler = new CompilationEngine(infile, outfile);
                }           
            }
            
            Console.ReadKey();
            
        }
    }
}
