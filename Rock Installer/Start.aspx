<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>


<script language="CS" runat="server">
	
	// you want the Rock ChMS, well here we go! (better hold on...)
	
	//
	// The purpose of this page is to download the base files for the install (Ionic.Zip.ddl and Install.aspx) and 
	// do some inital checks of the environment to ensure it is possible to write to the file system of the 
	// web server and make sure that the web server can connect to the internet. Hopefully no one every sees
	// this page as it should redirect to the Install.aspx page.
	//
	
	//
	// some constants
	//
	const string internetCheckSite = "www.google.com";
    const string rockZipAssemblyFile = "http://rockchms.blob.core.windows.net/install/Ionic.Zip.dll";
    const string rockInstallFile = "http://rockchms.blob.core.windows.net/install/Install.aspx";
    const string rockConfigureFile = "http://rockchms.blob.core.windows.net/install/Configure.aspx";
    
    const string rockWaitingImage = "http://rockchms.blob.core.windows.net/install/waiting.gif";
    const string rockLogoIco = "http://rockchms.blob.core.windows.net/install/rock-chms.ico";
    const string rockStyles = "http://rockchms.blob.core.windows.net/install/install.css";
        
        
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
			lOutput.Text= "<ul>" + checkMessages + "</ul>";
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
			
			// download the config file
			downloadSuccessful = DownloadFile(rockWaitingImage, Server.MapPath(".") + @"\waiting.gif", out checkMessages);	
			
			if (!downloadSuccessful) {
				lTitle.Text = "An Error Occurred...";
				lOutput.Text = "<p>" + checkMessages + "</p>";
				return;
			}
					
			// proceed with the install by downloading the installer files
			Response.Redirect("Install.aspx");
		}
		
	}
	
    // NOTE: Private helper methods are at the bottom of the file
	
    
</script>
<!DOCTYPE html>
<html>
	<head>
		<title>Rock ChMS Installer...</title>
		<link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
        <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
        <link rel="stylesheet" href="//netdna.bootstrapcdn.com/font-awesome/2.0/css/font-awesome.css">
        <link rel="stylesheet" href="<%=rockStyles %>">
		
        <link href="<%=rockLogoIco %>" rel="shortcut icon">
		<link href="<%=rockLogoIco %>" type="image/ico" rel="icon">

	</head>
	<body>
		<form runat="server">
		<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />
		<asp:UpdatePanel ID="GettingStartedUpdatePanel" runat="server" UpdateMode="Conditional">
			<ContentTemplate>
				<div id="content">
					<h1>Rock ChMS</h1>
					
					<div id="content-box" class="group">
						<h1><asp:Literal ID="lTitle" Text="Server Check" runat="server" /></h1>
						
						<asp:Label ID="lOutput" runat="server" />

                        <!-- message to show if asp.net is not installed -->
                        <asp:Literal runat="server" ID="lNoScripting">

                            <div class="alert alert-warning">
                                <p><strong>Configuration Alert</strong></p>

                                It appears that this website is not configured to run ASP.Net.  The Rock ChMS
                                requires that you run on a Windows Hosting Platform running IIS/ASP.Net.
                            </div>

                        </asp:Literal>

					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
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
		TcpClient tcpClient = new TcpClient();
		
		try {
			tcpClient.Connect(internetCheckSite, 80);
			
			if (!tcpClient.Connected) {
				checksFailed = true;
				errorDetails += "<li><i class='icon-warning-sign fail'></i> You don't seem to be connected to the internet. The Rock installer requires an Internet connection.</li>";
			}
		}
		catch(Exception ex) {
			checksFailed = true;
			errorDetails += "<li><i class='icon-warning-sign fail'></i> You don't seem to be connected to the internet. The Rock installer requires an Internet connection.</li>";
		}
		finally {
			tcpClient = null;
		}
		
		// check for write access to the file system
        
        // first get user that the server is running as
        var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
        string userName = user.Translate(typeof (System.Security.Principal.NTAccount)).ToString();

        bool canWrite = false;

        // check for write permissions
        string filename = Server.MapPath(".") + @"\write-permission.test";
        
        try
        {
            File.Create(filename).Dispose();
        }
        catch (Exception ex)
        {

        }

        if (File.Exists(filename))
        {
            canWrite = true;
            File.Delete(filename);
        }
        
        if (!canWrite) {
        	checksFailed = true;
            errorDetails += "<li><i class='icon-warning-sign fail'></i> The username " + userName + " does not have write access to the server's file system. <a class='btn btn-info btn-xs' href='TODO'>Let's Fix It Together</a> </li>";
        }

		return checksFailed;
	}


</script>