using System;
using System.IO;
using System.Collections.Generic;


namespace Nand2Tetris_Compiler
{
    class CompilationEngine
    {
        private JackTokenizer tokenizer;
        private JackVMEmitter emitter;
        private JackToken[] validatedTokens;
        private SymbolTable symbolTable;
        private int whileCounter;
        private int ifCounter;


        public CompilationEngine(StreamReader inStream, StreamWriter outStream)
        {
            tokenizer = new JackTokenizer(inStream);
            emitter = new JackVMEmitter(outStream);
            symbolTable = new SymbolTable();
            whileCounter = 0;
            ifCounter = 0;

            // This should probably go somewhere other than the constructor, but for now this is where initialization happens
            tokenizer.ReadFileIntoTokenQueue();
            CompileClass();
        }
        
        /// <summary>
        /// Used to check for (and consume) an expected token sequence. Strings are assumed to be exact values unless prefaced with '?', which indicates a token type.
        /// <para>Prefacing with "%" allows a space-separated list of possible values. "%type" is a special case allowed here that checks for allowed variable names</para>
        /// <para>Prefacing with "^" allows a space-separated list of possible types.</para>
        /// <para>Using "..." allows there to be any number of other parameters until the next term is matched.</para>
        /// <para>The out parameter validatedTokens holds the tokens that passed their checks.</para>
        /// </summary>
        private bool CheckTokenSequence(out JackToken[] validatedTokens, params string[] tokensToCheck)
        {
            List<JackToken> tokenVals = new List<JackToken>(tokensToCheck.Length);
            bool errorFound = false;
            string errorMsg = null;


            for (int i = 0; i < tokensToCheck.Length; i++)
            {
                tokenizer.Advance();

                // special logic for the ellipse; skip any number of tokens until finding a matching token
                if (tokensToCheck[i] == "...")
                {
                    int startLine = tokenizer.CurrentToken.LineNumber;

                    while (tokenizer.HasMoreTokens && !CheckToken(tokenizer.PeakAhead(), tokensToCheck[i + 1], out errorMsg))
                    {
                        tokenVals.Add(tokenizer.CurrentToken);
                        tokenizer.Advance();
                    }
                    if (!tokenizer.HasMoreTokens)
                    {
                        errorFound = true;
                        break;
                    }
                }

                else
                {
                    if (CheckToken(tokenizer.CurrentToken, tokensToCheck[i], out errorMsg))
                    {
                        tokenVals.Add(tokenizer.CurrentToken);
                    }
                    else
                    {
                        errorFound = true;
                        WriteError(errorMsg);
                        break;
                    }
                }
            }

            validatedTokens = tokenVals.ToArray();

            return !errorFound;
        }


        /// <summary>
        /// Helper function for CheckTokenSequence().  Should not be called independently. 
        /// </summary>
        private bool CheckToken(JackToken jt, string checkValue)
        {
            string dummy;
            return CheckToken(jt, checkValue, out dummy);
        }

        /// <summary>
        /// Helper function for CheckTokenSequence().  Should not be called independently. 
        /// </summary>
        private bool CheckToken(JackToken jt, string checkValue, out string errorMessage)
        {
            bool errorFound = false;
            bool checkType = false;
            bool listOfValues = false;
            bool listOfTypes = false;

            errorMessage = checkValue;
            
            checkType = (checkValue[0] == '?');
            listOfValues = (checkValue[0] == '%');
            listOfTypes = (checkValue[0] == '^');
            
            if (checkType)
            {
                if (jt.Type.ToString() != checkValue.Substring(1))
                {
                    errorMessage = checkValue.Substring(1);
                    errorFound = true;
                }
            }
            else if (listOfValues || listOfTypes)
            {
                bool matchFound = false;
                bool isTypeCheck = false;
                string[] values = checkValue.Split('%', '^', ' ');

                string matchValue = listOfValues ? jt.Value : jt.Type.ToString();

                foreach (string s in values)
                {
                    if (s == "type")
                    {
                        matchFound = jt.IsType();
                    }
                    else if (s == matchValue)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (isTypeCheck && !matchFound)
                {
                    if (jt.Type.ToString() == "identifier") matchFound = true;
                }

                if (!matchFound)
                {
                    errorMessage = "one of [" + checkValue.Substring(1) + "]";
                    errorFound = true;
                }
            }

            else
            {
                if (jt.Value != checkValue)
                {
                    errorMessage = checkValue;
                    errorFound = true;
                }
            }

            return !errorFound;

        }
    
        
        // this must be the first method called after the CompilationEngine is initialized
        public void CompileClass()
        {
            if (CheckTokenSequence(out validatedTokens, "class", "?identifier", "{"))
            {
                emitter.SetCurrentClassName(validatedTokens[1].Value);
                while(tokenizer.PeakAhead().Value == "static" || tokenizer.PeakAhead().Value == "field")
                {
                    CompileClassVarDec();
                }

                emitter.SetCurrentClassNumFields(symbolTable.VarCount(VariableCategory.FIELD));

                while (tokenizer.PeakAhead().Value == "constructor" || tokenizer.PeakAhead().Value == "method" || tokenizer.PeakAhead().Value == "function")
                {
                    CompileSubroutine();
                }
                CheckTokenSequence(out validatedTokens, "}");
            }
        }

        private void CompileClassVarDec()
        {
            
            if (CheckTokenSequence(out validatedTokens, "%static field", "%type", "?identifier", "%; ,"))
            {
                // define a new symbol table entry using the validated token name, type, and static or field status
                string varType = validatedTokens[1].Value;
                VariableCategory varKind = (validatedTokens[0].Value == "static") ? VariableCategory.STATIC : VariableCategory.FIELD;
                symbolTable.Define(validatedTokens[2].Value, varType, varKind);
                if (tokenizer.CurrentToken.Value != ";")                             
                {
                    List<JackToken> valTokenList = new List<JackToken>(validatedTokens);
                    while (tokenizer.CurrentToken.Value == ",")
                    {
                        if (CheckTokenSequence(out validatedTokens, "?identifier", "%; ,"))
                        {
                            symbolTable.Define(validatedTokens[0].Value, varType, varKind);
                               
                            valTokenList.AddRange(validatedTokens);
                        }
                    }
                }
            }
        }

        public void CompileSubroutine()
        {
            if (CheckTokenSequence(out validatedTokens, "%constructor function method", "%type void", "?identifier", "("))
            {
                string subName = validatedTokens[2].Value;
                string subType = validatedTokens[0].Value;
                int numLocals = 0;

                symbolTable.StartSubroutine();
                if(subType == "method")
                {
                    symbolTable.Define("this", "pointer", VariableCategory.ARG);
                }
                CompileParameterList();
                while (tokenizer.PeakAhead().Value == "var")
                {
                    numLocals += CompileVarDec();
                }
                emitter.emitSubroutine(subName, numLocals);
                switch (subType)
                {
                    case "constructor":
                        emitter.emitClassMemoryAllocation();
                        break;
                    case "method":
                        emitter.emitSetThisPointer();
                        break;
                    case "function":
                        break;
                }
                CompileStatements();
                CheckTokenSequence(out validatedTokens, "}");                
            }
        }


        public void CompileParameterList()
        {
            List<JackToken> parameters = new List<JackToken>();
 
            while (tokenizer.PeakAhead().Value != ")")
            {
                if (CheckTokenSequence(out validatedTokens, "%type", "?identifier"))
                {
                    parameters.AddRange(validatedTokens);
                    symbolTable.Define(validatedTokens[1].Value, validatedTokens[0].Value, VariableCategory.ARG);
                }
                if (tokenizer.PeakAhead().Value == ",")
                {
                    tokenizer.Advance();
                    parameters.Add(tokenizer.CurrentToken);
                }
            }                       
            CheckTokenSequence(out validatedTokens, ")", "{");
        }

        /// <summary>
        /// Compiles the local variable declarations for a subroutine.  Returns the number of local variables.
        /// </summary>
        public int CompileVarDec()
        {
            int numLocals = 0;

            if (CheckTokenSequence(out validatedTokens, "var", "%type", "?identifier", "%; ,"))
            {
                string varType = validatedTokens[1].Value;
                symbolTable.Define(validatedTokens[2].Value, varType, VariableCategory.VAR);
                numLocals++;
                if (tokenizer.CurrentToken.Value != ";")                
                {
                    List<JackToken> valTokenList = new List<JackToken>(validatedTokens);
                    while (tokenizer.CurrentToken.Value == ",")
                    {
                        if (CheckTokenSequence(out validatedTokens, "?identifier", "%; ,"))
                        {
                            symbolTable.Define(validatedTokens[0].Value, varType, VariableCategory.VAR);
                            numLocals++;
                            valTokenList.AddRange(validatedTokens);
                        }
                    }                    
                }
            }

            return numLocals;

        }

        public void CompileStatements()
        {
            while(tokenizer.PeakAhead().Value != "}")
            {
                switch(tokenizer.PeakAhead().Value)
                {
                    case "let":
                        CompileLet();
                        break;
                    case "do":
                        CompileDo();
                        break;
                    case "while":
                        CompileWhile();
                        break;
                    case "if":
                        CompileIf();
                        break;
                    case "return":
                        CompileReturn();
                        break;
                    default:
                        WriteError("statement (let, do, while, if, return)");
                        tokenizer.Advance();    // this will eventually get to a valid statement or {
                        break;
                }
            }
        }

        public void CompileDo()
        {
            if (CheckTokenSequence(out validatedTokens, "do", "?identifier"))
            {
                CompileSubroutineCall();
            }

            if (CheckTokenSequence(out validatedTokens, ";"))
            {
                emitter.emitDo();
            }
            
        }

        public void CompileLet()
        {
            if (CheckTokenSequence(out validatedTokens, "let", "?identifier"))
            {                
                VMWriter.Segment seg = emitter.GetVarSegment(symbolTable.KindOf(tokenizer.TokenValue));
                int segIndex = symbolTable.IndexOf(tokenizer.TokenValue);
                if(tokenizer.PeakAhead().Value == "[")
                {
                    // CompileArrayIndex will set the THAT pointer to the location of the array item, so the destination becomes (that, 0)
                    CompileArrayIndex();
                    seg = VMWriter.Segment.THAT;
                    segIndex = 0;
                }
                
                CheckTokenSequence(out validatedTokens, "=");                
                CompileExpression();
                tokenizer.Advance();
                emitter.emitLetStatement(seg, segIndex);
            }
        }

        public void CompileWhile()
        {
            int labelNum = whileCounter++;

            if (CheckTokenSequence(out validatedTokens, "while", "("))
            {
                emitter.emitWhileStart(labelNum);
                CompileExpression();
                if(CheckTokenSequence(out validatedTokens, ")", "{"))
                {
                    emitter.emitWhileStatementBlock(labelNum);
                    CompileStatements();
                    CheckTokenSequence(out validatedTokens, "}");
                    emitter.emitWhileEnd(labelNum);
                }
            }
        }

        public void CompileReturn()
        {
            if (CheckTokenSequence(out validatedTokens, "return"))
            {
                if(tokenizer.PeakAhead().Value != ";")
                {
                    CompileExpression();
                }
                else  // if there is no return value, must push constant 0 to the stack
                {
                    emitter.emitTerm(VMWriter.Segment.CONST, 0);
                }
                CheckTokenSequence(out validatedTokens, ";");
                emitter.emitReturn();
            }
        }

        public void CompileIf()
        {
            bool hasElseStatement = false;
            int labelNum = ifCounter++;
            if (CheckTokenSequence(out validatedTokens, "if", "("))
            {
                CompileExpression();
                emitter.emitIfStart(labelNum);
                CheckTokenSequence(out validatedTokens, ")", "{");
                CompileStatements();
                CheckTokenSequence(out validatedTokens, "}");

                if (tokenizer.PeakAhead().Value == "else")
                {
                    hasElseStatement = true;
                    CheckTokenSequence(out validatedTokens, "else", "{");
                    emitter.emitIfElseStatementBlock(labelNum);
                    CompileStatements();
                    CheckTokenSequence(out validatedTokens, "}");
                }

                emitter.emitIfEnd(labelNum, hasElseStatement);
            }
        }

        /// <summary>
        /// Compiles a complete expression, including terms and operators between them. Does NOT process enclosing '(' and ')'.
        /// </summary>
        public void CompileExpression()
        {
            string operatorToCall = "";
            tokenizer.Advance();
            CompileTerm();
            while(tokenizer.PeakAhead().IsBinOperator())
            {
                tokenizer.Advance();
                operatorToCall = tokenizer.TokenValue;
                // to distinguish between unary and binary "-" operator, spaces are added around the binary
                if (operatorToCall == "-")
                {
                    operatorToCall = " - ";
                }
                tokenizer.Advance();
                CompileTerm();
                emitter.emitOperator(operatorToCall);

            }            
        }

        /// <summary>
        /// Compiles an individual term of an expression.  The current token should already be the first token of the term (which may be a "(" denoting an enclosed expression) before calling.
        /// </summary>
        public void CompileTerm()
        {
            bool isUnaryOperator = (tokenizer.CurrentToken.IsBinOperator() && tokenizer.PeakAhead().IsUnaryOperator());

            switch (tokenizer.TokenType)
            {
                case JackToken.TokenType.integerConstant:
                    emitter.emitTerm(VMWriter.Segment.CONST, int.Parse(tokenizer.TokenValue));
                    break;
                case JackToken.TokenType.stringConstant:
                    emitter.emitStringConstant(tokenizer.TokenValue);
                    break;
                case JackToken.TokenType.identifier:
                    VMWriter.Segment seg = emitter.GetVarSegment(symbolTable.KindOf(tokenizer.TokenValue));
                    int segIndex = symbolTable.IndexOf(tokenizer.TokenValue);
                    if(segIndex != -1 && tokenizer.PeakAhead().Value != "[")
                    {
                        emitter.emitTerm(seg, segIndex);
                    }                    
                    break;
                case JackToken.TokenType.keyword:
                    switch(tokenizer.TokenValue)
                    {
                        case "true":
                        case "false":
                        case "null":
                            emitter.emitBooleanConstant(tokenizer.TokenValue);
                            break;
                        case "this":
                            emitter.emitTerm(VMWriter.Segment.POINTER, 0);
                            break;
                    }
                    break;
                case JackToken.TokenType.symbol:
                    // TODO: does this actually need to be here?  Seems to be only parentheses and unaries that actually make it this far.
                    break;
            }

            if (tokenizer.CurrentToken.Value == "(")
            {
                CompileExpression();
                CheckTokenSequence(out validatedTokens, ")");
            }

            else if(tokenizer.CurrentToken.IsUnaryOperator())
            {
                string unaryOp = tokenizer.TokenValue;
                tokenizer.Advance();
                CompileTerm();
                emitter.emitOperator(unaryOp);
            }
            else
            {
                // one token look ahead is needed to determine if the term is a single identifier, an array with index, or an object with a sub call
                switch (tokenizer.PeakAhead().Value)
                {
                    case "[":
                        CompileArrayIndex();
                        emitter.emitTerm(VMWriter.Segment.THAT, 0);
                        break;
                    case ".":
                        CompileSubroutineCall();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Compiles an array indexing term.  Current token should be the array name.  Processes the '[' and ']' tokens and the expression between.
        /// </summary>
        private void CompileArrayIndex()
        {
            var arrayPointerSeg = emitter.GetVarSegment(symbolTable.KindOf(tokenizer.TokenValue));
            int arrayPointerIndex = symbolTable.IndexOf(tokenizer.TokenValue);
            
            CheckTokenSequence(out validatedTokens, "[");
            CompileExpression();
            CheckTokenSequence(out validatedTokens, "]");
            bool isEqualNext = tokenizer.PeakAhead().Value == "=";
            emitter.emitArrayIndex(arrayPointerSeg, arrayPointerIndex, isEqualNext);
        }

        /// <summary>
        /// Compiles a call to a subroutine.  The current token should be the subroutine name if a one token name; otherwise, it should be the class or object name that preceeds the '.' operator.
        /// </summary>
        private void CompileSubroutineCall()
        {
            string subObject = tokenizer.TokenValue;
            string subroutineName;
            int numArgs = 0;
            int thisVarIndex = -1;
            
           

            if (tokenizer.PeakAhead().Value == ".")
            {
                CheckTokenSequence(out validatedTokens, ".", "?identifier");
                // if the object has a symbol table entry, it's an instance and requires its index to pass the "this" variable.  If not listed, it's a class and the index will remain -1.
                thisVarIndex = symbolTable.IndexOf(subObject);
                subroutineName = (thisVarIndex == -1) ? subObject + "." + tokenizer.TokenValue : symbolTable.TypeOf(subObject) + "." + tokenizer.TokenValue;

            }                               
            else   // subroutines called without a prefix in Jack are always methods of the current class, so we need to add in the class name and THIS pointer.
            {
                subroutineName = emitter.GetCurrentClassName() + "." + subObject;
                thisVarIndex = 0;
            }
            
            if (tokenizer.PeakAhead().Value == "(")
            {
                CheckTokenSequence(out validatedTokens, "(");
                
                // if this is a method being called from an instance, we need to push the "this" argument and take it into account for the number of arguments 
                if (thisVarIndex != -1)
                {
                    numArgs = 1;
                    VariableCategory objType = symbolTable.KindOf(subObject);
                    if (objType != VariableCategory.NONE)
                    {
                        emitter.emitTerm(emitter.GetVarSegment(objType), thisVarIndex);
                    }
                    else
                    {
                        emitter.emitTerm(VMWriter.Segment.POINTER, 0);
                    }
                }

                numArgs += CompileExpressionList();
                CheckTokenSequence(out validatedTokens, ")");
                emitter.emitSubroutineCall(subroutineName, numArgs);
            }               
        }

        /// <summary>
        /// Compiles a comma-separated list of expression (which may be empty).  Does NOT process enclosing '(' and ')'.  Returns number of expressions in the list.
        /// </summary>
        public int CompileExpressionList()
        {
            int numExpr = 0;
                        
            if(tokenizer.PeakAhead().Value != ")" )
            {
                numExpr++;
                CompileExpression();
                while (tokenizer.PeakAhead().Value == ",")
                {
                    tokenizer.Advance();
                    numExpr++;
                    CompileExpression();
                }
            }
                        
            return numExpr;
        }

        private void WriteError(string expected)
        {
            Console.Error.WriteLine("ERROR - line " + tokenizer.TokenLineNumber + "; expected '" + expected + "', found " + tokenizer.TokenValue);
        }
    }
}
