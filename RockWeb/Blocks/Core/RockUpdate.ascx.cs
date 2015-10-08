// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using NuGet;
using RestSharp;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Services.NuGet;
using Rock.VersionInfo;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "RockUpdate" )]
    [Category( "Core" )]
    [Description( "Handles checking for and performing upgrades to the Rock system." )]
    public partial class RockUpdate : Rock.Web.UI.RockBlock
    {
        #region Fields

        WebProjectManager nuGetService = null;
        private string _rockPackageId = "Rock";
        IEnumerable<IPackage> _availablePackages = null;
        SemanticVersion _installedVersion = new SemanticVersion( "0.0.0" );
        private int _numberOfAvailablePackages = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Obtains a WebProjectManager from the Global "UpdateServerUrl" Attribute.
        /// </summary>
        /// <value>
        /// The NuGet service or null if no valid service could be found using the UpdateServerUrl.
        /// </value>
        protected WebProjectManager NuGetService
        {
            get
            {
                if ( nuGetService == null )
                {
                    var globalAttributesCache = GlobalAttributesCache.Read();
                    string packageSource = globalAttributesCache.GetValue( "UpdateServerUrl" );
                    if ( packageSource.ToLowerInvariant().Contains( "rockalpha" ) || packageSource.ToLowerInvariant().Contains( "rockbeta" ) )
                    {
                        nbRepoWarning.Visible = true;
                    }

                    // Since you can use a URL or a local path, we can't just check for valid URI
                    try
                    {
                        string siteRoot = Request.MapPath( "~/" );
                        nuGetService = new WebProjectManager( packageSource, siteRoot );
                    }
                    catch
                    {
                        // if caught, we will return a null nuGetService
                    }
                }
                return nuGetService;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('#btn-restart').click(function () {
        var btn = $(this);
        btn.button('loading');
        location = location.href;
    });
";
            ScriptManager.RegisterStartupScript( pnlUpdateSuccess, pnlUpdateSuccess.GetType(), "restart-script", script, true );
        }

        /// <summary>
        /// Invoked on page load.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // Set timeout for up to 15 minutes (just like installer)
            Server.ScriptTimeout = 900;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 900;

            DisplayRockVersion();
            if ( !IsPostBack )
            {
                if ( NuGetService == null )
                {
                    pnlNoUpdates.Visible = false;
                    pnlError.Visible = true;
                    nbErrors.Text = string.Format( "Your UpdateServerUrl is not valid. It is currently set to: {0}", GlobalAttributesCache.Read().GetValue( "UpdateServerUrl" ) );
                }
                else
                {
                    try
                    {
                        _availablePackages = NuGetService.SourceRepository.FindPackagesById( _rockPackageId ).OrderByDescending( p => p.Version );
                        if ( IsUpdateAvailable() )
                        {
                            pnlUpdatesAvailable.Visible = true;
                            pnlNoUpdates.Visible = false;
                            cbIncludeStats.Visible = true;
                            BindGrid();
                        }

                        RemoveOldRDeleteFiles();
                    }
                    catch ( System.InvalidOperationException ex )
                    {
                        HandleNuGetException( ex );
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Bind the available packages to the repeater.
        /// </summary>
        private void BindGrid()
        {
            rptPackageVersions.DataSource = _availablePackages;
            rptPackageVersions.DataBind();
        }
        
        /// <summary>
        /// Wraps the install or update process in some guarded code while putting the app in "offline"
        /// mode and then back "online" when it's complete.
        /// </summary>
        /// <param name="version">the semantic version number</param>
        private void Update( string version )
        {
            WriteAppOffline();
            try
            {
                pnlUpdatesAvailable.Visible = false;

                if ( ! UpdateRockPackage( version ) )
                {
                    pnlError.Visible = true;
                    pnlUpdateSuccess.Visible = false;
                }
                else
                {
                    SendStatictics( version );
                }

                lRockVersion.Text = "";
            }
            catch ( Exception ex )
            {
                pnlError.Visible = true;
                pnlUpdateSuccess.Visible = false; 
                nbErrors.Text = string.Format( "Something went wrong.  Although the errors were written to the error log, they are listed for your review:<br/>{0}", ex.Message );
                LogException( ex );
            }
            RemoveAppOffline();
        }

        /// <summary>
        /// Enables and sets the appropriate CSS class on the install buttons and each div panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPackageVersions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                IPackage package = e.Item.DataItem as IPackage;
                if ( package != null )
                {
                    Boolean isExactPackageInstalled = NuGetService.IsPackageInstalled( package );
                    LinkButton lbInstall = e.Item.FindControl( "lbInstall" ) as LinkButton;
                    var divPanel = e.Item.FindControl( "divPanel" ) as HtmlGenericControl;
                    // Only the last item in the list is the primary
                    if ( e.Item.ItemIndex == _numberOfAvailablePackages - 1 )
                    {
                        lbInstall.Enabled = true;
                        lbInstall.AddCssClass( "btn-info" );
                        divPanel.AddCssClass( "panel-info" );
                    }
                    else
                    {
                        lbInstall.Enabled = false;
                        lbInstall.Text = "Pending";
                        lbInstall.AddCssClass( "btn-default" );
                        divPanel.AddCssClass( "panel-block" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the particular version button that was clicked.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPackageVersions_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string version = e.CommandArgument.ToString();
            Update( version );
        }

        #endregion

        #region Methods
        /// <summary>
        /// Updates an existing Rock package to the given version and returns true if successful.
        /// </summary>
        /// <returns>true if the update was successful; false if errors were encountered</returns>
        protected bool UpdateRockPackage( string version )
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            try
            {
                var update = NuGetService.SourceRepository.FindPackage( _rockPackageId, ( version != null ) ? SemanticVersion.Parse( version ) : null, false, false );
                var installed = NuGetService.GetInstalledPackage( _rockPackageId );
                
                if ( installed == null )
                {
                    errors = NuGetService.InstallPackage( update );
                }
                else
                {
                    errors = NuGetService.UpdatePackageAndBackup( update, installed );
                }

                CheckForManualFileMoves( version );

                nbSuccess.Text = ConvertToHtmlLiWrappedUl( update.ReleaseNotes ).ConvertCrLfToHtmlBr();
                lSuccessVersion.Text = update.Title;

                // Record the current version to the database
                Rock.Web.SystemSettings.SetValue( SystemSettingKeys.ROCK_INSTANCE_ID, version );

                // register any new REST controllers
                try
                {
                    RestControllerService.RegisterControllers();
                }
                catch ( Exception ex )
                {
                    LogException( ex );
                }
            }
            catch ( OutOfMemoryException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. It looks like your website ran out of memory. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#outofmemory'>this page for some assistance</a>", version ) } );
                LogException( ex );
            }
            catch( System.Xml.XmlException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. It looks one of the standard XML files ({1}) may have been customized which prevented us from updating it. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#customizedxml'>this page for some assistance</a>", version, ex.Message ) } );
                LogException( ex );
            }
            catch ( System.IO.IOException ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}. We were not able to replace an important file ({1}) after the update. Check out <a href='http://www.rockrms.com/Rock/UpdateIssues#unabletoreplacefile'>this page for some assistance</a>", version, ex.Message ) } );
                LogException( ex );
            }
            catch ( Exception ex )
            {
                errors = errors.Concat( new[] { string.Format( "There is a problem installing v{0}: {1}", version, ex.Message ) } );
                LogException( ex );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                pnlError.Visible = true;
                nbErrors.Text = errors.Aggregate( new StringBuilder( "<ul class='list-padded'>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
                return false;
            }
            else
            {
                pnlUpdateSuccess.Visible = true;
                rptPackageVersions.Visible = false;
                return true;
            }
        }

        /// <summary>
        /// Fetches and displays the official Rock product version.
        /// </summary>
        protected void DisplayRockVersion()
        {
            lRockVersion.Text = string.Format( "<b>Current Version: </b> {0}", VersionInfo.GetRockProductVersionFullName() );
            lNoUpdateVersion.Text = VersionInfo.GetRockProductVersionFullName();
        }
        
        /// <summary>
        /// Determines if there is an update available to install and
        /// puts the valid ones (that is those that meet the requirements)
        /// into the _availablePackages list.
        /// </summary>
        /// <returns>true if updates are available; false otherwise</returns>
        private bool IsUpdateAvailable()
        {
            List<IPackage> verifiedPackages = new List<IPackage>();
            try
            {
                // Get the installed package so we can check its version...
                var installedPackage = NuGetService.GetInstalledPackage( _rockPackageId );
                if ( installedPackage != null )
                {
                    _installedVersion = installedPackage.Version;
                }

                // Now go though all versions to find the newest, installable package
                // taking into consideration that a package may require that an earlier package
                // must already be installed -- in which case *that* package would be the
                // newest, most installable one.
                foreach ( IPackage package in _availablePackages )
                {
                    if ( package.Version <= _installedVersion )
                        break;

                    verifiedPackages.Add( package );

                    //if ( package.Tags != null && package.Tags.Contains( "requires-" ) )
                    //{
                    //    var requiredVersion = ExtractRequiredVersionFromTags( package );
                    //    // if that required version is greater than our currently installed version
                    //    // then we can't have any of the prior packages in the verifiedPackages list
                    //    // so we clear it out and keep processing.
                    //    if ( requiredVersion > _installedVersion )
                    //    {
                            
                    //        verifiedPackages.Clear();
                    //    }
                    //}
                }

                _availablePackages = verifiedPackages;
                _numberOfAvailablePackages = verifiedPackages.Count;
                if ( _numberOfAvailablePackages > 1 )
                {
                    nbMoreUpdatesAvailable.Visible = true;
                }
            }
            catch ( InvalidOperationException ex )
            {
                pnlNoUpdates.Visible = false;
                pnlError.Visible = true;
                lMessage.Text = string.Format( "<div class='alert alert-danger'>There is a problem with the packaging system. {0}</p>", ex.Message );
            }

            if (verifiedPackages.Count > 0 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the required SemanticVersion from the package's tags.
        /// </summary>
        /// <param name="package">a Rock nuget package</param>
        /// <returns>the SemanticVersion of the package that this particular package requires</returns>
        protected SemanticVersion ExtractRequiredVersionFromTags( IPackage package )
        {
            Regex regex = new Regex( @"requires-([\.\d]+)" );
            Match match = regex.Match( package.Tags );
            if ( match.Success )
            {
                return new SemanticVersion( match.Groups[1].Value );
            }
            else
            {
                throw new ArgumentException( string.Format( "There is a malformed 'requires-' tag in a Rock package ({0})", package.Version ) );
            }
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
        /// Copies the app_offline-template.htm file to app_offline.htm so no one else can hit the app.
        /// If the template file does not exist an app_offline.htm file will be created from scratch.
        /// </summary>
        private void WriteAppOffline()
        {
            var root = this.Request.PhysicalApplicationPath;

            var templateFile = System.IO.Path.Combine( root, "app_offline-template.htm" );
            var offlineFile = System.IO.Path.Combine( root, "app_offline.htm" );

            try
            {
                if ( File.Exists( templateFile ) )
                {
                    System.IO.File.Copy( templateFile, offlineFile, overwrite: true );
                }
                else
                {
                    CreateOfflineFileFromScratch( offlineFile );
                }
            }
            catch ( Exception )
            {
                if ( ! File.Exists( offlineFile ) )
                {
                    CreateOfflineFileFromScratch( offlineFile );
                }
            }
        }

        /// <summary>
        /// Simply creates an app_offline.htm file so no one else can hit the app.
        /// </summary>
        private void CreateOfflineFileFromScratch( string offlineFile )
        {
            System.IO.File.WriteAllText( offlineFile, @"
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

        /// <summary>
        /// Removes the old *.rdelete (Rock delete) files that were created during an update.
        /// </summary>
        private void RemoveOldRDeleteFiles()
        {
            var rockDirectory = new DirectoryInfo( Server.MapPath( "~" ) );

            foreach ( var file in rockDirectory.EnumerateFiles( "*.rdelete", SearchOption.AllDirectories ) )
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    //we'll try again later
                }
            }
        }

        private void CheckForManualFileMoves( string version )
        {
            var versionDirectory = new DirectoryInfo( Server.MapPath( "~/App_Data/" + version ) );
            if ( versionDirectory.Exists )
            {
                foreach ( var file in versionDirectory.EnumerateFiles( "*", SearchOption.AllDirectories ) )
                {
                    ManuallyMoveFile( file, file.FullName.Replace( @"\App_Data\" + version, "" ) );
                }

                versionDirectory.Delete( true );
            }
        }

        private void ManuallyMoveFile( FileInfo file, string newPath )
        {
            if ( newPath.EndsWith( ".dll" ) && !newPath.Contains( @"\bin\" ) )
            {
                int fileCount = 0;
                if ( File.Exists( newPath ) )
                {
                    // generate a unique *.#.rdelete filename
                    do
                    {
                        fileCount++;
                    }
                    while ( File.Exists( string.Format( "{0}.{1}.rdelete", newPath, fileCount ) ) );

                    string fileToDelete = string.Format( "{0}.{1}.rdelete", newPath, fileCount );
                    File.Move( newPath, fileToDelete );
                }
            }

            file.CopyTo( newPath, true );
        }

        /// <summary>
        /// Converts + and * to html line items (li) wrapped in unordered lists (ul).
        /// </summary>
        /// <param name="str">a string that contains lines that start with + or *</param>
        /// <returns>an html string of <code>li</code> wrapped in <code>ul</code></returns>
        public string ConvertToHtmlLiWrappedUl( string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            bool foundMatch = false;

            // Lines that start with  "+ *" or "+" or "*"
            var re = new System.Text.RegularExpressions.Regex( @"^\s*(\+ \* |[\+\*]+)(.*)" );
            var htmlBuilder = new StringBuilder();

            // split the string on newlines...
            string[] splits = str.Split( new[] { Environment.NewLine, "\x0A" }, StringSplitOptions.RemoveEmptyEntries );
            // look at each line to see if it starts with a + or * and then strip it and wrap it in <li></li>
            for ( int i = 0; i < splits.Length; i++ )
            {
                var match = re.Match( splits[i] );
                if ( match.Success )
                {
                    foundMatch = true;
                    htmlBuilder.AppendFormat( "<li>{0}</li>", match.Groups[2] );
                }
                else
                {
                    htmlBuilder.Append( splits[i] );
                }
            }

            // if we had a match then wrap it in <ul></ul> markup
            return foundMatch ? string.Format( "<ul class='list-padded'>{0}</ul>", htmlBuilder.ToString() ) : htmlBuilder.ToString();
        }

        /// <summary>
        /// Sends statistics to the SDN server but only if there are more than 100 person records
        /// or the sample data has not been loaded. 
        /// 
        /// The statistics are:
        ///     * Rock Instance Id
        ///     * Update Version
        ///     * IP Address - The IP address of your Rock server.
        ///     
        /// ...and we only send these if they checked the "Include Impact Statistics":
        ///     * Organization Name and Address
        ///     * Public Web Address
        ///     * Number of Active Records
        ///     
        /// As per http://www.rockrms.com/Rock/Impact
        /// </summary>
        /// <param name="version">the semantic version number</param>
        private void SendStatictics( string version )
        {
            try
            {
                var rockContext = new RockContext();
                int numberOfActiveRecords = new PersonService( rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();

                if ( numberOfActiveRecords > 100 || !Rock.Web.SystemSettings.GetValue( SystemSettingKeys.SAMPLEDATA_DATE ).AsDateTime().HasValue )
                { 
                    string organizationName = string.Empty;
                    ImpactLocation organizationLocation = null;
                    string publicUrl = string.Empty;

                    var rockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
                    var ipAddress = Request.ServerVariables["LOCAL_ADDR"];

                    if ( cbIncludeStats.Checked )
                    {
                        var globalAttributes = GlobalAttributesCache.Read();
                        organizationName = globalAttributes.GetValue( "OrganizationName" );
                        publicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );

                        // Fetch the organization address
                        var organizationAddressLocationGuid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid();
                        if ( !organizationAddressLocationGuid.Equals( Guid.Empty ) )
                        {
                            var location = new Rock.Model.LocationService( rockContext ).Get( organizationAddressLocationGuid );
                            if ( location != null )
                            {
                                organizationLocation = new ImpactLocation( location );
                            }
                        }
                    }
                    else
                    {
                        numberOfActiveRecords = 0;
                    }

                    var environmentData = GetEnvDataAsJson();

                    // now send them to SDN/Rock server
                    SendToSpark( rockInstanceId, version, ipAddress, publicUrl, organizationName, organizationLocation, numberOfActiveRecords, environmentData );
                }
            }
            catch ( Exception ex )
            {
                // Just catch any exceptions, log it, and keep moving... We don't want to mess up the experience
                // over a few statistics/metrics.
                try
                {
                    LogException( ex );
                }
                catch { }
            }
        }

        /// <summary>
        /// Returns the environment data as json.
        /// </summary>
        /// <returns>a JSON formatted string</returns>
        private string GetEnvDataAsJson()
        {
            string sqlVersion = "";
            try
            {
                sqlVersion = Rock.Data.DbService.ExecuteScaler( "SELECT SERVERPROPERTY('productversion')" ).ToString();
            }
            catch
            {
                // oh well, sorry, I have to move on...
            }

            EnvData envData = new EnvData()
            {
                AppRoot = ResolveRockUrl( "~/" ),
                Architecture = (IntPtr.Size == 4) ? "32bit" : "64bit",
                AspNetVersion = Environment.Version.ToString(),
                IisVersion = Request.ServerVariables["SERVER_SOFTWARE"],
                //Ram = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory,
                ServerOs = Environment.OSVersion.ToString(),
                SqlVersion = sqlVersion
            };

            return envData.ToJson();
        }

        /// <summary>
        /// Sends the public data and impact statistics to the Rock server.
        /// </summary>
        /// <param name="rockInstanceId">The rock instance identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="publicUrl">The public URL.</param>
        /// <param name="organizationName">Name of the organization.</param>
        /// <param name="organizationLocation">The organization location.</param>
        /// <param name="numberOfActiveRecords">The number of active records.</param>
        /// <param name="environmentData">The environment data (JSON).</param>
        private void SendToSpark( Guid rockInstanceId, string version, string ipAddress, string publicUrl, string organizationName, ImpactLocation organizationLocation, int numberOfActiveRecords, string environmentData )
        {
            ImpactStatistic impactStatistic = new ImpactStatistic()
            {
                RockInstanceId = rockInstanceId,
                Version = version,
                IpAddress = ipAddress,
                PublicUrl = publicUrl,
                OrganizationName = organizationName,
                OrganizationLocation = organizationLocation,
                NumberOfActiveRecords = numberOfActiveRecords,
                EnvironmentData = environmentData
            };

            var client = new RestClient( "http://www.rockrms.com/api/impacts/save" );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddBody( impactStatistic );
            var response = client.Execute( request );
        }

        /// <summary>
        /// Sets up the page to report the error in a nicer manner.
        /// </summary>
        /// <param name="ex"></param>
        private void HandleNuGetException( Exception ex )
        {
            pnlError.Visible = true;
            pnlUpdateSuccess.Visible = false;
            pnlNoUpdates.Visible = false;

            if ( ex.Message.Contains( "404" ) )
            {
                nbErrors.Text = string.Format( "It appears that someone configured your <code>UpdateServerUrl</code> setting incorrectly: {0}", GlobalAttributesCache.Read().GetValue( "UpdateServerUrl" ) );
            }
            else if ( ex.Message.Contains( "could not be resolved" ) )
            {
                nbErrors.Text = string.Format( "I think either the update server is down or your <code>UpdateServerUrl</code> setting is incorrect: {0}", GlobalAttributesCache.Read().GetValue( "UpdateServerUrl" ) );
            }
            else if ( ex.Message.Contains( "Unable to connect" ) )
            {
                nbErrors.Text = "The update server is down. Try again later.";
            }
            else
            {
                nbErrors.Text = string.Format( "...actually, I'm not sure what happened here: {0}", ex.Message );
            }
        }
        #endregion
}

    [Serializable]
    public class EnvData
    {
        public string AppRoot { get; set; }
        public string Architecture { get; set; }
        public string AspNetVersion { get; set; }
        public string IisVersion { get; set; }
        public string Ram { get; set; }
        public string ServerOs { get; set; }
        public string SqlVersion { get; set; }
    }

    [Serializable]
    public class ImpactStatistic
    {
        public Guid RockInstanceId { get; set; }
        public string Version { get; set; }
        public string IpAddress { get; set; }
        public string PublicUrl { get; set; }
        public string OrganizationName { get; set; }
        public ImpactLocation OrganizationLocation { get; set; }
        public int NumberOfActiveRecords { get; set; }
        public string EnvironmentData { get; set; }
    }

    [Serializable]
    public class ImpactLocation
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public ImpactLocation( Rock.Model.Location location )
        {
            Street1 = location.Street1;
            Street2 = location.Street2;
            City = location.City;
            State = location.State;
            PostalCode = location.PostalCode;
            Country = location.Country;
        }
    }

}