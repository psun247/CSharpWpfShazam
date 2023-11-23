using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CSharpWpfShazam.ViewModelsViews
{    
    public partial class AboutUserControl : UserControl
    {
        private bool _isAlreadyLoaded;

        public AboutUserControl()
        {
            InitializeComponent();

            Loaded += AboutUserControl_Loaded;
        }

        private void AboutUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isAlreadyLoaded)
            {
                return;
            }
            _isAlreadyLoaded = true;

            var aboutTabContent = (DataContext as MainViewModel)?.AboutTabContent;
            if (aboutTabContent != null)
            {
                var flowDocument = new FlowDocument();
                Paragraph docParagraph;
                if (aboutTabContent.HasError)
                {
                    docParagraph = new Paragraph { FontSize = 18 };
                    docParagraph.Inlines.Add(new Run(aboutTabContent.Error));
                    flowDocument.Blocks.Add(docParagraph);
                }
                else
                {
                    foreach (var paragraph in aboutTabContent.ParagraphList)
                    {
                        docParagraph = new Paragraph();
                        docParagraph.Inlines.Add(new Bold(new Run(paragraph.Header) { FontSize = 20 }));
                        docParagraph.Inlines.Add(new Run(paragraph.Detail) { FontSize = 18 });
                        flowDocument.Blocks.Add(docParagraph);
                    }
                }
                AboutRichTextBox.Document = flowDocument;
            }
        }
    }
}
