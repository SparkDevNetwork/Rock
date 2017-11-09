<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginModal.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.LoginModal" %>

<%-- LOGIN WRAPPER MODAL--%>
<asp:Panel runat="server">
    <div ID="bg-screen" class="bg-screen-hidden">
        <div ID="login-modal" class="login-modal-hidden">
            <div class="loader-bg loader-bg-hidden">
                <div class="loader loader-hidden"></div>
            </div>

            <div class="lm-base-panel">
                <%-- LOGIN PANEL --%>
			    <div id="login-panel">
                    <div class="row">
                        <div class="col-sm-12">
                            <div>
                                <h1 class="lm-form-title text-center">Log In</h1>
                                <p style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">Fill out the form below to securely access your account.</p>
                            </div>
                        
                            <div id="lp-form-result-panel" style="visibility: hidden;">
                                <p id="lp-form-result-message"></p>
                            </div>

                            <div class="col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-12">    
                                <div class="form-group rock-text-box lm-form-label">
                                    <label class="control-label" for="tb-lp-username">Username</label>
                                    <input class="form-control" name="tb-lp-username" type="text" id="tb-lp-username">
                                </div>

                            
                                <div style="margin: 25px 0 25px 0;"></div>

                                <div class="form-group rock-text-box lm-form-label">
                                    <label class="control-label" for="tb-lp-password">Password</label>
                                    <input class="form-control" name="tb-lp-password" type="password" id="tb-lp-password">
                                </div>
                            
                                <div class="checkbox ">
				                    <label><input id="cb-lp-rememberme" type="checkbox" name="cb-lp-rememberme">Remember Me</label>
			                    </div>

                                <div class="row v-center" style="margin: 25px 0 25px 0;">
                                    <div class="col-md-4 col-sm-4 col-xs-6" style="padding: 0;">
                                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="lm-form-button btn btn-primary" OnClientClick="return tryLogin();"/>
                                    </div>

                                    <div class="col-md-8 col-sm-4 col-xs-6">
                                        <asp:Button ID="btnHelp" runat="server" Text="Forgot username or password?" CssClass="small-paragraph lm-form-forgot" OnClientClick="displayForgotPasswordPanel(); return false;" CausesValidation="false" />
                                    </div>
                                </div>
                                
                                <asp:Button ID="btnNewAccount" runat="server" Text="Create Account" CssClass="lm-form-register btn btn-action" OnClientClick="displayCreateAccountPanel(); return false;" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>
                <%--END LOGIN PANEL--%>

                <%-- CREATE ACCOUNT PANEL--%>
                <div id="createaccount-panel" class="createaccount-panel-hidden">
                    <div class="row">
                        <div class="col-md-12">
                            <div>
                                <h1 class="lm-form-title text-center">REGISTER</h1>
                                <p style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">Create your account by filling out the form below.</p>
                            </div>
                        
                            <div id="ca-form-result-panel" style="visibility: hidden;">
                                <p id="ca-form-result-message"></p>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-username">Username</label>
                                <input class="form-control" name="tb-ca-username" type="text" id="tb-ca-username" value="jered-testyay">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-firstname">First Name</label>
                                <input class="form-control" name="tb-ca-firstname" type="text" id="tb-ca-firstname" value="jered">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-password">Password</label>
                                <input class="form-control" name="tb-ca-password" type="password" id="tb-ca-password" value="ccv123">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-lastname">Last Name</label>
                                <input class="form-control" name="tb-ca-lastname" type="text" id="tb-ca-lastname" value="mcferron">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-confirmpassword">Confirm Password</label>
                                <input class="form-control" name="tb-ca-confirmpassword" type="password" id="tb-ca-confirmpassword" value="ccv123">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-email">Email</label>
                                <input class="form-control" name="tb-ca-email" type="text" id="tb-ca-email" value="jered@mcferron.com">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Cancel" CssClass="lm-form-button btn btn-action" OnClientClick="hideCreateAccountPanel(); return false;" CausesValidation="false" />
                        </div>

                        <div class="text-right col-md-6 col-sm-8 col-xs-12">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Register" CssClass="lm-form-button btn btn-primary" OnClientClick="tryRegisterUser(); return false;" CausesValidation="false" />
                        </div>
                    </div>
                </div>
                <%-- END CREATE ACCOUNT PANEL--%>

                <%-- ACCOUNT CREATION DUPLICATES PANEL (This panel is used when the account info already exists for another user).--%>
                <div id="accountcreationduplicates-panel" class="accountcreationduplicates-panel-hidden">
                    <div class="row">
                        <div class="col-md-12">
                            <div>
                                <h1 id="accountcreationduplicates-header" class="lm-form-title text-center">ACCOUNT DUPLICATE</h1>
                                <p id="accountcreationduplicates-details" style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">We have this email and last name already on file. Are you any of these people?</p>
                            </div>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-12 col-sm-8 col-xs-12">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <div class="row">
                                <div class="col-md-4 col-sm-4 col-xs-4">
                                    <p>PERSON NAME</p>
                                </div>
                                <div class="col-md-4 col-sm-4 col-xs-4">
                                    <p>GENDER</p>
                                </div>
                                <div class="col-md-4 col-sm-4 col-xs-4">
                                    <p>BIRTHDAY</p>
                                </div>
                            </div>
                            <div id="duplicates-form">
                                <%--ITEM AS ITS OWN ROW--%>
                                <div class="duplicates-form-item">
                                    <div class="row">
                                        <div class="col-md-4 col-sm-4 col-xs-4">
                                            <input type="radio"  name="person" value="-1">None of the Above<br>
                                        </div>
                                    </div>
                                </div>


                            </div>

                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12 col-sm-8 col-xs-12 text-center">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Done" CssClass="lm-form-button btn btn-primary" OnClientClick="hideLoginModal(); return false;" CausesValidation="false" />
                        </div>
                    </div>
                </div>
                <%-- ACCOUNT CREATION RESULT PANEL--%>

                <%-- ACCOUNT CREATION RESULT PANEL (This panel is used to explain the result of the account creation attempt; did it succeed, do they need to confirm, etc.--%>
                <div id="accountcreationresult-panel" class="accountcreationresult-panel-hidden">
                    <div class="row">
                        <div class="col-md-12">
                            <div>
                                <h1 id="accountcreationresult-header" class="lm-form-title text-center">ACCOUNT CREATED</h1>
                                <p id="accountcreationresult-details" style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">Your account has been created. You should receive a confirmation email shortly. You may now login.</p>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12 col-sm-8 col-xs-12 text-center">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Done" CssClass="lm-form-button btn btn-primary" OnClientClick="hideLoginModal(); return false;" CausesValidation="false" />
                        </div>
                    </div>
                </div>
                <%-- ACCOUNT CREATION RESULT PANEL--%>

                <%-- FORGOT PASSWORD PANEL--%>
                <div id="forgotpassword-panel" class="forgotpassword-panel-hidden">
                    <div class="row">
                        <div class="col-md-12">
                            <div>
                                <h1 class="lm-form-title text-center">FORGOT PASSWORD</h1>
                                <p style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold"><%=LoginModal_GetForgotPasswordCaption( ) %></p>
                            </div>
                        
                            <div id="fp-form-result-panel" style="visibility: hidden;">
                                <p id="fp-form-result-message"></p>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12 col-sm-12 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-fp-email">Email</label>
                                <input class="form-control" name="tb-fp-email" type="text" id="tb-fp-email">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Cancel" CssClass="lm-form-button btn btn-action" OnClientClick="hideForgotPasswordPanel(); return false;" CausesValidation="false" />
                        </div>

                        <div class="text-right col-md-6 col-sm-8 col-xs-12">
                            <div style="margin: 25px 0 25px 0;"></div>
                            <asp:Button runat="server" Text="Confirm" CssClass="lm-form-button btn btn-primary" OnClientClick="trySendForgotPasswordEmail(); return false;" CausesValidation="false" />
                        </div>
                    </div>
                </div>
                <%-- END FORGOT PASSWORD PANEL--%>
            </div>
        </div>
    </div>
</asp:Panel>
<%-- END LOGIN WRAPPER MODAL--%>

<script type="text/javascript">
   $(document).keydown(function(e) {
		// ESCAPE key pressed
		if (e.keyCode == 27) {
			hideLoginModal();
		}
	});
	
	var modal = document.querySelector("#bg-screen");
	modal.addEventListener("click", function (e) {
	    if (this == e.target) {
	        hideLoginModal();
	    }
	}, false);
	
	function hideLoginModal() {

	    // fade OUT the bg screen (the grey overlay)
	    var bgScreen = $("#bg-screen");
	    bgScreen.removeClass("bg-screen-visible");
	    bgScreen.addClass("bg-screen-hidden");

	    // fly OUT the actual login modal
	    var loginModal = $("#login-modal");
	    loginModal.removeClass("login-modal-visible");
	    loginModal.addClass("login-modal-hidden");
	}

    function displayLoginModal() {

        //todo: do this without animation so it's instant
        hideAccountCreationDuplicatesPanel();
        hideAccountCreationResultPanel();
        hideCreateAccountPanel();
        hideForgotPasswordPanel();

        // fade in the bg screen (the grey overlay)
        var bgScreen = $("#bg-screen");
        bgScreen.removeClass("bg-screen-hidden");
        bgScreen.addClass("bg-screen-visible");

        // fly in the actual login modal
        var loginModal = $("#login-modal");
        loginModal.removeClass("login-modal-hidden");
        loginModal.addClass("login-modal-visible");

        // make sure the loader is hidden
        hideLoader();
    }

    function displayCreateAccountPanel() {
        var createAccountPanel = $("#createaccount-panel");
        createAccountPanel.removeClass("createaccount-panel-hidden");
        createAccountPanel.addClass("createaccount-panel-visible");
    }

    function hideCreateAccountPanel() {
        var createAccountPanel = $("#createaccount-panel");
        createAccountPanel.removeClass("createaccount-panel-visible");
        createAccountPanel.addClass("createaccount-panel-hidden");
    }

    function displayAccountCreationResultPanel() {
        var accountCreatedPanel = $("#accountcreationresult-panel");
        accountCreatedPanel.removeClass("accountcreationresult-panel-hidden");
        accountCreatedPanel.addClass("accountcreationresult-panel-visible");
    }

    function hideAccountCreationResultPanel() {
        var panel = $("#accountcreationresult-panel");
        panel.removeClass("accountcreationresult-panel-visible");
        panel.addClass("accountcreationresult-panel-hidden");
    }

    function displayForgotPasswordPanel() {
        var forgotPasswordPanel = $("#forgotpassword-panel");
        forgotPasswordPanel.removeClass("forgotpassword-panel-hidden");
        forgotPasswordPanel.addClass("forgotpassword-panel-visible");
    }

    function hideForgotPasswordPanel() {
        var forgotPasswordPanel = $("#forgotpassword-panel");
        forgotPasswordPanel.removeClass("forgotpassword-panel-visible");
        forgotPasswordPanel.addClass("forgotpassword-panel-hidden");
    }

    function tryLogin() {
        // hide the response panel
        hideResponsePanel("#lp-form-result-panel", "#lp-form-result-message");

        // show the spinner
        displayLoader();

        // get the inputted username / password and whether it's checked or not
        var username = $("#tb-lp-username").val();
        var password = $("#tb-lp-password").val();
        var rememberMe = $("#cb-lp-rememberme").is(":checked");

        setTimeout(function () {
            if (username.length == 0 || password.length == 0) {
                hideLoader();

                showResponsePanel("#lp-form-result-panel", "#lp-form-result-message", "Username and password need to both be filled out.");
            }
            else {

                $.ajax({
                    url: "/api/Web/Login",
                    contentType: "application/json",
                    data: JSON.stringify(
                        {
                            Username: username,
                            Password: password,
                            Persist: rememberMe
                        }),
                    type: "POST"
                }).done(function (returnData) {
                    handleLoginResponse(returnData);
                });
            }
        }, 250);

        return false;
    }

    function tryRegisterUser() {

        // this function will validate all info inputted, and report any errors found.
        // if no errors ARE found, it will actually try to register the user

        // hide the response panel
        hideResponsePanel("#ca-form-result-panel", "#ca-form-result-message");

        // show the spinner
        displayLoader();

        // get the input fields
        var username = $("#tb-ca-username").val();
        var password = $("#tb-ca-password").val();
        var confirmPassword = $("#tb-ca-confirmpassword").val();
        var firstname = $("#tb-ca-firstname").val();
        var lastname = $("#tb-ca-lastname").val();
        var email = $("#tb-ca-email").val();

        // force a timer so that if they didn't enter a valid password / email,
        // we can still show them the loader and give them the feel that they pressed 'Register'
        setTimeout(function () {
            // make sure there aren't any blank fields
            if (username.length == 0 || password.length == 0 || confirmPassword.length == 0 || firstname.length == 0 || lastname.length == 0 || email.length == 0) {
                hideLoader();

                showResponsePanel("#ca-form-result-panel", "#ca-form-result-message", "One or more fields are empty. Please fix and try again");
            }
            // make sure their passwords match
            else if (password != confirmPassword) {
                hideLoader();

                showResponsePanel("#ca-form-result-panel", "#ca-form-result-message", "Your password doesn't match.");
            }
            // and their email is ok
            else if ( validateEmail( email ) == false) {
                hideLoader();

                showResponsePanel("#ca-form-result-panel", "#ca-form-result-message", "Your email address isn't valid.");
            }
            else {

                // before actually registering, see if their username is available
                $.ajax({
                    url: "api/userlogins/available/" + username,
                    type: "GET"
                }).done(function (returnData) {
                    if (returnData == true) {
                        // all is good. now actually register them
                        sendUserRegistration(username, confirmPassword, firstname, lastname, email);
                    }
                    else {
                        // let them know their username isn't available
                        hideLoader();
                        showResponsePanel("#ca-form-result-panel", "#ca-form-result-message", "That username is not available.");
                    }
                });
            }
        }, 250);
    }

    function sendUserRegistration() {

        // get the input fields
        var username = $("#tb-ca-username").val();
        var confirmPassword = $("#tb-ca-confirmpassword").val();
        var firstname = $("#tb-ca-firstname").val();
        var lastname = $("#tb-ca-lastname").val();
        var email = $("#tb-ca-email").val();

        var confirmAccountUrl = "<%=LoginModal_GetConfirmAccountUrl( ) %>";
        var accountCreatedEmailTemplateGuid = "<%=LoginModal_GetAccountCreatedEmailTemplateGuid( ) %>";
        var appUrl = "<%=LoginModal_GetAppUrl( ) %>";
        var themeUrl = "<%=LoginModal_GetThemeUrl( ) %>";

        $.ajax({
            url: "api/web/RegisterNewUser",
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data:
                JSON.stringify({
                    FirstName: firstname,
                    LastName: lastname,
                    Email: email,
                    Username: username,
                    Password: confirmPassword,
                    ConfirmAccountUrl: confirmAccountUrl,
                    AccountCreatedEmailTemplateGuid: accountCreatedEmailTemplateGuid,
                    AppUrl: appUrl,
                    ThemeUrl : themeUrl
                })
        }).done(function (returnData) {
            handleRegistrationResponse(returnData);
        });
    }

    function trySendForgotPasswordEmail() {

        // hide the response panel
        hideResponsePanel("#fp-form-result-panel", "#fp-form-result-message");

        // show the spinner
        displayLoader();

        var email = $("#tb-fp-email").val();

        var confirmAccountUrl = "<%=LoginModal_GetConfirmAccountUrl( ) %>";
        var forgotPasswordEmailTemplateGuid = "<%=LoginModal_GetForgotPasswordEmailTemplateGuid( ) %>";
        var appUrlWithRoot = "<%=LoginModal_GetAppUrlIncludeRoot( ) %>";
        var themeUrlWithRoot = "<%=LoginModal_GetThemeUrlIncludeRoot( ) %>";
        
        // force a timer so that if they didn't enter a valid password / email,
        // we can still show them the loader and give them the feel that they pressed 'Register'
        setTimeout(function () {
            // if their email is invalid, warn them
            if (email.length == 0 || validateEmail(email) == false) {
                hideLoader();

                showResponsePanel("#fp-form-result-panel", "#fp-form-result-message", "Your email address isn't valid.");
            }
            else {

                $.ajax({
                    url: "/api/Web/SendForgotPasswordEmail" +
                         "?confirmAccountUrl=" + encodeURI(confirmAccountUrl) +
                         "&forgotPasswordEmailTemplateGuid=" + forgotPasswordEmailTemplateGuid +
                         "&appUrlWithRoot=" + encodeURI(appUrlWithRoot) +
                         "&themeUrlWithRoot=" + encodeURI(themeUrlWithRoot) +
                         "&personEmail=" + email,
                    type: "GET"
                }).done(function (responseData) {
                    handleForgotPasswordResponse(responseData);
                });
            }
        });
    }

    function validateEmail( emailText ) {
        var mailFormat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,9})+$/;

        if (emailText.match(mailFormat)) {
            return true;
        }

        return false;
    }

    function handleRegistrationResponse(responseData) {
        hideLoader();

        switch (responseData.RegisterStatus) {
            case "Created": displayAccountCreationResultPanel(); break;
            case "Duplicates": displayAccountCreationDuplicatesPanel( responseData.Duplicates ); break;
            default: {
                // invoke a postback so the server can redirect us if needed
                __doPostBack("", "__REGISTRATION_SUCCEEDED" + ":");
                break;
            }
        }
    }

    function displayAccountCreationDuplicatesPanel( duplicatesInfoList ) {
        var panel = $("#accountcreationduplicates-panel");
        panel.removeClass("accountcreationduplicates-panel-hidden");
        panel.addClass("accountcreationduplicates-panel-visible");

        // build a list with the duplicate people
        var duplicateHtmlList = $("#duplicates-form");

        for (var i = 0; i < duplicatesInfoList.length; i++) {
            duplicateHtmlList.prepend(
                "<div class=\"duplicates-form-item\">" +
                    "<div class=\"row\">" + 
                    "<div class=\"col-md-4\">" + "<input type=\"radio\" class=\"duplicates-form-item\" name=\"person\" value=\"" + duplicatesInfoList[i].Id + "\">" + duplicatesInfoList[i].FullName + "</div>" + 
                    "<div class=\"col-md-4\">" + duplicatesInfoList[i].Gender + "</div>" +
                    "<div class=\"col-md-4\">" + duplicatesInfoList[i].Birthday + "</div>" +
                "</div>" + "<br>");
        }

        //todo: $("#duplicates-form input[type=radio]:checked")[0] use this to see what's checked
        // need to finish handling these scenarios, and then should be done.
    }

    function hideAccountCreationDuplicatesPanel() {
        var panel = $("#accountcreationduplicates-panel");
        panel.removeClass("accountcreationduplicates-panel-visible");
        panel.addClass("accountcreationduplicates-panel-hidden");
    }

    function handleForgotPasswordResponse(responseData) {
        hideLoader();

        // invoke a postback so the server can redirect us if needed
        __doPostBack("", "__FORGOT_PASSWORD_SUCCEEDED" + ":");
    }

    function handleLoginResponse(responseData) {

        hideLoader();

        switch (responseData) {
            case "Success":
            {
                handleLoginSucceeded();
                break;
            }
            
            case "LockedOut":
            {
                showResponsePanel("#lp-form-result-panel", "#lp-form-result-message", "<%=LoginModal_GetLockedOutCaption( ) %>");
                break;
            }

            case "Confirm":
            {
                showResponsePanel("#lp-form-result-panel", "#lp-form-result-message", "<%=LoginModal_GetConfirmCaption( ) %>"); 
                
                var confirmAccountUrl = "<%=LoginModal_GetConfirmAccountUrl( ) %>";
                var confirmAccountEmailTemplateGuid = "<%=LoginModal_GetConfirmAccountEmailTemplateGuid( ) %>";
                var appUrl = "<%=LoginModal_GetAppUrl( ) %>";
                var themeUrl = "<%=LoginModal_GetThemeUrl( ) %>";
                var username = $("#tb-lp-username").val();

                // invoke the send email api
                $.ajax({
                    url: "/api/Web/SendConfirmationEmail" +
                         "?confirmAccountUrl=" + encodeURI(confirmAccountUrl) +
                         "&confirmAccountEmailTemplateGuid=" + confirmAccountEmailTemplateGuid +
                         "&appUrl=" + encodeURI(appUrl) +
                         "&themeUrl=" + encodeURI(themeUrl) +
                         "&username=" + username,
                    type: "GET"
                });

                break;
            }
            
            case "Invalid":
            default: showResponsePanel("#lp-form-result-panel", "#lp-form-result-message", "Invalid username or password"); break;
        }
    }

    function handleLoginSucceeded() {
        // invoke a postback so the server can redirect us if needed
        __doPostBack("btnLogin", "__LOGIN_SUCCEEDED" + ":");
    }

    function hideResponsePanel(panelId, panelMessageId) {

        var responsePanel = $(panelId);
        responsePanel.removeClass("alert alert-warning block-message margin-t-md");
        responsePanel.css("visibility", "none");
        responsePanel.css("height", "");
        responsePanel.css("margin", "");
        responsePanel.css("padding", "");

        var responseMessage = $(panelMessageId);
        responseMessage.css("height", "");
        responseMessage.css("margin", "");
        responseMessage.css("padding", "");
        responseMessage.css("visibility", "none");
        responseMessage.text("");
    }

    function showResponsePanel(panelId, panelMessageId, errorMsg) {

        // reveal the panel
        var responsePanel = $(panelId);
        responsePanel.css("visibility", "visible");
        responsePanel.addClass( "alert alert-warning block-message margin-t-md" );

        // display the appropriate message
        var responseMessage = $(panelMessageId);
        responseMessage.text(errorMsg);
    }
</script>
<%-- END LOGIN MODAL PANEL--%>
