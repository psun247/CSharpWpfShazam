// Copyright(c) 2023-2024 Peter Sun
using System;
using System.Windows;
using System.Windows.Controls;

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
            TabControlName.SelectionChanged += TabControlName_SelectionChanged;
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

        private void TabControlName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Note: the flow is Shazam tab is always handled first
            bool? tabActivated = IsTabActivated<ShazamUserControl>(e);
            if (tabActivated.HasValue)
            {
                _mainViewModel.OnShazamTabActivated(tabActivated.Value);
            }
            tabActivated = IsTabActivated<MySQLUserControl>(e);
            if (tabActivated.HasValue)
            {
                _mainViewModel.OnMySQLTabActivated(tabActivated.Value);
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
    }
}
