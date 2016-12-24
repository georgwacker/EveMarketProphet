using System.Diagnostics;
using System.Windows;
using ZXing;
using ZXing.Common;
using ZXing.Presentation;

namespace EveMarketProphet.Views
{
    /// <summary>
    /// Interaction logic for DonationView.xaml
    /// </summary>
    public partial class DonationView : Window
    {
        public DonationView()
        {
            InitializeComponent();

            var writer = new BarcodeWriterGeometry
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions { Height = (int)bitcoinQRCode.Height, Width = (int)bitcoinQRCode.Width, Margin = 0 }
            };

            bitcoinQRCode.Data = writer.Write("1DppXkNfPKbs1JiF2vZ3m89QQKnToPAuMZ");
        }

        private void OnPayPalButton(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SE7KHFVJ2UHQ4");
        }
    }
}
