using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectClient.ActiveDirectory;
using ConnectClient.Core.Settings;
using ConnectClient.Gui.UI;
using ConnectClient.Models.Response;
using ConnectClient.Rest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConnectClient.Gui.ViewModel
{
    public class MainViewModel : ObservableRecipient
    {
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        private double progress;

        public double Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }

        private string progressText;

        public string ProgressText
        {
            get { return progressText; }
            set { SetProperty(ref progressText, value); }
        }

        private string filter;

        public string Filter
        {
            get { return filter; }
            set
            {
                SetProperty(ref filter, value);
                UsersView.Refresh();
            }
        }


        /// <summary>
        /// Users read from Active Directory
        /// </summary>
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        public ICollectionView UsersView { get; }

        /// <summary>
        /// Selected Active Directory users to be provisioned (either create or update)
        /// </summary>
        public ObservableCollection<User> UsersToProvision { get; } = new ObservableCollection<User>();

        /// <summary>
        /// Users read from online IDP with no corresponding Active Directory user
        /// </summary>
        public ObservableCollection<OnlineUser> MissingUsers { get; } = new ObservableCollection<OnlineUser>();

        public ICollectionView MissingUsersView { get; }

        /// <summary>
        /// Selected Active Directory users to be removed from online IDP
        /// </summary>
        public ObservableCollection<OnlineUser> UsersToRemove { get; } = new ObservableCollection<OnlineUser> { };

        #region Command

        public AsyncRelayCommand ProvisionCommand { get; private set; }

        public AsyncRelayCommand RemoveCommand { get; private set; }

        public AsyncRelayCommand<bool> LoadActiveDirectoryUsers { get; private set; }

        public RelayCommand SelectAllCommand { get; private set; }

        public RelayCommand UnselectAllCommand { get; private set; }

        #endregion

        #region Services

        private readonly IClient client;
        private readonly ILdapUserProvider ldapUserProvider;
        private readonly SettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public MainViewModel(IClient client, ILdapUserProvider ldapUserProvider, SettingsManager settingsManager, IDialogHelper dialogHelper)
        {
            this.client = client;
            this.ldapUserProvider = ldapUserProvider;
            this.settingsManager = settingsManager;
            this.client = client;
            this.dialogHelper = dialogHelper;

            UsersView = CollectionViewSource.GetDefaultView(Users);
            UsersView.GroupDescriptions.Add(new PropertyGroupDescription("OU"));
            UsersView.SortDescriptions.Add(new SortDescription("OU", ListSortDirection.Ascending));
            UsersView.Filter += OnApplyFilter;

            MissingUsersView = CollectionViewSource.GetDefaultView(MissingUsers);
            MissingUsersView.GroupDescriptions.Add(new PropertyGroupDescription("Grade"));
            MissingUsersView.SortDescriptions.Add(new SortDescription("Grade", ListSortDirection.Ascending));

            ProvisionCommand = new AsyncRelayCommand(ProvisionAsync, CanProvision);
            RemoveCommand = new AsyncRelayCommand(RemoveUsersAsync, CanRemoveUsers);
            LoadActiveDirectoryUsers = new AsyncRelayCommand<bool>(LoadActiveDirectoryUsersAsync);
            SelectAllCommand = new RelayCommand(SelectAll);
            UnselectAllCommand = new RelayCommand(UnselectAll);

            UsersToProvision.CollectionChanged += delegate
            {
                ProvisionCommand?.NotifyCanExecuteChanged();
            };

            UsersToRemove.CollectionChanged += delegate
            {
                RemoveCommand?.NotifyCanExecuteChanged();
            };


        }

        private bool OnApplyFilter(object obj)
        {
            if(string.IsNullOrEmpty(Filter))
            {
                return true;
            }

            var user = obj as User;

            return (user.Username != null && user.Username.Contains(Filter, StringComparison.InvariantCultureIgnoreCase))
                || (user.Firstname != null && user.Firstname.Contains(Filter, StringComparison.InvariantCultureIgnoreCase))
                || (user.Lastname != null && user.Lastname.Contains(Filter, StringComparison.InvariantCultureIgnoreCase))
                || (user.Email != null && user.Email.Contains(Filter, StringComparison.InvariantCultureIgnoreCase));
        }

        private void SelectAll()
        {
            foreach (var user in Users)
            {
                if (!UsersToProvision.Contains(user))
                {
                    UsersToProvision.Add(user);
                }
            }
        }

        private void UnselectAll()
        {
            foreach(var user in Users)
            {
                UsersToProvision.Remove(user);
            }
        }

        public async Task ProvisionAsync()
        {
            try
            {
                IsBusy = true;
                ProgressText = "Lade Online-Benutzer";

                var settings = settingsManager.GetSettings();
                var onlineGuids = (await client.GetUsersAsync(settings.Endpoint) as ListActiveDirectoryUserResponse)?.Users.Select(x => x.Guid);

                if (onlineGuids == null)
                {
                    // ERROR
                }

                int adds = 0;
                int updates = 0;
                int total = UsersToProvision.Count;

                int idx = 0;
                var tasks = new List<Task>();
                foreach (var userToProvision in UsersToProvision)
                {
                    if (idx % 5 == 0)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                        Progress = (double)idx / total;
                        ProgressText = $"Benutzer provisionieren ({idx} / {total})";
                    }

                    if (onlineGuids.Contains(userToProvision.Guid))
                    {
                        tasks.Add(client.UpdateUserAsync(userToProvision, settings.Endpoint));
                        updates++;
                    }
                    else
                    {
                        tasks.Add(client.AddUserAsync(userToProvision, settings.Endpoint));
                        adds++;
                    }

                    idx++;
                }

                dialogHelper.ShowSuccess(adds, updates, 0);
            }
            catch (Exception e)
            {
                dialogHelper.ShowException(e);
            }
            finally
            {
                IsBusy = false;
                Progress = 0;
                ProgressText = string.Empty;
            }
        }

        public bool CanProvision() => UsersToProvision.Count > 0;

        public async Task LoadActiveDirectoryUsersAsync(bool skipIfNotInitial)
        {
            if(skipIfNotInitial && Users.Count > 0)
            {
                return;
            }

            try
            {
                IsBusy = true;
                ProgressText = "Lade Benutzer aus Active Directory";

                var settings = settingsManager.GetSettings();

                var users = await Task.Run(() => ldapUserProvider.GetUsers(settings.OrganizationalUnits, settings.Ldap));

                Users.Clear();
                UsersToProvision.Clear();
                MissingUsers.Clear();
                UsersToRemove.Clear();

                foreach (var user in users.OrderBy(x => x.Username))
                {
                    Users.Add(user);
                }

                ProgressText = "Lade Online-Benutzer";

                var onlineUsers = (await client.GetUsersAsync(settings.Endpoint) as ListActiveDirectoryUserResponse)?.Users;

                if (onlineUsers == null)
                {
                    throw new Exception("Fehler beim Einlesen der Online-Benutzer");
                }

                // Compute missing users
                var presentGuids = users.Select(x => x.Guid).ToList();

                foreach (var user in onlineUsers)
                {
                    if (!presentGuids.Contains(user.Guid))
                    {
                        MissingUsers.Add(user);
                    }
                }
            }
            catch (Exception e)
            {
                dialogHelper.ShowException(e);
            }
            finally
            {
                IsBusy = false;
                Progress = 0;
                ProgressText = string.Empty;
            }
        }

        public async Task RemoveUsersAsync()
        {
            try
            {
                IsBusy = true;
                ProgressText = string.Empty;

                var settings = settingsManager.GetSettings();

                int idx = 0;
                int total = UsersToRemove.Count;
                var tasks = new List<Task>();

                foreach (var user in UsersToRemove)
                {
                    if (idx % 5 == 0)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                        ProgressText = $"Lösche Benutzer ({idx} / {total})";
                    }

                    tasks.Add(client.RemoveUserAsync(user.Guid, settings.Endpoint));
                }

                dialogHelper.ShowSuccess(0, 0, total);

                await LoadActiveDirectoryUsersAsync(false);
            }
            catch (Exception e)
            {
                dialogHelper.ShowException(e);
            }
            finally
            {
                IsBusy = false;
                Progress = 0;
                ProgressText = string.Empty;
            }
        }

        public bool CanRemoveUsers() => UsersToRemove.Count > 0;
    }
}
