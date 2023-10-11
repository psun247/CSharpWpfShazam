namespace CSharpWpfShazam.Helpers
{
    public static class GeneralExtensions
    {
        /// <summary>
        /// Detect if a string is null / empty.  
        /// Can even handle a null string.  e.g.
        /// string x = null;
        /// if (x.IsBlank())...
        /// </summary>        
        public static bool IsBlank(this string str) => string.IsNullOrWhiteSpace(str);

        /// <summary>
        /// Detect if a string is not null / empty.
        /// </summary>        
        public static bool IsNotBlank(this string str) => !IsBlank(str);                
    }

}
