using MonkeyFinder.Services;
using CommunityToolkit.Mvvm.Input;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    MonkeyService monkeyService;
    IConnectivity connectivity;
    public ObservableCollection<Monkey> Monkeys { get; } = new();
    public MonkeysViewModel(MonkeyService monkeyService, IConnectivity connectivity)
    {
        this.monkeyService = monkeyService;
        Title = "MonkeyFinder";
        this.connectivity = connectivity;
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
        }
    }
}
