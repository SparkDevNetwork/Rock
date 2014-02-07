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

<%@ Import Namespace="Rock.Install.Utilities" %>

<script language="CS" runat="server">
  
    
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
        pHosting.Visible = true;

        // add timezones to dropdown
        foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
        {
            ddTimeZone.Items.Add(new ListItem(timeZone.DisplayName, timeZone.Id));
        }
    }

    void AddressesNext_Click(Object sender, EventArgs e)
    {
        // clean addresses
        string internalAddress = InstallUtilities.CleanBaseAddress(txtInternalAddress.Text);
        string publicAddress = InstallUtilities.CleanBaseAddress(txtPublicAddress.Text);
        
        // save addresses
        var globalAttributesCache = Rock.Web.Cache.GlobalAttributesCache.Read();
        globalAttributesCache.SetValue("InternalApplicationRoot", internalAddress, null, true);
        globalAttributesCache.SetValue("PublicApplicationRoot", publicAddress, null, true);

        // set timezone value
        Configuration rockWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        rockWebConfig.AppSettings.Settings["OrgTimeZone"].Value = ddTimeZone.SelectedValue;
        rockWebConfig.Save();
        
        // set organization address to their public address
        txtOrgWebsite.Text = publicAddress.TrimEnd('/');
        
        // set the site domains with the addresses above
        SiteService siteService = new SiteService();
        Site internalSite = siteService.Get(new Guid("C2D29296-6A87-47A9-A753-EE4E9159C4C4"));
        
        SiteDomain internalDomain = new SiteDomain();
        internalDomain.Domain = InstallUtilities.GetDomainFromString(internalAddress);
        internalDomain.IsSystem = false;

        internalSite.SiteDomains.Add(internalDomain);
        siteService.Save(internalSite, null);

        Site externalSite = siteService.Get(new Guid("F3F82256-2D66-432B-9D67-3552CD2F4C2B"));

        SiteDomain externalDomain = new SiteDomain();
        externalDomain.Domain = InstallUtilities.GetDomainFromString(publicAddress);
        externalDomain.IsSystem = false;

        externalSite.SiteDomains.Add(externalDomain);

        siteService.Save(externalSite, null);
        
        
        pHosting.Visible = false;
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
        globalAttributesCache.SetValue("OrganizationWebsite", txtOrgWebsite.Text.Replace("http://", "").Replace("https://", ""), null, true);
    	
    	pOrganization.Visible = false;
    	pEmailSettings.Visible = true;
    }
    
    void EmailNext_Click(Object sender, EventArgs e)
    {
        using (new Rock.Data.UnitOfWorkScope())
        {
            // save email settings
            AttributeService attribService = new AttributeService();
            Rock.Model.Attribute aSmtpServer = attribService.Get(new Guid("6CFFDF99-E93A-49B8-B440-0EF93878A51F"));
            Rock.Model.Attribute aSmtpPort = attribService.Get(new Guid("C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF"));
            Rock.Model.Attribute aSmtpSsl = attribService.Get(new Guid("B3B2308B-6CD2-4853-8220-C80D861F5D3C"));
            Rock.Model.Attribute aSmtpUsername = attribService.Get(new Guid("2CE8D3AC-F851-462C-93D5-DB82F48DDBFD"));
            Rock.Model.Attribute aSmtpPassword = attribService.Get(new Guid("D3641DA0-9E50-4C98-A994-978AF308E745"));

            AttributeValueService attribValueService = new AttributeValueService();

            // server
            AttributeValue vSmtpServer = new AttributeValue();
            vSmtpServer.Attribute = aSmtpServer;
            vSmtpServer.EntityId = 0;
            vSmtpServer.Value = txtEmailServer.Text.Trim();
            attribValueService.Add(vSmtpServer, null);
            attribValueService.Save(vSmtpServer, null);

            // port
            AttributeValue vSmtpPort = new AttributeValue();
            vSmtpPort.EntityId = 0;
            vSmtpPort.Attribute = aSmtpPort;
            vSmtpPort.Value = txtEmailServerPort.Text.Trim();
            attribValueService.Add(vSmtpPort, null);
            attribValueService.Save(vSmtpPort, null);

            // ssl
            AttributeValue vSmtpSsl = new AttributeValue();
            vSmtpSsl.EntityId = 0;
            vSmtpSsl.Attribute = aSmtpSsl;
            vSmtpSsl.Value = cbEmailUseSsl.Checked.ToString();
            attribValueService.Add(vSmtpSsl, null);
            attribValueService.Save(vSmtpSsl, null);

            // username
            if (txtEmailUsername.Text.Trim().Length > 0)
            {
                AttributeValue vSmtpUsername = new AttributeValue();
                vSmtpUsername.EntityId = 0;
                vSmtpUsername.Attribute = aSmtpUsername;
                vSmtpUsername.Value = txtEmailUsername.Text.Trim();
                attribValueService.Add(vSmtpUsername, null);
                attribValueService.Save(vSmtpUsername, null);
            }

            // password
            if (txtEmailPassword.Text.Trim().Length > 0)
            {
                AttributeValue vSmtpPassword = new AttributeValue();
                vSmtpPassword.EntityId = 0;
                vSmtpPassword.Attribute = aSmtpPassword;
                vSmtpPassword.Value = txtEmailPassword.Text.Trim();
                attribValueService.Add(vSmtpPassword, null);
                attribValueService.Save(vSmtpPassword, null);
            }
        }
        
    	var globalAttributesCache = Rock.Web.Cache.GlobalAttributesCache.Read();
    	globalAttributesCache.SetValue("EmailExceptionsList", txtEmailExceptions.Text, null, true);
    		
    	
    	pEmailSettings.Visible = false;
    	pFinished.Visible = true;
    	
    	// delete install files
        string installDirectory = Server.MapPath(".");
        File.Delete(installDirectory + @"\Install.aspx");
        File.Delete(installDirectory + @"\Configure.aspx");
        File.Delete(installDirectory + @"\RockInstall.zip");
        File.Delete(installDirectory + @"\Start.aspx");
        File.Delete(installDirectory + @"\bin\Rock.Install.Utilities.dll");
    }
    
    
</script>
<!DOCTYPE html>
<html>
	<head>
		<title>Rock ChMS Installer...</title>

		<link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
		<link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
        <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet">
        <link rel="stylesheet" href="<%=InstallSetting.RockStyles %>">
		
        <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>
		
		<link href="<%=InstallSetting.RockLogoIco %>" rel="shortcut icon">
		<link href="<%=InstallSetting.RockLogoIco %>" type="image/ico" rel="icon">

	</head>
	<body>
		<form runat="server">
		<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

        <asp:UpdateProgress id="updateProgress" runat="server">
		     <ProgressTemplate>
		         
                <div class="updateprogress-status">
                    <p>This could take a few minutes...</p>
                    <div class="spinner">
                        <div class="rect1"></div>
                        <div class="rect2"></div>
                        <div class="rect3"></div>
                        <div class="rect4"></div>
                        <div class="rect5"></div>
                    </div>
                </div>      
		            
		        <div class="updateprogress-bg"></div>
		     </ProgressTemplate>
		</asp:UpdateProgress>

		<asp:UpdatePanel ID="GettingStartedUpdatePanel" runat="server" UpdateMode="Conditional">
			<ContentTemplate>
				<div id="content">
					<h1>Rock ChMS</h1>
					
					<div id="content-box" class="group">
						
                        <!--#region Admin Account Panel  -->
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

                        <!-- panel javascript -->
                        <script>
                            function validateAdminAccount() {
                                var formValid = true;

                                // ensure that all values were provided
                                $("#pAdminAccount .required-field").each(function (index, value) {
                                    if (this.value.length == 0) {
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
                        </script>

                        <!--#endregion -->

                        <!--#region Hosting Panel  -->
                        <asp:Panel id="pHosting" Visible="false" runat="server">
							<h1>Hosting Configuration</h1>
						
							<p></p>
						
                            <h4>Hosting Addresses</h4>
							<p>Rock needs to know where you are installing the application so it can correctly assemble links when
                                you go to do things like send emails. These settings can be changed at anytime in your <span class="navigation-tip">Global Settings</span>.
                                <br />
                                <small>If you are installing Rock in subdirectory be sure to include it in the address.</small></p>

                            <div class="validation-summary"></div>

							<div class="form-group">
								<label class="control-label" for="inputEmail">Internal URL <small>Used Inside Organization</small></label>
								<asp:TextBox ID="txtInternalAddress" runat="server" placeholder="http://yourinternalsite.com/" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Public URL <small>Used Externally</small></label>
								<asp:TextBox ID="txtPublicAddress" runat="server" placeholder="http://(www.)yoursite.com/" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>

                            <div class="form-group">
								<label class="control-label" for="inputEmail">Organization Timezone</label>
								<asp:DropDownList ID="ddTimeZone" runat="server" CssClass="form-control"></asp:DropDownList>
							</div>
						
                            <div class="btn-list">
							    <asp:LinkButton id="btnAddressesNext" runat="server" OnClientClick="return validateHosting();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="AddressesNext_Click"></asp:LinkButton>
						    </div>
                        </asp:Panel>

                        <!-- panel javascript -->
                        <script>
                            function validateHosting() {
                                var formValid = true;
                                $("#pHosting .validation-summary").empty();
                                var validationMessages = '';

                                // ensure that all values were provided
                                $("#pHosting .required-field").each(function (index, value) {
                                    if (this.value.length == 0) {
                                        $(this).closest('.form-group').addClass('has-error');
                                        formValid = false;
                                    } else {
                                        $(this).closest('.form-group').removeClass('has-error');
                                    }
                                });

                                // ensure inputs are valid urls
                                if (!validateURL($('#txtInternalAddress').val())) {
                                    $('#txtInternalAddress').closest('.form-group').addClass('has-error');
                                    validationMessages += "<p>Internal URL must be in the format http://www.yourinternalsite.com</p>";
                                    formValid = false;
                                }

                                if (!validateURL($('#txtPublicAddress').val())) {
                                    $('#txtPublicAddress').closest('.form-group').addClass('has-error');
                                    validationMessages += "<p>External URL must be in the format http://(www.)yoursite.com</p>";
                                    formValid = false;
                                }

                                if (formValid) {
                                    return true;

                                } else {
                                    if (validationMessages.length > 0) {
                                        $("#pHosting .validation-summary").html("<div class='alert alert-danger'>" + validationMessages + "</div>");
                                    }
                                    return false;
                                }
                            }
                        </script>

						<!--#endregion -->

                        <!--#region Organization Panel  -->
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
								<asp:TextBox ID="txtOrgWebsite" placeholder="http://www.yourchurch.com" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
							</div>

                            <div class="btn-list">
							    <asp:LinkButton id="btnOrgNext" runat="server" OnClientClick="return validateOrgSettings();" Text="Next <i class='fa fa-chevron-right'></i>"  CssClass="btn btn-primary" OnClick="OrgNext_Click"></asp:LinkButton>
						    </div>
                        </asp:Panel>

                        <!-- panel javascript -->
                        <script>
                            function validateOrgSettings() {
                                var formValid = true;

                                // add spinner to button to tell user something is happening
                                //$('#btnOrgNext i').attr("class", "fa fa-spinner fa-spin");

                                // ensure that all values were provided
                                $("#pOrganization .required-field").each(function (index, value) {
                                    if (this.value.length == 0) {
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
                        </script>

                        <!--#endregion -->
						
                        <!--#region Email Settings Panel  -->
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
								<label class="control-label" for="inputEmail">Relay Email Username (optional) * if server requires authenication</label>
								<asp:TextBox ID="txtEmailUsername" runat="server" Text="" CssClass="form-control"></asp:TextBox>
							</div>
							
							<div class="form-group">
								<label class="control-label" for="inputEmail">Relay Email Password (optional )</label>
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

                        <!-- panel javascript -->
                        <script>
                            function validateEmailSettings() {
                                var formValid = true;

                                // ensure that all values were provided
                                $("#pEmailSettings .required-field").each(function (index, value) {
                                    if (this.value.length == 0) {
                                        $(this).closest('.form-group').addClass('has-error');
                                        formValid = false;
                                    } else {
                                        $(this).closest('.form-group').removeClass('has-error');
                                    }
                                });


                                if (formValid) {
                                    // add spinner to button to tell user something is happening
                                    //$('#btnEmailNext i').attr("class", "fa fa-spinner fa-spin");
                                    return true;
                                } else {
                                    return false;
                                }
                            }
                        </script>

						<!--#endregion -->

                        <!--#region Finished Panel  -->
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
						<!--#endregion -->

					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
		</form>
		
		<script>

            // functions for show/hiding passwords
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

            // validates urls 
			function validateURL(textval) {
			    var urlregex = new RegExp(
                      "^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-]+))*$");
			    return urlregex.test(textval);
			}

		</script>
		
	</body>

</html>



