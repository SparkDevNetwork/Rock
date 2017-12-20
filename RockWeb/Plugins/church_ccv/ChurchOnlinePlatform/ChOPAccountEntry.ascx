<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ChOPAccountEntry.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.ChurchOnlinePlatform.ChOPAccountEntry" %>
<script type="text/javascript">

    Sys.Application.add_load(function () {

        var availabilityMessageRow = $('#sso-ca-form-result-panel');
        var usernameUnavailable = $('#sso-ca-form-result-message');
        var usernameTextbox = $('#<%= tbUserName.ClientID %>');
        var usernameRegExp = new RegExp("<%= Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "core.ValidUsernameRegularExpression" ) %>");
        var usernameValidCaption = "<%= Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "core.ValidUsernameCaption" ) %>";

        availabilityMessageRow.css("visibility", "hidden");
        availabilityMessageRow.css("display", "none");

        usernameTextbox.blur(function () {
            if ($(this).val() && $.trim($(this).val()) != '') {

                if (!usernameRegExp.test($(this).val())) {
                    usernameUnavailable.html('Username is not valid. ' + usernameValidCaption);
                }
                else {
                    $.ajax({
                        type: 'GET',
                        contentType: 'application/json',
                        dataType: 'json',
                        url: Rock.settings.get('baseUrl') + 'api/userlogins/available/' + escape($(this).val()),
                        success: function (getData, status, xhr) {

                            if (getData) {
                                usernameUnavailable.html('');
                                availabilityMessageRow.css("visibility", "hidden");
                                availabilityMessageRow.css("display", "none");
                            } else {
                                
                                usernameUnavailable.html('That username is already taken.');
                                availabilityMessageRow.css("visibility", "visible");
                                availabilityMessageRow.css("display", "flex");
                            }
                        },
                        error: function (xhr, status, error) {
                            alert(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    });
                }

            } else {
                usernameUnavailable.html('Username is required.');
                availabilityMessageRow.css("visibility", "visible");
                availabilityMessageRow.css("display", "flex");
            }
        });

    });

</script>

<asp:UpdatePanel ID="upnlNewAccount" runat="server">
<ContentTemplate>
    <div class="sso-createaccount-panel-wrapper">
        <asp:PlaceHolder ID="phUserInfo" runat="server" Visible="true">
            <div id="create-panel-title" style="margin-bottom: 25px;">
                <img src="/themes/church_ccv_external_v8/assets/ccv_logo-hi-res.png" style="width: 125px;"/>
                <h1 class="lm-form-title text-center">REGISTER</h1>
                <p class="small-paragraph-bold">Create your account by filling out the form below.</p>
                <div id="sso-ca-form-result-panel" class="alert alert-info" style="visibility: hidden; display: none;">
                    <p id="sso-ca-form-result-message"></p>
                </div>
            </div>
            
            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-danger"/>

            <asp:Panel ID="pnlEntryForm" runat="server">
                <div class="sso-entry-row">
                    <div class="sso-entry-col">
                        <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" ></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" TextMode="Password" ValidateRequestMode="Disabled" ></Rock:RockTextBox>

                        <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirmation" Required="true" TextMode="Password" ValidateRequestMode="Disabled" ></Rock:RockTextBox>
                        <asp:CompareValidator ID="covalPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="Password and Confirmation do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
                    </div>

                    <div class="sso-entry-col">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
                    </div>
                </div>

                <div class="actions" style="margin-left: 50px; margin-top:25px;">
                    <asp:Button ID="btnUserInfoNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnUserInfoNext_Click" />
                </div>
            </asp:Panel>
        
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phDuplicates" runat="server" Visible="false">

            <div class="alert alert-warning">
                <asp:Literal ID="lFoundDuplicateCaption" runat="server" />
            </div>

            <div class="grid">
                <Rock:Grid ID="gDuplicates" runat="server">
                    <Columns>
                        <Rock:RockTemplateField>
                            <HeaderTemplate>You?</HeaderTemplate>
                            <ItemTemplate>
                                <input type="radio" value='<%# Eval("Id") %>' name="DuplicatePerson" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:RockBoundField DataField="FullName" HeaderText="Name" />
                        <Rock:RockBoundField DataField="Gender" HeaderText="Gender" />
                        <Rock:RockBoundField DataField="BirthDate" HeaderText="Birth Day" DataFormatString="{0:MMMM d}" />
                    </Columns>
                </Rock:Grid>
            </div>

            <div class="inputs-list not-me">
                <label>
                    <input type="radio" value="0" name="DuplicatePerson" />        
                    <span>None of these are me</span>    
                </label>
            </div>

            <div class="actions">
                <asp:Button ID="btnDuplicatesPrev" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnDuplicatesPrev_Click" />
                <asp:Button ID="btnDuplicatesNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnDuplicatesNext_Click" />
            </div>

        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phSendLoginInfo" runat="server" Visible="false">

            <asp:HiddenField ID="hfSendPersonId" runat="server" />

            <div class="alert alert-warning">
                <asp:Literal ID="lExistingAccountCaption" runat="server"/>
            </div>

            <div class="actions">
                <asp:Button ID="btnSendPrev" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnSendPrev_Click" />
                <asp:Button ID="btnSendYes" runat="server" Text="Yes, send it" CssClass="btn btn-primary" OnClick="btnSendYes_Click" />
                <asp:Button ID="btnSendLogin" runat="server" Text="No, just let me login" CssClass="btn btn-primary" OnClick="btnSendLogin_Click" />
            </div>

        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phSentLoginInfo" runat="server" Visible="false">

            <div>
                <asp:Literal ID="lSentLoginCaption" runat="server" />
            </div>
            <br />
            <div>
                <asp:Button ID="btnSentLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnSendLogin_Click" />
            </div>

        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phConfirmation" runat="server" Visible="false">

            <div class="alert alert-warning">
                <asp:Literal ID="lConfirmCaption" runat="server" />
            </div>

        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phSuccess" runat="server" Visible="false">

            <div class="alert alert-success">
                <asp:Literal ID="lSuccessCaption" runat="server" />
            </div>

            <div class="actions">
                <asp:Button ID="btnContinue" runat="server" Text="Continue" CssClass="btn btn-primary" OnClick="btnContinue_Click" Visible="false" />
            <div class="actions">

        </asp:PlaceHolder>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
