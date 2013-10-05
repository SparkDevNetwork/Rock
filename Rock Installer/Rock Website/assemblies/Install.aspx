<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.Data.SqlClient"  %>
<%@ Import Namespace="Microsoft.Win32"  %>
<%@ Import Namespace="Ionic.Zip"  %>
<%@ Import Namespace="System.Web.Security" %>



<script language="CS" runat="server">
	
	
	//
	// some constants
	//
	const string internetCheckSite = "www.google.com";
	const string dotNetVersionRequired = "4.5";
	const double iisVersionRequired = 7.0;
    const string rockInstallFile = "http://rockchms.blob.core.windows.net/install/rock-chms-latest.zip";

    const string rockLogoIco = "http://rockchms.blob.core.windows.net/install/rock-chms.ico";
    const string rockStyles = "http://rockchms.blob.core.windows.net/install/install.css";
    const string rockShowPass = "http://rockchms.blob.core.windows.net/install/showpassword.js";
	
	//
	// page events
	//
	
	private void Page_Init(object sender, System.EventArgs e)
	{
    	this.EnableViewState = false;
    }
	
	void Page_Load(object sender, EventArgs e)
	{
		// toggle the SSL warning
        lSslWarning.Visible = !Request.IsSecureConnection;
	}
	
	void WelcomeNext_Click(Object sender, EventArgs e)
    {
    	pWelcome.Visible = false;
    	pDatabaseConfig.Visible = true;
    }
	
	void DbConfigNext_Click(Object sender, EventArgs e)
    {
               
        // check settings
    	string databaseMessages = string.Empty; 
    	
        bool canConnect = CheckSqlServerConnection(txtServerName.Text, txtDatabaseName.Text, txtUsername.Text, txtPassword.Text, out databaseMessages);

    	if (!canConnect) {
    		lDatabaseMessages.Text = databaseMessages;
            
            pWelcome.Visible = false;
            pDatabaseConfig.Visible = true;
    		
            return;
    	} else {
    		

		    // run environment checks
		    string outputMessages = string.Empty;
		    string checkResults = string.Empty;
		    bool environmentClean = true;
		    
		    // check .Net version
		    if (CheckDotNetVersion(out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/dotnet-version.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
		    
		    // check web server permissions
		    if (CheckFileSystemPermissions(out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/filesystem-permissions.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
    		
    		// check IIS version
    		if (CheckIisVersion(out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/iis-version.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
		    
		    // check sql server version
    		if (CheckSqlServerVersion(txtServerName.Text, txtDatabaseName.Text, txtUsername.Text, txtPassword.Text, out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + "</li> <a href='http://www.rockchms.com/installer/help/sqlserver-version.html' class='fix'>Let's Fix It Together</a>";
    			environmentClean = false;
    		}
    		
    		// check sql server permissions
    		if (CheckSqlServerPermissions(txtServerName.Text, txtDatabaseName.Text, txtUsername.Text, txtPassword.Text, out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/sqlserver-permissions.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
    		
    		// check sql server permissions
    		if (CheckSqlServerEmpty(txtServerName.Text, txtDatabaseName.Text, txtUsername.Text, txtPassword.Text, out checkResults)) {
    			outputMessages += "<li><i class='icon-ok-sign pass'></i>" + checkResults + "</li>";
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/sqlserver-empty.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
    		
    		// check that rock is not installed already
    		if (CheckRockNotInstalled( out checkResults)) {
    			
    		} else {
    			outputMessages += "<li><i class='icon-warning-sign fail'></i>" + checkResults + " <a href='http://www.rockchms.com/installer/help/rock-installed.html' class='fix'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}

    		// setup environment screen based on results
		    if (environmentClean) {
		    	lTestEnvTitle.Text = "Pass!";
		    	lTestEnvResults.Text = "Your environment passed all tests and looks like a good home for the Rock ChMS.  What are we waiting for? Let's get started!!!";
		    	lTestEnvDetails.Text = "<ul>" + outputMessages + "</ul>";
		    	lTestEnvDetails.Text += "<div class='alert alert-info'><strong>Heads Up:</strong> The next step could take a few minutes to run. Don't worry it's normal.</div>";
		    	btnEnvNext.Visible = true;
		    	btnTryAgain.Visible = false;
		    	
		    	// write db config
		    	// write config file
			    string configContents = String.Format(@"<add name=""RockContext"" connectionString=""Data Source={0};Initial Catalog={1}; User Id={2}; password={3};MultipleActiveResultSets=true"" providerName=""System.Data.SqlClient""/>", txtServerName.Text, txtDatabaseName.Text, txtUsername.Text, txtPassword.Text);
			    
			    StreamWriter configFile = new StreamWriter(Server.MapPath("~/web.ConnectionStrings.config"), false);
			    configFile.WriteLine(@"<?xml version=""1.0""?>");
			    configFile.WriteLine(@"<connectionStrings>");
			    configFile.WriteLine("\t" + configContents);
			    configFile.WriteLine("</connectionStrings>");
			    configFile.Flush();
			    configFile.Close(); 
			    configFile.Dispose();

		    	
		    } else {
		    	lTestEnvTitle.Text = "We Have Some Work To Do";
		    	lTestEnvResults.Text = "The server environment doesn't currently meet all of the requirements.  That's OK let's we'll try to help you solve the issues.";
		    	lTestEnvDetails.Text = "<ul>" + outputMessages + "</ul>";
		    	btnEnvNext.Visible = false;
		    	btnTryAgain.Visible = true;
		    }

		    // move to next step
		    pTestEnv.Visible = true;
		    pDatabaseConfig.Visible = false;
		    pWelcome.Visible = false;
		    
		    
		}
	   
    }
    
    void EnvNext_Click(Object sender, EventArgs e)
    {
    	// set server timeout to 15 mins
        Server.ScriptTimeout = 900;
        
        // download install file
    	bool downloadSuccessful = false;
    	string checkMessages = string.Empty;
    	downloadSuccessful = DownloadFile(rockInstallFile, Server.MapPath(".") + @"\RockInstall.zip", out checkMessages);
    	
    	// change active panels
    	pTestEnv.Visible = false;
    	pDownloadZip.Visible = true;
    	pWelcome.Visible = false;
    	
    	if (downloadSuccessful) {
    		// let's unzip this bad boy
    		using (ZipFile zip = ZipFile.Read(Server.MapPath(".") + @"\RockInstall.zip"))
			{
				zip.ExtractAll(Server.MapPath("."), ExtractExistingFileAction.OverwriteSilently);    
			}
			
			// update the web.config with unique password key
			int passwordLength = 32;
			int alphaNumericalCharsAllowed = 2;
			string randomPassword = Membership.GeneratePassword(passwordLength, alphaNumericalCharsAllowed);
			
			// turn string into byte array
			byte[] bytes = System.Text.Encoding.Unicode.GetBytes( randomPassword );
            System.Text.StringBuilder hex = new System.Text.StringBuilder( bytes.Length * 2 );
            foreach ( byte b in bytes ) {
                hex.AppendFormat( "{0:x2}", b );
            }
            
            string hexBytes = hex.ToString().ToUpper();
			
			Configuration rockWebConfig  = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			rockWebConfig.AppSettings.Settings["PasswordKey"].Value = hexBytes;
            //rockWebConfig.AppSettings.Settings["BaseUrl"].Value = Request.Url.Scheme + @"://" + Request.Url.Host + Request.ApplicationPath;  // not needed removed from web.config per https://github.com/SparkDevNetwork/Rock-ChMS/commit/17b0d30082f0b98bec8bc31d2034fb774690b2e1
			rockWebConfig.Save();
			
			
			lDownloadTitle.Text = "Download Successful";
    		lDownloadDetails.Text = "<p>The Rock ChMS has been downloaded and installed on your web server.  The next step is to create the database.</p><div class='alert alert-info'><strong>Letting You Know:</strong> We'll be loading a new page as the database is created. This could take a couple of minutes also. Good things come to those who wait.</div>";
    	} else {
    		lDownloadTitle.Text = "Error on Download";
    		lDownloadDetails.Text = checkMessages;
    		//TODO: Where do we direct the user?
    	}
    	
    }
    
    void EnvBack_Click(Object sender, EventArgs e)
    {
    	pTestEnv.Visible = false;
		pDatabaseConfig.Visible = true;
    }
    
    
    void DownloadNext_Click(Object sender, EventArgs e)
    {
    	try {
    		Response.Redirect("Configure.aspx");
    	}
    	catch (Exception ex) {
    		ProcessPageException(ex.Message);
    	}
    }
	
    // NOTE: Private helper methods are at the bottom of the file
	
    
</script>
<!DOCTYPE html>
<html>
	<head>
		<title>Rock ChMS Installer...</title>
		<link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
		<link rel="stylesheet" href="//netdna.bootstrapcdn.com/font-awesome/2.0/css/font-awesome.css">
		<link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
        <link rel="stylesheet" href="<%=rockStyles %>">
		
        <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>
        		
		<link href="<%=rockLogoIco %>" rel="shortcut icon">
		<link href="<%=rockLogoIco %>" type="image/ico" rel="icon">
		
		<!--<style>
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
			
			a.fix {
				font-size: 12px;
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
    			margin: 24px auto 24px auto;
    			text-align: center;
    			width: 120px;
    		}
    		
    		.btn-list {
    			width: 100%;
    			text-align: center;
    		}

    		.group:after {
			    clear: both;
			    content: ".";
			    display: block;
			    height: 0;
			    visibility: hidden;
			}
			
			select, 
			textarea, 
			input[type="text"], 
			input[type="password"], 
			input[type="datetime"], 
			input[type="datetime-local"], 
			input[type="date"], 
			input[type="month"], 
			input[type="time"], 
			input[type="week"], 
			input[type="number"], 
			input[type="email"], 
			input[type="url"], 
			input[type="search"], 
			input[type="tel"], 
			input[type="color"], 
			uneditable-input {
				height: 30px;
			
			}
			
			#config-info {
				margin-top: 12px;
				margin-left: 60px;
			}
		</style> -->
	</head>
	<body>
		<form runat="server">
		<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" AsyncPostBackTimeout="900" />
		<asp:UpdateProgress id="updateProgress" runat="server">
		     <ProgressTemplate>
		            <div style="color: #fff; position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #343434; opacity: 0.97;">
		            	<asp:Image ID="imgUpdateProgress" runat="server" ImageUrl="./waiting.gif" AlternateText="Creating ..." ToolTip="Creating ..." style="padding: 10px;position:fixed;top:45%;left:50%;" />
		            
		            	<div class="alert alert-info">
		            		<h4>Heads Up</h4>
		            		Depending on your server this could take a couple of minutes. Don't worry, it'll be done soon!
						</div>
		            
		            </div>
		     </ProgressTemplate>
		</asp:UpdateProgress>
		<asp:UpdatePanel ID="GettingStartedUpdatePanel" runat="server" UpdateMode="Conditional">
			<ContentTemplate>
				<div id="content">
					<h1>Rock ChMS</h1>
					
					<div id="content-box" class="group">
						<asp:Panel id="pWelcome" Visible="true" runat="server">
							<h1>Rock Installer</h1>
							
                            <img src="http://www.rockchms.com/installer/assets/images/welcome.jpg"  />
							<asp:Label id="lTest" runat="server"></asp:Label>
							
                            <asp:Literal ID="lSslWarning" runat="server">
                                <div class="alert alert-warning">
                                    <p><strong>Just A Thought...</strong></p>
                                    Looks like you're not running over an encrypted connection (SSL).  Since you will be providing passwords for configuring
                                    your database and Rock install you may wish to run the install over an encrypted connection.
                                </div>
							</asp:Literal>

							<div class="btn-list">
								<asp:LinkButton id="btnWelcome" runat="server" Text="Get Started <i class='icon-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="WelcomeNext_Click"></asp:LinkButton>
							</div>
						</asp:Panel>
						
						<asp:Panel id="pDatabaseConfig" Visible="false" runat="server">
						
							<h1>Database Configuration</h1>
						
							<p>Please provide configuration information to the database below.  This information should come from your server
							   administrator or hosting provider.</p>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Server</label>
								<asp:TextBox ID="txtServerName" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Name</label>
								<asp:TextBox ID="txtDatabaseName" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Username</label>
								<asp:TextBox ID="txtUsername" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Password</label>
								
                                <div class="row">
                                    <div class="col-md-6">
                                        <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
                                    </div>
                                    <div class="col-md-6" style="padding-top: 6px;">
                                        <input id="show-password" type="checkbox" />
                                        <label for="show-password" id="show-password-label" style="font-weight:normal;">Show Password</label>
                                    </div>
                                </div>
							</div>
							
							<asp:Literal id="lDatabaseMessages" runat="server"></asp:Literal>
							
							<div class="btn-list">
								<asp:LinkButton id="btnDbConfig" runat="server" OnClientClick="return validateDbConnection();" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="DbConfigNext_Click"></asp:LinkButton> 
							</div>
						</asp:Panel>
						
						<asp:Panel id="pTestEnv" Visible="false" runat="server">
							<h1><asp:Label id="lTestEnvTitle" runat="server"></asp:Label></h1>
						
							<asp:Label id="lTestEnvResults" runat="server"></asp:Label>
						
							<asp:Label id="lTestEnvDetails" runat="server"></asp:Label>
							
							<div class="btn-list">
							
								<asp:LinkButton id="btnEnvBack" runat="server"  Text="<i class='icon-chevron-left'></i> Back"  CssClass="btn next-step" OnClick="EnvBack_Click"></asp:LinkButton>  
								<asp:LinkButton id="btnTryAgain" runat="server"  Text="Try Again <i class='icon-refresh'></i>"  CssClass="btn btn-inverse next-step" OnClick="DbConfigNext_Click"></asp:LinkButton> 
								<asp:LinkButton id="btnEnvNext" runat="server"  Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="EnvNext_Click"></asp:LinkButton> 
							</div>
						</asp:Panel>
						
						<asp:Panel id="pDownloadZip" Visible="false" runat="server">
							<h1><asp:Label id="lDownloadTitle" runat="server"></asp:Label></h1>
						
							<asp:Label id="lDownloadDetails" runat="server"></asp:Label>
							
							<div class="btn-list">		
								<asp:LinkButton id="btnDownloadNext" runat="server" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="DownloadNext_Click"></asp:LinkButton> 
							</div>
						</asp:Panel>
						
						
						<asp:Panel id="pError" Visible="false" runat="server">
							<h1>Bummer... Something Bad Happened!</h1>
						
							<p>We tried hard to think of everything that could go wrong and work through it, but I guess we missed this error. Feel free to
							post the error below into the community and someone might be able to assist.</p>
						
							<asp:Label id="lErrorDetails" runat="server"></asp:Label>
							
						</asp:Panel>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
		</form>
		
		<script language="javascript">
			function validateDbConnection()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pDatabaseConfig .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				        $(this).parent().addClass('has-error');
				     	formValid = false;
				     } else {
				        $(this).parent().removeClass('has-error');
					 }
				});
				
			    
			    if (formValid) {
				    return true;
				      
			    } else {
				    return false;
			    }
			}

          $(document).ready(function() {
              $('body').on('click', '#show-password', function (e) {

                  field = $('#txtPassword');
                  if (field.attr('type') == "text") { new_type = "password"; } else { new_type = "text"; }
                  new_field = field.clone();
                  new_field.attr("id", field.attr('id'));
                  new_field.attr("type", new_type);
                  field.replaceWith(new_field);
              });

              
          });
			

		</script>
		
	</body>

</html>



<script language="CS" runat="server">
	
	/* 
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
		Private Helper Methods
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><> 
	*/

	private void ProcessPageException(string errorMessage) {
		
		// hide all panels on page
		foreach (Control control in this.Controls)
        {
            if (control is Panel)
            {
                control.Visible = false;
            }
        }
        
        // show error panel
        pError.Visible = true;
        
        // add error message
        lErrorDetails.Text = errorMessage;
	}
	
	
	private bool CheckRockNotInstalled(out string errorDetails) {
		
		bool checksPassed = false;
		errorDetails = string.Empty;

		// check for rock database connection string
		string connectionStringFile = Server.MapPath(".") + @"\web.ConnectionStrings.config";
		
		
		if (File.Exists(connectionStringFile)) {
			errorDetails = "Rock is already installed on this server.";
			checksPassed = false;
		} else {
			errorDetails = "Rock is not yet installed on this machine.";
			checksPassed = true;
		}

		return checksPassed;
	}
	
	
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
	private bool CheckFileSystemPermissions(out string errorDetails) {
		
		bool checksPassed = false;
		errorDetails = string.Empty;

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
        	errorDetails += "The username " + userName + " does not have write access to the server's file system.";
        } else {
        	errorDetails += "Your server's file permissions look correct.";
        	checksPassed = true;
        }

		return checksPassed;
	}
	
	private bool CheckDotNetVersion(out string errorDetails) {
		
		bool checksFailed = false;
		errorDetails = string.Empty;

		// check .net
		// ok this is not easy as .net 4.5 actually reports as 4.0.30319.269 so instead we need to search for the existence of an assembly that
		// only exists in 4.5 (could also look for Environment.Version.Major == 4 && Environment.Version.Revision > 17000 but this is not future proof)
		// sigh... Microsoft... :)
		if (!(Type.GetType("System.Reflection.ReflectionContext", false) != null)) {
			errorDetails = "The server does not have the correct .Net runtime.  You have .Net version " + System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString() + " the Rock ChMS version requires " + dotNetVersionRequired + ".";
		} else {
			errorDetails += "You have the correct version of .Net (4.5+).";
			checksFailed = true;
		}

		return checksFailed;
	}
	
	private bool CheckIisVersion(out string errorDetails) {
		
		bool checksPassed = false;
		errorDetails = string.Empty;

		RegistryKey parameters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\W3SVC\\Parameters");
		string iisVersion = parameters.GetValue("MajorVersion") + "." + parameters.GetValue("MinorVersion");
		
		if (double.Parse(iisVersion) >= iisVersionRequired) {
			errorDetails = "Your IIS version is correct.  You have version " + iisVersion + ".";
			checksPassed = true;
		} else {
			errorDetails = "The server's IIS version is not correct.  You have version " + iisVersion + " Rock requires version " + iisVersionRequired.ToString() + " or greater.";
		}

		return checksPassed;
	}
	
	private bool CheckSqlServerConnection(string servername, string database, string username, string password, out string checkMessages) {
			
			checkMessages = string.Empty;
			bool canConnect = false;
			
			// setup connection
			string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
			SqlConnection testConnection = new SqlConnection(connectionString);
			
			// try connection
			try
			{
			    testConnection.Open();
			    canConnect = true;
			}
			catch(Exception ex)
			{
			    checkMessages = "<div class='alert alert-danger'><p><strong>Yikes!</strong><p> Could not connect to the database with the information provided. Please check the information provided. <p><small>" + ex.Message + "</small></p></div>";
			    canConnect = false;
			}
			finally {
				testConnection = null;
			}
			
			return canConnect;
	}
	
	private bool CheckSqlServerVersion(string servername, string database, string username, string password, out string checkMessages) {
			
			checkMessages = string.Empty;
			string version = "0";
			string versionInfo = string.Empty;
			bool versionPassed = false;
			
			// setup connection
			string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
			SqlConnection testConnection = new SqlConnection(connectionString);
			
			// try connection
			try
			{
			    testConnection.Open();
			    SqlCommand versionCommand= new SqlCommand("SELECT SERVERPROPERTY('productversion'), @@Version", testConnection);
			    
			    SqlDataReader versionReader = versionCommand.ExecuteReader();

			    while(versionReader.Read())
			    {
			        version = versionReader[0].ToString();
			        versionInfo = versionReader[1].ToString();
			    }
			    
			    string[] versionParts = version.Split('.');
			    
			    int majorVersion = -1;
			    Int32.TryParse(versionParts[0], out majorVersion);
			    	
			    if (majorVersion >= 10) {
			    	versionPassed = true;
			    }
			    
			    checkMessages = "You are running SQL Server version: " + versionInfo.Split('-')[0].ToString().Replace("(RTM)", "").Trim();
			}
			catch(Exception ex)
			{
			    checkMessages = "<div class='alert alert-danger'><p><strong>Yikes!</strong></p> Could not connect to the database with the information provided. Please check the information provided.</div>";
			    versionPassed = false;
			}
			finally {
				testConnection = null;
			}
			
			return versionPassed;
	}
	
	private bool CheckSqlServerPermissions(string servername, string database, string username, string password, out string checkMessages) {
			
			checkMessages = string.Empty;
			bool permissionsCorrect = false;
			
			// setup connection
			string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
			SqlConnection testConnection = new SqlConnection(connectionString);
			
			// try connection
			try
			{
			    testConnection.Open();
			    
			    // test permissions by creating and dropping a table
			    SqlCommand testCommand= new SqlCommand("CREATE TABLE InstallText (Name TEXT); ", testConnection);
			    testCommand.ExecuteNonQuery();
			    
			    testCommand= new SqlCommand("DROP TABLE InstallText; ", testConnection);
			    testCommand.ExecuteNonQuery();
			    
			    permissionsCorrect = true;
			    checkMessages = "Database security looks good.  We're able to create tables.";
			}
			catch(Exception ex)
			{
			    checkMessages = "Check the permissions of the database user account. Be sure it has permissions to create and drop tables.";
			    permissionsCorrect = false;
			}
			finally {
				testConnection = null;
			}
			
			return permissionsCorrect;
	}
	
	private bool CheckSqlServerEmpty(string servername, string database, string username, string password, out string checkMessages) {
			
			checkMessages = string.Empty;
			bool dbEmpty = false;
			
			// setup connection
			string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
			SqlConnection testConnection = new SqlConnection(connectionString);
			
			// try connection
			try
			{
			    testConnection.Open();
			    SqlCommand emptyCommand= new SqlCommand("SELECT count([name]) FROM sysobjects WHERE [type] in ('P', 'U', 'V') AND category = 0", testConnection);
			    
			    // get count of db objects
			    Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();
			    
			    if (objectCount > 0 ) {
			    	checkMessages = "It appears that the database you provided is not empty.  The RockChMS must be installed into a blank database.";
			    } else {
			    	checkMessages = "Your database is empty.  Just the way we like it.";
			    	dbEmpty = true;
			    	
			    }
			    
			}
			catch(Exception ex)
			{
			    checkMessages = "An error occurred when trying to check if the database is empty." + ex.Message;
			    dbEmpty = false;
			}
			finally {
				testConnection = null;
			}
			
			return dbEmpty;
	}


</script>