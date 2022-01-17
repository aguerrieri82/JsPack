using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException(Token token, string message = "")
            : base("Syntax error: " + message)
        {
            Token = token;
        }

        public Token Token { get;}
    }
}
