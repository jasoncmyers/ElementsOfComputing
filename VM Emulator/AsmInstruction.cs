using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nand2Tetris_VM
{
    // Utility class to keep the actual ASM implementations separate from the VM instructions referenced
    // by the rest of the VM emulator
    public static class AsmInstruction
    {
        public const string pushFromD =
            "@SP\n" +
            "A=M\n" +
            "M=D\n" +
            "@SP\n" +
            "M=M+1\n";

        public const string popToD =
            "@SP\n" +
            "AM=M-1\n" +
            "D=M\n";

        private const string AtoStackTop =
            "@SP\n" +
            "A=M-1\n";

        public const string stackAdd =
            popToD +
            "A=A-1\n" +
            "M=D+M\n";

        public const string stackSubtract =
            popToD +
            "A=A-1\n" +
            "M=M-D\n";

        public const string stackNegate =
            AtoStackTop +
            "M=-M\n";

        public const string stackAnd =
            popToD +
            "A=A-1\n" +
            "M=D&M\n";

        public const string stackOr =
            popToD +
            "A=A-1\n" +
            "M=M|D\n";

        public const string stackNot =
            AtoStackTop +
            "M=!M\n";

        private static string stackCmp(int LabelID, string cmpType)
        {
            return popToD +
                "A=A-1\n" +
                "D=M-D\n" +
                "@CmpTrue" + LabelID + "\n" +
                "D;J" + cmpType + "\n" +
                AtoStackTop +
                "M=0\n" +
                "@CmpDone" + LabelID + "\n" +
                "0;JMP\n" +
                "(CmpTrue" + LabelID + ")\n" +
                AtoStackTop +
                "M=-1\n" +
                "(CmpDone" + LabelID + ")\n";
        }

        public static string stackLT(int LabelID)
        {
            return stackCmp(LabelID, "LT");
        }

        public static string stackGT(int LabelID)
        {
            return stackCmp(LabelID, "GT");
        }

        public static string stackEQ(int LabelID)
        {
            return stackCmp(LabelID, "EQ");
        }

        public static string constToD(int constant)
        {
            return "@" + constant + "\n" + "D=A\n";
        }

        public static string segmentMemValToD(string segment, int index)
        {
            return constToD(index) + 
                "@" + segment + "\n" +
                "A=D+M\n" +
                "D=M\n";
        }

        public static string memLocValToD(int location)
        {
            return "@" + location + "\n" + "D=M\n";
        }

        public static string staticLabelToD(string label)
        {
            return "@" + label + "\n" + "D=M\n";
        }

        private static string segAddrToR13(string segment, int index)
        {
            return constToD(index) +
                "@" + segment + "\n" +
                "D=D+M\n" +
                "@R13\n" +
                "M=D\n";
        }


        private const string popToLocR13 =
            popToD +
            "@R13\n" +
            "A=M\n" +
            "M=D\n";

        public static string popToSegmentMemVal(string segment, int index)
        {
            return segAddrToR13(segment, index) + popToLocR13;             
        }

        public static string popToMemLocVal(int location)
        {
            return popToD +
                "@" + location + "\n" + "M=D\n";
        }

        public static string popToStaticLabel(string label)
        {
            return popToD +
                "@" + label + "\n" + "M=D\n";
        }

        public static string writeLabel(string label)
        {
            return "(" + label + ")\n";
        }

        public static string jumpToLabel(string label)
        {
            return "@" + label + "\n" +
                "0;JMP\n";
        }

        public static string jumpNotZeroLabel(string label)
        {
            return "@SP\n" +
                "AM=M-1\n" +
                "D=M\n" +
                "@" + label + "\n" +
                "D;JNE\n";
        }

        public static string functionHeader(string label, int numLocals)
        {
            string instructions = "(" + label + ")\n";
            for(int i = 0; i < numLocals; i++)
            {
                instructions += "D=0\n" + pushFromD;
            }
            return instructions;
        }

        public static string functionCall(string label, int numArgs, int returnLabelNum)
        {
            return "@ReturnAddress" + returnLabelNum + "\nD=A\n" + pushFromD +    // push return address (not yet written) to the stack
                "@LCL\nD=M\n" + pushFromD +       // then push LCL pointer 
                "@ARG\nD=M\n" + pushFromD +       // then ARG pointer
                "@THIS\nD=M\n" + pushFromD +      // then THIS and THAT
                "@THAT\nD=M\n" + pushFromD +

                "@SP\n" +       // set LCL pointer to SP pointer
                "D=M\n" +
                "@LCL\n" +
                "M=D\n" +
                
                "@5\n" +        // set ARG pointer to SP - 5 - numArgs (D still = SP)
                "D=D-A\n" +
                "@" + numArgs + "\n" +
                "D=D-A\n" +
                "@ARG\n" +
                "M=D\n" +

                jumpToLabel(label) +    // jump to the specified function label
                writeLabel("ReturnAddress" + returnLabelNum);
        }

        public const string functionReturn =

            // Store the return address in R14 before any other operations to avoid possibly overwriting 
            // (e.g., if the function had no args)
            "@5\n" +        
            "D=A\n" +
            "@LCL\n" +
            "A=M-D\n" +
            "D=M\n" +
            "@R14\n" +
            "M=D\n" +

            // Pop the top stack value (function return value) and place it at what will become the top 
            // of the stack (currently pointed by ARG)
            popToD +        
            "@ARG\n" +
            "A=M\n" +
            "M=D\n" +

            // Place SP just above the new stack top (i.e., ARG + 1)
            "D=A+1\n" +     
            "@SP\n" +
            "M=D\n" +

            // Store the value of LCL-1 in R13 and load the value at that address back into D 
            "@LCL\n" +      
            "D=M-1\n" +
            "@R13\n" +
            "AM=D\n" +
            "D=M\n" +
            "@THAT\n" +     // set THAT to the address in D
            "M=D\n" +

            "@R13\n" +      // decrement the value from R13 and load into D
            "AM=M-1\n" +
            "D=M\n" +
            "@THIS\n" +     // then set THIS to the address in D
            "M=D\n" +

            "@R13\n" +      // repeat for ARG
            "AM=M-1\n" +
            "D=M\n" +
            "@ARG\n" +
            "M=D\n" +

            "@R13\n" +      // and for LCL
            "AM=M-1\n" +
            "D=M\n" +
            "@LCL\n" +
            "M=D\n" +

            "@R14\n" +      // finally, jump to the return address, which was stored in R14
            "A=M\n" +
            "0;JMP\n";


        // set SP to 256 (the beginning of VM stack memory)
        public const string bootstrapSP =
            "@256\n" +              
            "D=A\n" +
            "@SP\n" +
            "M=D\n";
    }
}
