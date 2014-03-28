<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>


<script language="CS" runat="server">
	
	// you want the Rock RMS, well here we go! (better hold on...)
	
	//
	// The purpose of this page is to download the base files for the install (Ionic.Zip.ddl and Install.aspx) and 
	// do some inital checks of the environment to ensure it is possible to write to the file system of the 
	// web server and make sure that the web server can connect to the internet. Hopefully no one every sees
	// this page as it should redirect to the Install.aspx page.
	//
	
	//
	// some constants
	//
    const string dotNetVersionRequired = "4.5";
    const string internetCheckSite = "https://rockrms.blob.core.windows.net/install/html-alive.txt";
    const string rockZipAssemblyFile = "http://storage.rockrms.com/install/Ionic.Zip.dll";
    const string rockInstallFile = "http://storage.rockrms.com/install/Install.aspx";
    const string rockConfigureFile = "http://storage.rockrms.com/install/Configure.aspx";
    const string rockUtilitiesAssembly = "http://storage.rockrms.com/install/Rock.Install.Utilities.dll";
    const string rockInitalWebConfig = "http://storage.rockrms.com/install/web.config";
    const string rockInstalledFile = @"\bin\Rock.dll";

    const string rockLogoIco = "http://storage.rockrms.com/install/rock-chms.ico";
    const string rockStyles = "http://storage.rockrms.com/install/install.css";
        
        
	//
	// page events
	//
	
	void Page_Load(object sender, EventArgs e)
	{
		// first disable the no ASP.Net message
        lNoScripting.Visible = false;
        
        // set location of the bin directory
		string binDirectoryLocation = Server.MapPath(".") + @"\bin";
		
		// first make some checks to determine if we can write to the file system and have access to the internet
		string checkMessages = string.Empty;
		bool checksFailed = CheckEnvironment(out checkMessages);

		if (checksFailed) {
			// we can't proceed with the install
			
			lTitle.Text = "Before We Get Started...";
            lOutput.Text = "<ul class='list-unstyled'>" + checkMessages + "</ul>";
			return;
		}
		else {
			bool downloadSuccessful = true;
			
			// download Ionic.Zip.dll
			try {
				// create the bin directory
				if(!Directory.Exists(binDirectoryLocation)) {
					Directory.CreateDirectory(binDirectoryLocation);
				}
		
				// download assembly
				WebClient myWebClient = new WebClient();
				myWebClient.DownloadFile(rockZipAssemblyFile, binDirectoryLocation + @"\Ionic.Zip.dll");
				downloadSuccessful = true;
			}
			catch (Exception ex) {
				downloadSuccessful = false;
				checkMessages = "Error Details: " + ex.Message + " when downloading " + rockZipAssemblyFile + "." ;
			}
			
			if (!downloadSuccessful) {
				lTitle.Text = "An Error Occurred...";
				lOutput.Text = "<p>" + checkMessages + "</p>";
				return;
			} 
			
			// download the install file
			downloadSuccessful = DownloadFile(rockInstallFile, Server.MapPath(".") + @"\Install.aspx", out checkMessages);	
			
			if (!downloadSuccessful) {
				lTitle.Text = "An Error Occurred...";
				lOutput.Text = "<p>" + checkMessages + "</p>";
				return;
			}
			
			// download the config file
			downloadSuccessful = DownloadFile(rockConfigureFile, Server.MapPath(".") + @"\Configure.aspx", out checkMessages);	
			
			if (!downloadSuccessful) {
				lTitle.Text = "An Error Occurred...";
				lOutput.Text = "<p>" + checkMessages + "</p>";
				return;
			}

            // download the assembly file
            downloadSuccessful = DownloadFile(rockUtilitiesAssembly, Server.MapPath(".") + @"\bin\Rock.Install.Utilities.dll", out checkMessages);

            if (!downloadSuccessful)
            {
                lTitle.Text = "An Error Occurred...";
                lOutput.Text = "<p>" + checkMessages + "</p>";
                return;
            }

            // download the install file
            downloadSuccessful = DownloadFile( rockInitalWebConfig, Server.MapPath( "." ) + @"\web.config", out checkMessages );

            if ( !downloadSuccessful )
            {
                lTitle.Text = "An Error Occurred...";
                lOutput.Text = "<p>" + checkMessages + "</p>";
                return;
            }
					
			// proceed with the install by downloading the installer files
            lRedirect.Visible = true;
		}
		
	}
	
    // NOTE: Private helper methods are at the bottom of the file
	
    
</script>
<!DOCTYPE html>
<html>
	<head>
		<title>Rock RMS Installer...</title>
		<link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
        <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
        <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet">
        <link rel="stylesheet" href="<%=rockStyles %>">
		
        <link href="<%=rockLogoIco %>" rel="shortcut icon">
		<link href="<%=rockLogoIco %>" type="image/ico" rel="icon">

        <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>

	</head>
	<body>
		<form runat="server">
		
				<div id="content">
					<h1>Rock RMS</h1>
					
					<div id="content-box" class="group">
						<h1><asp:Literal ID="lTitle" Text="" runat="server" /></h1>
						
						<asp:Label ID="lOutput" runat="server" />

                        <!-- message to show if asp.net is not installed -->
                        <asp:Literal runat="server" ID="lNoScripting">

                            <div class="alert alert-warning">
                                <p><strong>Configuration Alert</strong></p>

                                It appears that this website is not configured to run ASP.Net.  The Rock RMS
                                requires that you run on a Windows Hosting Platform running IIS/ASP.Net.
                            </div>

                        </asp:Literal>

                        <asp:Literal runat="server" ID="lRedirect" Visible="false">    
                            <div id="divNoJavascript" class="alert alert-danger">
                                <p>
                                    <strong>JavaScript Required</strong> To enable a robust installation experience we require JavaScript to be enabled.
                                </p>
                                <p><em>If you are running this locally on a server, consider completing the install on a client machine or temporarily 
                                    enabling JavaScript.
                                   </em>
                                </p>
                            </div>

                            <script>
                                $( document ).ready(function() {
                                    $('#content-box').hide();
                                    window.location = "Install.aspx";
                                });
                            </script>

                        </asp:Literal>

					</div>
				</div>
		</form>
		
		
	</body>

</html>



<script language="CS" runat="server">
	
	/* 
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
		Private Helper Methods
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><> 
	*/

	//
	// method downloads a file from a url
	private bool DownloadFile(string remoteLocation, string destinationFile, out string checkMessages) {
		bool downloadSuccess = true;
		checkMessages = string.Empty;
		
		try {
			WebClient myWebClient = new WebClient();
			myWebClient.DownloadFile(remoteLocation,destinationFile);
		}
		catch (Exception ex) {
			downloadSuccess = false;
			checkMessages = "An error occurred downloading the file from " + remoteLocation + ".</p>  <p>Error Message: " + ex.Message;
			
			if (ex.InnerException != null) {
				checkMessages += " " + ex.InnerException.Message;
			}
		}
		
		return downloadSuccess;
	}


	//
	// CheckEnvironment checks to ensure we are connected to the internet and have write access to the web 
	// server file system.
	// 
	private bool CheckEnvironment(out string errorDetails) {
		
		bool checksFailed = false;
		errorDetails = string.Empty;
		
	
		// check for internet connection
        WebClient client = new WebClient();
		
		try {
            string results = client.DownloadString( internetCheckSite );
			
            if (!results.Contains("success")) {
                checksFailed = true;
                errorDetails += "<li><i class='fa fa-exclamation-triangle fail'></i> Could not connect to the Rock Install Server. Please check your internet connection or try again soon.</li>";
            }
		}
		catch(Exception ex) {
			checksFailed = true;
            errorDetails += "<li><i class='fa fa-exclamation-triangle fail'></i> Could not connect to the Rock Install Server. Please check your internet connection or try again soon.</li>";
		}
		finally {
			client = null;
		}
		
		// check for write access to the file system
        
        // first get user that the server is running as
        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

        bool canWrite = false;

        // check for write permissions
        string filename = Server.MapPath(".") + @"\write-permission.test";

        try
        {
            File.Create( filename ).Dispose();


            if ( File.Exists( filename ) )
            {
                canWrite = true;
                File.Delete( filename );
            }
            
        } catch(Exception ex){}
        
        if (!canWrite) {
        	checksFailed = true;
            errorDetails += "<li><i class='fa fa-exclamation-triangle fail'></i> The username " + userName + " does not have write access to the server's file system. <a class='btn btn-info btn-xs' href='http://www.rockrms.com/Rock/LetsFixThis#WebServerPermissions'>Let's Fix It Together</a> </li>";
        }

        // check asp.net version
        string checkResults = string.Empty;
        
        if (!CheckDotNetVersion(out checkResults))
        {
            checksFailed = true;
            errorDetails += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockrms.com/Rock/LetsFixThis#IncorrectDotNETVersion' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
        }
        
        // check that rock not already installed
        string rockFile = Server.MapPath( "." ) + rockInstalledFile;
        if ( File.Exists( rockFile ) )
        {
            checksFailed = true;
            errorDetails += "<li><i class='fa fa-exclamation-triangle fail'></i> It appears that Rock is already installed in this directory. You must remove this version of Rock before proceeding.</a></li>";
        }
        
		return checksFailed;
	}

    private bool CheckDotNetVersion(out string errorDetails)
    {
        bool checksPassed = false;
        errorDetails = string.Empty;

        // check .net
        // ok this is not easy as .net 4.5.1 actually reports as 4.0.378675 or 4.0.378758 depending on how it was installed
        // http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions
        if ( System.Environment.Version.Major > 4 )
        {
            checksPassed = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build > 30319 )
        {
            checksPassed = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 && System.Environment.Version.Revision >= 18408 )
        {
            checksPassed = true;
        }

        if ( checksPassed )
        {
            errorDetails += String.Format( "You have the correct version of .Net ({0}+).", dotNetVersionRequired );
        }
        else
        {
            errorDetails = "The server does not have the correct .Net runtime.  You have .Net version " + System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString() + " the Rock ChMS version requires " + dotNetVersionRequired + ".";
        }

        return checksPassed;
    }


</script>