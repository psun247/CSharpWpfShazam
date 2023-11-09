// Copyright(c) 2023-2024 Peter Sun
using System;
using System.IO;
using System.Windows;
using RestoreWindowPlace;
using CSharpWpfShazam.ViewModelsViews;

namespace CSharpWpfShazam
{
    public partial class App : Application
    {
        private WindowPlace? _windowPlace;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                string appConfigFilePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\CSharpWpfShazamConfig.json";
                var mainViewModel = new MainViewModel(appConfigFilePath);                
                var mainWindow = new MainWindow(mainViewModel);
                Current.MainWindow = mainWindow;
                SetupRestoreWindowPlace(mainWindow);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"This app will exit on error ({ex.GetType()})");
                Current?.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                _windowPlace?.Save();
            }
            catch (Exception)
            {
            }
        }

        private void SetupRestoreWindowPlace(MainWindow mainWindow)
        {
            string windowPlaceConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSharpWpfShazamWindowPlace.config");
            _windowPlace = new WindowPlace(windowPlaceConfigFilePath);
            _windowPlace.Register(mainWindow);         
        }
    }
}
