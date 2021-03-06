﻿using System.Threading.Tasks;
using System.Windows.Input;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Framework;
using Azuser.Client.Framework.Resolver;
using Azuser.Client.Helpers;
using Azuser.Client.Views.Explorer;
using Azuser.Client.Views.Login;

namespace Azuser.Client.Views.Shell
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IResolver _resolver;

        private ConnectionScope _connectionScope;

        private ViewModelBase _currentViewModel;
        private bool _isLoadingData;
        private bool _isLoggedIn;
        private string _loggedInServerAddress;
        private string _loggedInUser;

        public ShellViewModel(IResolver resolver, IMessengerService messengerService)
        {
            _resolver = resolver;

            messengerService.Register<LoadingDataMessage>(this, msg => IsLoadingData = msg.IsLoadingData);
        }

        public string WindowTitle => "Azuser - v" + App.Version;

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set => Set(ref _isLoadingData, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }

        public string LoggedInServerAddress
        {
            get => _loggedInServerAddress;
            set => Set(ref _loggedInServerAddress, value);
        }

        public string LoggedInUser
        {
            get => _loggedInUser;
            set => Set(ref _loggedInUser, value);
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel?.Cleanup();
                Set(ref _currentViewModel, value);
            }
        }

        public async Task InitializeAsync()
        {
            var loginViewModel = _resolver.Get<LoginViewModel>();

            CurrentViewModel = loginViewModel;

            _connectionScope = await loginViewModel.WaitForLogin();

            IsLoggedIn = true;
            LoggedInServerAddress = _connectionScope.ServerAddress;
            LoggedInUser = _connectionScope.Username;

            var explorerViewModel = _resolver.Get<ServerExplorerViewModel>();

            await explorerViewModel.InitializeAsync(_connectionScope);

            CurrentViewModel = explorerViewModel;
        }

        public ICommand DisconnectCommand => new AsyncRelayCommand(Disconnect);

        private Task Disconnect()
        {
            CurrentViewModel = null;
            _connectionScope = null;

            IsLoggedIn = false;
            LoggedInServerAddress = null;
            LoggedInUser = null;

            return InitializeAsync();
        }
    }
}
