using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class TsConfig
    {
        public class TsCompilerOptions
        {
            public Dictionary<string, string[]> Paths { get; set; }

            public string OutDir { get; set; }

            public string ModuleResolution { get; set; }
        }


        public TsCompilerOptions CompilerOptions { get; set; }

        public string[] Include { get; set; }

        public string[] Exclude { get; set; }

        public DateTime ParseTime { get; set; }
    }

    public class TypeScriptModuleResolver : BaseConfigResolver<TsConfig>
    {

        public TypeScriptModuleResolver()
            : base("tsconfig.json")
        {
        }

        public override JsModule Resolve(JsModuleResolveContext context, string module)
        {
            var config = FindConfig(context.Root);
            if (config == null)
                return null;
            if (config.Value.CompilerOptions?.Paths != null && config.Value.CompilerOptions.Paths.TryGetValue(module, out var paths))
            {
                foreach (var path in paths)
                {
                    if (path.EndsWith("/") || path.EndsWith("\\"))
                    {
                        var entry = Path.Combine(Path.Combine(config.Path, path), "index.js");
                        if (!File.Exists(entry))
                        {
                            var refConfig = FindConfig(Path.Combine(config.Path, path));
                           
                            if (refConfig?.Value.CompilerOptions?.OutDir != null)
                            {
                                entry = Path.Combine(Path.Combine(refConfig.Path, refConfig.Value.CompilerOptions.OutDir), "index.js");
                                if (!File.Exists(entry))
                                    return null;
                            }
                        }
                        return new JsModule()
                        {
                            Name = module,
                            Path = entry,
                        };
                    }
                }
            }
            return null;
        }
    }
}
