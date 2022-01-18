using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class JsModuleResolveContext
    {
        public string Root { get; set; }
    } 


    public interface IJsModuleResolver
    {
        JsModule Resolve(JsModuleResolveContext context, string module);

        int Priority { get; set; }
    }
}
