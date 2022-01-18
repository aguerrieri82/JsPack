using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{
    public class JsModuleWriter
    {
        public JsModuleWriter()
            : this(new StringWriter())
        {

        }
        public JsModuleWriter(TextWriter writer)
        {
            Writer = writer;
        }

        public JsModuleWriter Write(JsImportElement element)
        {
            Writer.Write("import ");
            if (element.AllAlias == null && element.DefaultAlias == null && (element.Items == null || element.Items.Count == 0))
            {
                WriteString(element.FromModule);
            }

            Write(";").WriteNewLine();

            return this;
        }


        public JsModuleWriter Write(JsExportElement element)
        {
            Writer.Write("export ");
            if (element.FromModule != null)
            {
                if (element.Items == null || element.Items.Count == 0)
                    Write("*");
                else
                {
                    Write("{ ");
                    for (var i = 0; i < element.Items.Count; i++)
                    {
                        var item = element.Items[i];
                        if (i > 0)
                            Write(",");
                        Write(item.Identifier);
                        if (item.Alias != null)
                            Write(" as ").Write(item.Alias);
                    }    
                    Write(" }");
                }
                Write(" from ").WriteString(element.FromModule).Write(";").WriteNewLine();
            }
            return this;
        }

        public JsModuleWriter WriteString(string text)
        {
            Writer.Write('"');
            foreach (var c in text)
            {
                if (c == '\n')
                    Writer.Write("\\n");
                else if (c == '\r')
                    Writer.Write("\\r");
                else if (c == '"')
                    Writer.Write("\\\"");
                else if (c == '\\')
                    Writer.Write("\\\\");
                else
                    Writer.Write(c);
            }
            Writer.Write('"');
            return this;
        }

        public JsModuleWriter Write(string text)
        {
            Writer.Write(text);
            return this;
        }

        public JsModuleWriter WriteString()
        {
            Writer.WriteLine();
            return this;
        }

        public JsModuleWriter WriteNewLine()
        {
            Writer.WriteLine();
            return this;
        }

        public TextWriter Writer { get; }
    }
}
