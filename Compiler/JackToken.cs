namespace Nand2Tetris_Compiler
{
    class JackToken
    {
        public enum TokenType { keyword, symbol, identifier, integerConstant, stringConstant, INVALID }

        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public int LineNumber { get; private set; }


        public JackToken(TokenType type, string value)
        {
            Type = type;
            Value = value;
            LineNumber = 0;
        }

        public JackToken(TokenType type, string value, int linenum)
        {
            Type = type;
            Value = value;
            LineNumber = linenum;
        }

        // Utility functions for determining if the token matches a specific subset
        public bool IsBinOperator()
        {
            switch(Value)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "&":
                case "|":
                case "<":
                case ">":
                case "=":
                    return true;
                default:
                    return false;
            }
        }

        public bool IsType()
        {
            if(Type == TokenType.keyword)
            {
                switch(Value)
                {
                    case "int":
                    case "char":
                    case "boolean":
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                // TODO: eventually this will need to check the symbol table for a valid type
                return (Type == TokenType.identifier);
            }
        }

        public bool IsKeywordConst()
        {
            switch (Value)
            {
                case "true":
                case "false":
                case "null":
                case "this":
                    return true;
                default:
                    return false;
            }
        }

        public bool IsUnaryOperator()
        {
            if (Value == "-" || Value == "~")
                return true;
            else
                return false;
        }

        public bool IsIntegerConst()
        {
            return (Type == TokenType.integerConstant);
        }

        public bool IsStringConst()
        {
            return (Type == TokenType.stringConstant);
        }

           
    }
}
