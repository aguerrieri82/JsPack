using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
{
    public class MultiModuleResolver : IJsModuleResolver
    {
        public MultiModuleResolver()
        {
            Resolvers = new IJsModuleResolver[]
            {
                new LocalModuleResolver() { Priority = 0 },
                new NodeModuleResolver() { Priority = 1 },
                new TypeScriptModuleResolver() { Priority = 2 }
            };
        }
        public JsModule Resolve(JsModuleResolveContext context, string module)
        {
            if (Resolvers == null)
                return null;

            foreach (var resolver in Resolvers.OrderBy(a=> a.Priority))
            {
                var result = resolver.Resolve(context, module);
                if (result != null)
                    return result;
            }

            return null;
        }

        public int Priority { get; set; }

        public IList<IJsModuleResolver> Resolvers { get; set; }
    }
}
