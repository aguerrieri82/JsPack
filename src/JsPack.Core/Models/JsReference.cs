namespace JsPack.Core
{
    public enum JsReferenceStatus
    {
        NotResolved, 
        Success,
        Error
    }

    public class JsReference
    {
         public string FromModuleName { get; set; }

         public JsParsedModule FromModule { get; set; }

         public string Name { get; set; }

         public string Alias { get; set; }

         public JsReferenceStatus ResolveStatus { get; set; }

    }
}
