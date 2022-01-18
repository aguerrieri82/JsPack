using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core.Services
{
    public class MultiModuleResolver : IModuleResolver
    {
        public MultiModuleResolver()
        {
            Resolvers = new IModuleResolver[]
            {
                new LocalModuleResolver() { Priority = 0 },
                new NodeModuleResolver() { Priority = 1 },
                new TypeScriptModuleResolver() { Priority = 2 }
            };
        }
        public JsModule Resolve(ModuleResolveContext context, string module)
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

        public IList<IModuleResolver> Resolvers { get; set; }
    }
}
