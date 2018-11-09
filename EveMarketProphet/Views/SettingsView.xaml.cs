using System.Collections.Generic;
using System.Windows;
using EveMarketProphet.Properties;
using EveMarketProphet.Services;

namespace EveMarketProphet.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();

            AccountingComboBox.ItemsSource = new List<int>() {0, 1, 2, 3, 4, 5};
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void ManageApiButton_OnClick(object sender, RoutedEventArgs e)
        {
            var view = new ManageApiView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            view.ShowDialog();
        }

        private void AuthButton_OnClick(object sender, RoutedEventArgs e)
        {
            Listener.Instance.Async();
        }

        private void StopAuthButton_OnClick(object sender, RoutedEventArgs e)
        {
            Listener.Instance.Stop();
        }
    }
}
