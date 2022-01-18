namespace JsPack.Core
{
    public enum JsTokenType
    {
        Whitespace,
        String,
        Identifier,
        Number,
        Comment,
        Puntuacator,
        Keyword
    }

    public class JsToken
    {
        public string? Text { get; set; }

        public int Position { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public JsTokenType Type { get; set; }

        public override string ToString()
        {
            return Type.ToString()[0] + ": " + Text;
        }
    }
}
