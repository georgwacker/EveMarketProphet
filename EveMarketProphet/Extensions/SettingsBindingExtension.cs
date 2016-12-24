using System.Windows.Data;

namespace EveMarketProphet.Extensions
{
    public class SettingsBindingExtension : Binding
    {
        public SettingsBindingExtension()
        {
            Initialize();
        }

        public SettingsBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            Source = Properties.Settings.Default;
            Mode = BindingMode.TwoWay;
        }
    }

    public class AuthenticationBindingExtension : Binding
    {
        public AuthenticationBindingExtension()
        {
            Initialize();
        }

        public AuthenticationBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            Source = Properties.Authentication.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
