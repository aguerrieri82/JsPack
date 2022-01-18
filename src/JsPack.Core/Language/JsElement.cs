namespace JsPack.Core
{
    public enum JsElementType
    {
        Export,
        Import,
        Identifier
    }

    public abstract class JsElement
    {
        public abstract JsElementType Type { get; }
    }
}
