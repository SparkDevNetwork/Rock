<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>


<%@ Import Namespace="System.Web.Configuration"  %>
<%@ Import Namespace="System.Configuration"  %>
<%@ Import Namespace="System.Collections.Specialized"  %>
<%@ Import Namespace="System.Xml"  %>

<%@ Import Namespace="Rock"  %>

<script language="CS" runat="server">

		
	void Page_Load(object sender, EventArgs e)
	{

	}

    
    void AdminNext_Click(Object sender, EventArgs e)
    {
    	// update the admin password
    	var service = new Rock.Model.UserLoginService();
		var user = service.GetByUserName( "Admin" );
		if ( user != null )
		{
		    user.UserName = txtAdminUsername.Text.Trim();
		    service.ChangePassword( user, txtAdminPassword.Text.Trim() );
		    service.Save( user, null );
		}
		
		pAdminAccount.Visible = false;
		pOrganization.Visible = true;
    }
    
    void OrgNext_Click(Object sender, EventArgs e)
    {
    	// save org settings
    	var globalAttributesCache = Rock.Web.Cache.GlobalAttributesCache.Read();
        globalAttributesCache.SetValue( "OrganizationName", txtOrgName.Text, null, true );
    	globalAttributesCache.SetValue("OrganizationName", txtOrgName.Text, null, true);
    	globalAttributesCache.SetValue("OrganizationEmail", txtOrgEmail.Text, null, true);
    	globalAttributesCache.SetValue("OrganizationPhone", txtOrgPhone.Text, null, true);
    	globalAttributesCache.SetValue("OrganizationWebsite", txtOrgWebsite.Text, null, true);
    	
    	pOrganization.Visible = false;
    	pEmailSettings.Visible = true;
    }
    
    void EmailNext_Click(Object sender, EventArgs e)
    {
    	// save email settings
    	var globalAttributesCache = Rock.Web.Cache.GlobalAttributesCache.Read();
    	globalAttributesCache.SetValue("SMTPServer", txtEmailServer.Text, null, true);
    	globalAttributesCache.SetValue("SMTPPort", txtEmailServerPort.Text, null, true);
    	globalAttributesCache.SetValue("SMTPUseSSL", cbEmailUseSsl.Checked.ToString(), null, true);
    	
    	if (txtEmailUsername.Text.Length > 0)
    		globalAttributesCache.SetValue("SMTPUsername", txtEmailUsername.Text, null, true);
    	else
    		globalAttributesCache.SetValue("SMTPUsername", "", null, true);
    		
    	if (txtEmailPassword.Text.Length > 0)	
    		globalAttributesCache.SetValue("SMTPPassword", txtEmailPassword.Text, null, true);
    	else
    		globalAttributesCache.SetValue("SMTPPassword", "", null, true);

    	globalAttributesCache.SetValue("EmailExceptionsList", txtEmailExceptions.Text, null, true);
    		
    	
    	pEmailSettings.Visible = false;
    	pFinished.Visible = true;
    	
    	// delete install files
    	File.Delete(Server.MapPath(".") + @"\waiting.gif");
    	File.Delete(Server.MapPath(".") + @"\Install.aspx");
    	File.Delete(Server.MapPath(".") + @"\Configure.aspx");
    	File.Delete(Server.MapPath(".") + @"\RockInstall.zip");
    	File.Delete(Server.MapPath(".") + @"\Start.aspx");
    }
    
    
</script>

<html>
	<head>
		<title>Rock ChMS Installer...</title>

		<link href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' rel='stylesheet' type='text/css'>
		<link href="http://www.rockchms.com/installer/css/bootstrap.min.css" rel="stylesheet">
		<link href="//netdna.bootstrapcdn.com/font-awesome/2.0/css/font-awesome.css" rel="stylesheet">
		
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
						
						<asp:Literal id="lTest" runat="server"></asp:Literal>
						
						<asp:Panel id="pAdminAccount" Visible="true" runat="server">
							<h1>Rock Configuration</h1>
						
							<p>Rock is installed now let's do some quick configuration.</p>
						
							<h4>Administrator's Account</h4>
							<p>Please provide a username and password for the administrator's account</p>
							<div class="control-group">
								<label class="control-label" for="inputEmail">Administrator Username</label>
								<div class="controls">
									<asp:TextBox ID="txtAdminUsername" runat="server" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Administrator Password</label>
								<div class="controls">
									<asp:TextBox ID="txtAdminPassword" runat="server" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
						
							<asp:LinkButton id="btnAdminNext" runat="server" OnClientClick="return validateAdminAccount();" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="AdminNext_Click"></asp:LinkButton>
						</asp:Panel>
						
						<asp:Panel id="pOrganization" Visible="false" runat="server">
							<h1>Organization Information</h1>
						
							<p>Please enter some information about your organization.  These fields are used to provide default information in the database. It
								is in no way shared with us or anyone else.
							</p>

							<div class="control-group">
								<label class="control-label" for="inputEmail">Organization Name</label>
								<div class="controls">
									<asp:TextBox ID="txtOrgName" runat="server" placeholder="Your Church" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Organization Default Email Address</label>
								<div class="controls">
									<asp:TextBox ID="txtOrgEmail" runat="server" placeholder="info@yourchurch.com" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Organization Phone Number</label>
								<div class="controls">
									<asp:TextBox ID="txtOrgPhone" placeholder="(555) 555-5555" runat="server" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Organization Website</label>
								<div class="controls">
									<asp:TextBox ID="txtOrgWebsite" placeholder="www.yourchurch.com" runat="server" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
						
							<asp:LinkButton id="btnOrgNext" runat="server" OnClientClick="return validateOrgSettings();" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="OrgNext_Click"></asp:LinkButton>
						</asp:Panel>
						
						<asp:Panel id="pEmailSettings" Visible="false" runat="server">
							<h1>Email Server Settings</h1>
						
							<p>Email is an essential part of the Rock ChMS.  Please provide a few details about your email environment.  You can change 
							these values at an time inside the app. 
							</p>

							<div class="control-group">
								<label class="control-label" for="inputEmail">Email Server</label>
								<div class="controls">
									<asp:TextBox ID="txtEmailServer" runat="server" placeholder="mail.yourchurch.com" CssClass="required-field" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Email Server SMTP Port (default is 25)</label>
								<div class="controls">
									<asp:TextBox ID="txtEmailServerPort" runat="server" placeholder="mail.yourchurch.com" CssClass="required-field" Text="25"></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Use SSL For SMTP (default no)</label>
								<div class="controls">
									<asp:CheckBox ID="cbEmailUseSsl" runat="server" />
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Email Username (optional)</label>
								<div class="controls">
									<asp:TextBox ID="txtEmailUsername" runat="server" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Email Password (optional)</label>
								<div class="controls">
									<asp:TextBox ID="txtEmailPassword" runat="server" Text=""></asp:TextBox>
								</div>
							</div>
							
							<div class="control-group">
								<label class="control-label" for="inputEmail">Email Address to Send Error Reports To (optional)</label>
								<div class="controls">
									<asp:TextBox ID="txtEmailExceptions" placeholder="administrator@yourchurch.com" runat="server" Text=""></asp:TextBox>
								</div>
							</div>
						
							<asp:LinkButton id="btnEmailNext" runat="server" OnClientClick="return validateEmailSettings();" Text="Next <i class='icon-chevron-right'></i>"  CssClass="btn btn-inverse next-step" OnClick="EmailNext_Click"></asp:LinkButton>
						</asp:Panel>
						
						<asp:Panel id="pFinished" Visible="false" runat="server">
							<h1>Congratulations!!!</h1>
						
							<p>
								You have finished the install and initial configuration of the Rock ChMS! All that's left to do is to get started.
							</p>
							
							<p></p>

							<asp:LinkButton id="btnDone" runat="server" OnClientClick="return redirectHome();" Text="Let's Get Started <i class='icon-bolt'></i>"  CssClass="btn btn-inverse next-step" ></asp:LinkButton>
						</asp:Panel>
						
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
		</form>
		
		<script language="javascript">
			function validateAdminAccount()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pAdminAccount .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				     	$(this).parent().parent().addClass('error');
				     	formValid = false;
				     } else {
					 	$(this).parent().parent().removeClass('error');
					 }
				});
				
			    
			    if (formValid) {
				    return true;
				      
			    } else {
				    return false;
			    }
			}
			
			function validateOrgSettings()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pOrganization .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				     	$(this).parent().parent().addClass('error');
				     	formValid = false;
				     } else {
					 	$(this).parent().parent().removeClass('error');
					 }
				});
				
			    
			    if (formValid) {
				    return true;
				      
			    } else {
				    return false;
			    }
			}
			
			function validateEmailSettings()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pEmailSettings .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				     	$(this).parent().parent().addClass('error');
				     	formValid = false;
				     } else {
					 	$(this).parent().parent().removeClass('error');
					 }
				});
				
			    
			    if (formValid) {
				    return true;
				      
			    } else {
				    return false;
			    }
			}
			
			function redirectHome()
			{
				window.location = "/"
			}
		</script>
		
	</body>

</html>



