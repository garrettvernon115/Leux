using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateToAsync(string route, Dictionary<string, object> parameters);
        Task GoBackAsync();
        Task GoToRootAsync();
        Task NavigateToDashboardAsync();
        Task NavigateToLoginAsync();
        Task NavigateToRegistrationAsync();
        Task NavigateToMainAsync();
        void SwitchToMainApp();
        void SwitchToLogin();
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task NavigateToAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }

        public async Task NavigateToAsync(string route, Dictionary<string, object> parameters)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task GoToRootAsync()
        {
            await Shell.Current.Navigation.PopToRootAsync();
        }

        public async Task NavigateToDashboardAsync()
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SwitchToMainApp();
                await Shell.Current.GoToAsync("//MainApp/DashboardTab/DashboardPage");
            }
        }

        public async Task NavigateToLoginAsync()
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SwitchToLogin();
            }
        }

        public async Task NavigateToRegistrationAsync()
        {
            await Shell.Current.GoToAsync(nameof(RegistrationPage));
        }

        public async Task NavigateToMainAsync()
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SwitchToMainApp();
                await Shell.Current.GoToAsync("//MainApp/MainTab/MainPage");
            }
        }

        public void SwitchToMainApp()
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SwitchToMainApp();
            }
        }

        public void SwitchToLogin()
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SwitchToLogin();
            }
        }
    }
}
