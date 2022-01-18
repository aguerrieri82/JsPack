using JsPack;

public static class Test
{
    public static void Test1()
    {
        var parser = new JsModuleParser();

        var path = @"D:\Development\Personal\Git\DataLab\src\DataLab.Web\obj\js\DataLab\src\DataLab.Web\Scripts\";

        var modules = parser.Parse(path + "index.js");

        var deps = parser.FindDependances(modules, ExpandReferenceMode.ExternalModule).ToArray();

        var jsWriter = new JsModuleWriter();

        foreach (var dep in deps)
        {
            string moduleName;

            bool isPath = false;

            if (dep.Module.Name != null)
            {
                moduleName = dep.Module.Name;
                if (moduleName.StartsWith("."))
                    isPath = true;
            }
            else
            {
                isPath = true;
                moduleName = dep.Module.Path;
            }

            if (isPath)
            {
                moduleName = Path.GetRelativePath(path, dep.Module.Path);
                if (!moduleName.StartsWith("."))
                    moduleName = "./" + moduleName;

                var extIndex = moduleName.LastIndexOf('.');
                if (extIndex != -1)
                    moduleName = moduleName.Substring(0, extIndex);
                moduleName = moduleName.Replace('\\', '/');
            }

            if (dep.Exports.Count > 0)
            {
                var export = new JsExportElement()
                {
                    FromModule = moduleName,
                    Items = dep.Exports.Select(e => new JsIdentifierAlias()
                    {
                        Identifier = e.Key
                    }).ToArray()
                };
                jsWriter.Write(export);
            }
            else
            {
                var import = new JsImportElement()
                {
                    FromModule = moduleName,
                };
                jsWriter.Write(import);
            }
        }

        var text = jsWriter.Writer.ToString();

    }
}