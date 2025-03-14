﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectClient.Core.Settings;
using ConnectClient.Gui.UI;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ConnectClient.Gui.ViewModel
{
    public partial class SettingsViewModel : ObservableRecipient
    {
        private string uniqueIdAttributeName;

        public string UniqueIdAttributeName
        {
            get { return uniqueIdAttributeName; }
            set { SetProperty(ref uniqueIdAttributeName, value); }
        }

        public ObservableCollection<string> SelectedOrganizationalUnits { get; } = [];

        public ObservableCollection<string> OrganizatioalUnits { get; } = [];

        private string endpointUrl;

        public string EndpointUrl
        {
            get { return endpointUrl; }
            set { SetProperty(ref endpointUrl, value); }
        }

        private string endpointToken;

        public string EndpointToken
        {
            get { return endpointToken; }
            set { SetProperty(ref endpointToken, value); }
        }

        private string ldapServer;

        public string LdapServer
        {
            get { return ldapServer; }
            set
            {
                SetProperty(ref ldapServer, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private int ldapPort;

        public int LdapPort
        {
            get { return ldapPort; }
            set
            {
                SetProperty(ref ldapPort, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private bool ldapUseSsl;

        public bool LdapUseSsl
        {
            get { return ldapUseSsl; }
            set
            {
                SetProperty(ref ldapUseSsl, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private bool ldapUseTls;

        public bool LdapUseTls
        {
            get { return ldapUseTls; }
            set
            {
                SetProperty(ref ldapUseTls, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private string ldapFqdn;

        public string LdapFqdn
        {
            get { return ldapFqdn; }
            set
            {
                SetProperty(ref ldapFqdn, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private string ldapNetBios;

        public string LdapNetBIOS
        {
            get { return ldapNetBios; }
            set
            {
                SetProperty(ref ldapNetBios, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private string ldapUsername;

        public string LdapUsername
        {
            get { return ldapUsername; }
            set
            {
                SetProperty(ref ldapUsername, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private string ldapPassword;

        public string LdapPassword
        {
            get { return ldapPassword; }
            set
            {
                SetProperty(ref ldapPassword, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        private string ldapCertificateThumbprint;

        public string LdapCertificateThumbprint
        {
            get { return ldapCertificateThumbprint; }
            set
            {
                SetProperty(ref ldapCertificateThumbprint, value);
                LoadOrganizationalUnitsCommand?.NotifyCanExecuteChanged();
            }
        }

        public List<UsernameProperty> LdapUsernameProperties { get; } = new List<UsernameProperty> { UsernameProperty.sAMAccountName, UsernameProperty.UserPrincipalName };

        private UsernameProperty ldapUsernameProperty;

        public UsernameProperty LdapUsernameProperty
        {
            get { return ldapUsernameProperty; }
            set { SetProperty(ref ldapUsernameProperty, value); }
        }

        #region Commands

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand LoadOrganizationalUnitsCommand { get; private set; }

        #endregion

        #region Services

        private readonly SettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;

        #endregion

        public SettingsViewModel(SettingsManager settingsManager, IDialogHelper dialogHelper)
        {
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;

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

                var results = ldapConnection.Search(searchBase, LdapConnection.SCOPE_SUB, "(objectCategory=organizationalUnit)", [], false);
                var ous = new List<string>();

                while (results.HasMore())
                {
                    try
                    {
                        var entry = results.Next();

                        ous.Add(entry.DN);
                    }
                    catch (LdapReferralException) { }
                }

                foreach (var ou in ous.OrderBy(x => x, new OuDnComparer()))
                {
                    OrganizatioalUnits.Add(ou);
                }
            }
            catch (LdapException e)
            {
                dialogHelper.ShowException(e);
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
                if (cert.Certificate.Thumbprint.Equals(LdapCertificateThumbprint, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        private partial class OuDnComparer : IComparer<string>
        {
            [LibraryImport("shlwapi.dll", EntryPoint = "StrCmpLogicalW", StringMarshalling = StringMarshalling.Utf16)]
            private static partial int StrCmpLogicalW(string psz1, string psz2);

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
