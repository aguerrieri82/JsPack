namespace JsPack
{
    public class JsImportElement : JsElement
    {
        public string FromModule { get; set; }

        public string AllAlias { get; set; }

        public string DefaultAlias { get; set; }

        public IList<JsIdentifierAlias> Items { get; set; }

        public override JsElementType Type => JsElementType.Import;
    }
}
