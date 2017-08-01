using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Nand2Tetris_Compiler
{
    class JackTokenizer
    {
        private StreamReader stream;
        private JackToken currentToken;
        Queue<JackToken> tokenQueue;
        private bool blockCommentActive;
        private int lineNumber;

        static readonly char[] controlSymbols = { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~', '"' };
        static readonly string[] keywords = { "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean", "void",
            "true", "false", "null", "this", "let", "do", "if", "else", "while", "return" };


        public JackTokenizer(StreamReader inputStream)
        {
            stream = inputStream;
            currentToken = null;
            tokenQueue = new Queue<JackToken>();
            blockCommentActive = false;
            lineNumber = 0;
        }

        public bool HasMoreTokens
        {
            get
            {
                return (tokenQueue.Count > 0);
            }
        }

        public void Advance()
        {
            if (tokenQueue.Count > 0)
            {
                currentToken = tokenQueue.Dequeue();
            }
        }

        public void ReadFileIntoTokenQueue()
        {
            while(!stream.EndOfStream)
            {
                SplitIntoTokens(GetNextLineClean());
            }
        }

        public JackToken.TokenType TokenType
        {
            get
            {
                return currentToken.Type;
            }
        }

        public string TokenValue
        {
            get
            {
                return currentToken.Value;
            }
        }

        public JackToken CurrentToken
        {
            get
            {
                return currentToken;
            }
        }

        public JackToken PeakAhead()
        {
            return tokenQueue.Peek();
        }

        public int TokenLineNumber
        {
            get
            {
                return currentToken.LineNumber;
            }
        }

        // these are mostly here to conform to the text API.  They don't seem very useful, honestly.        
        public string KeyWord
        {
            get
            {
                if (currentToken.Type == JackToken.TokenType.keyword)
                {
                    return currentToken.Value;
                }
                else
                {
                    return "ERROR - not keyword type token";
                }
            }
        }

        public char Symbol
        {
            get
            {
                if (currentToken.Type == JackToken.TokenType.symbol)
                {
                    return currentToken.Value[0];
                }
                else
                {
                    return ' ';
                }
            }
        }

        public string Identifier
        {
            get
            {
                if (currentToken.Type == JackToken.TokenType.identifier)
                {
                    return currentToken.Value;
                }
                else
                {
                    return "ERROR - not identifier type token";
                }
            }
        }

        public int IntVal
        {
            get
            {
                // this should already have been validated, so the possible exception is actually desireable here
                return int.Parse(currentToken.Value);
            }
        }

        public string StringVal
        {
            get
            {
                if (currentToken.Type == JackToken.TokenType.stringConstant)
                {
                    return currentToken.Value;
                }
                else
                {
                    return "ERROR - not string_const type token";
                }
            }
        }





        private string GetNextLineClean()
        {
            string nextLine = CleanLine(stream.ReadLine());
            lineNumber++;
            while (!stream.EndOfStream && String.IsNullOrWhiteSpace(nextLine))
            {
                nextLine = CleanLine(stream.ReadLine());
                lineNumber++;
            }

            return nextLine;
        }

        // remove whitespace, line comments, and lines inside blockcomments
        private string CleanLine(string textLine)
        {
            int length = textLine.Length;
            char[] chars = textLine.ToCharArray();
            char[] cleanText = new char[255];
            int newLength = 0;

            bool skipNextChar = false;
            bool addWhiteSpace = false;
            bool inStringConstant = false;

            for (int i = 0; i < length; i++)
            {
                // if the next character has already been processed via lookahead, skip to the next iteration
                if (skipNextChar)
                {
                    skipNextChar = false;
                    continue;
                }

                char c = chars[i];

                // if in a block comment, only comment end characters are of potential interest
                if(blockCommentActive)
                {
                    if (c == '*')
                    {
                        if (i < length - 1 && chars[i + 1] == '/')
                        {
                            blockCommentActive = false;
                            skipNextChar = true;
                        }
                    }
                    continue;
                }

                // all whitespace inside string constants must be preserved, so special logic applies
                if (c == '"')
                {
                    if (inStringConstant)
                    {
                        inStringConstant = false;
                        cleanText[newLength++] = c;
                        cleanText[newLength++] = ' ';
                    }
                    else
                    {
                        inStringConstant = true;
                        cleanText[newLength++] = ' ';
                        cleanText[newLength++] = c;
                    }
                    continue;
                }
                if (inStringConstant)
                {
                    cleanText[newLength++] = c;
                    continue;
                }                

                // check for line comments and beginning of block comments
                if (c == '/')
                {
                    if(i < length - 1)
                    {
                        char c2 = chars[i + 1];
                        if (c2 == '/')       // starts a line comment.  We're done with this line.
                        {
                            skipNextChar = true;
                            break;
                        }
                        else if(c2 == '*')
                        {
                            blockCommentActive = true;
                            skipNextChar = true;
                            continue;
                        }
                    }
                }

                // non-comment character
                // whitespace is reduced to single spaces, so mark it as present and go to the next character
                if (char.IsWhiteSpace(c))
                {
                    addWhiteSpace = true;
                    continue;
                }
                // add valid characters to the new string, with whitespace reduced
                else
                {
                    // separate control symbols by surrounding with whitespace
                    if (controlSymbols.Contains(c))
                    {
                        cleanText[newLength++] = ' ';
                        addWhiteSpace = true;
                    }
                    else if (addWhiteSpace)
                    {
                        cleanText[newLength++] = ' ';
                        addWhiteSpace = false;
                    }

                    cleanText[newLength++] = c;
                }
            }  // end of parsing for loop

            return new string(cleanText, 0, newLength);
        }


        private void SplitIntoTokens(string line)
        {
            string[] quoteTokens = line.Split('"');

            // if the line contains any string constants, the first and last splits will be non-string (or empty), with every other split in the middle being a string
            if (quoteTokens.Length > 1)
            {
                // process non-string sections normally, as if their own lines
               SplitIntoTokens(quoteTokens[0]);
                for (int i = 1; i < quoteTokens.Length-1; i+=2)
                {
                    tokenQueue.Enqueue(new JackToken(JackToken.TokenType.stringConstant, quoteTokens[i], lineNumber));
                    SplitIntoTokens(quoteTokens[i+1]);
                }
            }
            else
            {
                string[] tokens = line.Split(' ');
                JackToken.TokenType tokenType = JackToken.TokenType.identifier;
                string tokenValue = "";
                foreach (string s in tokens)
                {
                    //skip empty tokens that somehow get through (quotes that end a line, other splitting oddities, etc.)
                    if (s == "") continue;

                    // string constants must be appended together until the closing quote is reached
                    tokenValue = s;
                    char c = s[0];
                    if (keywords.Contains(s))
                    {
                        tokenType = JackToken.TokenType.keyword;
                    }
                    else if (controlSymbols.Contains(c))
                    {
                        tokenType = JackToken.TokenType.symbol;
                    }
                    else if (s.All(Char.IsDigit))
                    {
                        tokenType = JackToken.TokenType.integerConstant;
                    }
                    else if ((Char.IsLetterOrDigit(c) || c == '_') && s.All(c2 => Char.IsLetterOrDigit(c2) || c2 == '_'))
                    {
                        tokenType = JackToken.TokenType.identifier;
                    }
                    else
                    {
                        tokenValue = "ERROR - invalid token type: " + s;
                        tokenType = JackToken.TokenType.INVALID;
                    }
                    

                    tokenQueue.Enqueue(new JackToken(tokenType, tokenValue, lineNumber));
                    
                }
            }              
        }
    }
}
