using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nand2Tetris_VM
{
    class CodeWriter
    {
        private StreamWriter outFile;
        private string instructionsToWrite;
        private int _labelID;
        private string staticPrefix;

        private const int pointerBase = 3;
        private const int tempBase = 5;

        public CodeWriter(StreamWriter outFile, string inFileName)
        {
            this.outFile = outFile;
            setStaticVarPrefix(inFileName);
            _labelID = 0;            
        }

        private int nextIntLabel
        {
            get
            {
                return _labelID++;
            }
        }

        // the name of the input file is used as a prefix for static variables
        public void setStaticVarPrefix(string fileName)
        {
            staticPrefix = Path.GetFileNameWithoutExtension(fileName);
        }

        private void WriteArithmetic(string command)
        {
            switch(command)
            {
                case "add":
                    instructionsToWrite = AsmInstruction.stackAdd;
                    break;
                case "sub":
                    instructionsToWrite = AsmInstruction.stackSubtract;
                    break;
                case "neg":
                    instructionsToWrite = AsmInstruction.stackNegate;
                    break;
                case "and":
                    instructionsToWrite = AsmInstruction.stackAnd;
                    break;
                case "or":
                    instructionsToWrite = AsmInstruction.stackOr;
                    break;
                case "not":
                    instructionsToWrite = AsmInstruction.stackNot;
                    break;
                case "eq":
                    instructionsToWrite = AsmInstruction.stackEQ(nextIntLabel);
                    break;
                case "gt":
                    instructionsToWrite = AsmInstruction.stackGT(nextIntLabel);
                    break;
                case "lt":
                    instructionsToWrite = AsmInstruction.stackLT(nextIntLabel);
                    break;
                default:
                    Console.Error.WriteLine("ERROR: CodeWriter.WriteArithmetic() called with unknown command");
                    instructionsToWrite = "Unknown arithmetic command: " + command + "\n";
                    break;
            }
        }

        private void WritePushPop(CommandType command, string segment, int index)
        {
            if(command == CommandType.C_PUSH)
            {
                switch (segment)
                {
                    case "constant":
                        instructionsToWrite = AsmInstruction.constToD(index);
                        break;
                    case "local":
                        instructionsToWrite = AsmInstruction.segmentMemValToD("LCL", index);
                        break;
                    case "argument":
                        instructionsToWrite = AsmInstruction.segmentMemValToD("ARG", index);
                        break;
                    case "this":
                        instructionsToWrite = AsmInstruction.segmentMemValToD("THIS", index);
                        break;
                    case "that":
                        instructionsToWrite = AsmInstruction.segmentMemValToD("THAT", index);
                        break;
                    case "pointer":
                        instructionsToWrite = AsmInstruction.memLocValToD(pointerBase + index);
                        break;
                    case "temp":
                        instructionsToWrite = AsmInstruction.memLocValToD(tempBase + index);
                        break;
                    case "static":
                        instructionsToWrite = AsmInstruction.staticLabelToD(staticPrefix + "." + index);
                        break;
                    default:
                        instructionsToWrite = "Push from invalid segment: " + segment + "\n";
                        break;
                }
                // finish by pushing the contents of D to the top of the stack
                instructionsToWrite += AsmInstruction.pushFromD;
            }
            else if(command == CommandType.C_POP)
            {
                switch(segment)
                {
                    case "constant":
                        instructionsToWrite = "Attempted to pop to segment constant\n";
                        break;
                    case "local":
                        instructionsToWrite = AsmInstruction.popToSegmentMemVal("LCL", index);
                        break;
                    case "argument":
                        instructionsToWrite = AsmInstruction.popToSegmentMemVal("ARG", index);
                        break;
                    case "this":
                        instructionsToWrite = AsmInstruction.popToSegmentMemVal("THIS", index);
                        break;
                    case "that":
                        instructionsToWrite = AsmInstruction.popToSegmentMemVal("THAT", index);
                        break;
                    case "pointer":
                        instructionsToWrite = AsmInstruction.popToMemLocVal(pointerBase + index);
                        break;
                    case "temp":
                        instructionsToWrite = AsmInstruction.popToMemLocVal(tempBase + index);
                        break;
                    case "static":
                        instructionsToWrite = AsmInstruction.popToStaticLabel(staticPrefix + "." + index);
                        break;
                    default:
                        instructionsToWrite = "Pop to invalid segment: " + segment + "\n";
                        break;

                }
            }
            else
            {
                Console.Error.WriteLine("ERROR: CodeWriter.WritePushPop() called with non push/pop command type");
                instructionsToWrite = "Unknown push/pop command: " + command + " " + segment + " " + index +"\n";
            }
        }

        private void WriteLabel(string labelName)
        {
            instructionsToWrite = AsmInstruction.writeLabel(labelName);
        }

        private void WriteGoto(string labelName)
        {
            instructionsToWrite = AsmInstruction.jumpToLabel(labelName);
        }

        private void WriteIfGoto(string labelName)
        {
            instructionsToWrite = AsmInstruction.jumpNotZeroLabel(labelName);
        }

        private void WriteFunctionHeader(string funcName, int numLocals)
        {
            instructionsToWrite = AsmInstruction.functionHeader(funcName, numLocals);
        }

        private void WriteCall(string funcName, int numArgs)
        {
            instructionsToWrite = AsmInstruction.functionCall(funcName, numArgs, nextIntLabel);
        }

        private void WriteReturn()
        {
            instructionsToWrite = AsmInstruction.functionReturn;
        }

        public void WriteCommand(CommandType command, string arg1, int arg2)
        {
            switch (command)
            {
                case CommandType.C_ARITHMETIC:
                    WriteArithmetic(arg1);
                    break;
                case CommandType.C_POP:
                case CommandType.C_PUSH:
                    WritePushPop(command, arg1, arg2);
                    break;
                case CommandType.C_LABEL:
                    WriteLabel(arg1);
                    break;
                case CommandType.C_GOTO:
                    WriteGoto(arg1);
                    break;
                case CommandType.C_IF:
                    WriteIfGoto(arg1);
                    break;
                case CommandType.C_FUNCTION:
                    WriteFunctionHeader(arg1, arg2);
                    break;
                case CommandType.C_CALL:
                    WriteCall(arg1, arg2);
                    break;
                case CommandType.C_RETURN:
                    WriteReturn();
                    break;

                default:
                    Console.Error.WriteLine("ERROR: attempt to write unknown command type");
                    return;
            }
            outFile.Write(instructionsToWrite);
        }

        public void writeInit()
        {
            string instructionsToWrite = AsmInstruction.bootstrapSP +
                AsmInstruction.functionCall("Sys.init", 0, nextIntLabel);
            outFile.Write(instructionsToWrite);
        }
    }
}
