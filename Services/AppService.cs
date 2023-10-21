using System;
using CSharpWpfShazam.Models;
using CSharpWpfShazam.Helpers;

namespace CSharpWpfShazam.Services
{
    public class AppService
    {
        private string _appConfigFilePath = string.Empty;

        public AppService(string appConfigFilePath)
        {
            _appConfigFilePath = appConfigFilePath;
            LoadAppSettings();
        }

        public AppSettings AppSettings { get; private set; } = new AppSettings();

        public void UpdateDeviceinfo(string selectedDeviceName, string selectedDeviceID)
        {
            AppSettings.SelectedDeviceName = selectedDeviceName;
            AppSettings.SelectedDeviceID = selectedDeviceID;
            SaveAppSettings();
        }

        public void UpdateMySQLEnabled(bool isMySQLEnabled)
        {
            AppSettings.IsMySQLEnabled = isMySQLEnabled;
            SaveAppSettings();
        }

        public void UpdateSongInfoSectionVisibility(bool isVisible)
        {
            AppSettings.IsSongInfoSectionVisible = isVisible;
            SaveAppSettings();
        }

        public void SaveAppSettings()
        {
            try
            {
                JsonHelper.SaveAsJsonToFile(AppSettings, _appConfigFilePath);
            }
            catch (Exception)
            {
            }
        }

        private void LoadAppSettings()
        {
            AppSettings? appSettings = null;
            try
            {
                appSettings = JsonHelper.DeserializeFromFile<AppSettings?>(_appConfigFilePath);
            }
            catch (Exception)
            {
            }
            AppSettings = appSettings ?? new AppSettings();
        }
    }
}
