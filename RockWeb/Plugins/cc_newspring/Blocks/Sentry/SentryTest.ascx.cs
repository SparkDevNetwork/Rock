using System;
using System.ComponentModel;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using SharpRaven;
using SharpRaven.Data;

namespace RockWeb.Plugins.cc_newspring.Blocks.Sentry
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>
    [DisplayName( "Sentry Test" )]
    [Category( "NewSpring" )]
    [Description( "Test block to send errors to Sentry" )]
    public partial class SentryTest : RockBlock
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            nbInformation.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnSubmitError control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="Exception"></exception>
        protected void btnSubmitError_Click( object sender, EventArgs e )
        {
            try
            {
                throw new Exception( tbError.Text );
            }
            catch ( Exception ex )
            {
                var sentryDSN = GlobalAttributesCache.Read().GetValue( "SentryDSN" ) ?? string.Empty;
                var sentryClient = new RavenClient( sentryDSN );
                if ( !string.IsNullOrEmpty( sentryDSN ) && sentryClient != null )
                {
                    sentryClient.Capture( new SentryEvent( ex ) );
                    nbInformation.Visible = true;
                }
            }
        }
    }
}