<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveChat.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.LiveChat.LiveChat" %>

<asp:HiddenField ID="hdnToken" runat="server" />
<asp:HiddenField ID="hdnDate" runat="server" />
<asp:HiddenField ID="hdnPerson" runat="server" />
<asp:HiddenField ID="hdnRoom" runat="server" />
<asp:HiddenField ID="hdnDisplayName" runat="server" />
<asp:HiddenField ID="hdnEmail" runat="server" />
<asp:HiddenField ID="hdnPhoto" runat="server" />

<!-- Start Content Area -->
<div id="chat-container" style="display:none;" class="demo-layout mdl-layout mdl-js-layout mdl-layout--fixed-header">


    <!-- Header section containing logo -->
    <header class="mdl-layout__header mdl-color-text--white mdl-color--light-green-600">
        <div class="mdl-cell mdl-cell--12-col mdl-cell--12-col-tablet mdl-grid">
            <div class="mdl-layout__header-row mdl-cell mdl-cell--12-col mdl-cell--12-col-tablet mdl-cell--12-col-desktop">
                <h3><i class="material-icons">chat_bubble_outline</i> Newpointe Chat</h3>
            </div>
            <div id="user-container">
                <div hidden id="user-pic"></div>
                <div hidden id="user-name"></div>
                <button hidden id="sign-out" class="mdl-button mdl-js-button mdl-js-ripple-effect mdl-color-text--white">
                    Sign-out
                </button>
                <button hidden id="sign-in" class="mdl-button mdl-js-button mdl-js-ripple-effect mdl-color-text--white">
                    <i class="material-icons">account_circle</i>Sign-in with Google
                </button>
            </div>
        </div>
    </header>

    <main class="mdl-layout__content">
        <div id="messages-card-container" class="mdl-cell mdl-cell--12-col mdl-grid">

            <!-- Messages container -->
            <div id="messages-card" class="mdl-card mdl-shadow--2dp mdl-cell mdl-cell--12-col mdl-cell--12-col-tablet mdl-cell--12-col-desktop">
                <div class="mdl-card__supporting-text mdl-color-text--grey-600">
                    <div id="messages">
                        <span id="message-filler"></span>
                    </div>
                    <div id="message-form">
                        <div class="mdl-textfield mdl-js-textfield mdl-textfield--floating-label">
                            <input class="mdl-textfield__input" type="text" id="message">
                            <label class="mdl-textfield__label" for="message">Message...</label>
                        </div>
                        <button id="submit" disabled type="submit" class="mdl-button mdl-js-button mdl-button--raised mdl-js-ripple-effect">
                            Send
                        </button>
                    </div>
                </div>
                <div class="alert alert-danger col-xs-offset-1 col-xs-10" style="display: none;" id="danger-alert">
                </div>
            </div>

            <div id="must-signin-snackbar" class="mdl-js-snackbar mdl-snackbar">
                <div class="mdl-snackbar__text"></div>
                <button class="mdl-snackbar__action" type="button"></button>
            </div>
        </div>
    </main>
</div>

<div class="col-xs-12" id="chat-unavailable" style="display:none;">
    <h4>Chat is unavailable at this time</h4>

    <div class="col-xs-12 text-center">
        <a href="/prayer" class="btn btn-primary btn-lg" target="_blank">Need Prayer? Click Here</a>
    </div>
    
</div>

<asp:Panel ID="pnlPrivatePrayer" runat="server" Visible="false" style="margin-top:15px;" CssClass="row">
    <div class="col-xs-12 col-md-offset-2 col-md-8">
        <input type="button" id="btnPrivatePrayer" class="btn btn-primary btn-lg" value="Need Prayer? Click Here" />
        
    </div>
</asp:Panel>

<script>
    $('document').ready(function () {
        var r = $("#hdnRedirectChat").val()

        if (r == "1") {
            $('#chat-container').remove();
            $('div[id *= "PrivatePrayer"]').remove();
            $('#chat-unavailable').show();
        } else {
            $("#chat-container").fadeIn();
        }
    });
</script>
