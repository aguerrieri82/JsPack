
using JsPack;

if (args[0] == "expand")
{
    if (args.Length >= 3)
    {
        var root = args[1];
        var target = args[2];
        JsPackTask.ExpandExports(root, target);
    }
}