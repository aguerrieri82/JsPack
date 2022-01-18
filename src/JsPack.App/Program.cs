// See https://aka.ms/new-console-template for more information
using JsPack.Core;

Console.WriteLine("Hello, World!");

var parser = new JsModuleParser();

var path = @"D:\Development\Personal\Git\DataLab\src\DataLab.Web\obj\js\DataLab\src\DataLab.Web\Scripts\";

var modules = parser.Parse(path + "index.js");

var deps = parser.FindDependances(modules).ToArray();

var jsWriter = new JsModuleWriter();

foreach (var dep in deps)
{
    string moduleName;
    if (dep.Module.Name != null)
        moduleName = dep.Module.Name;
    else
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

Console.ReadKey();