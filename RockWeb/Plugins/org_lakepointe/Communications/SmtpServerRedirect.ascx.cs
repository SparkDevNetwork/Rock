using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Web.UI;
using AngleSharp.Text;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Communication;

namespace RockWeb.Plugins.org_lakepointe.Communications
{
    [DisplayName( "Dev SMTP Server Redirect" )]
    [Category( "LPC > Communications" )]
    [Description( "Redirects the user to the development SMTP server's web UI." )]

    public partial class SmtpServerRedirect : RockBlock
    {
        private RockContext _context;

        #region Properties

        // If we ever add a new dev environment, adding it to this block is as simple as adding it to this dictionary here
        // - The dictionary key needs to be the lowercase of the site's database's name without the 'rock'
        //     - For example: the DB name of RockDev is 'RockDev', which is 'rockdev' in lowercase, and without the 'rock' it is just 'dev'
        // - The port needs to be the SMTP port of the dev email server
        //     - This is configured in the startup and nightly scripts
        // - The uiPort needs to be the WebUI port of the dev email server
        //     - This is configured in the startup and nightly scripts
        Dictionary<string, SmtpConfiguration> smtpConfigurations = new Dictionary<string, SmtpConfiguration>
        {
            { "default", new SmtpConfiguration( port:1025, uiPort:8025 ) },
            { "dev",     new SmtpConfiguration( port:1026, uiPort:8026 ) },
            { "train",   new SmtpConfiguration( port:1027, uiPort:8027 ) },
            { "gamma",   new SmtpConfiguration( port:1028, uiPort:8028 ) },
            { "beta",    new SmtpConfiguration( port:1029, uiPort:8029 ) }
        };

        #endregion
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Check if environment is prod
                string environmentName = GetEnvironmentName();
                if ( environmentName.IsNullOrWhiteSpace() || environmentName == "prod" )
                {
                    pnlAlert.Visible = true;
                    btnConfigureSmtp.Visible = false;
                    lContent.Visible = false;
                    btnGoToSmtpUi.Visible = false;
                    lWarning.Text = "You are on a production/staging environment. There is no use for a dev SMTP server here. All functions of this block have been disabled.";

                    return;
                }

                pnlAlert.Visible = false;

                // Check if SMTP is configured correctly
                var smtpConfig = GetCurrentSmtpConfig();
                var warnings = new List<string>();

                // Check if any other communication transports are active
                var attributeValues = new AttributeValueService( _context ).Queryable()
                    .Where( a => a.Attribute.EntityType.AssemblyName.StartsWith( "Rock.Communication.Transport." ) && a.Attribute.Key == "Active" && a.Value == "True" );
                foreach ( var value in attributeValues )
                {
                    if ( value.Attribute.EntityType.Name != "Rock.Communication.Transport.SMTP" )
                    {
                        warnings.Add( $"The {value.Attribute.EntityType.FriendlyName} communication transport is currently active." );
                    }
                }

                // Check configuration values
                if ( smtpConfig.Uri != smtpConfigurations[environmentName].Url )
                {
                    warnings.Add( $"The current URL of \"{smtpConfig.Uri}\" does not match the desired config value of \"{smtpConfigurations[environmentName].Url}\"." );
                }
                if ( smtpConfig.Active != true )
                {
                    warnings.Add( $"The SMTP server config is not currently active." );
                }
                if ( smtpConfig.Port != smtpConfigurations[environmentName].Port )
                {
                    warnings.Add( $"The SMTP server port of \"{smtpConfig.Port}\" does not match the desired config value of \"{smtpConfigurations[environmentName].Port}\"." );
                }
                if ( smtpConfig.UserName != "" )
                {
                    warnings.Add( $"The SMTP server username is not currently empty." );
                }
                if ( smtpConfig.Password != "" )
                {
                    warnings.Add( $"The SMTP server password is not currently empty." );
                }
                if ( smtpConfig.UseSSL != false )
                {
                    warnings.Add( $"The SMTP server has UseSSL enabled." );
                }
                if ( smtpConfig.MaxParallelization != 1 )
                {
                    warnings.Add( $"The SMTP server max parallelization config of \"{smtpConfig.MaxParallelization}\" does not match the desired config value of \"1\"." );
                }

                if ( smtpConfig.Active == true )
                {
                    var medium = MediumContainer.GetComponent( "Rock.Communication.Medium.Email" );
                    if ( medium != null )
                    {
                        var transportContainer = medium.GetAttributeValue( "TransportContainer" );
                        var transport = TransportContainer.GetComponent( "Rock.Communication.Transport.SMTP" );
                        if ( transportContainer.AsGuidOrNull() != transport.TypeGuid )
                        {
                            warnings.Add( $"The SMTP server is not set as the active Email transport." );
                        }
                    }
                    else
                    {
                        warnings.Add( $"The Email communication medium is not active." );
                    }
                }

                // Set the URL of the SMTP server web UI so that we can set it or redirect to it
                string http = smtpConfigurations[environmentName].Url.Contains( "http" ) ? "" : "http://";
                string fullUrl = $"{http}{(environmentName == "default" ? smtpConfig.Uri : smtpConfig.Url)}:{smtpConfigurations[environmentName].UiPort}";

                // Display any warnings
                if ( warnings.Any() )
                {
                    // Show the warning alert
                    lWarning.Text = "<ul>";
                    foreach ( string warning in warnings )
                    {
                        lWarning.Text += $"<li>{warning}</li>";
                    }
                    lWarning.Text += "</ul>";

                    pnlAlert.Visible = true;
                }
                else
                {
                    // Check if query strings has "redirect=false"
                    bool? redirectParam = Request.QueryString["redirect"].AsBooleanOrNull();
                    if ( redirectParam != false )
                    {
                        // If there are no warnings, redirect the user to the SMTP server web UI
                        Response.Redirect( fullUrl );
                    }
                }

                // If the user is still here, set the link to the SMTP server web UI
                btnGoToSmtpUi.HRef = fullUrl;
            }
        }

        #endregion  
        #region Events

        protected void btnConfigureSmtp_Click( object sender, EventArgs e )
        {
            string environmentName = GetEnvironmentName();

            if ( environmentName.IsNullOrWhiteSpace() || environmentName == "prod" )
            {
                pnlAlert.Visible = true;
                btnConfigureSmtp.Visible = false;
                lContent.Visible = false;
                btnGoToSmtpUi.Visible = false;
                lWarning.Text = "You are on a production/staging environment. There is no use for a dev SMTP server here. All functions of this block have been disabled.";

                return;
            }

            var transportsDictionary = TransportContainer.Instance.Components.Values;
            foreach ( var transportKeyValue in transportsDictionary )
            {
                var transport = transportKeyValue.Value;
                if ( transport.EntityType.Name != "Rock.Communication.Transport.SMTP" )
                {
                    transport.SetAttributeValue( "Active", "False" );
                    transport.SaveAttributeValue( "Active", _context );
                }
                else
                {
                    transport.SetAttributeValue( "Active", "True" );
                    transport.SetAttributeValue( "Server", smtpConfigurations[environmentName].Url );
                    transport.SetAttributeValue( "Port", smtpConfigurations[environmentName].Port );
                    transport.SetAttributeValue( "UserName", "" );
                    transport.SetAttributeValue( "Password", "" );
                    transport.SetAttributeValue( "UseSSL", "False" );
                    transport.SetAttributeValue( "MaxParallelization", "1" );
                    transport.SaveAttributeValues( _context );

                    var mediumsDictionary = MediumContainer.Instance.Components.Values
                        .Where( c => c.Value.EntityType.Name == "Rock.Communication.Medium.Email" );
                    foreach ( var mediumKeyValue in mediumsDictionary )
                    {
                        var medium = mediumKeyValue.Value;

                        medium.SetAttributeValue( "Active", "True" );
                        medium.SetAttributeValue( "TransportContainer", transport.TypeGuid );
                        medium.SaveAttributeValues( _context );
                    }
                }
            }

            // Set the URL of the SMTP server web UI so that we can set it or redirect to it
            var smtpConfig = GetCurrentSmtpConfig();
            string http = smtpConfigurations[environmentName].Url.Contains( "http" ) ? "" : "http://";
            string fullUrl = $"{http}{( environmentName == "default" ? smtpConfig.Uri : smtpConfig.Url )}:{smtpConfigurations[environmentName].UiPort}";

            // Check if query strings has "redirect=false"
            bool? redirectParam = Request.QueryString["redirect"].AsBooleanOrNull();
            if ( redirectParam != false )
            {
                // If there are no warnings, redirect the user to the SMTP server web UI
                Response.Redirect( fullUrl );
            }

            // If the user is still here, set the link to the SMTP server web UI
            pnlAlert.Visible = false;
            btnGoToSmtpUi.HRef = fullUrl;
        }

        #endregion
        #region Methods

        private ( string Uri, string Url, bool Active, int Port, string UserName, string Password, bool UseSSL, int MaxParallelization ) GetCurrentSmtpConfig()
        {
            var attributeValues = new AttributeValueService( _context ).Queryable()
                .Where( a => a.Attribute.EntityType.Name == "Rock.Communication.Transport.SMTP" );
            string uri = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Server" )?.Value ?? "";
            bool active = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Active" )?.Value.AsBoolean() ?? false;
            int port = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Port" )?.Value.AsInteger() ?? 0;
            string userName = attributeValues.FirstOrDefault( a => a.Attribute.Key == "UserName" )?.Value ?? "";
            string password = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Password" )?.Value ?? "";
            bool useSSL = attributeValues.FirstOrDefault( a => a.Attribute.Key == "UseSSL" )?.Value.AsBoolean() ?? false;
            int maxParallelization = attributeValues.FirstOrDefault( a => a.Attribute.Key == "MaxParallelization" )?.Value.AsInteger() ?? 0;

            string url = "";

            if ( uri.ToLower() == "localhost" || uri == "." )
            {
                try
                {
                    url = GetLocalIPAddress();
                }
                catch { }
            }

            if ( url == "" )
            {
                url = uri;
            }

            return ( uri, url, active, port, userName, password, useSSL, maxParallelization );
        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry( Dns.GetHostName() );
            foreach ( var ip in host.AddressList )
            {
                if ( ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString().StartsWith( "172." ) == false )
                {
                    return ip.ToString();
                }
            }
            throw new Exception( "No network adapters with an IPv4 address in the system!" );
        }

        private string GetEnvironmentName()
        {
            var acceptableEnvironmentNames = new List<string>( smtpConfigurations.Keys );

            string dbName = _context.Database.Connection.Database.ToLower();

            // Check if environment is production or staging
            if ( dbName == "rock" || dbName == "staging" )
            {
                return "prod";
            }

            dbName = dbName.Replace( "rock", "" );

            if ( acceptableEnvironmentNames.Contains( dbName ) )
            {
                return dbName;
            }
            else
            {
                return "default";
            }
        }

        #endregion

        private class SmtpConfiguration
        {
            public string Url { get; set; }
            public int Port { get; set; }
            public int UiPort { get; set; }

            public SmtpConfiguration(int port, int uiPort, string url = "localhost" )
            {
                Url = url;
                Port = port;
                UiPort = uiPort;
            }
        }
    }
}
