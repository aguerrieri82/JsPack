namespace JsPack.Core
{
    public enum TokenType
    {
        Whitespace,
        String,
        Identifier,
        Number,
        Comment,
        Puntuacator,
        Keyword
    }

    public class Token
    {
        public string? Text { get; set; }

        public int Position { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public TokenType Type { get; set; }

        public override string ToString()
        {
            return Type.ToString()[0] + ": " + Text;
        }
    }
}
