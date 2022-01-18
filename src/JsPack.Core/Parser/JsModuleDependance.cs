using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class JsModuleDependance
    {
        public JsParsedModule Module { get; set; }

        public IDictionary<string, JsReference> Exports { get; set; }
    }
}
