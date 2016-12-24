using System.Windows;
using EveMarketProphet.ViewModels;

namespace EveMarketProphet.Views
{
    /// <summary>
    /// Interaction logic for ManageApiView.xaml
    /// </summary>
    public partial class ManageApiView : Window
    { 
        public ManageApiView()
        {
            InitializeComponent();
            DataContext = new ManageApiViewModel();
        }
    }
}
