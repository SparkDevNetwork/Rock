<%@ Page Language="C#"%>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="Microsoft.Win32"  %>

<!--
                               INNNN
                              NNNNNNN
                            .NNNNNNNNNN
                           NNNNNNNNNNNNN
                          NNNNNNNNNNNNNNN.
                         NNNNNNNNNNNNNNNNNN
                       NNNNNNNNN? .NNNNNNNNN
                      NNNNNNNNN     NNNNNNNNN+
                    ?NNNNNNNNN       .NNNNNNNNN
                   NNNNNNNNN          .NNNNNNNNN
                  NNNNNNNNN             NNNNNNNNNN
                NNNNNNNNNN               .NNNNNNNNN
                                NNNNNNNNNNNNNNNNNNNN
                               DNNNNNNNNNNNNNNNNNNNNN
                               NNNNNNNNNNNNNNNNNNNNNN7
                               .NNNNNNNNNNNNNNNNNNNNN
                                 NNNNNNNNN  .....
                                  DNNNNNNNNN
                                    NNNNNNNNN
                                     NNNNNNNNN.
                                      :NNNNNNNNN
                                        NNNNNNNNN.
                                         NNNNNNNNNZ
                                           NNNNNNNNN


                             !### --  READ THIS -- ###!

    If you are seeing this on in your browser it means one of a few things:

    + You are not running on a Windows server. Rock requires a Windows hosting
      platform running IIS 7+ and ASP.Net 4.5.1. For more information be sure
      to read our hosting guides.

    + Your Windows server is not configured to run ASP.Net. Please see our
      guides to help you walk though the configuration of ASP.Net.

    + The Internet has crashed. Better take the day off while it gets rebooted.


-->


<script language="CS" runat="server">

    const string requiredDotnetVersion = "4.5.2";
    public string redirectPage = string.Empty;
    public string serverUrl = "https://rockrms.blob.core.windows.net/install/";

    void Page_Load( object sender, EventArgs e )
    {
        // first disable the no ASP.Net message
        lNoScripting.Visible = false;

        string version = "2_9_2";
        bool isDebug = false;

        // Make sure the latest security protocols for .net 4.5.2 are turned on for the client in case they are required by the server or network.
        System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

        // prepare redirect links
        redirectPage = "Install.aspx?";

        if ( Request["Version"] != null )
        {
            redirectPage += "Version=" + Request["Version"];
            version = Request["Version"];
        }

        if ( Request["Debug"] != null )
        {
            redirectPage += "&Debug=" + Request["Debug"];
            isDebug = Convert.ToBoolean( Request["Debug"] );
        }

        redirectPage = redirectPage.TrimEnd( '?' );


        // download files
        string serverPath = Server.MapPath( "." ) + @"\";
        string binDirectoryLocation = Server.MapPath( "." ) + @"\bin";
        serverUrl += version + "/";

        // run initial checks
        // -----------------------------
        StringBuilder errorDetails = new StringBuilder();

        // internet connection
        EnvironmentCheckResult internetResult = ConnectedToInternetTest();
        if ( !internetResult.DidPass )
        {
            errorDetails.Append( internetResult.AsListItem );
        }

        // write permissions
        EnvironmentCheckResult filesystemResult = WriteToFilesystemTest();
        if ( !filesystemResult.DidPass )
        {
            errorDetails.Append( filesystemResult.AsListItem );
        }

        // dot net version
        EnvironmentCheckResult dotnetResult = DotNetVersionTest();
        if ( !dotnetResult.DidPass )
        {
            errorDetails.Append( dotnetResult.AsListItem );
        }

        // rock not installed
        EnvironmentCheckResult rockInstalledResult = RockInstalledTest();
        if ( !rockInstalledResult.DidPass )
        {
            errorDetails.Append(  rockInstalledResult.AsListItem );
        }

        // install directory is an asp.net application
        EnvironmentCheckResult rockDirectoryIsAppResult = DirectoryIsApplicationTest();
        if ( !rockDirectoryIsAppResult.DidPass )
        {
            errorDetails.Append( rockDirectoryIsAppResult.AsListItem );
        }

        // if any test failed display errors
        if ( errorDetails.Length > 0 )
        {
            lLogo.Visible = true;
            lEnvironmentErrors.Text += String.Format( "<ul class='list-unstyled fa-ul environment-test'>{0}</ul>", errorDetails.ToString() );
        }
        else
        {
            bool downloadSuccessful = true;

            try
            {
                if ( isDebug )
                {
                    serverUrl = "/";
                } else {
                    // create the bin directory
                    if ( !Directory.Exists( binDirectoryLocation ) )
                    {
                        Directory.CreateDirectory( binDirectoryLocation );
                    }

                    // download files
                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( "Install.aspx", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( "Complete.aspx", serverUrl, serverPath );
                    }

                    // signalr bin files
                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.AspNet.SignalR.Core.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.AspNet.SignalR.SystemWeb.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.Owin.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.Owin.Host.SystemWeb.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.Owin.Security.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Owin.dll", serverUrl, serverPath );
                    }

                    // other bin files
                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Ionic.Zip.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Newtonsoft.Json.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\Microsoft.ApplicationBlocks.Data.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\RockInstaller.dll", serverUrl, serverPath );
                    }

                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( @"bin\RockInstallTools.dll", serverUrl, serverPath );
                    }

                    // download web.config
                    if ( downloadSuccessful )
                    {
                        downloadSuccessful = DownloadFile( "web.config", serverUrl, serverPath );
                    }
                }

            }
            catch ( Exception ex ) {
                downloadSuccessful = false;
            }

            if ( downloadSuccessful )
            {
                pnlRedirect.Visible = true;
            }
        }
    }

    private bool DownloadFile( string filename, string serverUrl, string serverPath )
    {
        string nextFileUrl = string.Empty;
        string nextFilePath = string.Empty;

        try
        {
            WebClient wc = new WebClient();

            nextFileUrl = serverUrl + filename;
            nextFilePath = serverPath + filename;
            wc.DownloadFile( nextFileUrl, nextFilePath );

            return true;
        }
        catch ( Exception ex )
        {
            lEnvironmentErrors.Visible = false;

            if ( ex.InnerException != null )
            {
                lErrors.Text = String.Format( "<h1>An Error Occurred</h1> <div class='alert alert-danger'><strong>Error:</strong> Could not download the file {0} to {1}. Message: {2}</div>", nextFileUrl, nextFilePath, ex.InnerException.Message );
            }
            else
            {
                lErrors.Text = String.Format( "<h1>An Error Occurred</h1> <div class='alert alert-danger'><strong>Error:</strong> Could not download the file {0} to {1}. Message: {2}</div>", nextFileUrl, nextFilePath, ex.Message );
            }

            return false;
        }
    }

 </script>


<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title>Rock Installer</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha256-bZLfwXAP04zRMK2BjiO8iu9pf4FbLqX6zitd+tIvLhE=" crossorigin="anonymous" />
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.3.1/css/all.css" integrity="sha384-mzrmE5qonljUremFsqc01SB46JvROS7bZs3IO2EmfFsd15uHvIt+Y8vEf7N7fWAU" crossorigin="anonymous">

    <link href="<%=String.Format("{0}Styles/rock-installer.css", serverUrl) %>" rel="stylesheet" />
    <link rel="shortcut icon" href="<%=String.Format("{0}Images/favicon.ico", serverUrl) %>" />

    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/1.12.4/jquery.min.js" integrity="sha256-ZosEbRLbNQzLpnKIkEdrPv7lOy9C27hHQ+Xp8a4MxAQ=" crossorigin="anonymous"></script>
    <script src="<%=String.Format("{0}Scripts/rock-install.js", serverUrl) %>"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="content">
	        <asp:Literal ID="lLogo" runat="server" Visible="false"><h1 id="logo">Rock RMS</h1></asp:Literal>

            <div id="content-box">

                <!-- message to show if asp.net is not installed -->
                <asp:Literal runat="server" ID="lNoScripting">
                    <h1>Something Isn't Right Here...</h1>
                    <div class="alert alert-danger">
                        <strong>ASP.Net Required</strong>

                        It appears that this website is not configured to run ASP.Net.  The Rock RMS
                        requires that you run on a Windows Hosting Platform running IIS/ASP.Net.
                    </div>

                </asp:Literal>

                <asp:Panel runat="server" ID="pnlRedirect" Visible="false">

                    <div id="divNoJavascript">
                        <div class="alert alert-danger" style="margin-top: 24px;">
                            <strong>JavaScript Required</strong> To enable a robust installation experience we require JavaScript to be enabled.

                            <p><em>If you are running this locally on a server, consider completing the install on a client machine or temporarily
                            enabling JavaScript.</em></p>
                        </div>
                    </div>

                    <script>
                        $( document ).ready(function() {
                            $('#content-box').hide();
                            window.location = "<%=redirectPage%>";
                        });
                    </script>
                </asp:Panel>

                <asp:Label ID="lEnvironmentErrors" runat="server">

                    <h1>Before We Get Started...</h1>
                    <p>Before we can get started we have some work to do. Your system's environment does not meet all of the
                        specs for Rock. Read through our <a href='http://www.rockrms.com/Rock/Learn' target="_blank"> install guides</a> for details on preparing your server for install.
                    </p>
                </asp:Label>

                <asp:Literal ID="lErrors" runat="server"></asp:Literal>
            </div>
        </div>
    </form>
</body>


<script language="CS" runat="server">
    // internet connection test method
    private EnvironmentCheckResult ConnectedToInternetTest()
    {
        EnvironmentCheckResult result = new EnvironmentCheckResult();
        result.Message = "You are connected to the Internet.";
        result.DidPass = true;

        WebClient client = new WebClient();

        try
        {
            string results = client.DownloadString( "https://rockrms.blob.core.windows.net/install/html-alive.txt" );

            if ( !results.Contains( "success" ) )
            {
                result.DidPass = false;
                result.Message = "It does not appear you are connected to the Internet. Rock requires a connection to download the installer.";
            }
        }
        catch ( Exception ex )
        {
            result.DidPass = false;
            result.Message = "Could not connect to the Internet.  Error: " + ex.Message;
            return result;
        }
        finally
        {
            client = null;
        }

        return result;
    }

    // writer permissions test method
    private EnvironmentCheckResult WriteToFilesystemTest()
    {
        EnvironmentCheckResult result = new EnvironmentCheckResult();
        result.Message = "Could not write to the server's file system.";
        result.Message = String.Format( "The username {0} does not have write access to the server's file system.", System.Security.Principal.WindowsIdentity.GetCurrent().Name );
        result.HelpLink = "https://www.rockrms.com/Rock/LetsFixThis#WebServerPermissions";
        result.DidPass = false;

        string filename = Server.MapPath( "." ) + @"\write-permission.test";

        try
        {
            File.Create( filename ).Dispose();

            if ( File.Exists( filename ) )
            {
                File.Delete( filename );
                result.DidPass = true;
                result.Message = "Your server's file permissions look correct.";
            }

        }
        catch ( Exception ex ) {
            result.DidPass = false;
            result.Message = "Could not write to the file system. Error: " + ex.Message;
        }

        return result;
    }

    // check dot net version
    private EnvironmentCheckResult DotNetVersionTest()
    {
        EnvironmentCheckResult result = new EnvironmentCheckResult();
        result.HelpLink = "https://www.rockrms.com/Rock/LetsFixThis#IncorrectDotNETVersion";
        result.DidPass = false;

        // check .net
        // ok this is not easy as .net 4.5.1 actually reports as 4.0.378675 or 4.0.378758 depending on how it was installed
        // http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions
        if ( System.Environment.Version.Major > 4 )
        {
            result.DidPass = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build > 30319 )
        {
            result.DidPass = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 )
        {
            // Once we get to 4.5 Microsoft recommends we test via the Registry...
            result.DidPass = Check45PlusFromRegistry();
        }

        string version = System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString();

        if ( result.DidPass )
        {
            result.Message = String.Format( "You have the correct version of .Net ({0}+).", requiredDotnetVersion );
        }
        else
        {
            result.Message = String.Format( "The server does not have the correct .Net runtime.  You have .Net version {0} Rock requires version {1}.", version, requiredDotnetVersion );
        }

        return result;
    }

    /// <summary>
    /// Suggested approach to check which version of the .Net framework is intalled when using version 4.5 or later
    /// as per https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx.
    /// </summary>
    /// <returns>a string containing the human readable version of the .Net framework</returns>
    private static bool Check45PlusFromRegistry()
    {
        const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
        using ( RegistryKey ndpKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ).OpenSubKey( subkey ) )
        {
            // Check if Release is >= 379893 (4.5.2)
            if ( ndpKey != null && ndpKey.GetValue( "Release" ) != null && ( int ) ndpKey.GetValue( "Release" ) >= 379893 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // check dot net version
    private EnvironmentCheckResult DirectoryIsApplicationTest()
    {
        EnvironmentCheckResult result = new EnvironmentCheckResult();
        result.HelpLink = "https://www.rockrms.com/Rock/LetsFixThis#DirectoryNotApplication";
        result.DidPass = true;

        string applicationPath = Request.ServerVariables["APPL_PHYSICAL_PATH"].ToLower();
        string directoryPath = Request.ServerVariables["PATH_TRANSLATED"].ToLower().Replace("start.aspx", "");

        if ( directoryPath != applicationPath )
        {
            result.DidPass = false;
            result.Message = "The folder where you're installing Rock from needs to be converted to an Application in IIS using the 'Convert to Application' option in the context menu.";
        }

        return result;
    }

    private EnvironmentCheckResult RockInstalledTest()
    {
        EnvironmentCheckResult isInstalledResult = new EnvironmentCheckResult();
        isInstalledResult.HelpLink = "https://www.rockrms.com/Rock/LetsFixThis#IsRockInstalledAlready";
        isInstalledResult.Message = "Website is empty";
        isInstalledResult.DidPass = true;

        string rockFile = Server.MapPath( "." ) + @"\bin\Rock.dll";
        if ( File.Exists( rockFile ) )
        {
            isInstalledResult.DidPass = false;
            isInstalledResult.Message = "It appears that Rock is already installed in this directory. You must remove this version of Rock before proceeding.";
        }

        // check that sql server spatial files don't exist
        string sqlSpatialFiles32Bit = Server.MapPath( "." ) + @"\SqlServerTypes\x86\SqlServerSpatial110.dll";
        string sqlSpatialFiles64Bit = Server.MapPath( "." ) + @"\SqlServerTypes\x64\SqlServerSpatial110.dll";
        string sqlMsCore32Bit = Server.MapPath( "." ) + @"\SqlServerTypes\x86\msvcr100.dll";
        string sqlMsCore64Bit = Server.MapPath( "." ) + @"\SqlServerTypes\x64\msvcr100.dll";

        if ( File.Exists( sqlSpatialFiles32Bit ) || File.Exists( sqlSpatialFiles64Bit ) || File.Exists( sqlMsCore32Bit ) || File.Exists( sqlMsCore64Bit ) )
        {
            isInstalledResult.DidPass = false;
            isInstalledResult.Message = "You must remove the 'SqlServerTypes' folder before proceeding. You may need to stop the webserver to enable deletion.";
        }

        return isInstalledResult;
    }

    public class EnvironmentCheckResult {

        private bool _didPass = false;
        private string _adListItem = string.Empty;
        private string _iconCss = string.Empty;
        private string _message = string.Empty;
        private string _helpText = string.Empty;
        private string _helpLink = string.Empty;

        public bool DidPass
        {
            get { return this._didPass;}
            set { this._didPass = value; }
        }

        public string AsListItem
        {
            get
            {
                return String.Format("<li><i class='{0}'></i> {1} {2} </li>", this.IconCss, this.Message, this.HelpText);
            }
        }

        public string IconCss
        {
            get
            {
                if ( this._didPass )
                {
                    return "fas fa-check-circle pass";
                }
                else
                {
                    return "fas fa-exclamation-triangle fail";
                }
            }
        }

        public string Message
        {
            get { return this._message; }
            set { this._message = value; }
        }

        public string HelpText
        {
            get
            {
                return String.Format( "<a href='{0}' class='btn btn-info btn-xs'>Let's Fix It Together</a>", this._helpLink );
            }
        }

        public string HelpLink
        {
            get { return this._helpLink; }
            set { this._helpLink = value; }
        }
    }
</script>

</html>