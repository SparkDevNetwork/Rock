//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Rock.Services.NuGet;
using Rock.Web.Cache;
using NuGet;

namespace RockWeb.Blocks.Administration
{
    public partial class RockUpdate : Rock.Web.UI.RockBlock
    {
        WebProjectManager nuGetService = null;
        private string rockPackageId = "RockUpdate";
        private string rockCoreVersionAssembly = "Rock.dll"; 

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
        protected void Page_Load( object sender, EventArgs e )
        {
            DisplayRockVersion();
            if ( !IsPostBack )
            {
                CheckForUpdate();
            }
        }

        protected void btnInstall_Click( object sender, EventArgs e )
        {
            WriteAppOffline();
            try
            {
                InstallFirstRockPackage();
                btnInstall.CssClass = "btn btn-primary disabled";
                btnInstall.Text = "Installed";
                badge.Visible = false;
                litRockVersion.Text = "";
            }
            catch ( Exception ex )
            {
                nbErrors.Visible = true;
                nbErrors.Text = string.Format( "Something went wrong.  Although the errors were written to the error log, here they are for your perusal:<br/>{0}", ex.Message );
            }
            RemoveAppOffline();
        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            WriteAppOffline();
            try
            {
                UpdateRockPackage();
                btnUpdate.CssClass = "btn btn-primary disabled";
                btnUpdate.Text = "Installed";
                badge.Visible = false;
                litRockVersion.Text = "";
            }
            catch ( Exception ex )
            {
                // TODO do something smart...and minimally tell the admin the update failed.
                nbErrors.Visible = true;
                nbErrors.Text = string.Format( "Something went wrong.  Although the errors were written to the error log, here they are for your perusal:<br/>{0}", ex.Message );
            }
            RemoveAppOffline();
        }

        #endregion

        protected void DisplayRockVersion()
        {
            Assembly rockDLL = Assembly.LoadFrom( System.IO.Path.Combine( this.Request.PhysicalApplicationPath, "bin", rockCoreVersionAssembly ) );
            FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockDLL.Location );
            litRockVersion.Text = string.Format( "<b>Current Rock Version: </b> {0}", rockDLLfvi.ProductVersion );
        }

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
                        litPackageTitle.Text = package.Title;
                        litPackageDescription.Text = ( package.Description != null ) ? package.Description : package.Summary ; 
                        btnInstall.Text += string.Format( " to version {0}", package.Version );
                        litReleaseNotes.Text = ConvertCRLFtoBR( package.ReleaseNotes );
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
                        litPackageTitle.Text = latestPackage.Title;
                        litPackageDescription.Text = ( latestPackage.Description != null ) ? latestPackage.Description : latestPackage.Summary; 
                        btnUpdate.Text += string.Format( " to version {0}", latestPackage.Version );
                        litReleaseNotes.Text = ConvertCRLFtoBR( latestPackage.ReleaseNotes );
                    }
                }
            }
            catch ( InvalidOperationException ex )
            {
                litMessage.Text = string.Format( "<div class='alert alert-error'>There is a problem with the packaging system. {0}</p>", ex.Message );
            }
        }
   
        protected void InstallFirstRockPackage()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();
            var package = NuGetService.SourceRepository.FindPackage( rockPackageId );

            try
            {
                if ( package != null )
                {
                    errors = NuGetService.InstallPackage( package );
                    nbSuccess.Text = ConvertCRLFtoBR( package.ReleaseNotes );
                    //IEnumerable<IPackage> packagesRequiringLicenseAcceptance = NuGetService.GetPackagesRequiringLicenseAcceptance(package);
                }
            }
            catch ( InvalidOperationException ex )
            {
                errors.Concat( new[] { string.Format( "There is a problem with {0}: {1}", package.Title, ex.Message ) } );
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
        
        protected void UpdateRockPackage()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            var installed = NuGetService.GetInstalledPackage( rockPackageId );

            try
            {
                var update = NuGetService.GetUpdate( installed );
                errors = NuGetService.UpdatePackage( update );
                nbSuccess.Text = ConvertCRLFtoBR( update.ReleaseNotes );
            }
            catch ( InvalidOperationException ex )
            {
                errors.Concat( new[] { string.Format( "There is a problem with {0}: {1}", installed.Title, ex.Message ) } );
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

        private string ConvertCRLFtoBR( string somestring )
        {
            if ( somestring == null )
            {
                return "";
            }

            string x = somestring.Replace( Environment.NewLine, "<br />" );
            x = x.Replace( "\x0A", "<br />" );
            return x;
        }

        #region OLD
        protected void btnUpdate_Click_ORIG( object sender, EventArgs e )
        {
            WriteAppOffline();
            try
            {
                System.Threading.Thread.Sleep( 8000 );

                StringBuilder sb = new StringBuilder();
                sb.Append( "<ol>" );

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo( assembly.Location );
                string version = fvi.FileVersion;

                sb.AppendFormat( "<li>GetExecutingAssembly file '{0}'</li>", fvi.FileName );
                sb.AppendFormat( "<li>GetExecutingAssembly assembly name file version '{0}'</li>", AssemblyName.GetAssemblyName( assembly.Location ).Version.ToString() );

                var root = this.Request.PhysicalApplicationPath;
                var sourceFile = System.IO.Path.Combine( root, "Rock.dll" );
                var destFile = System.IO.Path.Combine( root, "bin", "Rock.dll" );

                FileInfo destFileInfo = new FileInfo( destFile );
                FileInfo sourceFileInfo = new FileInfo( sourceFile );

                Assembly rockDLL = Assembly.LoadFrom( destFile );
                FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockDLL.Location );
                sb.AppendFormat( "<li>Rock.DLL ({0}) FileVersion: {1}</li>", rockDLL.Location, rockDLLfvi.FileVersion );


                sb.AppendFormat( "<li>destination file ({0}) 'last write' time: {1}</li>", destFile, destFileInfo.LastWriteTime.ToString() );
                sb.AppendFormat( "<li>source file ({0}) 'last write' time: {1}</li>", sourceFile, sourceFileInfo.LastWriteTime.ToString() );
                sb.AppendFormat( "<li>copying source file '{0}' to destination file '{1}'</li>", sourceFile, destFile );

                System.IO.File.Copy( sourceFile, destFile, true );

                destFileInfo = new FileInfo( destFile );

                sb.AppendFormat( "<li>current destination file ({0}) 'last write' time: {1}</li>", destFile, destFileInfo.LastWriteTime.ToString() );

                rockDLL = Assembly.LoadFrom( destFile );
                rockDLLfvi = FileVersionInfo.GetVersionInfo( rockDLL.Location );
                sb.AppendFormat( "<li>Rock.DLL ({0}) FileVersion: {1}</li>", rockDLL.Location, rockDLLfvi.FileVersion );

                sb.Append( "</ol>" );
                litMessage.Text = sb.ToString();

                DisplayRockVersion();
            }
            catch ( Exception )
            {
                // TODO do something smart...and minimally tell the admin the update failed.
            }

            RemoveAppOffline();
        }

        #endregion

        private void RemoveAppOffline()
        {
            var root = this.Request.PhysicalApplicationPath;
            var file = System.IO.Path.Combine( root, "app_offline.htm" );
            System.IO.File.Delete( file );
        }

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