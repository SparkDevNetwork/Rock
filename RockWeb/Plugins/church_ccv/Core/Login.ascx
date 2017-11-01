<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.Login" %>

<asp:Panel ID="mdLoginShell" runat="server">
    <div ID="login-main-panel" style="box-sizing: border-box; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); transition: all 450ms cubic-bezier(0.23, 1, 0.32, 1) 0ms; position: relative; width: 75%; max-width: 768px; margin: 0px auto; z-index: 1500; opacity: 1; transform: translate(0px, 64px);">
        <div style="color: rgba(0, 0, 0, 0.87); background-color: rgb(255, 255, 255); transition: all 450ms cubic-bezier(0.23, 1, 0.32, 1) 0ms; box-sizing: border-box; font-family: Roboto, sans-serif; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); box-shadow: rgba(0, 0, 0, 0.25) 0px 14px 45px, rgba(0, 0, 0, 0.22) 0px 10px 18px; border-radius: 2px;">
            <div style="font-size: 16px; color: rgba(0, 0, 0, 0.6); padding: 24px; box-sizing: border-box; overflow-y: hidden; border-top: none; border-bottom: none; background-color: rgb(255, 255, 255);">
                <div class="row">
                    <div id="divOrgLogin" runat="server" class="col-sm-12">
                        <div>
                            <h1 class="login-form-title text-center">Log In</h1>
                            <p style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">Fill out the form below to securely access your account.</p>
                        </div>
                        
                        <div id="login-form-result-panel" style="visibility: hidden;" class="">
                            <p id="login-form-result-message"></p>
                        </div>

                        <div class="col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-12">    
                            <div class="form-group rock-text-box login-form-label">
                                <label class="control-label" for="tbUserName">Username</label>
                                <input class="form-control" name="tbUserName" type="text" id="tbUserName">
                            </div>

                            
                            <div style="margin: 25px 0 25px 0;"></div>

                            <div class="form-group rock-text-box login-form-label">
                                <label class="control-label" for="tbPassword">Password</label>
                                <input class="form-control" name="tbPassword" type="password" id="tbPassword">
                            </div>
                            
                            <div class="checkbox ">
				                <label><input id="cbRememberMe" type="checkbox" name="cbRememberMe">Remember Me</label>
			                </div>

                            <div class="row v-center" style="margin: 25px 0 25px 0;">
                                <div class="col-md-4 col-sm-4 col-xs-6" style="padding: 0;">
                                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="login-form-button btn btn-primary" OnClientClick="return tryLogin();"/>
                                </div>

                                <div class="col-md-8 col-sm-4 col-xs-6">
                                    <asp:Button ID="btnHelp" runat="server" Text="Forgot username or password?" CssClass="small-paragraph login-form-forgot" OnClick="btnHelp_Click" CausesValidation="false" />
                                </div>
                            </div>
                                
                            <asp:Button ID="btnNewAccount" runat="server" Text="Create Account" CssClass="login-form-register btn btn-action" OnClick="btnNewAccount_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <p style="visibility: hidden;" id="locked-out-message"><%=GetLockedOutMessage() %></p>
</asp:Panel>

<asp:Panel ID="pnlTempLoginTrigger" runat="server">

    <div class="alert alert-danger">
        <asp:Button ID="ButtonTempLoginTrigger" runat="server" Text="Login" CssClass="btn btn-primary" OnClientClick="return displayLoginPanel();" />
    </div>

</asp:Panel>

<script type="text/javascript">
    function displayLoginPanel() {

        var loginPanel = $("#login-main-panel");
        loginPanel.css("visibility", "visible");
        loginPanel.css("height", "");

        //loginPanel.css("animation-name", "fly-in");
        //loginPanel.css("animation-duration", "2s");

        return false;
    }

    function tryLogin() {

        // hide the response panel
        hideResponsePanel();

        // get the inputted username / password and whether it's checked or not
        var username = $("#tbUserName").val();
        var password = $("#tbPassword").val();
        var rememberMe = $("#cbRememberMe").is(":checked");

        // attempt to login
        var xmlRequest = new XMLHttpRequest();
        xmlRequest.onreadystatechange = function () { if (this.readyState == 4 && this.status == 200) { return handleLoginResponse(this); } }
        xmlRequest.open("POST", "/api/Web/Login?username=" + username + "&password=" + password + "&persist=" + rememberMe, true);
        xmlRequest.send();

        return false;
    }

    function handleLoginResponse(xmlRequest) {

        switch (xmlRequest.responseText) {
            case "Success": handleLoginSucceeded(); break;
            
            case "Invalid": showResponsePanel("Invalid username or password"); break;
            case "LockedOut": showResponsePanel($("#locked-out-message").text()); break;
            case "Confirm": showResponsePanel("This account needs to be confirmed"); break;
            default: showResponsePanel("An unknown error has occurred"); break;
        }
    }

    function handleLoginSucceeded() {
        // invoke a postback so the server can redirect us if needed
        __doPostBack("btnLogin", "__LOGIN_SUCCEEDED" + ":");
    }

    function hideResponsePanel() {

        var responsePanel = $("#login-form-result-panel");
        responsePanel.removeClass("alert alert-warning block-message margin-t-md");
        responsePanel.css("visibility", "none");
        responsePanel.css("height", "");
        responsePanel.css("margin", "");
        responsePanel.css("padding", "");

        var responseMessage = $("#login-form-result-message");
        responseMessage.css("height", "");
        responseMessage.css("margin", "");
        responseMessage.css("padding", "");
        responseMessage.css("visibility", "none");
        responseMessage.text("");
    }

    function showResponsePanel(errorMsg) {

        // reveal the panel
        var responsePanel = $("#login-form-result-panel");
        responsePanel.css("visibility", "visible");
        responsePanel.addClass( "alert alert-warning block-message margin-t-md" );

        // display the appropriate message
        var responseMessage = $("#login-form-result-message");
        responseMessage.text(errorMsg);
    }
</script>