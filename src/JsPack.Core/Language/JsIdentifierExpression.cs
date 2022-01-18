namespace JsPack
{
    public class JsIdentifierExpression : JsElement
    {
        public IList<string> Parts { get; set; }

        public override JsElementType Type => JsElementType.Identifier;
    }
}
