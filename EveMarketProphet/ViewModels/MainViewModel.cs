using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using EveMarketProphet.Models;
using EveMarketProphet.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace EveMarketProphet.ViewModels
{
    public class RegionSelect
    {
        public string Label { get; set; }
        public List<int> Regions { get; set; }
    }

    public class MainViewModel : BindableBase
    {
        private string _statusBarText;

        public string StatusBarText
        {
            get { return _statusBarText; }
            set { SetProperty(ref _statusBarText, value); }
        }

        private ObservableCollection<Trip> trips;
        public ObservableCollection<Trip> Trips
        {
            get { return trips; }
            private set { SetProperty(ref trips, value); }
        }

        /*private ObservableCollection<RoundTrip> roundtrips;
        public ObservableCollection<RoundTrip> RoundTrips
        {
            get { return roundtrips; }
            private set { SetProperty(ref roundtrips, value); }
        }*/

        private ICollectionView tripView;
        public ICollectionView TripView
        {
            get { return tripView; }
            private set { SetProperty(ref tripView, value); }
        }

        private bool isAvailable;
        public bool IsAvailable
        {
            get { return isAvailable; }
            private set { SetProperty(ref isAvailable, value); }
        }

        private Visibility _showProgressBar = Visibility.Hidden;

        public Visibility ShowProgressBar
        {
            get { return _showProgressBar; }
            set { SetProperty(ref _showProgressBar, value); }
        }

        public List<RegionSelect> RegionLists { get; set; }
        public RegionSelect SelectedRegionList { get; set; }

        public ICommand FetchDataCommand { get; private set; }
        public ICommand FindRoutesCommand { get; private set; }
        public ICommand FilterResultsCommand { get; private set; }
        public ICommand OpenMarketWindowCommand { get; private set; }
        public ICommand SetWaypointsCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }
        public ICommand OpenDonationCommand { get; private set; }
        public ICommand OpenAboutCommand { get; private set; } 


        private int currentProgress;
        public int CurrentProgress
        {
            get { return currentProgress; }
            set { SetProperty(ref currentProgress, value); }
        }

        private readonly IProgress<int> progress;


        public MainViewModel()
        {

            FetchDataCommand = new DelegateCommand(this.OnFetchData, this.CanFetchData);
            FindRoutesCommand = new DelegateCommand(this.OnFindRoutes);
            FilterResultsCommand = new DelegateCommand<object>(this.OnFilterResults);
            OpenMarketWindowCommand = new DelegateCommand<object>(this.OnOpenMarketWindow);
            SetWaypointsCommand = new DelegateCommand<object>(this.OnSetWaypoints);
            ClearFiltersCommand = new DelegateCommand(this.OnClearFilters);
            OpenSettingsCommand = new DelegateCommand(this.OnOpenSettings);
            OpenDonationCommand = new DelegateCommand(this.OnOpenDonation);
            OpenAboutCommand = new DelegateCommand(this.OnOpenAbout);
            //(FetchDataCommand as DelegateCommand).RaiseCanExecuteChanged

            //TripView = (CollectionView)CollectionViewSource.GetDefaultView(Trips);

            progress = new Progress<int>(ProgressChanged);
            IsAvailable = true;

            SetupRegionCombobox();
            SetupFakeData();
        }

        [Conditional("DEBUG")]
        private void SetupFakeData()
        {
            var list = new List<Trip>()
            {
                new Trip() {Cost = 12234432, Jumps = 12, ProfitPerJump = 1343543, Profit = 3213765, Weight = 125,
                    SecurityWaypoints = new List<SolarSystem>() {new SolarSystem() {security = 1.0, solarSystemName = "Jita"} },
                    Waypoints = new List<int>() {1,1,1,1 },
                    Transactions = new List<Transaction>()
                    {
                        new Transaction(new MarketOrder() {TypeName = "My long item type name" }, new MarketOrder()),
                        new Transaction(new MarketOrder(), new MarketOrder()),
                        new Transaction(new MarketOrder(), new MarketOrder()),
                        new Transaction(new MarketOrder(), new MarketOrder()),
                        new Transaction(new MarketOrder(), new MarketOrder())

                    }
                }
            };

            Trips = new ObservableCollection<Trip>(list);
            TripView = CollectionViewSource.GetDefaultView(Trips);
            //TripView.Refresh();
        }

        private void SetupRegionCombobox()
        {
            const int forge = 10000002;     // The Forge, Jita
            const int domain = 10000043;    // Domain, Amarr
            const int heimatar = 10000030;  // Heimatar, Rens
            const int sinq = 10000032; // Sinq Laison, Dodixie
            const int metropolis = 10000042;//Metropolis, Hek
            const int essence = 10000064;   //Essence, Oursalaert
            const int tash = 10000020;      //Tash-Murkon, Tash-Murkon Prim
            const int khanid = 10000049;    //Khanid, Agil

            // 67 regions without wormhole space

            var regionAll = Db.Instance.Regions.Where(r => r.regionID < 11000000).Select(r => r.regionID).ToList();
            var regionHubMain = new List<int> { forge, domain, heimatar, sinq };
            var regionHubAll = new List<int> { forge, domain, heimatar, sinq, metropolis, essence, tash, khanid };

            RegionLists = new List<RegionSelect>
            {
                new RegionSelect {Label = "Hubs Main", Regions = regionHubMain},
                new RegionSelect {Label = "Hubs All", Regions = regionHubAll},
                new RegionSelect {Label = "All", Regions = regionAll}
            };

            SelectedRegionList = RegionLists[0];
        }

        private async void OnFetchData()
        {
            var r = SelectedRegionList.Regions;
            //EveMarketData.Instance.MarketOrders.Clear();

            IsAvailable = false;
            ShowProgressBar = Visibility.Visible;
            StatusBarText = "Fetching market data ...";

            var s = Stopwatch.StartNew();
            await Market.Instance.FetchOrders(r);

            s.Stop();
            var ts = s.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            Console.WriteLine("Fetched market data in " + elapsedTime);
            StatusBarText = "Fetched market data in " + elapsedTime;
            ShowProgressBar = Visibility.Hidden;

            IsAvailable = true;
        }

        private void ProgressChanged(int i)
        {
            CurrentProgress = i;
        }

        private bool CanFetchData()
        {
            return true;
        }

        private async void OnFindRoutes()
        {
            IsAvailable = false;
            ShowProgressBar = Visibility.Visible;
            StatusBarText = "Processing market data ...";

            var s = Stopwatch.StartNew();
            var list = await Task.Run(() => Prophet.FindTradeRoutes());
            s.Stop();
            var ts = s.Elapsed;
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
            Debug.WriteLine("Processed market data in " + elapsedTime);
            StatusBarText = "Processed market data in " + elapsedTime;
            
            //Trips = new ObservableCollection<Trip>(EveMarketData.Instance.Trips);

            if (list != null)
            {
                Trips = new ObservableCollection<Trip>(list);
                TripView = (CollectionView)CollectionViewSource.GetDefaultView(Trips);
                //TripView.Filter = new Predicate<object>(x => Trips.IndexOf((Trip)x) < Settings.Default.FilterShowAmount);
                TripView.Refresh();
            }

            ShowProgressBar = Visibility.Hidden;
            IsAvailable = true;
        }

        private int FilterSystemStartId { get; set; }
        private int FilterSystemEndId { get; set; }

        private bool FilterSystemPair(object sender)
        {
            var trip = sender as Trip;
            var tx = trip.Transactions.First();

            if (tx.StartSystemId == FilterSystemStartId && tx.EndSystemId == FilterSystemEndId) return true;

            if (tx.StartSystemId == FilterSystemEndId && tx.EndSystemId == FilterSystemStartId) return true;

            return false;

            //return (run.startSystemID == FilterSystemStartId || run.startSystemID == FilterSystemEndId || run.endSystemID == FilterSystemStartId || run.endSystemID == FilterSystemEndId);
        }

        private void OnClearFilters()
        {
            TripView.Filter = null;
            TripView.Refresh();
        }

        private void OnFilterResults(object sender)
        {
            var tx = sender as Transaction;
            FilterSystemStartId = tx.StartSystemId;
            FilterSystemEndId = tx.EndSystemId;

            TripView.Filter = FilterSystemPair;
            TripView.Refresh();
        }

        private void OnOpenSettings()
        {
            var settings = new Views.SettingsView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            settings.Show();
        }

        private void OnOpenDonation()
        {
            var donation = new Views.DonationView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            donation.Show();
        }

        private void OnOpenAbout()
        {
            var about = new Views.AboutView()
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            about.Show();
        }

        private void OnOpenMarketWindow(object sender)
        {
            var typeId = Convert.ToInt32(sender);
            Debug.WriteLine("Market window for: {0}", typeId);
            Auth.Instance.OpenMarketWindow(typeId);
        }

        private void OnSetWaypoints(object sender)
        {
            var systemId = Convert.ToInt32(sender);
            var clearOtherWaypoints = true;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                clearOtherWaypoints = false;
                Debug.WriteLine("Pressed shift while button pressing");
            }

            Auth.Instance.SetWaypoints(systemId, clearOtherWaypoints);
        }
    }
}
