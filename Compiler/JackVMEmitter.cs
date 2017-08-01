using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nand2Tetris_Compiler
{
    class JackVMEmitter
    {

        private VMWriter writer;
        string currentClassName;
        int currentClassNumFields;

        public JackVMEmitter(StreamWriter outStream)
        {
            writer = new VMWriter(outStream);
            currentClassName = "";
            currentClassNumFields = 0;
        }

        public void SetCurrentClassName(string name)
        {
            currentClassName = name;
        }

        public string GetCurrentClassName()
        {
            return currentClassName;
        }

        public void SetCurrentClassNumFields(int numFields)
        {
            currentClassNumFields = numFields;
        }


        /// <summary>
        /// Gets the proper memory segment for a given variable category from the symbol table. Returns "temp" for invalid variables, because that does the least damage.
        /// </summary>
        public VMWriter.Segment GetVarSegment(VariableCategory varCat)
        {
            switch (varCat)
            {
                case VariableCategory.ARG:
                    return VMWriter.Segment.ARG;
                case VariableCategory.FIELD:
                    return VMWriter.Segment.THIS;
                case VariableCategory.STATIC:
                    return VMWriter.Segment.STATIC;
                case VariableCategory.VAR:
                    return VMWriter.Segment.LOCAL;
                default:
                    return VMWriter.Segment.TEMP;
            }
        }

        public void emitSubroutine(string name, int numLocals)
        {
            writer.WriteFunction(currentClassName + "." + name, numLocals);
        }

        public void emitLetStatement(VMWriter.Segment seg, int num)
        {
            // if we're popping to an array object, we need to grab the THAT pointer off the stack first
            if (seg == VMWriter.Segment.THAT)
            {
                emitPopSecondStackValue(VMWriter.Segment.POINTER, 1);
            }
            writer.WritePop(seg, num);
        }

        public void emitOperator(string op)
        {
            switch (op)
            {
                // binary commands
                case "+":
                    writer.WriteArithmetic(VMWriter.ArithCommand.add);
                    break;
                case " - ":
                    writer.WriteArithmetic(VMWriter.ArithCommand.sub);
                    break;
                case "*":
                    writer.WriteCall("Math.multiply", 2);
                    break;
                case "/":
                    writer.WriteCall("Math.divide", 2);
                    break;
                case "&":
                    writer.WriteArithmetic(VMWriter.ArithCommand.and);
                    break;
                case "|":
                    writer.WriteArithmetic(VMWriter.ArithCommand.or);
                    break;
                case "<":
                    writer.WriteArithmetic(VMWriter.ArithCommand.lt);
                    break;
                case ">":
                    writer.WriteArithmetic(VMWriter.ArithCommand.gt);
                    break;
                case "=":
                    writer.WriteArithmetic(VMWriter.ArithCommand.eq);
                    break;

                // unary commands
                case "-":
                    writer.WriteArithmetic(VMWriter.ArithCommand.neg);
                    break;
                case "~":
                    writer.WriteArithmetic(VMWriter.ArithCommand.not);
                    break;



            }
        }


        public void emitArrayIndex(VMWriter.Segment arraySeg, int arrayIndex, bool isLetStatement = false)
        {
            writer.WritePush(arraySeg, arrayIndex);
            writer.WriteArithmetic(VMWriter.ArithCommand.add);
            // TODO: this needs to change; leave it on the stack, perform other operations, then pop when needed.
            if(!isLetStatement)
            {
                writer.WritePop(VMWriter.Segment.POINTER, 1);
            }
        }


        public void emitTerm(VMWriter.Segment seg, int value)
        {
            writer.WritePush(seg, value);
        }

              
        public void emitSubroutineCall(string name, int numArgs)
        {
            writer.WriteCall(name, numArgs);
        }

        
        public void emitDo()
        {
            // TODO: right now, this only handles things that discard the return value.  Needs to be changed for... let statements, maybe?
            writer.WritePop(VMWriter.Segment.TEMP, 0);
        }

        
        public void emitReturn()
        {
            writer.WriteReturn();
        }

        public void emitWhileStart(int labelNum)
        {
            writer.WriteLabel("WHILE_START_" + labelNum);
        }

        public void emitWhileStatementBlock(int labelNum)
        {
            writer.WriteArithmetic(VMWriter.ArithCommand.not);
            writer.WriteIf("WHILE_END_" + labelNum);
        }

        public void emitWhileEnd(int labelNum)
        {
            writer.WriteGoto("WHILE_START_" + labelNum);
            writer.WriteLabel("WHILE_END_" + labelNum);
        }

        public void emitIfStart(int labelNum)
        {
            writer.WriteArithmetic(VMWriter.ArithCommand.not);
            writer.WriteIf("IF_FALSE_" + labelNum);
        }


        public void emitIfElseStatementBlock(int labelNum)
        {
            writer.WriteGoto("IF_END_" + labelNum);
            writer.WriteLabel("IF_FALSE_" + labelNum);
        }

        public void emitIfEnd(int labelNum, bool hasElse = false)
        {
            if(!hasElse)
            {
                writer.WriteLabel("IF_FALSE_" + labelNum);
            }
            else
            {
                writer.WriteLabel("IF_END_" + labelNum);
            }
        }


        public void emitBooleanConstant(string boolConst)
        {
            if(boolConst == "true")
            {
                writer.WritePush(VMWriter.Segment.CONST, 1);
                writer.WriteArithmetic(VMWriter.ArithCommand.neg);
            }
            else
            {
                writer.WritePush(VMWriter.Segment.CONST, 0);
            }
        }

        /// <summary>
        /// Allocates memory for an instance of the current class (one word for each field) and then pops the returned pointer to THIS.
        /// </summary>
        public void emitClassMemoryAllocation()
        {
            writer.WritePush(VMWriter.Segment.CONST, currentClassNumFields);
            writer.WriteCall("Memory.alloc", 1);
            writer.WritePop(VMWriter.Segment.POINTER, 0);
        }

        public void emitSetThisPointer()
        {
            writer.WritePush(VMWriter.Segment.ARG, 0);
            writer.WritePop(VMWriter.Segment.POINTER, 0);
        }

        public void emitStringConstant(string stringConst)
        {
            writer.WritePush(VMWriter.Segment.CONST, stringConst.Length);
            writer.WriteCall("String.new", 1);
            foreach(char c in stringConst)
            {
                writer.WritePush(VMWriter.Segment.CONST, (int)c);
                writer.WriteCall("String.appendChar", 2);
            }            
        }

        public void emitPopSecondStackValue(VMWriter.Segment seg, int segIndex)
        {
            writer.WritePop(VMWriter.Segment.TEMP, 0);
            writer.WritePop(seg, segIndex);
            writer.WritePush(VMWriter.Segment.TEMP, 0);
        }
    }
}
