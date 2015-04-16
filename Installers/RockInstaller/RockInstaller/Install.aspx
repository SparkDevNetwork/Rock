<%@ Page Language="C#"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Installer</title>
    <link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:300,400,600,700' type='text/css' />
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.1.0/css/font-awesome.min.css" rel="stylesheet" />
    
    <style type="text/css">

        body {
            background-color: #dbd5cb;
            border-top: 24px solid #282526;
        }

    </style>

    <script src="//code.jquery.com/jquery-1.9.0.min.js"></script>

    <script src="<%=String.Format("{0}Scripts/jquery.signalR.min.js", storageUrl) %>"></script>
    <script src="<%=String.Format("{0}Scripts/rock-install.js", storageUrl) %>"></script>
    <link href="<%=String.Format("{0}Styles/rock-installer.css", storageUrl) %>" rel="stylesheet" />
    <link rel="shortcut icon" href="<%=String.Format("{0}Images/favicon.ico", storageUrl) %>" />

    <script src="./signalr/hubs" type="text/javascript"></script>

    <script>

        $(document).ready(function () {

            var baseVersion = '<%=baseVersion %>';
            var isDebug = <%=isDebug.ToString().ToLower() %>;
            var queryString = '<%=Request.Url.PathAndQuery %>';

            // connect to the install controller signalr hub
            var installcontroller = $.connection.installController; var installcontroller = $.connection.installController;
            $.connection.hub.start().done(function () {
                // fix for ie which does not have window.location.origin
                if (!window.location.origin) {
                    window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
                }

                $("#console ol").append("<li class='highlight'><span>Connected to server via: " + window.location.origin + "</span></li>");
            });

            // console message recieve
            // add message to the console
            installcontroller.client.addConsoleMessage = function (message) {
                $("#console ol").append("<li title='" + message.UtcTime + "' class='" + message.Type.toLowerCase() + " clearfix'><span>" + message.Message + "</span></li>");
                $("#console").scrollTop($("#console")[0].scrollHeight);
            }

            // update the progress bar
            installcontroller.client.updateProgressBar = function (percentComplete) {
                $('.progress .progress-bar').css('width', percentComplete + '%').attr('aria-valuenow', percentComplete);
            }

            // update the step
            installcontroller.client.updateStep = function (previousStep, nextStepTitle) {
                $('.progress .progress-bar').css('width', '0%').attr('aria-valuenow', 0);
                $('#step-name').text(nextStepTitle);
                $('#' + previousStep).addClass('complete');
            }

            // report errors
            installcontroller.client.reportError = function (errorMessage) {
                $('#error-message').html(errorMessage);
            }

            // redirect to complete page
            installcontroller.client.redirectToComplete = function () {
                $('#step-unzip').addClass('complete');
                window.location = queryString.replace('Install.aspx', 'Complete.aspx');
            }

            //
            // logic for database connection test
            //
            $('body').on('click', '#btnDatabaseConfigNext', function (e) {
                var formIsValid = validateForm('#pnlDatabaseConfig');

                if (formIsValid) {
                    installcontroller.server.testDatabaseConnection($('#txtServerName').val(), $('#txtUsername').val(), $('#txtPassword').val());
                }
            });

            // callback for db connection
            installcontroller.client.respondDbConnect = function (dbLoginResult) {
                console.log(dbLoginResult);
                if (dbLoginResult.CanConnect) {
                    $('#dbMessages').html('');
                    $('#pnlDatabaseConfig').fadeOut(function () {
                        installcontroller.server.testRockEnvironment($('#txtServerName').val(), $('#txtUsername').val(), $('#txtPassword').val(), $('#txtDatabaseName').val());
                    });
                } else {
                    $('#dbMessages').fadeOut(function () {
                        $('#dbMessages').html("<div class='alert alert-warning'><strong>Could Not Connect</strong> " + dbLoginResult.Message + "</div>");
                        $('#dbMessages').fadeIn();
                    });
                }
            }

            //
            // logic for environment tests
            //

            // callback for environment checks
            installcontroller.client.respondEnvironmentCheck = function (envResults) {
                console.log(envResults);
                $('#envTestResults').html(envResults.Results);
                $('#pnlEnvChecks').fadeIn();

                if (envResults.Success) {
                    $('#btnEnvChecksRetry').hide();
                    $('#btnEnvChecksNext').show();
                } else {
                    $('#btnEnvChecksRetry').show();
                    $('#btnEnvChecksNext').hide();
                }
            }

            $('body').on('click', '#btnEnvChecksRetry', function (e) {
                installcontroller.server.testRockEnvironment($('#txtServerName').val(), $('#txtUsername').val(), $('#txtPassword').val(), $('#txtDatabaseName').val());
            });

            //
            // logic for install start
            //

            $('body').on('click', '#btnEmailSettingsNext', function (e) {

                var formIsValid = validateForm('#pnlEmailSettings');
                var validationMessages = '';

                if (formIsValid) {
                    $('#pnlEmailSettings').fadeOut(function () {
                        $('#pnlInstallWatch').fadeIn();
                        startInstall();
                    });
                } else {
                    if (validationMessages.length > 0) {
                        $("#pnlOrgInfo .validation-summary").html("<div class='alert alert-danger'>" + validationMessages + "</div>");
                    }
                }
            });

            function startInstall() {
                var installData = {
                    "connectionString": {
                        "server": $('#txtServerName').val(),
                        "database": $('#txtDatabaseName').val(),
                        "username": $('#txtUsername').val(),
                        "password": $('#txtPassword').val()
                    },
                    "adminUser": {
                        "username": $('#txtAdminUsername').val(),
                        "password": $('#txtAdminPassword').val()
                    },
                    "hostingInfo": {
                        "internalUrl": $('#txtInternalAddress').val(),
                        "externalUrl": $('#txtPublicAddress').val(),
                        "timezone": $('#ddTimeZone').val()
                    },
                    "organization": {
                        "name": $('#txtOrgName').val(),
                        "email": $('#txtOrgEmail').val(),
                        "phone": $('#txtOrgPhone').val(),
                        "website": $('#txtOrgWebsite').val()
                    },
                    "emailSettings": {
                        "server": $('#txtEmailServer').val(),
                        "port": $('#txtEmailServerPort').val(),
                        "useSsl": $('#cbEmailUseSsl').is(':checked'),
                        "relayUsername": $('#txtEmailUsername').val(),
                        "relayPassword": $('#txtEmailPassword').val()
                    },
                    "installerProperties": {
                        "installVersion": baseVersion,
                        "isDebug": '<%=isDebug %>'
                    }   
                };

                console.log(installData);

                installcontroller.server.startInstall(installData);
            }
        });
    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div id="content">
	        <h1 id="logo">Rock RMS</h1>


                <div id="content-box" class="no-fade">
                    <!-- welcome panel -->
                    <div id="pnlWelcome">
                        
                        <div class="content-narrow">
                            <img src="<%=storageUrl %>Images/laptop.png" />

                            <h1>It's Time For Something New...</h1>

                        

                            <div class="btn-list clearfix">
						        <a href="http://www.rockrms.com/Learn/Install" target="_blank" class="btn btn-default pull-left"><i class="fa fa-desktop"></i> Install Video</a>
                                <a id="btnWelcomeNext" class="btn btn-primary pull-right">Get Started <i class='fa fa-chevron-right'></i></a>
					        </div>
                        </div>

                        <asp:Literal ID="lSslWarning" runat="server">
                            <div class="alert alert-warning ssl-alert">
                                <strong>Just A Thought...</strong></p>
                                        Looks like you're not running over an encrypted connection (SSL).  Since you will be providing passwords for configuring
                                        your database and Rock administrator login, you may wish to run the install over an encrypted connection.
                            </div>
                        </asp:Literal>

                        <script>
                            $('body').on('click', '#btnWelcomeNext', function (e) {
                                $('#pnlWelcome').fadeOut(function () {
                                    $('#pnlDatabaseConfig').fadeIn();
                                });;
                            });
                        </script>
                    </div>

                    <!-- database config panel -->
                    <div id="pnlDatabaseConfig" style="display: none;">
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
                                    <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" CssClass="required-field form-control password-field"></asp:TextBox>
                                </div>
                                <div class="col-md-4" style="padding-top: 6px;">
                                    <input id="show-password" type="checkbox" class="show-password" />
                                    <label for="show-password" id="show-password-label" style="font-weight:normal;">Show Password</label>
                                </div>
                            </div>
					    </div>
							
					    <div id="dbMessages"></div>
							
					    <div class="btn-list clearfix">
						    <a id="btnDatabaseConfigNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
					    </div>
                    </div>

                    <!-- environment checks panel -->
                    <div id="pnlEnvChecks" style="display: none;">

                        <div id="envTestResults"></div>

                        <div class="btn-list clearfix">
                            <a id="btnEnvChecksBack" class="btn btn-default pull-left"><i class='fa fa-chevron-left'></i>  Back</a>
						    <a id="btnEnvChecksNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
                            <a id="btnEnvChecksRetry" class="btn btn-primary pull-right"><i class='fa fa-refresh'></i> Retry</a>
					    </div>

                        <script>
                            $('body').on('click', '#btnEnvChecksBack', function (e) {
                                $('#pnlEnvChecks').fadeOut(function () {
                                    $('#pnlDatabaseConfig').fadeIn();
                                });;
                            });

                            $('body').on('click', '#btnEnvChecksNext', function (e) {
                                $('#pnlEnvChecks').fadeOut(function () {
                                    $('#pnlAdminUser').fadeIn();
                                });;
                            });
                        </script>
                    </div>

                    <!-- admin user panel -->
                    <div id="pnlAdminUser" style="display: none;">

                        <h1>Administrator Login</h1>
						
					    <p>Please provide a username and password for the administrator's account</p>

                        <div class="validation-summary"></div>

					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Administrator Username</label>
						    <asp:TextBox ID="txtAdminUsername" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Administrator Password</label>
                            <asp:TextBox ID="txtAdminPassword" TextMode="Password" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label class="control-label" for="inputEmail">Administrator Password (confirm)</label>
                            <asp:TextBox ID="txtAdminPasswordConfirm" TextMode="Password" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>

                        <div class="btn-list clearfix">
                            <a id="btnAdminUserBack" class="btn btn-default pull-left"><i class='fa fa-chevron-left'></i>  Back</a>
						    <a id="btnAdminUserNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
					    </div>

                        <script>
                            $('body').on('click', '#btnAdminUserBack', function (e) {
                                $('#pnlAdminUser').fadeOut(function () {
                                    $('#pnlEnvChecks').fadeIn();
                                });
                            });

                            $('body').on('click', '#btnAdminUserNext', function (e) {

                                var formIsValid = validateForm('#pnlAdminUser');
                                var validationMessages = '';

                                if ($('#txtAdminPassword').val() != $('#txtAdminPasswordConfirm').val()) {
                                    validationMessages += "<p>The administrator password does not match the confirmation.</p>";
                                    formIsValid = false;
                                }

                                if (formIsValid) {
                                    $('#pnlAdminUser').fadeOut(function () {
                                        $('#pnlHostingConfig').fadeIn();
                                    });;
                                } else {
                                    if (validationMessages.length > 0) {
                                        $("#pnlAdminUser .validation-summary").html("<div class='alert alert-danger'>" + validationMessages + "</div>");
                                    }
                                }
                            });
                        </script>

                    </div>

                    <!-- hosting config panel -->
                    <div id="pnlHostingConfig" style="display: none;">
                        <h1>Hosting Configuration</h1>
						
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

                        <div class="btn-list clearfix">
                            <a id="btnHostingConfigBack" class="btn btn-default pull-left"><i class='fa fa-chevron-left'></i>  Back</a>
						    <a id="btnHostingConfigNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
					    </div>

                        <script>
                            $('body').on('click', '#btnHostingConfigBack', function (e) {
                                $('#pnlHostingConfig').fadeOut(function () {
                                    $('#pnlAdminUser').fadeIn();
                                });
                            });

                            $('body').on('click', '#btnHostingConfigNext', function (e) {

                                var formIsValid = validateForm('#pnlHostingConfig');
                                var validationMessages = '';

                                // ensure inputs are valid urls
                                if (!validateURL($('#txtInternalAddress').val())) {
                                    $('#txtInternalAddress').closest('.form-group').addClass('has-error');
                                    validationMessages += "<p>Internal URL must be in the format http://www.yourinternalsite.com</p>";
                                    formIsValid = false;
                                }

                                if (!validateURL($('#txtPublicAddress').val())) {
                                    $('#txtPublicAddress').closest('.form-group').addClass('has-error');
                                    validationMessages += "<p>External URL must be in the format http://www.yoursite.com</p>";
                                    formIsValid = false;
                                }

                                if (formIsValid) {

                                    // auto fillin the public website
                                    $('#txtOrgWebsite').val($('#txtPublicAddress').val());

                                    $('#pnlHostingConfig').fadeOut(function () {
                                        $('#pnlOrgInfo').fadeIn();
                                    });
                                } else {
                                    if (validationMessages.length > 0) {
                                        $("#pnlHostingConfig .validation-summary").html("<div class='alert alert-danger'>" + validationMessages + "</div>");
                                    }
                                }
                            });
                        </script>

                    </div>

                    <!-- hosting config panel -->
                    <div id="pnlOrgInfo" style="display: none;">
                        <h1>Organization Information</h1>
						
					    <p>Please enter some information about your organization.  These fields are used to provide default information in the database. It
						    is in no way shared with us or anyone else.
					    </p>

                        <div class="validation-summary"></div>

					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Organization Name</label>
						    <asp:TextBox ID="txtOrgName" runat="server" placeholder="Your Organization" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Organization Default Email Address</label>
						    <asp:TextBox ID="txtOrgEmail" runat="server" placeholder="info@yourorg.com" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Organization Phone Number</label>
						    <asp:TextBox ID="txtOrgPhone" placeholder="(555) 555-5555" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Organization Website</label>
						    <asp:TextBox ID="txtOrgWebsite" placeholder="http://www.yourorg.com" runat="server" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>

                        <div class="btn-list clearfix">
                            <a id="btnOrgInfoBack" class="btn btn-default pull-left"><i class='fa fa-chevron-left'></i>  Back</a>
						    <a id="btnOrgInfoNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
					    </div>

                        <script>
                            $('body').on('click', '#btnOrgInfoBack', function (e) {
                                $('#pnlOrgInfo').fadeOut(function () {
                                    $('#pnlHostingConfig').fadeIn();
                                });
                            });

                            $('body').on('click', '#btnOrgInfoNext', function (e) {

                                var formIsValid = validateForm('#pnlOrgInfo');
                                var validationMessages = '';

                                // ensure inputs are valid urls
                                if (!validateURL($('#txtOrgWebsite').val())) {
                                    $('#txtOrgWebsite').closest('.form-group').addClass('has-error');
                                    validationMessages += "<p>Organization website must be in the format http://www.yoursite.com</p>";
                                    formIsValid = false;
                                }

                                if (formIsValid) {
                                    $('#pnlOrgInfo').fadeOut(function () {
                                        $('#pnlEmailSettings').fadeIn();
                                    });
                                } else {
                                    if (validationMessages.length > 0) {
                                        $("#pnlOrgInfo .validation-summary").html("<div class='alert alert-danger'>" + validationMessages + "</div>");
                                    }
                                }
                            });
                        </script>

                    </div>

                    <!-- email config panel -->
                    <div id="pnlEmailSettings" style="display: none;">
                        <h1>Email Server Settings</h1>
						
					    <p>Email is an essential part of the Rock RMS.  Please provide a few details about your email environment.  You can change 
					    these values at any time inside the app. 
					    </p>

					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Email Server</label>
						    <asp:TextBox ID="txtEmailServer" runat="server" placeholder="mail.yourorg.com" CssClass="required-field form-control" Text=""></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Email Server SMTP Port (default is 25)</label>
						    <asp:TextBox ID="txtEmailServerPort" runat="server" placeholder="mail.yourorg.com" CssClass="required-field form-control" Text="25"></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Use SSL For SMTP (default no)</label>
						    <asp:CheckBox ID="cbEmailUseSsl" runat="server" />
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Relay Email Username (optional) * if server requires authentication</label>
						    <asp:TextBox ID="txtEmailUsername" runat="server" Text="" CssClass="form-control"></asp:TextBox>
					    </div>
							
					    <div class="form-group">
						    <label class="control-label" for="inputEmail">Relay Email Password (optional )</label>
						    <div class="row">
                                <div class="col-md-8"><asp:TextBox ID="txtEmailPassword" TextMode="Password" runat="server" Text="" CssClass="form-control password-field"></asp:TextBox></div>
							    <div class="col-md-4" style="padding-top: 6px;">
                                    <input id="show-password-email" type="checkbox" class="show-password" />
                                    <label for="show-password-email" id="show-password-email-label" style="font-weight:normal;">Show Password</label>
                                </div>
                            </div>
					    </div>

                        <div class="btn-list clearfix">
                            <a id="btnEmailSettingsBack" class="btn btn-default pull-left"><i class='fa fa-chevron-left'></i>  Back</a>
						    <a id="btnEmailSettingsNext" class="btn btn-primary pull-right">Next <i class='fa fa-chevron-right'></i></a>
					    </div>

                        <script>
                            $('body').on('click', '#btnEmailSettingsBack', function (e) {
                                $('#pnlEmailSettings').fadeOut(function () {
                                    $('#pnlOrgInfo').fadeIn();
                                });
                            });
                        </script>

                    </div>

                    <!-- install watch -->
                    <div id="pnlInstallWatch" style="display: none;">

                        <div id="installer-working">
                            <img src="<%=storageUrl %>Images/laptop-sm.png" />

                            <div class="spinner">
                                <div class="rect1"></div>
                                <div class="rect2"></div>
                                <div class="rect3"></div>
                                <div class="rect4"></div>
                                <div class="rect5"></div>
                            </div>
                        </div>


                        <!-- progress bar -->
                        
                        <div class="row progress-header">
                            <div class="col-sm-6">
                                <h4 id="step-name">Step 1: Downloading Database</h4>
                            </div>

                            <div class="col-sm-6">
                                <ul id="install-steps" class="list-unstyled clearfix">
                                    <li id="step-downloadsql" class="fade-in"></li>
                                    <li id="step-downloadrock" class="fade-in"></li>
                                    <li id="step-sql" class="fade-in"></li>
                                    <li id="step-configure" class="fade-in"></li>
                                    <li id="step-unzip" class="fade-in"></li>
                                </ul>
                            </div>
                        </div>

                        <div class="progress active">
                            <div class="progress-bar"  role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%"></div>
                        </div>

                        <div id="error-message"></div>

                        <!-- console -->
                        <div class="clearfix">
                            <button id="btn-showconsole" class="btn btn-default btn-xs pull-right"><i class="fa fa-chevron-down"></i> <span>Show Console</span></button>
                        </div>
                        <div id="console">
                            <ol></ol>
                        </div>

                        <script>
                            // toggle console
                            $("#btn-showconsole").click(function () {
                                var txt = $("#console").is(':visible') ? 'Show Console' : 'Hide Console';
                                $("#btn-showconsole span").text(txt);
                                $("#btn-showconsole i").toggleClass('fa-chevron-down fa-chevron-up');
                                $('#console').slideToggle();
                                $("#console").scrollTop($("#console")[0].scrollHeight);
                                return false;
                            });
                        </script>

                    </div>

                    <!-- show success -->
                    <div id="pnlRedirect" style="display: none;">
                        
                        <script>

                        </script>
                    
                    </div>
                </div>

        </div>
    </form>
</body>

<script language="CS" runat="server">
    
    const string baseStorageUrl = "//rockrms.blob.core.windows.net/install/";
    const string baseVersion = "2_7_0";

    string storageUrl = string.Empty;
    bool isDebug = false;
    
    void Page_Init( object sender, EventArgs e )
    {
        // toggle the SSL warning
        lSslWarning.Visible = !Request.IsSecureConnection;
        
        if ( Request["Version"] != null )
        {
            storageUrl = String.Format( "{0}{1}/", baseStorageUrl, Request["Version"] );
        }
        else
        {
            storageUrl = String.Format( "{0}{1}/", baseStorageUrl, baseVersion );
        }

        if ( Request["Debug"] != null )
        {
            isDebug = Convert.ToBoolean( Request["Debug"] );
        }

        if ( isDebug )
        {
            // set default values for input fields if debugging
            txtServerName.Text = "localhost";
            txtDatabaseName.Text = "RockInstallTest";
            txtUsername.Text = "RockInstallUser";
            txtPassword.TextMode = TextBoxMode.SingleLine;
            txtPassword.Text = "rocktestpassword23";

            txtAdminUsername.Text = "admin";
            txtAdminPassword.TextMode = TextBoxMode.SingleLine;
            txtAdminPassword.Text = "admin";
            txtAdminPasswordConfirm.TextMode = TextBoxMode.SingleLine;
            txtAdminPasswordConfirm.Text = "admin";

            txtInternalAddress.Text = "http://rock.rocksolidchurchdemo.com";
            txtPublicAddress.Text = "http://www.rocksolidchurchdemo.com";

            txtOrgName.Text = "Rock Solid Church (install)";
            txtOrgEmail.Text = "info@rocksolidchurchdemo.com";
            txtOrgPhone.Text = "(602) 555-5555";

            txtEmailServer.Text = "mail.rocksolidchurchdemo.com";

            storageUrl = "/";
        }
        
        // load timezones
        // add timezones to dropdown
        foreach ( TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones() )
        {
            ddTimeZone.Items.Add( new ListItem( timeZone.DisplayName, timeZone.Id ) );
        }
    }

</script>

</html>
