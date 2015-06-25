using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock UI controls.
    /// </summary>
    [DisplayName( "Developer Environment Info" )]
    [Category( "Examples" )]
    [Description( "Shows Information about the Development environment" )]
    public partial class DevelopEnvironmentInfo : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var csBuilder = new SqlConnectionStringBuilder( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
                lDatabaseName.Text = string.Format(
                    @"
Name: {0}
Server: {1}",
            csBuilder.InitialCatalog,
            csBuilder.DataSource );

                lHostingEnvironment.Text = string.Format(
                    @"
SiteName: {0}
IsDevelopmentEnvironment: {1}
Development Path: {2}",
                    System.Web.Hosting.HostingEnvironment.SiteName,
                    System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment,
                    System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath );
            }
        }

        #region Events

        /// <summary>
        /// Handles the Click event of the btnShutdown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShutdown_Click( object sender, EventArgs e )
        {
            System.Web.Hosting.HostingEnvironment.InitiateShutdown();
            NavigateToPage( new Rock.Web.PageReference( this.RockPage.PageId ) );
        }

        #endregion
    }
}
