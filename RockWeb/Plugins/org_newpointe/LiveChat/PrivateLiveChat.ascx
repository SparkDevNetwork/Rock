<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrivateLiveChat.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.LiveChat.PrivateLiveChat" %>

<!-- Start Content Area -->
<div class="demo-layout mdl-layout mdl-js-layout mdl-layout--fixed-header">

    <asp:HiddenField ID="hdnToken" runat="server" />
    <asp:HiddenField ID="hdnDate" runat="server" />
    <asp:HiddenField ID="hdnPerson" runat="server" />
    <asp:HiddenField ID="hdnRoom" runat="server" />
    <asp:HiddenField ID="hdnDisplayName" runat="server" />
    <asp:HiddenField ID="hdnEmail" runat="server" />
    <asp:HiddenField ID="hdnPhoto" runat="server" />
    <!-- Header section containing logo -->
    <header class="mdl-layout__header mdl-color-text--white mdl-color--light-green-600">
        <div class="mdl-cell mdl-cell--12-col mdl-cell--12-col-tablet mdl-grid">
            <div class="mdl-layout__header-row mdl-cell mdl-cell--12-col mdl-cell--12-col-tablet mdl-cell--12-col-desktop">
                <h3><i class="material-icons">chat_bubble_outline</i>Private Newpointe Chat</h3>
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
