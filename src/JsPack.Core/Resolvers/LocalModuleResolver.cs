using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
{
    internal class LocalModuleResolver : IJsModuleResolver
    {
        public JsModule Resolve(JsModuleResolveContext context, string module)
        {
            if (!module.StartsWith("."))
                return null;
            var entry = Path.Combine(context.Root, module);
            if (!entry.EndsWith(".js"))
                entry += ".js";
            if (!File.Exists(entry))
                return null;
            return new JsModule()
            {
                Path = Path.GetFullPath(entry)
            };
        }

        public int Priority { get; set; }
    }
}
