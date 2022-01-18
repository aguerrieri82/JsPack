using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
{
    public class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException(JsToken token, string message = "")
            : base("Syntax error: " + message)
        {
            Token = token;
        }

        public JsToken Token { get;}
    }
}
