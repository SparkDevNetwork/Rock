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
	const string rockZipAssemblyFile = "http://www.rockchms.com/installer/assemblies/Ionic.Zip.dll";
	const string rockInstallFile = "http://www.rockchms.com/installer/assemblies/Install.aspx";
	const string rockConfigureFile = "http://www.rockchms.com/installer/assemblies/Configure.aspx";
	const string rockWaitingImage = "http://www.rockchms.com/installer/assets/images/waiting.gif";
	
	//
	// page events
	//
	
	void Page_Load(object sender, EventArgs e)
	{
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

<html>
	<head>
		<title>Rock ChMS Installer...</title>
		<link href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' rel='stylesheet' type='text/css'>

		
		<style>
			body {
				background-color: #3c3c3c;
				color: #333;
				font-family: 'Open Sans', sans-serif;
				background-image: url("http://www.rockchms.com/installer/assets/images/header-texture.png");
			}
			
			div#content {
				width: 600px;
				margin: 0 auto;
			}
			
			i {
				font-family: FontAwesome;
				font-size: 22px;
			}
			
			i.pass {
				color: #218921;
			}
			
			i.fail {
				color: #e43030;
			}
			
			.btn i {
				font-size: 12px;
				margin-top: 6px;
			}
			
			ul {
				list-style-type: none;
				margin-top: 22px;
				margin-left: 90px;
			}
			
			ul li {
				margin-bottom: 12px;
			}
			
			ul li i[class^="icon-"] {
				height: 30px;
				float: left;
				margin-right: 12px;
				line-height: 18px;
			} 
			
			div#content > h1 {
				background: url("http://www.rockchms.com/installer/assets/images/header-logo.png") no-repeat scroll 0 0 transparent;
			    color: #E7E7E7;
			    display: block;
			    float: left;
			    height: 50px;
			    margin-bottom: 6px;
			    margin-top: 65px;
			    text-indent: -9999px;
			    width: 123px;
    		}
    		
    		div#content #content-box {
	    		background-color: #E7E7E7;
			    border-radius: 5px 5px 5px 5px;
			    box-shadow: 0 0 20px #000000;
			    clear: both;
			    min-height: 254px;
			    padding: 16px;
			    width: 584px;
			    color: #;
    		}
    		
    		div#content #content-box h1 {
    			margin-top: 0;
    		}
    		
    		    		
    		a.btn {
    			text-decoration: none;
    		}
    		
    		p.wait {
    			width: 48px;
    			margin: 0 auto;
    			margin-top: 24px;
    		}
    		
    		.btn.start-install, .btn.next-step {
    			display: block;
    			margin: 24px auto 24px auto;
    			text-align: center;
    			width: 180px;
    		}

    		.group:after {
			    clear: both;
			    content: ".";
			    display: block;
			    height: 0;
			    visibility: hidden;
			}
		</style>
	</head>
	<body>
		<form runat="server">
		<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />
		<asp:UpdatePanel ID="GettingStartedUpdatePanel" runat="server" UpdateMode="Conditional">
			<ContentTemplate>
				<div id="content">
					<h1>Rock ChMS</h1>
					
					<div id="content-box" class="group">
						<h1><asp:Label ID="lTitle" runat="server" /></h1>
						
						<asp:Label ID="lOutput" runat="server" />

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
			} else {
				//errorDetails += "<li><i class='icon-ok-sign pass'></i> You are connected to the internet.</li>";
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
        
        // check rights on directory
        DirectorySecurity dirSecurity = Directory.GetAccessControl(Server.MapPath("."), AccessControlSections.Access);
		foreach(FileSystemAccessRule fsRule in dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
        {             
            if(fsRule.IdentityReference.Value.ToLower() == userName.ToLower()) {
	            	            
	            if ((fsRule.FileSystemRights.ToString().Contains("Write"))  
	            		|| (fsRule.FileSystemRights.ToString().Contains("FullControl")) 
	            		|| (fsRule.FileSystemRights.ToString().Contains("Modify")))
	            {
	            	canWrite = true;
	            	break;
	            }
            }
        }
        
        if (!canWrite) {
        	checksFailed = true;
        	errorDetails += "<li><i class='icon-warning-sign fail'></i> The username " + userName + " does not have write access to the server's file system.</li>";
        } else {
        	//errorDetails += "<li><i class='icon-ok-sign pass'></i> Your server's file permissions look correct.</li>";
        }

		return checksFailed;
	}


</script>