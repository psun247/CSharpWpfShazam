namespace CSharpWpfShazam.Models
{
    public class DeviceInfo
    {
        // Device is microphone, speaker, sound card input, etc.
        public string DeviceID { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        // Microphone (Sound Blaster Omni Surround 5.1), Speakers (Realtek High Definition Audio), etc.
        public string FriendlyName { get; set; } = string.Empty;
    }
}
