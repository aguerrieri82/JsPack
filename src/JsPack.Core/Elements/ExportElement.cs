﻿namespace JsPack.Core
{
    public enum ExportElementType
    {
        Class,
        Const,
        Function,
        Var,
        Let
    }
    public class ExportElement : JsElement
    {
        public bool IsDefault { get; set; }

        public string FromModule { get; set; }

        public string ExportName { get; set; }

        public string ExportType { get; set; }

        public IList<IdentifierAlias> Items { get; set; }

        public override JsElementType Type => JsElementType.Export;

    }
}
