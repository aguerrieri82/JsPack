using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
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
            
            var moduleName = module.Split('/')[0];

            if (config.Value.CompilerOptions?.Paths == null)
                return null;

            if (!config.Value.CompilerOptions.Paths.TryGetValue(moduleName, out var paths))
                return null;

            string commonPath = null;
            string outRoot = null;
            //string contextOutRoot = null;

            if (config.Value.CompilerOptions?.OutDir != null)
            {
                var pathList = config.Value.CompilerOptions.Paths.SelectMany(a => a.Value).Union(new[] { config.Path });
                commonPath = Utils.FindCommonPath(pathList);
                outRoot = Path.Combine(config.Path, Path.Combine(config.Value.CompilerOptions.OutDir));
                //contextOutRoot = Path.GetFullPath(Path.Combine(outRoot, Path.GetRelativePath(commonPath, config.Path)));
            }

            foreach (var path in paths)
            {
                if (!(path.EndsWith("/") || path.EndsWith("\\")))
                    continue;

                string entry;

                if (commonPath != null)
                {
                    var moduleRoot = Path.Combine(config.Path, path);
                    moduleRoot = Path.GetRelativePath(commonPath, moduleRoot);
                    moduleRoot = Path.GetFullPath(Path.Combine(outRoot, moduleRoot));
                    entry = Path.Combine(moduleRoot, "index.js");
                }
                else
                    entry = Path.Combine(Path.Combine(config.Path, path), "index.js");

                if (!File.Exists(entry))
                {
                    var refConfig = FindConfig(Path.Combine(config.Path, path));

                    if (!module.StartsWith("."))
                    {
                        entry = Path.Combine(config.Path, path) + module.Substring(moduleName.Length);
                        if (!File.Exists(entry))
                            return null;
                    }
                    else if (refConfig?.Value.CompilerOptions?.OutDir != null)
                    {
                        entry = Path.Combine(Path.Combine(refConfig.Path, refConfig.Value.CompilerOptions.OutDir), "index.js");
                        if (!File.Exists(entry))
                            return null;
                    }
                }
                entry = Path.GetFullPath(entry);

                return new JsModule()
                {
                    Name = module,
                    Path = entry,
                };
            }
            return null;
        }
    }
}
