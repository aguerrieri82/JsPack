// See https://aka.ms/new-console-template for more information
using JsPack.Core;

Console.WriteLine("Hello, World!");

var parser = new JsModuleParser();

var result = parser.Parse(@"D:\Development\Personal\Git\DataLab\src\DataLab.Web\obj\js\index.js", true);


Console.ReadKey();