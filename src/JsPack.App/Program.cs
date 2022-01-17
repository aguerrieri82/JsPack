// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var parser = new JsPack.Core.JsModuleParser();
var reader = new StreamReader(@"D:\Development\Personal\Git\JsPack\src\JsPack.App\test.js");

var tokens = parser.Tokenize(reader).ToList();
var result = parser.Parse(tokens).ToArray();

var files = parser.ParseAll(@"D:\Development\Personal\Git\Eusoft\WebApp\src\Eusoft.WebApp\obj\js").ToList();
 
Console.ReadKey();