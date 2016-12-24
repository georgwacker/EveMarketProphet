using System;
using System.Diagnostics;
using System.Web;
using System.Windows;
using System.Windows.Input;
using EveMarketProphet.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace EveMarketProphet.ViewModels
{
    class ManageApiViewModel : BindableBase
    {
		private string _pastedUrl;
		public string PastedUrl
        {
            get { return _pastedUrl; }
            set { SetProperty(ref _pastedUrl, value); }
        }

        public ICommand GetTokenCommand { get; private set; }
        public ICommand OpenLinkCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }

        public ManageApiViewModel()
        {
            GetTokenCommand = new DelegateCommand(OnGetToken);
            OpenLinkCommand = new DelegateCommand(OnOpenLink);
			SaveCommand = new DelegateCommand<Window>(OnSave);
			ResetCommand = new DelegateCommand(OnReset);
        }

        private void OnReset()
        {
            Properties.Authentication.Default.Reset();
        }

        private void OnSave(Window window)
        {
            Properties.Authentication.Default.Save();
            window?.Close();
        }

        private void OnOpenLink()
        {
            var link = Auth.Instance.CreateAuthLink(Properties.Authentication.Default.ClientId);
            if (string.IsNullOrEmpty(link)) return;

            Process.Start(link);
        }

        private void OnGetToken()
        {
			if(string.IsNullOrEmpty(PastedUrl)) return;

            var pastedUri = new Uri(PastedUrl);
            var accessToken = HttpUtility.ParseQueryString(pastedUri.Query).Get("code");
            Auth.Instance.TryAuthentication(accessToken);
        }
    }
}
