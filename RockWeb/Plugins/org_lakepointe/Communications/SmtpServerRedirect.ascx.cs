using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Communication;
using System.Web;

namespace RockWeb.Plugins.org_lakepointe.Communications
{
    [DisplayName( "Dev SMTP Server Redirect" )]
    [Category( "LPC > Communications" )]
    [Description( "Redirects the user to the development SMTP server's web UI." )]

    public partial class SmtpServerRedirect : RockBlock
    {
        private RockContext _context;

        #region Properties

        private readonly SmtpConfiguration _smtpConfiguration = new SmtpConfiguration
        {
            Server = "localhost",
            Active = true,
            Port = 1025,
            UserName = "",
            Password = "",
            UseSSL = false,
            MaxParallelization = 1
        };
        private const int _uiPort = 8025;

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
                if ( CheckIfEnvironmentIsProduction() == true )
                {
                    pnlAlert.Visible = true;
                    btnConfigureSmtp.Visible = false;
                    lContent.Visible = false;
                    btnGoToSmtpUi.Visible = false;
                    lWarning.Text = "You are on a production environment. There is no use for a dev SMTP server here. All functions of this block have been disabled.";

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
                foreach ( var prop in typeof( SmtpConfiguration ).GetProperties() )
                {
                    var currentValue = prop.GetValue( smtpConfig, null );
                    var desiredValue = prop.GetValue( _smtpConfiguration, null );

                    if ( currentValue.Equals( desiredValue ) == false )
                    {
                        warnings.Add( $"The SMTP server {prop.Name} config of \"{currentValue}\" does not match the desired config value of \"{desiredValue}\"" );
                    }
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
                string redirectUrl = $"http://{HttpContext.Current.Request.Url.Host}:{_uiPort}";

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
                        Response.Redirect( redirectUrl );
                    }
                }

                // If the user is still here, set the link to the SMTP server web UI
                btnGoToSmtpUi.HRef = redirectUrl;
            }
        }

        #endregion  
        #region Events

        protected void btnConfigureSmtp_Click( object sender, EventArgs e )
        {
            if ( CheckIfEnvironmentIsProduction() == true )
            {
                pnlAlert.Visible = true;
                btnConfigureSmtp.Visible = false;
                lContent.Visible = false;
                btnGoToSmtpUi.Visible = false;
                lWarning.Text = "You are on a production environment. There is no use for a dev SMTP server here. All functions of this block have been disabled.";

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
                    transport.SetAttributeValue( "Active", _smtpConfiguration.Active.ToString() );
                    transport.SetAttributeValue( "Server", _smtpConfiguration.Server );
                    transport.SetAttributeValue( "Port", _smtpConfiguration.Port );
                    transport.SetAttributeValue( "UserName", _smtpConfiguration.UserName );
                    transport.SetAttributeValue( "Password", _smtpConfiguration.Password );
                    transport.SetAttributeValue( "UseSSL", _smtpConfiguration.UseSSL.ToString() );
                    transport.SetAttributeValue( "MaxParallelization", _smtpConfiguration.MaxParallelization );
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
            string redirectUrl = $"http://{HttpContext.Current.Request.Url.Host}:{_uiPort}";

            // Check if query strings has "redirect=false"
            bool? redirectParam = Request.QueryString["redirect"].AsBooleanOrNull();
            if ( redirectParam != false )
            {
                // If there are no warnings, redirect the user to the SMTP server web UI
                Response.Redirect( redirectUrl );
            }

            // If the user is still here, set the link to the SMTP server web UI
            pnlAlert.Visible = false;
            btnGoToSmtpUi.HRef = redirectUrl;
        }

        #endregion
        #region Methods

        private SmtpConfiguration GetCurrentSmtpConfig()
        {
            var attributeValues = new AttributeValueService( _context ).Queryable()
                .Where( a => a.Attribute.EntityType.Name == "Rock.Communication.Transport.SMTP" );
            string server = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Server" )?.Value ?? "";
            bool active = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Active" )?.Value.AsBoolean() ?? false;
            int port = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Port" )?.Value.AsInteger() ?? 0;
            string userName = attributeValues.FirstOrDefault( a => a.Attribute.Key == "UserName" )?.Value ?? "";
            string password = attributeValues.FirstOrDefault( a => a.Attribute.Key == "Password" )?.Value ?? "";
            bool useSSL = attributeValues.FirstOrDefault( a => a.Attribute.Key == "UseSSL" )?.Value.AsBoolean() ?? false;
            int maxParallelization = attributeValues.FirstOrDefault( a => a.Attribute.Key == "MaxParallelization" )?.Value.AsInteger() ?? 0;

            return new SmtpConfiguration
            {
                Server = server,
                Active = active,
                Port = port,
                UserName = userName,
                Password = password,
                UseSSL = useSSL,
                MaxParallelization = maxParallelization
            };
        }

        private bool CheckIfEnvironmentIsProduction()
        {
            string dbName = _context.Database.Connection.Database.ToLower();

            // Check if environment is production
            if ( dbName == "rock" )
            {
                return true;
            }
            return false;
        }

        #endregion

        private class SmtpConfiguration
        {
            public string Server { get; set; }
            public bool Active { get; set; }
            public int Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool UseSSL { get; set; }
            public int MaxParallelization { get; set; }
        }
    }
}
