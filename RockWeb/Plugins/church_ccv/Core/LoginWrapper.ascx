<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginWrapper.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.LoginWrapper" %>

<div id="divProfilePhoto" runat="server" class="profile-photo"></div>   
<ul class="loginstatus"> 
    
    <li class="dropdown" ID="liDropdown" runat="server">
        
        <a class="masthead-navitem dropdown-toggle navbar-link loginstatus" href="#" data-toggle="dropdown">
        
            <asp:PlaceHolder ID="phHello" runat="server"><asp:Literal ID="lHello" runat="server" /></asp:PlaceHolder>
            <b class="fa fa-caret-down"></b>
        </a>

        <ul class="dropdown-menu">
            <asp:PlaceHolder ID="phMyAccount" runat="server">
                <li>
                    <asp:HyperLink ID="hlMyAccount" runat="server" Text="My Account" />
                </li>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phMySettings" runat="server">
                <li>
                    <asp:HyperLink ID="hlMySettings" runat="server" Text="My Settings" />
                </li>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phMyProfile" runat="server">
                <li>
                    <asp:HyperLink ID="hlMyProfile" runat="server" Text="My Profile" />
                </li>
            </asp:PlaceHolder>
            <asp:Literal ID="lDropdownItems" runat="server" />

            <li><asp:LinkButton ID="lbLogout" runat="server" OnClick="LoginStatus_lbLogout_Click" CausesValidation="false"></asp:LinkButton></li>
        </ul>

    </li>
    <li ID="liLogin" runat="server" Visible="false"><asp:LinkButton CssClass="masthead-navitem" ID="lbLogin" runat="server" OnClientClick="displayLoginModal(); return false;" CausesValidation="false" Text="Login">
    <asp:PlaceHolder ID="phNewAccount" runat="server" Visible="false" ><asp:HyperLink ID="hlNewAccount" CssClass="masthead-navitem" runat="server" Text="Create Account" /></asp:PlaceHolder></asp:LinkButton></li>
</ul>

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
                                        <asp:Button ID="btnHelp" runat="server" Text="Forgot username or password?" CssClass="small-paragraph lm-form-forgot" OnClick="LoginModal_btnHelp_Click" CausesValidation="false" />
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
                                <input class="form-control" name="tb-ca-username" type="text" id="tb-ca-username">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-firstname">First Name</label>
                                <input class="form-control" name="tb-ca-firstname" type="text" id="tb-ca-firstname">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-password">Password</label>
                                <input class="form-control" name="tb-ca-password" type="password" id="tb-ca-password">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-lastname">Last Name</label>
                                <input class="form-control" name="tb-ca-lastname" type="text" id="tb-ca-lastname">
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-confirmpassword">Confirm Password</label>
                                <input class="form-control" name="tb-ca-confirmpassword" type="password" id="tb-ca-confirmpassword">
                            </div>
                        </div>

                        <div class="col-md-6 col-sm-8 col-xs-12">    
                            <div class="form-group rock-text-box lm-form-label">
                                <label class="control-label" for="tb-ca-email">Email</label>
                                <input class="form-control" name="tb-ca-email" type="text" id="tb-ca-email">
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
            </div>
            <%-- END CREATE ACCOUNT PANEL--%>
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
	
	var modal = document.querySelector('#bg-screen');
	modal.addEventListener('click', function (e) {
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
                // attempt to login
                var xmlRequest = new XMLHttpRequest();
                xmlRequest.onreadystatechange = function () { if (this.readyState == 4 && this.status == 200) { return handleLoginResponse(this); } }
                xmlRequest.open("POST", "/api/Web/Login?username=" + username + "&password=" + password + "&persist=" + rememberMe, true);
                xmlRequest.send();
            }
        }, 250);

        return false;
    }

    function tryRegisterUser() {

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
                // todo - actually register them
                handleRegistrationResponse(null);
            }
        }, 250);
    }

    function validateEmail( emailText )
    {
        var mailFormat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

        if (emailText.match(mailFormat)) {
            return true;
        }

        return false;
    }

    function handleRegistrationResponse(xmlRequest) {
        hideLoader();

        // invoke a postback so the server can redirect us if needed
        __doPostBack("btnLogin", "__REGISTRATION_SUCCEEDED" + ":");
    }

    function handleLoginResponse(xmlRequest) {

        hideLoader();

        switch (xmlRequest.responseText) {
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
                
                var confirmationUrl = "<%=LoginModal_GetConfirmationUrl( ) %>";
                var confirmAccountTemplate = "<%=LoginModal_GetConfirmAccountTemplate( ) %>";
                var appUrl = "<%=LoginModal_GetAppUrl( ) %>";
                var themeUrl = "<%=LoginModal_GetThemeUrl( ) %>";
                var username = $("#tb-lp-username").val();

                // invoke the send email api
                var xmlRequest = new XMLHttpRequest();
                xmlRequest.onreadystatechange = function () { if (this.readyState == 4 && this.status == 200) { return handleLoginResponse(this); } }
                xmlRequest.open("GET",  "/api/Web/SendConfirmationEmail" +
                                        "?confirmationUrl=" + encodeURI(confirmationUrl) +
                                        "&confirmAccountTemplate=" + confirmAccountTemplate +
                                        "&appRoot=" + encodeURI(appUrl) +
                                        "&themeRoot=" + encodeURI(themeUrl) +
                                        "&username=" + username
                                        , true);
                xmlRequest.send();

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
