// Copyright(c) 2023-2024 Peter Sun
using System;
using System.Windows;

namespace CSharpWpfShazam
{
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            DataContext = _mainViewModel = mainViewModel;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ReloadAndRebindAll(isAppStartup: true);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!_mainViewModel.Shutdown())
                {
                    // Busy, try to close later
                    e.Cancel = true;                    
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
