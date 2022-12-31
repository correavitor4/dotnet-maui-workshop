using MonkeyFinder.Services;
using CommunityToolkit.Mvvm.Input;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    MonkeyService monkeyService;
    public ObservableCollection<Monkey> Monkeys { get; } = new();
    public MonkeysViewModel(MonkeyService monkeyService)
    {
        this.monkeyService = monkeyService;
        Title = "MonkeyFinder";
    }

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
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
