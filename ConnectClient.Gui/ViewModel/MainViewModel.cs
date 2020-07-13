using ConnectClient.Core.Sync;
using ConnectClient.Gui.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;

namespace ConnectClient.Gui.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                Set(() => IsBusy, ref isBusy, value);
                SyncCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool fullSync;

        public bool FullSync
        {
            get { return fullSync; }
            set { Set(() => FullSync, ref fullSync, value); }
        }

        #region Commands

        public RelayCommand SyncCommand { get; private set; }

        #endregion

        #region Services

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        private readonly ISyncEngine syncEngine;

        #endregion

        public MainViewModel(ISyncEngine syncEngine, IMessenger messenger)
            : base(messenger)
        {
            this.syncEngine = syncEngine;

            SyncCommand = new RelayCommand(Sync, CanSync);
        }

        private bool CanSync()
        {
            return !IsBusy;
        }

        private async void Sync()
        {
            try
            {
                IsBusy = true;
                await syncEngine.SyncAsync(FullSync);
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Header = "Fehler", Title = "Fehler bei der Synchronisation", Text = "Bei der Synchronisation ist ein Fehler aufgetreten." });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
