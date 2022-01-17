namespace JsPack.Core
{
    public class IdentifierElement : JsElement
    {
        public IList<string> Parts { get; set; }

        public override JsElementType Type => JsElementType.Identifier;
    }
}
