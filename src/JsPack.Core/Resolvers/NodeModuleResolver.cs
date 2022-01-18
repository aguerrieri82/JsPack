using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class NodePackage
    {
        public string Name { get; set; }

        public string Main { get; set; }

        public string Module { get; set; }
    }


    public class NodeModuleResolver : BaseConfigResolver<NodePackage>
    {
        public NodeModuleResolver()
            : base("package.json")
        {
        }


        public override JsModule Resolve(ModuleResolveContext context, string module)
        {
            var projPackage = FindConfig(context.Root);

            var modulePath = Path.Combine(projPackage.Path + "\\node_modules\\" + module + "\\package.json");
            if (!File.Exists(modulePath))
                return null;

            var package = JsonConvert.DeserializeObject<NodePackage>(File.ReadAllText(modulePath));
            var entry = string.IsNullOrWhiteSpace(package.Module) ? (string.IsNullOrWhiteSpace(package.Main) ? "index.js" : package.Main) : package.Module;
            var entryPath = Path.Combine(Path.GetDirectoryName(modulePath), entry);

            if (!File.Exists(entryPath))
                return null;

            return new JsModule
            {
                Path = entryPath,
                Name = module
            };
        }

        
    }
}
