using System.Windows;
using EveMarketProphet.ViewModels;

namespace EveMarketProphet.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {

        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void RegionSelect_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*var textbox = (TextBox)RegionSelect.Template.FindName("PART_EditableTextBox", RegionSelect);
            if (textbox != null)
            {
                var parent = (Border)textbox.Parent;
                parent.Background = Brushes.Black;
                textbox.Background = Brushes.Black;
            }*/
        }

    }
}
