namespace JsPack.Core
{
    public enum JsParsedModuleFlags
    {
        Parsed = 0x0,
        Exports = 0x1,
        Imports = 0x2,
        Resolving = 04
    }

    public class JsParsedModule : JsModule
    {
        public IList<JsElement> Elements { get; set; }

        public IDictionary<string, JsReference> Imports { get; set; }

        public IDictionary<string, JsReference> Exports { get; set; }

        public JsParsedModuleFlags Flags { get; set; }

        public DateTime ParseTime { get; set; }
    }
}
