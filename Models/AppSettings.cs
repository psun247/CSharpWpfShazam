namespace CSharpWpfShazam.Models
{
    public class AppSettings
    {
        // Shown in device combo box
        public string SelectedDeviceName { get; set; } = string.Empty;
        // Used for Shazam API
        public string SelectedDeviceID { get; set; } = string.Empty;
        // true if MySQL properly installed and configured
        public bool IsMySQLEnabled { get; set; }
        // Song info section on the right
        public bool IsSongInfoSectionVisible { get; set; } = true;
    }
}
