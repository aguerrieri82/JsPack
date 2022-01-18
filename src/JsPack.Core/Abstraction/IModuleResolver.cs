using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class ModuleResolveContext
    {
        public string Root { get; set; }
    } 


    public interface IModuleResolver
    {
        JsModule Resolve(ModuleResolveContext context, string module);

        int Priority { get; set; }
    }
}
