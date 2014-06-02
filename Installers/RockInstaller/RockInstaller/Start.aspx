<%@ Page Language="C#"%>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.IO"  %>

<script language="CS" runat="server">

    const string requiredDotnetVersion = "4.5.1";
    public string redirectPage = string.Empty;

    void Page_Load( object sender, EventArgs e )
    {        
        // first disable the no ASP.Net message
        lNoScripting.Visible = false;
        
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
        
        // if any test failed display errors
        if ( errorDetails.Length > 0 )
        {
            lLogo.Visible = true;
            lEnvironmentErrors.Text += String.Format( "<ul class='list-unstyled fa-ul environment-test'>{0}</ul>", errorDetails.ToString() );
        }
        else
        {
            string version = "2_0_0";
            bool isDebug = false;
            
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

            redirectPage.TrimEnd( '?' );
            
            
            // download files
            string serverPath = Server.MapPath( "." );
            string binDirectoryLocation = Server.MapPath( "." ) + @"\bin";
            string serverUrl = "http://storage.rockrms.com/" + version;
            
            try
            {
                if ( !isDebug )
                {
                    // create the bin directory
                    if ( !Directory.Exists( binDirectoryLocation ) )
                    {
                        Directory.CreateDirectory( binDirectoryLocation );
                    }

                    // download assembly
                    WebClient wc = new WebClient();
                    wc.DownloadFile( serverUrl + @"\Ionic.Zip.dll", binDirectoryLocation + @"\Ionic.Zip.dll" );
                }
                
            }
            catch ( Exception ex ) { }
            
            
            
            pnlRedirect.Visible = true;
        }
    }
 </script>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Installer</title>
    <link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet">

    <link href="rock-installer.css" rel="stylesheet">

    <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>
    <script src="Scripts/rock-install.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="content">
	        <asp:Literal ID="lLogo" runat="server" Visible="false"><h1>Rock RMS</h1></asp:Literal>

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
                        <div class="alert alert-danger">
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
        result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#WebServerPermissions";
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
        result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#IncorrectDotNETVersion";
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
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 && System.Environment.Version.Revision >= 18408 )
        {
            result.DidPass = true;
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

    private EnvironmentCheckResult RockInstalledTest()
    {
        EnvironmentCheckResult isInstalledResult = new EnvironmentCheckResult();
        isInstalledResult.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#IsRockInstalledAlready";
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
        if ( File.Exists( sqlSpatialFiles32Bit ) || File.Exists( sqlSpatialFiles64Bit ) )
        {
            isInstalledResult.DidPass = false;
            isInstalledResult.Message = "You must remove the 'SqlServerTypes' folder before proceeding. You may need to stop the webserver to enable deletion.";
        }

        return isInstalledResult;
    }
    
    public class EnvironmentCheckResult {
        public bool DidPass
        {
            get;
            set;
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
                if ( this.DidPass )
                {
                    return "fa fa-check-circle pass";
                }
                else
                {
                    return "fa fa-exclamation-triangle fail";
                }
            }
        }
        
        public string Message
        {
            get;
            set;
        }

        public string HelpText
        {
            get
            {
                return String.Format( "<a href='{0}' class='btn btn-info btn-xs'>Let's Fix It Together</a>", this.HelpLink );
            }
        }
        
        public string HelpLink
        {
            get;
            set;
        }
    }
</script>

</html>
