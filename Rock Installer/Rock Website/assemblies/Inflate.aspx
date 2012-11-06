<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>


<%@ Import Namespace="System.Web.Configuration"  %>
<%@ Import Namespace="System.Configuration"  %>
<%@ Import Namespace="System.Collections.Specialized"  %>
<%@ Import Namespace="System.Xml"  %>

<%@ Import Namespace="Ionic.Zip"  %>



<script language="CS" runat="server">
	// you want the Rock ChMS, well here we go!
	
	//
	// some constants
	//
	
		
	void Page_Load(object sender, EventArgs e)
	{

		// might add check here for the necessary zip file and zip assemby? oh and the rock.dll so the migrator can be found...
		
	}

    
    void NextStep_Click(Object sender, EventArgs e)
    {
    	using (ZipFile zip = ZipFile.Read(Server.MapPath(".") + @"\RockInstall.zip"))
		{
			//zip.ExtractAll(Server.MapPath("."), ExtractExistingFileAction.OverwriteSilently);    
		} 
    	
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
    	
    	Response.Redirect("Configure.aspx");	
    }

</script>

<html>
	<head>
		<title>Rock ChMS Installer...</title>
		<link href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' rel='stylesheet' type='text/css'>
		<link href="//netdna.bootstrapcdn.com/font-awesome/2.0/css/font-awesome.css" rel="stylesheet">
		<link href="http://www.rockchms.com/installer/css/bootstrap.min.css" rel="stylesheet">
		
		<script src="http://www.rockchms.com/installer/scripts/jquery-1.8.2.min.js" type="text/javascript"></script>
		
		<link href="http://www.rockchms.com/assets/images/rock-chms.ico" rel="shortcut icon">
		<link href="http://www.rockchms.com/assets/images/rock-chms.ico" type="image/ico" rel="icon">
		
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
			    margin-top: 115px;
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
						<asp:Label ID="lTitle" runat="server" Text="<h1>You're One Step Closer...</h1>" />
						
						<asp:Label ID="lOutput" runat="server" Text="The Rock ChMS will now be installed on your web server.  Please provide the database information below then click the 'Next' button to continue with the installation process." />
						
						
						<div id="config-info">
							 <div class="control-group">
								<label class="control-label" for="inputEmail">Database Server</label>
								<div class="controls">
									<asp:TextBox ID="txtServerName" runat="server" CssClass="required-field"></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Database Name</label>
								<div class="controls">
									<asp:TextBox ID="txtDatabaseName" runat="server" CssClass="required-field"></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Database Username</label>
								<div class="controls">
									<asp:TextBox ID="txtUsername" runat="server" CssClass="required-field"></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Database Password</label>
								<div class="controls">
									<asp:TextBox ID="txtPassword" runat="server" CssClass="required-field"></asp:TextBox>
								</div>
							</div>
						 </div> 
					</div>
					
					<asp:LinkButton id="btnNext" runat="server" OnClientClick="return startInflate();" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="NextStep_Click"></asp:LinkButton>
					
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
		</form>
		
		<script language="javascript">
			function startInflate()
			{
 			    
			    var formValid = true;

			    // ensure that all values were provided
			    $("#config-info .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				     	$(this).parent().parent().addClass('error');
				     	formValid = false;
				     } else {
					 	$(this).parent().parent().removeClass('error');
					 }
				});
				
			    
			    
			    if (formValid) {
				    // disable start button
				    var nextButton = document.getElementById("btnNext");
				    nextButton.style.display = "none";
				    $('#config-info').hide();
	
				    // blank out messages
				    document.getElementById("lOutput").innerHTML = '<p>Starting the installation of the Rock ChMS on your server.  This could take a moment...</p><p class="wait"><img src="http://www.rockchms.com/installer/assets/images/waiting.gif" /></p>'; 
				    
				    return true;
				      
			    } else {
				    return false;
			    }

			}
		</script>
		
	</body>

</html>