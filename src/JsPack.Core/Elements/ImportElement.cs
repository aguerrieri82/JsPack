namespace JsPack.Core
{
    public class ImportElement : JsElement
    {
        public string FromModule { get; set; }

        public string AllAlias { get; set; }

        public string DefaultAlias { get; set; }

        public IList<IdentifierAlias> Items { get; set; }

        public override JsElementType Type => JsElementType.Import;
    }
}
