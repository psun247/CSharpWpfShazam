// Copyright(c) 2023-2024 Peter Sun
using System;
using System.Windows;
using System.Windows.Controls;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.ViewModelsViews
{
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;
        private bool _isFirstTabLoading = true;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            TabControlName.SelectionChanged += TabControlName_SelectionChanged;
            DataContext = _mainViewModel = mainViewModel;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _mainViewModel.InitializeAsync();

            switch (_mainViewModel.AppSettings.SelectedTabName)
            {
                case AppSettings.ShazamTabName:
                    // This is first-time (Loaded) and Shazam tab (the first tab) is already selected, so 'ShazamTabItem.IsSelected = true;'
                    // won't fire TabControlName_SelectionChanged, hence directly calling OnShazamTabActivated.         
                    _mainViewModel.OnShazamTabActivated(true);
                    break;
                case AppSettings.AzureTabName:
                    AzureTabItem.IsSelected = true;
                    break;
                case AppSettings.MySQLTabName:
                    MySQLTabItem.IsSelected = true;
                    break;
                case AppSettings.AboutTabName:
                    AboutTabItem.IsSelected = true;
                    break;
            }
        }

        private void TabControlName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isFirstTabLoading)
            {
                // Ignore first tab loading to let MainWindow_Loaded select the right tab
                _isFirstTabLoading = false;
                return;
            }

            // Note: the flow is Shazam tab is always handled first
            bool? tabActivated = IsTabActivated<ShazamUserControl>(e);
            if (tabActivated.HasValue)
            {
                _mainViewModel.OnShazamTabActivated(tabActivated.Value);
            }
            tabActivated = IsTabActivated<AzureUserControl>(e);
            if (tabActivated.HasValue)
            {
                _mainViewModel.OnAzureTabActivated(tabActivated.Value);
            }
            tabActivated = IsTabActivated<MySQLUserControl>(e);
            if (tabActivated.HasValue)
            {
                _mainViewModel.OnMySQLTabActivated(tabActivated.Value);
            }
            tabActivated = IsTabActivated<AboutUserControl>(e);
            if (tabActivated.HasValue && tabActivated.Value)
            {
                // Note: only thing to do is to set tab name, so don't implement OnAboutTabActivated()
                if (tabActivated.Value)
                {
                    _mainViewModel.AppSettings.SelectedTabName = AppSettings.AboutTabName;
                }
            }
        }

        // true: tab activated, false: tab deactivated, null: nothing
        private bool? IsTabActivated<T>(SelectionChangedEventArgs e)
        {
            var tabControl = e.OriginalSource as TabControl;
            if (tabControl != null)
            {
                if (e.AddedItems?.Count > 0 &&
                    (e.AddedItems[0] as TabItem)?.Content is T)
                {
                    return true;
                }

                if (e.RemovedItems?.Count > 0 &&
                    (e.RemovedItems[0] as TabItem)?.Content is T)
                {
                    return false;
                }
            }
            return null;
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
