namespace CSharpWpfShazam.Models
{        
    public class DeviceSetting
    {                
        // From device API
        public string DeviceID { get; set; } = string.Empty;
        // From device API (already kind of friendly name)        
        public string DeviceName { get; set; } = string.Empty;
        
        public override string ToString() => DeviceName;        
    }
}
