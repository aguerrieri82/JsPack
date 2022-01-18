namespace JsPack.Core
{
    public enum JsReferenceStatus
    {
        NotResolved, 
        Success,
        Error
    }
    
    [Flags]
    public enum JsReferenceFlags
    {
        None =0,
        Internal = 0x1
    }

    public class JsReference
    {
         public string FromModuleName { get; set; }

         public JsParsedModule FromModule { get; set; }

         public string Name { get; set; }

         public string Alias { get; set; }

         public JsReferenceStatus ResolveStatus { get; set; }
        
         public JsReferenceFlags Flags { get; set; }

    }
}
