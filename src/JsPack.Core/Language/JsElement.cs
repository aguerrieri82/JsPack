namespace JsPack
{
    public enum JsElementType
    {
        Export,
        Import,
        Identifier
    }

    public abstract class JsElement
    {
        public JsElement()
        {
            Tokens = new List<JsToken>();
        }

        public abstract JsElementType Type { get; }

        public IList<JsToken> Tokens { get; }
    }
}
