using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack
{
    public static class JsPackTask
    {
        static void ExportDependance(string basePath, JsModuleDependance dep, JsModuleWriter jsWriter)
        {
            Dictionary<string, JsExportElement> exports = new Dictionary<string, JsExportElement>();

            foreach (var export in dep.Exports.Values)
            {
                var curExport = export;

                while (curExport.FromModule != null &&
                               (curExport.Flags & JsReferenceFlags.Internal) == 0 &&
                               curExport.ResolveStatus == JsReferenceStatus.Success &&
                               curExport.Name != "*")
                {
                    curExport = curExport.FromModule.Exports[curExport.Name];
                }

                string moduleName = curExport.FromModule.Path;

                moduleName = Path.GetRelativePath(basePath, moduleName);
                if (!moduleName.StartsWith("."))
                    moduleName = "./" + moduleName;

                var extIndex = moduleName.LastIndexOf('.');
                if (extIndex != -1)
                    moduleName = moduleName.Substring(0, extIndex);

                moduleName = moduleName.Replace('\\', '/');

                if (!exports.TryGetValue(moduleName, out var newExport))
                {
                    newExport = new JsExportElement()
                    {
                        FromModule = moduleName,
                        Items = new List<JsIdentifierAlias>()
                    };
                    exports[moduleName] = newExport;
                }
                newExport.Items.Add(new JsIdentifierAlias { Identifier = curExport.Name });
            }

            foreach (var export in exports.Values)
                jsWriter.Write(export);
        }

        public static void ExpandExports(string mainPath, string expandPath)
        {
            var parser = new JsModuleParser();

            var mainModule = parser.Parse(mainPath);

            var expandModule = parser.Parse(expandPath);

            var deps = parser.FindDependances(mainModule, ExpandReferenceMode.ExternalModule).ToArray();

            var writer = new JsModuleWriter(Console.Out);

            foreach (var export in expandModule.Elements.OfType<JsExportElement>())
            {
                if (export.FromModule == null || export.ExportName != null || (export.Items != null && export.Items.Count > 0))
                    continue;
                var expDeps = deps.FirstOrDefault(a => a.Module.Name == export.FromModule);
                if (expDeps != null)
                    ExportDependance(Path.GetDirectoryName(mainPath), expDeps, writer);
            }
        }
    }
}
