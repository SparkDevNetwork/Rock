<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.Data.SqlClient"  %>
<%@ Import Namespace="Microsoft.Win32"  %>
<%@ Import Namespace="Ionic.Zip"  %>
<%@ Import Namespace="System.Web.Security" %>

<%@ Import Namespace="Rock.Install.Utilities" %>


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
    const string rockWelcomeImg = "http://rockchms.blob.core.windows.net/install/welcome.jpg";
	
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

        // show db settings if they are in session
        if (Session["dbServer"] != null)
        {
            txtServerName.Text = Session["dbServer"].ToString();
            txtDatabaseName.Text = Session["dbDatabasename"].ToString();
            txtUsername.Text = Session["dbUsername"].ToString();
            txtPassword.Text = Session["dbPassword"].ToString();
        }
        
        
    }
	
	void DbConfigNext_Click(Object sender, EventArgs e)
    {
               
        // check settings
    	string databaseMessages = string.Empty; 
        
        // write database settings to session
        if (txtServerName.Text != string.Empty) { 
            Session["dbServer"] = txtServerName.Text;
            Session["dbDatabasename"] = txtDatabaseName.Text;
            Session["dbUsername"] = txtUsername.Text;
            Session["dbPassword"] = txtPassword.Text;
        }
        else
        {
            txtServerName.Text = Session["dbServer"].ToString();
            txtDatabaseName.Text = Session["dbDatabasename"].ToString();
            txtUsername.Text = Session["dbUsername"].ToString();
            txtPassword.Text = Session["dbPassword"].ToString();
        }

        bool canConnect = EnvironmentChecks.CheckSqlLogin(txtServerName.Text, txtUsername.Text, txtPassword.Text);

    	if (!canConnect) {
    		lDatabaseMessages.Text = String.Format("<div class='alert alert-warning'>Could not connect to '{0}' as '{1}'.</div>", txtServerName.Text, txtUsername.Text);
            
            pWelcome.Visible = false;
            pDatabaseConfig.Visible = true;
    		
            return;
    	} else {

            lDatabaseMessages.Text = string.Empty;
            
		    // run environment checks
		    string outputMessages = string.Empty;
		    string checkResults = string.Empty;
		    bool environmentClean = true;
		    
		    // check .Net version
            if (EnvironmentChecks.CheckDotNetVersion(out checkResults))
            {
                outputMessages += "<li><i class='fa fa-check-circle pass'></i> " + checkResults + "</li>";
    		} else {
                outputMessages += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockchms.com/installer/help/dotnet-version.html' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
		    
		    // check web server permissions
            if (EnvironmentChecks.CheckFileSystemPermissions(Server.MapPath("."), out checkResults))
            {
                outputMessages += "<li><i class='fa fa-check-circle pass'></i> " + checkResults + "</li>";
    		} else {
                outputMessages += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockchms.com/installer/help/filesystem-permissions.html' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
    		
    		// check IIS version
            if (EnvironmentChecks.CheckIisVersion(out checkResults))
            {
                outputMessages += "<li><i class='fa fa-check-circle pass'></i> " + checkResults + "</li>";
    		} else {
                outputMessages += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockchms.com/installer/help/iis-version.html' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
    			environmentClean = false;
    		}
		    
		    // check sql server environment
            if (EnvironmentChecks.CheckSqlServer( txtServerName.Text, txtUsername.Text, txtPassword.Text, txtDatabaseName.Text, out checkResults))
            {
                outputMessages += "<li><i class='fa fa-check-circle pass'></i> " + checkResults + "</li>";
    		} else {
                outputMessages += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + "</li>";
    			environmentClean = false;
    		}
    		
    		// check that rock is not installed already
            if (EnvironmentChecks.CheckRockNotInstalled(Server.MapPath("."), out checkResults))
            {
    			
    		} else {
                outputMessages += "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockchms.com/installer/help/rock-installed.html' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
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
		    	lTestEnvResults.Text = "The server environment doesn't currently meet all of the requirements.  That's OK, we'll try to help you solve the issues.";
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
            rockWebConfig.AppSettings.Settings["DataEncryptionKey"].Value = InstallUtilities.GenerateRandomDataEncryptionKey();
            
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
        pWelcome.Visible = false;
        pTestEnv.Visible = false;
		pDatabaseConfig.Visible = true;

        // show db settings if they are in session
        if (Session["dbServer"] != null)
        {
            txtServerName.Text = Session["dbServer"].ToString();
            txtDatabaseName.Text = Session["dbDatabasename"].ToString();
            txtUsername.Text = Session["dbUsername"].ToString();
            txtPassword.Text = Session["dbPassword"].ToString();
        }
    }
    
    
    void DownloadNext_Click(Object sender, EventArgs e)
    {
    	try {
            Response.Redirect("Configure.aspx");
    	}
    	catch (Exception ex) {
    		ProcessPageException(ex.Message);
            lDownloadDetails.Text += "Exception on redirect";
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
        <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet">
        <link rel="stylesheet" href="<%=rockStyles %>">
		
        <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>
        		
		<link href="<%=rockLogoIco %>" rel="shortcut icon">
		<link href="<%=rockLogoIco %>" type="image/ico" rel="icon">
		
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
					
					<div id="content-box">
						<asp:Panel id="pWelcome" Visible="true" runat="server">
							<h1>Rock Installer</h1>
							
                            <img src="<%=rockWelcomeImg %>"  />
							<asp:Label id="lTest" runat="server"></asp:Label>
							
                            <asp:Literal ID="lSslWarning" runat="server">
                                <div class="alert alert-warning">
                                    <p><strong>Just A Thought...</strong></p>
                                    Looks like you're not running over an encrypted connection (SSL).  Since you will be providing passwords for configuring
                                    your database and Rock install you may wish to run the install over an encrypted connection.
                                </div>
							</asp:Literal>

							<div class="btn-list">
								<asp:LinkButton id="btnWelcome" runat="server" Text="Get Started <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="WelcomeNext_Click"></asp:LinkButton>
							</div>
						</asp:Panel>
						
						<asp:Panel id="pDatabaseConfig" Visible="false" runat="server">
						
							<h1>Database Configuration</h1>
						
							<p>Please provide configuration information to the database below.  This information should come from your server
							   administrator or hosting provider.</p>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Server</label>
								<asp:TextBox ID="txtServerName" Text="" runat="server" CssClass="required-field form-control"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Name</label>
								<asp:TextBox ID="txtDatabaseName" Text="" runat="server" CssClass="required-field form-control"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Username</label>
								<asp:TextBox ID="txtUsername" Text="" runat="server" CssClass="required-field form-control"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Database Password</label>
								
                                <div class="row">
                                    <div class="col-md-8">
                                        <asp:TextBox ID="txtPassword" Text="" TextMode="Password" runat="server" CssClass="required-field form-control"></asp:TextBox>
                                    </div>
                                    <div class="col-md-4" style="padding-top: 6px;">
                                        <input id="show-password" type="checkbox" />
                                        <label for="show-password" id="show-password-label" style="font-weight:normal;">Show Password</label>
                                    </div>
                                </div>
							</div>
							
							<asp:Literal id="lDatabaseMessages" runat="server"></asp:Literal>
							
							<div class="btn-list">
								<asp:LinkButton id="btnDbConfig" runat="server" OnClientClick="return validateDbConnection();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="DbConfigNext_Click"></asp:LinkButton> 
							</div>
						</asp:Panel>
						
						<asp:Panel id="pTestEnv" Visible="false" runat="server">
							<h1><asp:Label id="lTestEnvTitle" runat="server"></asp:Label></h1>
						
							<asp:Label id="lTestEnvResults" runat="server"></asp:Label>
						
							<asp:Label id="lTestEnvDetails" runat="server"></asp:Label>
							
							<div class="btn-list">
							
								<asp:LinkButton id="btnEnvBack" runat="server"  Text="<i class='fa fa-chevron-left'></i> Back"  CssClass="btn btn-default" OnClick="EnvBack_Click"></asp:LinkButton>  
								<asp:LinkButton id="btnTryAgain" runat="server"  Text="Try Again <i class='fa fa-refresh'></i>"  CssClass="btn btn-primary" OnClick="DbConfigNext_Click"></asp:LinkButton> 
								<asp:LinkButton id="btnEnvNext" runat="server"  Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="EnvNext_Click"></asp:LinkButton> 
							</div>
						</asp:Panel>
						
						<asp:Panel id="pDownloadZip" Visible="false" runat="server">
							<h1><asp:Label id="lDownloadTitle" runat="server"></asp:Label></h1>
						
							<asp:Label id="lDownloadDetails" runat="server"></asp:Label>
							
							<div class="btn-list">		
								<asp:LinkButton id="btnDownloadNext" runat="server" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="DownloadNext_Click"></asp:LinkButton> 
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
				        $(this).closest('.form-group').addClass('has-error');
				     	formValid = false;
				     } else {
				        $(this).closest('.form-group').removeClass('has-error');
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
	
	
	
	


</script>