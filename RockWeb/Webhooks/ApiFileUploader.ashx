<%@ WebHandler Language="C#" Class="com.SimpleDonation.Webhooks.ApiFileUploader" %>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using Microsoft.Web.XmlTransform;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.VersionInfo;
using Rock.Web.Cache;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace com.SimpleDonation.Webhooks
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class ApiFileUploader : IHttpHandler, IRequiresSessionState
    {
        #region Fields

        const string _xdtExtension = ".rock.xdt";
        private HttpRequest request;
        private HttpResponse response;
        private string interactionDeviceType;

        #endregion

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="WebFaultException">Must be logged in</exception>
        public virtual void ProcessRequest( HttpContext context )
        {
            request = context.Request;
            response = context.Response;
            interactionDeviceType = InteractionDeviceType.GetClientType( request.UserAgent );

            if ( !context.User.Identity.IsAuthenticated )
            {
                // If not, see if there's a valid token
                string authToken = context.Request.Headers[Rock.Rest.HeaderTokens.AuthorizationToken];
                if ( string.IsNullOrWhiteSpace( authToken ) )
                {
                    authToken = context.Request.Params["apikey"];
                }

                if ( !string.IsNullOrWhiteSpace( authToken ) )
                {
                    var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                    var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                    if ( userLogin != null )
                    {
                        var identity = new GenericIdentity( userLogin.UserName );
                        var principal = new GenericPrincipal( identity, null );
                        context.User = principal;
                    }
                }
            }

            var currentUser = UserLoginService.GetCurrentUser();
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            try
            {
                if ( !context.User.Identity.IsAuthenticated )
                {
                    throw new Rock.Web.FileUploadException( "Must be logged in", System.Net.HttpStatusCode.Forbidden );
                }
                else
                {
                    GroupTypeCache securityRoleCache = GroupTypeCache.GetSecurityRoleGroupType();
                    var securityMembers = currentPerson.GetGroupMembers( securityRoleCache.Id, true );
                    var rockAdminGroupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();

                    if ( securityMembers.Any( m => m.Group.Guid == rockAdminGroupGuid && m.GroupMemberStatus == GroupMemberStatus.Active ) )
                    {
                        var sourcePath = request.QueryString["sourcePath"] != null ? request.QueryString["sourcePath"] : string.Empty;
                        if ( sourcePath.IsNotNullOrWhiteSpace() )
                        {
                            ProcessFile( context );
                        }

                        string versionLabel = request.QueryString["versionLabel"] != null ? request.QueryString["versionLabel"] : string.Empty;
                        DateTime? installedDateTime = request.QueryString["installedDateTime"] != null ? request.QueryString["installedDateTime"].AsDateTime() : null;
                        string installedBy = request.QueryString["installedBy"] != null ? request.QueryString["installedBy"] : string.Empty;

                        if ( versionLabel.IsNotNullOrWhiteSpace() || installedDateTime.HasValue || installedBy.IsNotNullOrWhiteSpace() )
                        {
                            UpdateInstalledStorePackage( context, versionLabel, installedDateTime, installedBy );
                        }
                    }
                    else
                    {
                        throw new Rock.Web.FileUploadException( "Must be a Rock Admin to use the API File Uploader", System.Net.HttpStatusCode.Forbidden );
                    }
                }
            }
            catch ( Rock.Web.FileUploadException fex )
            {
                ExceptionLogService.LogException( fex, context );
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = (int)fex.StatusCode;
                context.Response.Write( fex.Detail );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                context.Response.Write( "error: " + ex.Message );
            }
        }

        private void SendBadRequest( HttpContext httpContext, string addlInfo = "" )
        {
            httpContext.Response.StatusCode = HttpStatusCode.BadRequest.ConvertToInt();
            httpContext.Response.StatusDescription = "Request is invalid or malformed. " + addlInfo;
            httpContext.ApplicationInstance.CompleteRequest();
        }

        private void UpdateInstalledStorePackage( HttpContext context, string versionLabel, DateTime? installedDateTime, string installedBy )
        {
            /* Sample Package File:
                {
                   "PackageId":48,
                   "PackageName":"Simple Donation Gateway",
                   "VersionId":717,
                   "VersionLabel":"1.18.1",
                   "VendorId":26908,
                   "VendorName":"Simple Donation",
                   "InstalledBy":"Taylor Cavaletto",
                   "InstallDateTime":"2021-11-09T08:57:07.2210281-05:00"
                }
             */
            var installedPackages = GetPluginPackages();
            var package = installedPackages.Where( p => p.PackageName == "Simple Donation Gateway" ).FirstOrDefault();

            if ( package != null )
            {
                if ( versionLabel.IsNotNullOrWhiteSpace() )
                {
                    package.VersionLabel = versionLabel;
                }

                if ( installedDateTime.HasValue )
                {
                    package.InstallDateTime = installedDateTime.Value;
                }

                if ( installedBy.IsNotNullOrWhiteSpace() )
                {
                    package.InstalledBy = installedBy;
                }
            }

            string packageFile = HostingEnvironment.MapPath( "~/App_Data/InstalledStorePackages.json" );
            string packagesAsJson = installedPackages.ToJson();
            System.IO.File.WriteAllText( packageFile, packagesAsJson );

            context.Response.Write( package.ToJson() );
        }

        public static List<PluginPackage> GetPluginPackages()
        {
            string packageFile = HostingEnvironment.MapPath( "~/App_Data/InstalledStorePackages.json" );
            if ( !File.Exists( packageFile ) )
            {
                return new List<PluginPackage>();
            }

            try
            {
                using ( StreamReader r = new StreamReader( packageFile ) )
                {
                    string json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<PluginPackage>>( json );
                }
            }
            catch
            {
                return new List<PluginPackage>();
            }
        }

        /// <summary>
        /// Processes the content file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessFile( HttpContext context )
        {
            string untrustedTargetFolder = String.Empty;
            string untrustedSourcePath, untrustedTargetFolderParam;
            ValidateRequest( out untrustedSourcePath, out untrustedTargetFolderParam );

            untrustedTargetFolder = GetTargetFolder( untrustedTargetFolder, untrustedSourcePath, untrustedTargetFolderParam );

            string sourcePath = ScrubFilePath( untrustedSourcePath );
            string targetFolder = ScrubFilePath( untrustedTargetFolder );

            string appRoot = HttpContext.Current.Request.MapPath( "~/" );
            string workingDirectory = "";
            workingDirectory = String.Format( "{0}{1}", appRoot, targetFolder );

            string fileName = sourcePath.Split( '/' ).Last();
            int index = fileName.IndexOf( "?" );
            if ( index >= 0 )
            {
                fileName = fileName.Substring( 0, index );
            }
            fileName = fileName.Replace( ".download", "" );

            string destinationFile = string.Format( "{0}/{1}", workingDirectory, fileName );

            // check that the directory exists
            if ( !Directory.Exists( workingDirectory ) )
            {
                Directory.CreateDirectory( workingDirectory );
            }

            // download file
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile( sourcePath, destinationFile );
            }
            catch ( Exception ex )
            {
                CleanUpPackage( destinationFile );
                throw new Rock.Web.FileUploadException( String.Format( "Failed to Download File: {0}", ex.Message ), System.Net.HttpStatusCode.BadRequest );
            }

            // Handle .plugin files
            if ( fileName.Contains( ".plugin" ) )
            {
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
                                    using ( var rockContext = new RockContext() )
                                    {
                                        rockContext.Database.ExecuteSqlCommand( sqlScript );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            throw new Rock.Web.FileUploadException( String.Format( "Error Updating Database: {0}", ex.Message ), System.Net.HttpStatusCode.BadRequest );
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
                            throw new Rock.Web.FileUploadException( String.Format( "Error Modifying Files: {0}", ex.Message ), System.Net.HttpStatusCode.BadRequest );
                        }

                    }
                }
                catch ( Exception ex )
                {
                    throw new Rock.Web.FileUploadException( String.Format( "Error Extracting Package: {0}", ex.Message ), System.Net.HttpStatusCode.BadRequest );
                }
            }

            // Clear all cached items
            RockCache.ClearAllCachedItems();

            var response = new
            {
                Id = string.Empty,
                FileName = Path.Combine( workingDirectory, fileName )
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Scrubs a file path to make sure it doesn't have any invalid characters
        /// </summary>
        /// <param name="untrustedFilePath">The file path.</param>
        /// <returns>A scrubed file path.</returns>
        public string ScrubFilePath( string untrustedFilePath )
        {
            // Scrub invalid path characters
            return Regex.Replace( untrustedFilePath.Trim(), "[" + Regex.Escape( Path.GetInvalidPathChars().ToString() ) + "]", string.Empty, RegexOptions.CultureInvariant );
        }

        private void ValidateRequest( out string sourcePath, out string targetFolderParam )
        {
            sourcePath = request.QueryString["sourcePath"] != null ? request.QueryString["sourcePath"] : string.Empty;
            targetFolderParam = request.QueryString["targetFolder"] != null ? request.QueryString["targetFolder"] : string.Empty;
            string minimumRockVersion = request.QueryString["minimumRockVersion"] != null ? request.QueryString["minimumRockVersion"] : string.Empty;
            string maximumRockVersion = request.QueryString["maximumRockVersion"] != null ? request.QueryString["maximumRockVersion"] : string.Empty;

            RockSemanticVersion rockVersion = RockSemanticVersion.Parse( VersionInfo.GetRockSemanticVersionNumber() );
            RockSemanticVersion minVersion = null;
            RockSemanticVersion maxVersion = null;

            if ( RockSemanticVersion.TryParse( minimumRockVersion, out minVersion ) && rockVersion < minVersion )
            {
                throw new Rock.Web.FileUploadException( "Installed Rock Version Does Not Meet The Minimum Requirements", System.Net.HttpStatusCode.BadRequest );
            }

            if ( RockSemanticVersion.TryParse( maximumRockVersion, out maxVersion ) && rockVersion > maxVersion )
            {
                throw new Rock.Web.FileUploadException( "Installed Rock Version Exceeds the Maximum Specified Version.", System.Net.HttpStatusCode.BadRequest );
            }

            if ( sourcePath.IsNullOrWhiteSpace() )
            {
                throw new Rock.Web.FileUploadException( "Please Specify A Source Path.", System.Net.HttpStatusCode.BadRequest );
            }

            sourcePath = HttpUtility.HtmlDecode( sourcePath );

            if ( !sourcePath.Contains( "s3.amazonaws.com" ) )
            {
                throw new Rock.Web.FileUploadException( "Please Specify A Trusted Source Path.", System.Net.HttpStatusCode.Forbidden );
            }
        }

        private static string GetTargetFolder( string targetFolder, string sourcePath, string targetFolderParam )
        {
            if ( sourcePath.Contains( ".plugin" ) )
            {
                targetFolder = "App_Data/SimpleDonationClientPackage/";
            }

            if ( sourcePath.Contains( ".dll" ) )
            {
                targetFolder = "Bin/";
            }

            if ( sourcePath.Contains( ".ashx" ) )
            {
                targetFolder = "Webhooks/";
            }

            if ( targetFolder.IsNullOrWhiteSpace() )
            {
                targetFolder = String.Format( "Plugins/com_simpleDonation/{0}", targetFolderParam );
            }

            return targetFolder;
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
                return;
            }
        }

        public class PluginPackage
        {
            /// <summary>
            /// Gets or sets the PackageId of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.Int32"/> representing the PackageId.
            /// </value>
            public int PackageId { get; set; }

            /// <summary>
            /// Gets or sets the package label of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the package label.
            /// </value>
            public string PackageName { get; set; }

            /// <summary>
            /// Gets or sets the VersionId of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.Int32"/> representing the VersionId.
            /// </value>
            public int VersionId { get; set; }

            /// <summary>
            /// Gets or sets the version label of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the version label.
            /// </value>
            public string VersionLabel { get; set; }

            /// <summary>
            /// Gets or sets the VendorId of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.Int32"/> representing the VendorId.
            /// </value>
            public int VendorId { get; set; }

            /// <summary>
            /// Gets or sets the vendor name of the installation. 
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the vendor name.
            /// </value>
            public string VendorName { get; set; }

            /// <summary>
            /// Gets or sets the Name of the person who installed the package. 
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the Name of the installer.
            /// </value>
            public string InstalledBy { get; set; }

            /// <summary>
            /// Gets or sets the install date/time. 
            /// </summary>
            /// <value>
            /// A <see cref="System.DateTime"/> representing the install date/time.
            /// </value>
            public DateTime InstallDateTime { get; set; }

        }
    }

}