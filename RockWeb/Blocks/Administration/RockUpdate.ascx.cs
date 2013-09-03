//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

using Rock.Services.NuGet;
using Rock.VersionInfo;
using Rock.Web.Cache;

using NuGet;

namespace RockWeb.Blocks.Administration
{
    public partial class RockUpdate : Rock.Web.UI.RockBlock
    {
        WebProjectManager nuGetService = null;
        private string rockPackageId = "RockChMS";

        /// <summary>
        /// Obtains a WebProjectManager from the Global "PackageSourceUrl" Attribute.
        /// </summary>
        protected WebProjectManager NuGetService
        {
            get
            {
                if ( nuGetService == null )
                {
                    var globalAttributesCache = GlobalAttributesCache.Read();
                    string packageSource = globalAttributesCache.GetValue( "PackageSourceUrl" );
                    string siteRoot = Request.MapPath( "~/" );
                    nuGetService = new WebProjectManager( packageSource, siteRoot );
                }
                return nuGetService;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Invoked on page load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load( object sender, EventArgs e )
        {
            DisplayRockVersion();
            if ( !IsPostBack )
            {
                CheckForUpdate();
            }
        }

        /// <summary>
        /// Handles the clicking of the Install button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInstall_Click( object sender, EventArgs e )
        {
            UpdateOrInstall( isUpdate: false );
        }

        /// <summary>
        /// Handles the clicking of the Update button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            UpdateOrInstall( isUpdate: true);
        }

        /// <summary>
        /// Wraps the install or update process in some guarded code while putting the app in "offline"
        /// mode and then back "online" when it's complete.
        /// </summary>
        /// <param name="isUpdate"></param>
        private void UpdateOrInstall( bool isUpdate)
        {
            WriteAppOffline();
            try
            {
                if ( ! isUpdate )
                {
                    InstallFirstRockPackage();
                    btnInstall.CssClass = "btn btn-primary disabled";
                    btnInstall.Text = "Installed";
                }
                else
                {
                    UpdateRockPackage();
                    btnUpdate.CssClass = "btn btn-primary disabled";
                    btnUpdate.Text = "Installed";
                }
                badge.Visible = false;
                litRockVersion.Text = "";
            }
            catch ( Exception ex )
            {
                // TODO Log the error and do something smart
                nbErrors.Visible = true;
                nbErrors.Text = string.Format( "Something went wrong.  Although the errors were written to the error log, here they are for your perusal:<br/>{0}", ex.Message );
            }
            RemoveAppOffline();
        }

        #endregion

        /// <summary>
        /// Installs the first RockPackage.
        /// </summary>
        protected void InstallFirstRockPackage()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();
            var package = NuGetService.SourceRepository.FindPackage( rockPackageId );

            try
            {
                if ( package != null )
                {
                    errors = NuGetService.InstallPackage( package );
                    nbSuccess.Text = ConvertCRLFtoBR( System.Web.HttpUtility.HtmlEncode( package.ReleaseNotes ) );
                    nbSuccess.Text += "<p><b>NOTE:</b> Any database changes will take effect at the next page load.</p>";
                }
            }
            catch ( InvalidOperationException ex )
            {
                errors.Concat( new[] { string.Format( "There is a problem with {0}: {1}", System.Web.HttpUtility.HtmlEncode( package.Title ), ex.Message ) } );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                nbErrors.Visible = true;
                nbErrors.Text = errors.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
            }
            else
            {
                nbSuccess.Visible = true;
            }
        }

        /// <summary>
        /// Updates an existing RockPackage.
        /// </summary>
        protected void UpdateRockPackage()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();
            var installed = NuGetService.GetInstalledPackage( rockPackageId );

            try
            {
                var update = NuGetService.GetUpdate( installed );
                errors = NuGetService.UpdatePackage( update );
                nbSuccess.Text = ConvertCRLFtoBR( System.Web.HttpUtility.HtmlEncode( update.ReleaseNotes ) );
                nbSuccess.Text += "<p><b>NOTE:</b> Any database changes will take effect at the next page load.</p>";
            }
            catch ( InvalidOperationException ex )
            {
                errors.Concat( new[] { string.Format( "There is a problem with {0}: {1}", System.Web.HttpUtility.HtmlEncode( installed.Title ), ex.Message ) } );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                nbErrors.Visible = true;
                nbErrors.Text = errors.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
            }
            else
            {
                nbSuccess.Visible = true;
            }
        }

        /// <summary>
        /// Fetches and displays the official Rock product version.
        /// </summary>
        protected void DisplayRockVersion()
        {
            litRockVersion.Text = string.Format( "<b>Current Version: </b> {0}", VersionInfo.GetRockProductVersion() );
        }

        /// <summary>
        /// Checks to see if any updates are availble for the installed or not-yet-installed package.
        /// </summary>
        protected void CheckForUpdate()
        {
            try
            {
                var package = NuGetService.GetInstalledPackage( rockPackageId );

                // The package is not yet installed at all...
                if ( package == null )
                {
                    package = NuGetService.GetRemotePackage( rockPackageId );
                    if ( package != null )
                    {
                        btnInstall.Visible = true;
                        divPackage.Visible = true;
                        badge.Visible = true;
                        litPackageTitle.Text = System.Web.HttpUtility.HtmlEncode( package.Title );
                        litPackageDescription.Text = System.Web.HttpUtility.HtmlEncode( ( package.Description != null ) ? package.Description : package.Summary ) ; 
                        btnInstall.Text += string.Format( " to version {0}", package.Version );
                        litReleaseNotes.Text = ConvertCRLFtoBR( System.Web.HttpUtility.HtmlEncode( package.ReleaseNotes ) );
                    }
                }
                else
                {
                    Boolean isPackageInstalled = NuGetService.IsPackageInstalled( package, anyVersion: true );

                    // Checking "IsLatestVersion" does not work because of what's discussed here:
                    // http://nuget.codeplex.com/discussions/279837
                    // if ( !installedPackage.IsLatestVersion )...
                    var latestPackage = NuGetService.GetUpdate( package );
                    if ( latestPackage != null )
                    {
                        btnUpdate.Visible = true;
                        divPackage.Visible = true;
                        badge.Visible = true;
                        litPackageTitle.Text = System.Web.HttpUtility.HtmlEncode( latestPackage.Title );
                        litPackageDescription.Text = System.Web.HttpUtility.HtmlEncode( ( latestPackage.Description != null ) ? latestPackage.Description : latestPackage.Summary ); 
                        btnUpdate.Text += string.Format( " to version {0}", latestPackage.Version );
                        litReleaseNotes.Text = ConvertCRLFtoBR( System.Web.HttpUtility.HtmlEncode( latestPackage.ReleaseNotes ) );
                    }
                }
            }
            catch ( InvalidOperationException ex )
            {
                litMessage.Text = string.Format( "<div class='alert alert-error'>There is a problem with the packaging system. {0}</p>", ex.Message );
            }
        }

        /// <summary>
        /// Converts CR (Carriage Return) LF (Line Feed) to HTML breaks (br).
        /// </summary>
        /// <param name="somestring">a string that contains CR LF</param>
        /// <returns>the string with CRLF replaced with BR</returns>
        private string ConvertCRLFtoBR( string somestring )
        {
            if ( somestring == null )
            {
                return "";
            }

            string x = somestring.Replace( Environment.NewLine, "<br/>" );
            x = x.Replace( "\x0A", "<br/>" );
            return x;
        }

        /// <summary>
        /// Removes the app_offline.htm file so the app can be used again.
        /// </summary>
        private void RemoveAppOffline()
        {
            var root = this.Request.PhysicalApplicationPath;
            var file = System.IO.Path.Combine( root, "app_offline.htm" );
            System.IO.File.Delete( file );
        }

        /// <summary>
        /// Simply creates an app_offline.htm file so no one else can hit the app.
        /// </summary>
        private void WriteAppOffline()
        {
            var root = this.Request.PhysicalApplicationPath;
            var file = System.IO.Path.Combine( root, "app_offline.htm" );

            System.IO.File.WriteAllText( file, @"
<html>
    <head>
    <title>Application Updating...</title>
    </head>
    <body>
        <h1>One Moment Please</h1>
        This application is undergoing an essential update and is temporarily offline.  Please give me a minute or two to wrap things up.
    </body>
</html>
" );
        }
    }
}