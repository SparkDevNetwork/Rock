// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using Rock.Utility;
using Rock.VersionInfo;
using System.Net;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Microsoft.Web.XmlTransform;
using Rock.Security;
using RestSharp;
using System.Web;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_bemaservices.ClientTools
{
    [DisplayName( "BEMA Package Installer" )]
    [Category( "BEMA Services > Client Tools" )]
    [Description( "Allows a client to download the latest BEMA Packages." )]
    [BooleanField( "Download BEMA Standard Packages", "Whether to check for and prompt the user to install the latest BEMA Standard Package", false, "", 0, BemaAttributeKey.DownloadStandardPackage )]
    [BooleanField( "Download BEMA Client Packages", "Whether to check for and prompt the user to install the latest BEMA Standard Package", false, "", 1, BemaAttributeKey.DownloadClientPackage )]
    [BooleanField( "Download BEMA Client Tool Updates", "Whether to check for and prompt the user to install updates to BEMA Client Tools", true, "", 2, BemaAttributeKey.DownloadClientToolUpdates )]
    [BooleanField( "Enable Support Widget", "Whether to show BEMA's support widget", true, "", 3, BemaAttributeKey.EnableSupportWidget )]
    [EncryptedTextField( "Client Key", "Your Client Key. Please contact BEMA to recieve a client key.", true, "", "", 4, BemaAttributeKey.ClientKey )]
    public partial class BemaPackageInstaller : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for custom BEMA Attributes
        /// </summary>
        protected static class BemaAttributeKey
        {
            public const string DownloadStandardPackage = "DownloadStandardPackage";
            public const string DownloadClientPackage = "DownloadClientPackage";
            public const string DownloadClientToolUpdates = "DownloadClientToolUpdates";
            public const string EnableSupportWidget = "EnableSupportWidget";
            public const string ClientKey = "ClientKey";
        }

        #endregion Attribute Keys

        #region Fields

        const string _xdtExtension = ".rock.xdt";

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );
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
                ShowDetail();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        protected void lbInstall_Click( object sender, EventArgs e )
        {
            string errorResponse = string.Empty;

            string clientFolder = GetClientFolder();
            if ( clientFolder.IsNotNullOrWhiteSpace() )
            {
                if ( GetAttributeValue( BemaAttributeKey.DownloadClientToolUpdates ).AsBoolean() )
                {
                    InstallPackage( clientFolder, BemaPackageType.ClientTools );
                }

                if ( GetAttributeValue( BemaAttributeKey.DownloadStandardPackage ).AsBoolean() )
                {
                    InstallPackage( clientFolder, BemaPackageType.StandardPackage );
                }

                if ( GetAttributeValue( BemaAttributeKey.DownloadClientPackage ).AsBoolean() )
                {
                    InstallPackage( clientFolder, BemaPackageType.ClientPackage );
                }
            }
        }

        private void InstallPackage( string clientFolder, BemaPackageType bemaPackageType )
        {
            UpdateFile installedVersion = GetInstalledFileVersion( bemaPackageType );
            UpdateFile latestVersion = GetLatestFileVersion( clientFolder, bemaPackageType );

            if ( !IsLatestVersionInstalled( installedVersion, latestVersion ) )
            {
                string appRoot = Server.MapPath( "~/" );
                string bemaCodePackageWorkingDir = "";
                switch ( bemaPackageType )
                {
                    case BemaPackageType.ClientTools:
                        bemaCodePackageWorkingDir = String.Format( "{0}App_Data/BemaClientPackage/ClientTools/", appRoot );
                        break;
                    case BemaPackageType.StandardPackage:
                        bemaCodePackageWorkingDir = String.Format( "{0}App_Data/BemaClientPackage/BEMA/", appRoot );
                        break;
                    case BemaPackageType.ClientPackage:
                        bemaCodePackageWorkingDir = String.Format( "{0}App_Data/BemaClientPackage/{1}/", appRoot, clientFolder );
                        break;
                }

                string sourceFile = latestVersion.FullPath;
                string destinationFile = string.Format( "{0}/{1}", bemaCodePackageWorkingDir, latestVersion.FileName );

                // check that the BemaCodePackage directory exists
                if ( !Directory.Exists( bemaCodePackageWorkingDir ) )
                {
                    Directory.CreateDirectory( bemaCodePackageWorkingDir );
                }

                // download file
                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile( sourceFile, destinationFile );
                }
                catch ( Exception ex )
                {
                    CleanUpPackage( destinationFile );
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Downloading Package</strong> An error occurred while downloading the package. Please try again later. <br><em>Error: {0}</em></div>", ex.Message );
                    return;
                }

                // process zip folder
                try
                {
                    using ( ZipArchive packageZip = ZipFile.OpenRead( destinationFile ) )
                    {
                        // unzip content folder and process xdts
                        foreach ( ZipArchiveEntry entry in packageZip.Entries.Where( e1 => e1.FullName.StartsWith( "content/", StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            if ( entry.FullName.EndsWith( _xdtExtension, StringComparison.OrdinalIgnoreCase ) )
                            {
                                // process xdt
                                string filename = entry.FullName.Replace( "content/", "" );
                                string transformTargetFile = appRoot + filename.Substring( 0, filename.LastIndexOf( _xdtExtension ) );

                                // process transform
                                using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                                {
                                    document.PreserveWhitespace = true;
                                    document.Load( transformTargetFile );

                                    using ( XmlTransformation transform = new XmlTransformation( entry.Open(), null ) )
                                    {
                                        if ( transform.Apply( document ) )
                                        {
                                            document.Save( transformTargetFile );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // process all content files
                                string fullpath = Path.Combine( appRoot, entry.FullName.Replace( "content/", "" ) );
                                string directory = Path.GetDirectoryName( fullpath ).Replace( "content/", "" );

                                // if entry is a directory ignore it
                                if ( entry.Length != 0 )
                                {
                                    if ( !Directory.Exists( directory ) )
                                    {
                                        Directory.CreateDirectory( directory );
                                    }

                                    entry.ExtractToFile( fullpath, true );
                                }

                            }
                        }

                        // process install.sql
                        try
                        {
                            var sqlInstallEntry = packageZip.Entries.Where( e2 => e2.FullName == "install/run.sql" ).FirstOrDefault();
                            if ( sqlInstallEntry != null )
                            {
                                string sqlScript = System.Text.Encoding.Default.GetString( sqlInstallEntry.Open().ReadBytesToEnd() );

                                if ( !string.IsNullOrWhiteSpace( sqlScript ) )
                                {
                                    using ( var context = new RockContext() )
                                    {
                                        context.Database.ExecuteSqlCommand( sqlScript );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Updating Database</strong> An error occurred while updating the database. <br><em>Error: {0}</em></div>", ex.Message );
                            return;
                        }

                        // process deletefile.lst
                        try
                        {
                            var deleteListEntry = packageZip.Entries.Where( e3 => e3.FullName == "install/deletefile.lst" ).FirstOrDefault();
                            if ( deleteListEntry != null )
                            {

                                string deleteList = System.Text.Encoding.Default.GetString( deleteListEntry.Open().ReadBytesToEnd() );

                                string[] itemsToDelete = deleteList.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

                                foreach ( string deleteItem in itemsToDelete )
                                {
                                    if ( !string.IsNullOrWhiteSpace( deleteItem ) )
                                    {
                                        string deleteItemFullPath = appRoot + deleteItem;

                                        if ( Directory.Exists( deleteItemFullPath ) )
                                        {
                                            Directory.Delete( deleteItemFullPath, true );
                                        }

                                        if ( File.Exists( deleteItemFullPath ) )
                                        {
                                            File.Delete( deleteItemFullPath );
                                        }
                                    }
                                }

                            }
                        }
                        catch ( Exception ex )
                        {
                            lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Modifying Files</strong> An error occurred while modifying files. <br><em>Error: {0}</em></div>", ex.Message );
                            return;
                        }

                    }
                }
                catch ( Exception ex )
                {
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Extracting Package</strong> An error occurred while extracting the contents of the package. <br><em>Error: {0}</em></div>", ex.Message );
                    return;
                }

                // update package install json file
                UpdateInstalledVersion( latestVersion, bemaPackageType );

                // Clear all cached items
                RockCache.ClearAllCachedItems();

                // show result message
                lMessages.Text = "<div class='alert alert-success margin-t-md'><strong>Package Installed</strong>";
            }
        }

        #endregion

        #region Methods
        private void ShowDetail()
        {
            var isLatestClientInstalled = true;
            var isLatestStandardInstalled = true;
            var isLatestToolsInstalled = true;
            string clientFolder = GetClientFolder();
            if ( clientFolder.IsNotNullOrWhiteSpace() )
            {
                if ( GetAttributeValue( BemaAttributeKey.DownloadClientToolUpdates ).AsBoolean() )
                {
                    UpdateFile installedStandardVersion = GetInstalledFileVersion( BemaPackageType.ClientTools );
                    UpdateFile latestStandardVersion = GetLatestFileVersion( clientFolder, BemaPackageType.ClientTools );
                    isLatestToolsInstalled = IsLatestVersionInstalled( installedStandardVersion, latestStandardVersion );
                }

                if ( GetAttributeValue( BemaAttributeKey.DownloadStandardPackage ).AsBoolean() )
                {
                    UpdateFile installedStandardVersion = GetInstalledFileVersion( BemaPackageType.StandardPackage );
                    UpdateFile latestStandardVersion = GetLatestFileVersion( clientFolder, BemaPackageType.StandardPackage );
                    isLatestStandardInstalled = IsLatestVersionInstalled( installedStandardVersion, latestStandardVersion );
                }

                if ( GetAttributeValue( BemaAttributeKey.DownloadClientPackage ).AsBoolean() )
                {
                    UpdateFile installedClientVersion = GetInstalledFileVersion( BemaPackageType.ClientPackage );
                    UpdateFile latestClientVersion = GetLatestFileVersion( clientFolder, BemaPackageType.ClientPackage );
                    isLatestClientInstalled = IsLatestVersionInstalled( installedClientVersion, latestClientVersion );
                }
            }

            pnlView.Visible = ( !isLatestStandardInstalled || !isLatestClientInstalled || !isLatestToolsInstalled );

            if ( GetAttributeValue( BemaAttributeKey.EnableSupportWidget ).AsBoolean() )
            {
                var content = "{% include '~/Plugins/com_bemaservices/ClientTools/Assets/Lava/JiraEmbed.lava' %}";
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "CurrentPage", this.PageCache );

                if ( CurrentPerson != null )
                {
                    // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                    mergeFields.AddOrIgnore( "Person", CurrentPerson );
                }

                mergeFields.Add( "CurrentBrowser", this.RockPage.BrowserClient );

                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                mergeFields.Add( "CurrentPersonCanEdit", IsUserAuthorized( Rock.Security.Authorization.EDIT ) );
                mergeFields.Add( "CurrentPersonCanAdministrate", IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) );

                var html = content.ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
                lHtmlContent.Text = html;
            }
        }

        private string GetClientFolder()
        {
            var clientFolder = "";
            string clientKey = Encryption.DecryptString( GetAttributeValue( BemaAttributeKey.ClientKey ) );
            if ( clientKey.IsNotNullOrWhiteSpace() )
            {
                RockSemanticVersion rockVersion = RockSemanticVersion.Parse( VersionInfo.GetRockSemanticVersionNumber() );
                UpdateFile installedToolsVersion = GetInstalledFileVersion( BemaPackageType.ClientTools );
                UpdateFile installedStandardVersion = GetInstalledFileVersion( BemaPackageType.StandardPackage );
                UpdateFile installedClientVersion = GetInstalledFileVersion( BemaPackageType.ClientPackage );

                var url = string.Format( "Webhooks/Lava.ashx/GetClientPackageLocation/{0}/{1}/{2}/{3}/{4}", clientKey, rockVersion.ToString(), installedStandardVersion.ToString(), installedClientVersion.ToString(), installedToolsVersion.ToString() );
                var restClient = new RestClient( "https://rock.bemaservices.com/" );
                var request = new RestRequest( url );
                request.Method = Method.GET;
                request.RequestFormat = DataFormat.Json;
                var response = restClient.Execute( request );

                if ( response != null && response.StatusCode == HttpStatusCode.OK )
                {
                    if ( !string.IsNullOrEmpty( response.Content ) )
                    {
                        try
                        {
                            var apiResponse = JsonConvert.DeserializeObject<List<ClientFolder>>( response.Content );
                            clientFolder = apiResponse.First().ShortName;
                        }
                        catch ( Exception ex )
                        {
                            // Creating Exception is Exception Log
                            HttpContext context2 = HttpContext.Current;
                            ExceptionLogService.LogException( ex, context2 );

                        }
                    }
                }
            }

            return clientFolder;
        }

        private static UpdateFile GetInstalledFileVersion( BemaPackageType bemaPackageType )
        {
            var installedVersion = new UpdateFile();
            var packageVersion = "";

            var attributeGuid = string.Empty;

            switch ( bemaPackageType )
            {
                case BemaPackageType.ClientTools:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_TOOLS_VERSION_ATTRIBUTE_GUID;
                    break;
                case BemaPackageType.StandardPackage:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID;
                    break;
                case BemaPackageType.ClientPackage:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID;
                    break;
            }

            var attribute = AttributeCache.Get( attributeGuid );
            packageVersion = GlobalAttributesCache.Value( attribute.Key );

            var versionArray = packageVersion.Split( '.' ).AsIntegerList();
            if ( versionArray.Count() == 4 )
            {
                installedVersion = new UpdateFile( "", "", versionArray[0], versionArray[1], versionArray[2], versionArray[3] );
            }

            return installedVersion;
        }

        private static bool IsLatestVersionInstalled( UpdateFile installedVersion, UpdateFile latestVersion )
        {
            var isLatestVersionInstalled = true;
            if ( latestVersion == null )
            {
                isLatestVersionInstalled = true;
            }
            else
            {
                if ( installedVersion.Major < latestVersion.Major )
                {
                    isLatestVersionInstalled = false;
                }
                else
                {
                    if ( installedVersion.Minor < latestVersion.Minor )
                    {
                        isLatestVersionInstalled = false;
                    }
                    else
                    {
                        if ( installedVersion.Build < latestVersion.Build )
                        {
                            isLatestVersionInstalled = false;
                        }
                        else
                        {
                            if ( installedVersion.Revision < latestVersion.Revision )
                            {
                                isLatestVersionInstalled = false;
                            }
                            else
                            {
                                isLatestVersionInstalled = true;
                            }
                        }
                    }
                }
            }


            return isLatestVersionInstalled;
        }

        private UpdateFile GetLatestFileVersion( string clientFolder, BemaPackageType bemaPackageType )
        {
            RockSemanticVersion rockVersion = RockSemanticVersion.Parse( VersionInfo.GetRockSemanticVersionNumber() );
            var clientUrl = string.Format( "https://rockadmin.bemaservices.com/Content/ExternalSite/ClientPackages/{0}/", clientFolder );
            var baseUrl = "https://rockadmin.bemaservices.com/Content/ExternalSite/ClientPackages/BEMA/";
            var toolsUrl = "https://rockadmin.bemaservices.com/Content/ExternalSite/ClientPackages/ClientTools/";

            if ( clientFolder.IsNotNullOrWhiteSpace() )
            {
                var isValidClient = true;
                HttpWebRequest testRequest = ( HttpWebRequest ) WebRequest.Create( clientUrl );
                try
                {
                    HttpWebResponse testResponse = ( HttpWebResponse ) testRequest.GetResponse();
                }
                catch ( WebException ex )
                {
                    isValidClient = false;
                }

                if ( isValidClient )
                {
                    try
                    {
                        try
                        {
                            List<UpdateFile> updateFiles = new List<UpdateFile>();
                            List<string> fileNames = new List<string>();
                            var pluginFileAcronym = clientFolder;
                            var requestUrl = string.Empty;

                            switch ( bemaPackageType )
                            {
                                case BemaPackageType.ClientTools:
                                    requestUrl = toolsUrl;
                                    pluginFileAcronym = "ClientTools";
                                    break;
                                case BemaPackageType.StandardPackage:
                                    requestUrl = baseUrl;
                                    pluginFileAcronym = "BEMA";
                                    break;
                                case BemaPackageType.ClientPackage:
                                    requestUrl = clientUrl;
                                    pluginFileAcronym = clientFolder;
                                    break;
                            }

                            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( requestUrl );
                            using ( HttpWebResponse response = ( HttpWebResponse ) request.GetResponse() )
                            {
                                using ( StreamReader reader = new StreamReader( response.GetResponseStream() ) )
                                {
                                    string html = reader.ReadToEnd();
                                    AddRegexMatches( fileNames, html, String.Format( ".plugin\">(?<name>ClientPackage-{0}-v........plugin)</A>", pluginFileAcronym ) );
                                    AddRegexMatches( fileNames, html, String.Format( ".plugin\">(?<name>ClientPackage-{0}-v.........plugin)</A>", pluginFileAcronym ) );
                                    AddRegexMatches( fileNames, html, String.Format( ".plugin\">(?<name>ClientPackage-{0}-v..........plugin)</A>", pluginFileAcronym ) );
                                }
                            }

                            foreach ( var fileName in fileNames )
                            {
                                var fileEntry = requestUrl + fileName;
                                var versionNumber = fileName.Replace( String.Format( "ClientPackage-{0}-v", pluginFileAcronym ), "" ).Replace( ".plugin", "" );
                                var versionArray = versionNumber.Split( '.' ).AsIntegerList();

                                if ( versionArray.Count() == 4 )
                                {
                                    var updateFile = new UpdateFile( fileName, fileEntry, versionArray[0], versionArray[1], versionArray[2], versionArray[3] );
                                    var updateSemanticVersion = new RockSemanticVersion( updateFile.Major, updateFile.Minor, updateFile.Build );
                                    if ( updateSemanticVersion <= rockVersion )
                                    {
                                        updateFiles.Add( updateFile );
                                    }
                                }
                            }

                            var latestVersion = updateFiles.OrderByDescending( f => f.Major ).ThenByDescending( f => f.Minor ).ThenByDescending( f => f.Build ).ThenByDescending( f => f.Revision ).FirstOrDefault();
                            return latestVersion;
                        }
                        catch ( InvalidCastException ex )
                        {
                            nbInfo.Text = "Valid Url needed to check for updates.";
                            nbInfo.NotificationBoxType = NotificationBoxType.Warning;
                            nbInfo.Visible = true;
                            return null;
                        }

                    }
                    catch ( NotSupportedException ex )
                    {
                        nbInfo.Text = "Valid Url needed to check for updates.";
                        nbInfo.NotificationBoxType = NotificationBoxType.Warning;
                        nbInfo.Visible = true;
                        return null;
                    }

                }
                else
                {
                    nbInfo.Text = "You are not registered to recieve packages. Please contact BEMA for help.";
                    nbInfo.NotificationBoxType = NotificationBoxType.Warning;
                    nbInfo.Visible = true;
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        private static void AddRegexMatches( List<string> fileNames, string html, string regexString )
        {
            Regex regex = new Regex( regexString );
            MatchCollection matches = regex.Matches( html );
            if ( matches.Count > 0 )
            {
                foreach ( System.Text.RegularExpressions.Match match in matches )
                {
                    if ( match.Success )
                    {
                        fileNames.Add( match.Groups["name"].ToString() );
                    }
                }
            }
        }

        private void CleanUpPackage( string packageFile )
        {
            try
            {
                if ( File.Exists( packageFile ) )
                {
                    File.Delete( packageFile );
                }

            }
            catch ( Exception ex )
            {
                lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Cleaning Up</strong> An error occurred while cleaning up after the install. <br><em>Error: {0}</em></div>", ex.Message );
                return;
            }
        }

        private static void UpdateInstalledVersion( UpdateFile latestVersion, BemaPackageType bemaPackageType )
        {
            var attributeGuid = string.Empty;

            switch ( bemaPackageType )
            {
                case BemaPackageType.ClientTools:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_TOOLS_VERSION_ATTRIBUTE_GUID;
                    break;
                case BemaPackageType.StandardPackage:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID;
                    break;
                case BemaPackageType.ClientPackage:
                    attributeGuid = com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID;
                    break;
            }

            var attribute = AttributeCache.Get( attributeGuid );

            var rockContext = new RockContext();
            var attributeValueService = new AttributeValueService( rockContext );
            var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, null );
            if ( attributeValue == null )
            {
                attributeValue = new AttributeValue
                {
                    AttributeId = attribute.Id,
                    EntityId = null
                };
                attributeValueService.Add( attributeValue );
            }

            attributeValue.Value = String.Format( "{0}.{1}.{2}.{3}", latestVersion.Major, latestVersion.Minor, latestVersion.Build, latestVersion.Revision );

            rockContext.SaveChanges();
        }

        #endregion

    }

    public class UpdateFile
    {
        public UpdateFile( string fileName, string fullPath, int major, int minor, int build, int revision )
        {
            FileName = fileName;
            FullPath = fullPath;
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public UpdateFile()
        {
            FileName = "";
            FullPath = "";
            Major = 0;
            Minor = 0;
            Build = 0;
            Revision = 0;
        }

        public string FileName { get; set; }
        public string FullPath { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }

        public override string ToString()
        {
            var version = "" + Major + "." + Minor + "." + Build;
            if ( Revision != null )
                version += "." + Revision;
            return version;
        }
    }

    public class ClientFolder
    {
        public string ShortName { get; set; }
    }

    public enum BemaPackageType
    {
        ClientTools = 0,

        StandardPackage = 1,

        ClientPackage = 2
    }
}