using ConnectClient.ActiveDirectory;
using ConnectClient.Core.Settings;
using ConnectClient.Gui.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ConnectClient.Gui.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private string uniqueIdAttributeName;

        public string UniqueIdAttributeName
        {
            get { return uniqueIdAttributeName; }
            set { Set(() => UniqueIdAttributeName, ref uniqueIdAttributeName, value); }
        }

        public ObservableCollection<string> SelectedOrganizationalUnits { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> OrganizatioalUnits { get; } = new ObservableCollection<string>();

        private string endpointUrl;

        public string EndpointUrl
        {
            get { return endpointUrl; }
            set { Set(() => EndpointUrl, ref endpointUrl, value); }
        }

        private string endpointToken;

        public string EndpointToken
        {
            get { return endpointToken; }
            set { Set(() => EndpointToken, ref endpointToken, value); }
        }

        private string ldapServer;

        public string LdapServer
        {
            get { return ldapServer; }
            set
            {
                Set(() => LdapServer, ref ldapServer, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private int ldapPort;

        public int LdapPort
        {
            get { return ldapPort; }
            set
            {
                Set(() => LdapPort, ref ldapPort, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool ldapUseSsl;

        public bool LdapUseSsl
        {
            get { return ldapUseSsl; }
            set
            {
                Set(() => LdapUseSsl, ref ldapUseSsl, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool ldapUseTls;

        public bool LdapUseTls
        {
            get { return ldapUseTls; }
            set
            {
                Set(() => LdapUseTls, ref ldapUseTls, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string ldapFqdn;

        public string LdapFqdn
        {
            get { return ldapFqdn; }
            set
            {
                Set(() => LdapFqdn, ref ldapFqdn, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string ldapNetBios;

        public string LdapNetBIOS
        {
            get { return ldapNetBios; }
            set
            {
                Set(() => LdapNetBIOS, ref ldapNetBios, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string ldapUsername;

        public string LdapUsername
        {
            get { return ldapUsername; }
            set
            {
                Set(() => LdapUsername, ref ldapUsername, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string ldapPassword;

        public string LdapPassword
        {
            get { return ldapPassword; }
            set
            {
                Set(() => LdapPassword, ref ldapPassword, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string ldapCertificateThumbprint;

        public string LdapCertificateThumbprint
        {
            get { return ldapCertificateThumbprint; }
            set
            {
                Set(() => LdapCertificateThumbprint, ref ldapCertificateThumbprint, value);
                LoadOrganizationalUnitsCommand?.RaiseCanExecuteChanged();
            }
        }

        public List<UsernameProperty> LdapUsernameProperties { get; } = new List<UsernameProperty> { UsernameProperty.sAMAccountName, UsernameProperty.UserPrincipalName };

        private UsernameProperty ldapUsernameProperty;

        public UsernameProperty LdapUsernameProperty
        {
            get { return ldapUsernameProperty; }
            set { Set(() => LdapUsernameProperty, ref ldapUsernameProperty, value); }
        }

        #region Commands

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand LoadOrganizationalUnitsCommand { get; private set; }

        #endregion

        #region Services

        private readonly SettingsManager settingsManager;

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        #endregion

        public SettingsViewModel(SettingsManager settingsManager, IMessenger messenger)
            : base(messenger)
        {
            this.settingsManager = settingsManager;

            SaveCommand = new RelayCommand(Save);
            LoadOrganizationalUnitsCommand = new RelayCommand(LoadOrganizationalUnits, CanLoadOrganizationalUnits);
        }

        private void LoadOrganizationalUnits()
        {
            try
            {
                OrganizatioalUnits.Clear();

                using var ldapConnection = new LdapConnection();
                ConnectLdapConnection(ldapConnection);
                ldapConnection.Bind(LdapUsername, LdapPassword);

                var searchBase = string.Join(',', LdapFqdn.Split('.').Select(x => $"DC={x}"));

                var results = ldapConnection.Search(searchBase, LdapConnection.SCOPE_SUB, "(objectCategory=organizationalUnit)", Array.Empty<string>(), false);
                var ous = new List<string>();

                while (results.HasMore())
                {
                    try
                    {
                        var entry = results.Next();

                        ous.Add(entry.DN);
                    }
                    catch (LdapReferralException e) { }
                }

                foreach (var ou in ous.OrderBy(x => x, new OuDnComparer()))
                {
                    OrganizatioalUnits.Add(ou);
                }
            }
            catch (LdapException e)
            {
                Messenger.Send(new ErrorDialogMessage { Exception = e, Header = "Fehler", Title = "Fehler beim Laden der OUs", Text = "Beim Laden der Organisationseinheiten aus dem Active Directory ist ein Fehler aufgetreten." });
            }
        }

        private bool CanLoadOrganizationalUnits()
        {
            return !string.IsNullOrEmpty(LdapUsername)
                && !string.IsNullOrEmpty(LdapPassword)
                && LdapPort > 0
                && !string.IsNullOrEmpty(LdapServer)
                && ((LdapUseSsl == false && LdapUseTls == false) || !string.IsNullOrEmpty(LdapCertificateThumbprint));
        }

        public void LoadSettings()
        {
            var settings = settingsManager.GetSettings();

            UniqueIdAttributeName = settings.UniqueIdAttributeName;

            EndpointUrl = settings.Endpoint.Url;
            EndpointToken = settings.Endpoint.Token;
            LdapServer = settings.Ldap.Server;
            LdapPort = settings.Ldap.Port;
            LdapUseSsl = settings.Ldap.UseSSL;
            LdapUseTls = settings.Ldap.UseTLS;
            LdapFqdn = settings.Ldap.DomainFQDN;
            LdapNetBIOS = settings.Ldap.DomainNetBIOS;
            LdapUsername = settings.Ldap.Username;
            LdapPassword = settings.Ldap.Password;
            LdapCertificateThumbprint = settings.Ldap.CertificateThumbprint;
            LdapUsernameProperty = settings.Ldap.UsernameProperty;

            if(CanLoadOrganizationalUnits())
            {
                LoadOrganizationalUnits();
            }

            foreach (var ou in settings.OrganizationalUnits)
            {
                SelectedOrganizationalUnits.Add(ou);
            }
        }

        private void Save()
        {
            var settings = settingsManager.GetSettings();
            settings.UniqueIdAttributeName = UniqueIdAttributeName;
            settings.OrganizationalUnits = SelectedOrganizationalUnits.Distinct().ToArray();
            settings.Endpoint.Url = EndpointUrl;
            settings.Endpoint.Token = EndpointToken;
            settings.Ldap.Server = LdapServer;
            settings.Ldap.Port = LdapPort;
            settings.Ldap.UseSSL = LdapUseSsl;
            settings.Ldap.UseTLS = LdapUseTls;
            settings.Ldap.DomainFQDN = LdapFqdn;
            settings.Ldap.DomainNetBIOS = LdapNetBIOS;
            settings.Ldap.Username = LdapUsername;
            settings.Ldap.Password = LdapPassword;
            settings.Ldap.CertificateThumbprint = LdapCertificateThumbprint;
            settings.Ldap.UsernameProperty = LdapUsernameProperty;

            settingsManager.SaveSettings();
        }

        private void ConnectLdapConnection(LdapConnection ldapConnection)
        {
            ldapConnection.UserDefinedServerCertValidationDelegate += CheckCertificateCallback;

            /**
             * CONFIGURE SSL
             */
            if (LdapUseSsl)
            {
                ldapConnection.SecureSocketLayer = true;
            }

            /**
             * CONNECT
             */
            ldapConnection.Connect(LdapServer, LdapPort);

            /**
             * CONFIGURE TLS
             */
            if (LdapUseTls)
            {
                ldapConnection.StartTls();
            }
        }

        private bool CheckCertificateCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            foreach (var cert in chain.ChainElements)
            {
                if (cert.Certificate.Thumbprint.ToLower() == LdapCertificateThumbprint.ToLower())
                {
                    return true;
                }
            }

            return false;
        }


        private class OuDnComparer : IComparer<string>
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            private static extern int StrCmpLogicalW(string psz1, string psz2);

            public int Compare([AllowNull] string x, [AllowNull] string y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null)
                {
                    return -1;
                }
                else if (y == null)
                {
                    return 1;
                }

                var dnX = string.Join(',', x.Split(',').Reverse().ToArray());
                var dnY = string.Join(',', y.Split(',').Reverse().ToArray());

                return StrCmpLogicalW(dnX, dnY);
            }
        }
    }
}
