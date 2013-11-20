<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System"  %>

<%@ Import Namespace="System.Web.Configuration"  %>
<%@ Import Namespace="System.Configuration"  %>
<%@ Import Namespace="System.Collections.Specialized"  %>
<%@ Import Namespace="System.Xml"  %>

<%@ Import Namespace="Rock"  %>
<%@ Import Namespace="Rock.Data"  %>
<%@ Import Namespace="Rock.Model"  %>

<script language="CS" runat="server">


    const string rockLogoIco = "http://rockchms.blob.core.windows.net/install/rock-chms.ico";
    const string rockStyles = "http://rockchms.blob.core.windows.net/install/install.css";
		
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

        // add timezones to dropdown
        foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
        {
            ddTimeZone.Items.Add(new ListItem(timeZone.DisplayName, timeZone.Id));
        }
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

        // update the external site domain with the value provided
        /*SiteDomainService domainService = new SiteDomainService();
        SiteDomain domain = domainService.Get(new Guid(Rock.SystemGuid.SiteDomain.SITEDOMAIN_EXTERNAL));
        domain.Domain = txtOrgWebsite.Text;
        domainService.Save(domain, null);*/
        
        // set timezone value
        Configuration rockWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        rockWebConfig.AppSettings.Settings["OrgTimeZone"].Value = ddTimeZone.SelectedValue;
        rockWebConfig.Save();
    	
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
        string installDirectory = Server.MapPath(".");
        File.Delete(installDirectory + @"\waiting.gif");
        File.Delete(installDirectory + @"\Install.aspx");
        File.Delete(installDirectory + @"\Configure.aspx");
        File.Delete(installDirectory + @"\RockInstall.zip");
        File.Delete(installDirectory + @"\Start.aspx");
    }
    
    
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
							<div class="form-group">
								<label class="control-label" for="inputEmail">Administrator Username</label>
								<asp:TextBox ID="txtAdminUsername" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Administrator Password</label>
								<div class="row">
                                    <div class="col-md-8"><asp:TextBox ID="txtAdminPassword" TextMode="Password" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox></div>
                                    <div class="col-md-4" style="padding-top: 6px;">
                                        <input id="show-password-admin" type="checkbox" />
                                        <label for="show-password-admin" id="show-password-admin-label" style="font-weight:normal;">Show Password</label>
                                    </div>
								</div>
                                
							</div>
						
                            <div class="btn-list">
							    <asp:LinkButton id="btnAdminNext" runat="server" OnClientClick="return validateAdminAccount();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="AdminNext_Click"></asp:LinkButton>
						    </div>
                        </asp:Panel>
						
						<asp:Panel id="pOrganization" Visible="false" runat="server">
							<h1>Organization Information</h1>
						
							<p>Please enter some information about your organization.  These fields are used to provide default information in the database. It
								is in no way shared with us or anyone else.
							</p>

							<div class="form-group">
								<label class="control-label" for="inputEmail">Organization Name</label>
								<asp:TextBox ID="txtOrgName" runat="server" placeholder="Your Church" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Organization Default Email Address</label>
								<asp:TextBox ID="txtOrgEmail" runat="server" placeholder="info@yourchurch.com" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Organization Phone Number</label>
								<asp:TextBox ID="txtOrgPhone" placeholder="(555) 555-5555" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Organization Website</label>
								<asp:TextBox ID="txtOrgWebsite" placeholder="www.yourchurch.com" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>

                            <div class="form-group">
								<label class="control-label" for="inputEmail">Organization Timezone</label>
								<asp:DropDownList ID="ddTimeZone" runat="server" CssClass="form-control"></asp:DropDownList>
							</div>
						
                            <div class="btn-list">
							    <asp:LinkButton id="btnOrgNext" runat="server" OnClientClick="return validateOrgSettings();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="OrgNext_Click"></asp:LinkButton>
						    </div>
                       </asp:Panel>
						
						<asp:Panel id="pEmailSettings" Visible="false" runat="server">
							<h1>Email Server Settings</h1>
						
							<p>Email is an essential part of the Rock ChMS.  Please provide a few details about your email environment.  You can change 
							these values at an time inside the app. 
							</p>

							<div class="form-group">
								<label class="control-label" for="inputEmail">Email Server</label>
								<asp:TextBox ID="txtEmailServer" runat="server" placeholder="mail.yourchurch.com" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Email Server SMTP Port (default is 25)</label>
								<asp:TextBox ID="txtEmailServerPort" runat="server" placeholder="mail.yourchurch.com" CssClass="required-field form-control" Text="25"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Use SSL For SMTP (default no)</label>
								<asp:CheckBox ID="cbEmailUseSsl" runat="server" />
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Email Username (optional)</label>
								<asp:TextBox ID="txtEmailUsername" runat="server" Text="" CssClass="form-control"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Email Password (optional)</label>
								<div class="row">
                                    <div class="col-md-8"><asp:TextBox ID="txtEmailPassword" TextMode="Password" runat="server" Text="" CssClass="form-control"></asp:TextBox></div>
								    <div class="col-md-4" style="padding-top: 6px;">
                                        <input id="show-password-email" type="checkbox" />
                                        <label for="show-password-email" id="show-password-email-label" style="font-weight:normal;">Show Password</label>
                                    </div>
                                </div>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Email Address to Send Error Reports To (optional)</label>
								<asp:TextBox ID="txtEmailExceptions" placeholder="administrator@yourchurch.com" CssClass="form-control" runat="server" Text=""></asp:TextBox>
							</div>
						
							<div class="btn-list">
                                <asp:LinkButton id="btnEmailNext" runat="server" OnClientClick="return validateEmailSettings();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="EmailNext_Click"></asp:LinkButton>
						    </div>
                        </asp:Panel>
						
						<asp:Panel id="pFinished" Visible="false" runat="server">
							<h1>Congratulations!!!</h1>
						
							<p>
								You have finished the install and initial configuration of the Rock ChMS! All that's left to do is login and get started.
							</p>
							
							<p></p>

                            <div class="btn-list">
							    <a class="btn btn-primary" href="./"><i class='fa fa-road'></i> Let's Get This Show On The Road</a>
                            </div>
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
			
			function validateOrgSettings()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pOrganization .required-field").each( function(index, value) {
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
			
			function validateEmailSettings()
			{
			    var formValid = true;

			    // ensure that all values were provided
			    $("#pEmailSettings .required-field").each( function(index, value) {
				    if(this.value.length == 0){
				        $(this).closest('.form-group').addClass('has-error');
				     	formValid = false;
				     } else {
				        $(this).closest('.form-group').removeClass('has-error');
					 }
				});
				
			    
			    if (formValid) {
			        // add spinner to button to tell user something is happening
			        $('#btnEmailNext i').attr("class", "fa fa-spinner fa-spin");
			        return true;
			    } else {
				    return false;
			    }
			}
			
			function redirectHome()
			{
				window.location = "/"
			}

			$(document).ready(function () {
			    $('body').on('click', '#show-password-admin', function (e) {

			        field = $('#txtAdminPassword');
			        if (field.attr('type') == "text") { new_type = "password"; } else { new_type = "text"; }
			        new_field = field.clone();
			        new_field.attr("id", field.attr('id'));
			        new_field.attr("type", new_type);
			        field.replaceWith(new_field);
			    });

			    $('body').on('click', '#show-password-email', function (e) {

			        field = $('#txtEmailPassword');
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



