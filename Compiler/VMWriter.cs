using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nand2Tetris_Compiler
{
    // extension class to allow easy forming of strings from the VMWriter.Segment enum
    public static class VMWriterExtensions
    {
        internal static string VMString(this VMWriter.Segment seg)
        {
            switch (seg)
            {
                case VMWriter.Segment.CONST:
                    return "constant";
                case VMWriter.Segment.ARG:
                    return "argument";
                case VMWriter.Segment.LOCAL:
                    return "local";
                case VMWriter.Segment.POINTER:
                    return "pointer";
                case VMWriter.Segment.STATIC:
                    return "static";
                case VMWriter.Segment.TEMP:
                    return "temp";
                case VMWriter.Segment.THAT:
                    return "that";
                case VMWriter.Segment.THIS:
                    return "this";
                default:
                    return "";
            }
        }
    }

    class VMWriter
    {

        public enum Segment { CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP };
        public enum ArithCommand { add, sub, neg, eq, gt, lt, and, or, not };

        private StreamWriter outFile;

        public VMWriter(StreamWriter file)
        {
            outFile = file;
        }

        public void WritePush(Segment seg, int index)
        {
            outFile.WriteLine("push " + seg.VMString() + " " + index);
        }

        public void WritePop(Segment seg, int index)
        {
            outFile.WriteLine("pop " + seg.VMString() + " " + index);
        }

        public void WriteArithmetic(ArithCommand command)
        {
            outFile.WriteLine(command.ToString());
        }

        public void WriteLabel (string label)
        {
            outFile.WriteLine("label " + label);
        }

        public void WriteGoto(string label)
        {
            outFile.WriteLine("goto " + label);
        }

        public void WriteIf(string label)
        {
            outFile.WriteLine("if-goto " + label);
        }

        public void WriteCall(string name, int numArgs)
        {
            outFile.WriteLine("call " + name + " " + numArgs);
        }

        public void WriteFunction(string name, int numLocals)
        {
            outFile.WriteLine("function " + name + " " + numLocals);
        }

        public void WriteReturn()
        {
            outFile.WriteLine("return");
        }

    }
}
