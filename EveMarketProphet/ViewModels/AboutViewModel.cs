using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace EveMarketProphet.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        public string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();
        public FileVersionInfo AssemblyInfo => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

        public ICommand OpenProjectWebsite => new DelegateCommand<string>(OnOpenProjectWebsite);

        private void OnOpenProjectWebsite(string url)
        {
            Process.Start(url);
        }
    }
}
