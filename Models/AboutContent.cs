using System.Collections.Generic;

// Multiple classes in this file
namespace CSharpWpfShazam.Models
{
    // Content displayed on About tab
    public class AboutContent
    {
        public string Error { get; set; } = string.Empty;
        public bool HasError => !string.IsNullOrWhiteSpace(Error);
        public List<Paragraph> ParagraphList { get; set; } = new List<Paragraph>();
    }

    public class Paragraph
    {
        public string Header { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
