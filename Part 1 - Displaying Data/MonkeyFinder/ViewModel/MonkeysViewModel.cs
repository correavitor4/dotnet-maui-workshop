using MonkeyFinder.Services;
using CommunityToolkit.Mvvm.Input;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    MonkeyService monkeyService;
    IConnectivity connectivity;
    IGeolocation geolocation;
    public ObservableCollection<Monkey> Monkeys { get; } = new();
    public MonkeysViewModel(MonkeyService monkeyService,
        IConnectivity connectivity,
        IGeolocation geolocation)
    {
        this.monkeyService = monkeyService;
        Title = "MonkeyFinder";
        this.connectivity = connectivity;
        this.geolocation = geolocation;
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetClosestMonkeyAsync()
    {
        if (IsBusy || Monkeys.Count == 0)
            return;

        try
        {
            var location = await geolocation.GetLastKnownLocationAsync();
            if (location is null || location.Timestamp.AddSeconds(30) < DateTime.UtcNow)
            {
                location = await geolocation.GetLocationAsync(
                    new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(30),
                    });
            }

            if (location is null)
                return;

            var first = Monkeys.OrderBy(m => 
                location.CalculateDistance(m.Latitude, m.Longitude, DistanceUnits.Miles)
                ).FirstOrDefault();

            if (first == null) return;

            await Shell.Current.DisplayAlert("Closest Monkey", $"{first.Name} in {first.Location}", "Ok");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", "Unable to get closest monkey", "OK");
            return;
        }
    }

    [RelayCommand]
    async Task GoToDetailsAsync(Monkey monkey)
    {
        if(monkey is null)
            return;

        await Shell.Current.GoToAsync($"{nameof(DetailsPage)}", true,
            new Dictionary<string, object>
            {
                {"Monkey", monkey}
            });
    }

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
        if(connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Error!", "Check your internet and try again", "OK");
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy= true;
            var monkeys = await monkeyService.GetMonkeys();
            
     
            if(Monkeys.Count != 0)
                Monkeys.Clear();
            

            foreach(var monkey in monkeys)
            {
                Monkeys.Add(monkey);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"Unable to get Monkeys: {ex.Message}", "Close");
        }
        finally { 
            IsBusy = false;
            IsRefreshing= false;
        }
    }
}
