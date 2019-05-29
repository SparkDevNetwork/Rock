<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountEntry.ascx.cs" Inherits="RockWeb.Blocks.Security.AccountEntry" %>
<script type="text/javascript">

    Sys.Application.add_load(function () {

        var availabilityMessageRow = $('#availabilityMessageRow');
        var usernameUnavailable = $('#availabilityMessage');
        var usernameTextbox = $('#<%= tbUserName.ClientID %>');
        var usernameRegExp = new RegExp("<%= Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "core.ValidUsernameRegularExpression" ) %>");
        var usernameValidCaption = "<%= Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "core.ValidUsernameCaption" ) %>";

        availabilityMessageRow.hide();

        usernameTextbox.blur(function () {
            if ($(this).val() && $.trim($(this).val()) != '') {

                if (!usernameRegExp.test($(this).val())) {
                    usernameUnavailable.html('Username is not valid. ' + usernameValidCaption);
                    usernameUnavailable.addClass('alert-warning');
                    usernameUnavailable.removeClass('alert-success');
                }
                else {
                    $.ajax({
                        type: 'GET',
                        contentType: 'application/json',
                        dataType: 'json',
                        url: Rock.settings.get('baseUrl') + 'api/userlogins/available/' + escape($(this).val()),
                        success: function (getData, status, xhr) {

                            if (getData) {
                                usernameUnavailable.html('That username is available.');
                                usernameUnavailable.addClass('alert-success');
                                usernameUnavailable.removeClass('alert-warning');
                            } else {
                                availabilityMessageRow.show();
                                usernameUnavailable.html('That username is already taken!');
                                usernameUnavailable.addClass('alert-warning');
                                usernameUnavailable.removeClass('alert-success');
                            }
                        },
                        error: function (xhr, status, error) {
                            alert(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    });
                }

            } else {
                usernameUnavailable.html('Username is required!');
                usernameUnavailable.addClass('alert-warning');
                usernameUnavailable.removeClass('alert-success');
            }
            availabilityMessageRow.show();
        });

    });

</script>

<asp:UpdatePanel ID="upnlNewAccount" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-validation"/>
    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"/>

    <asp:PlaceHolder ID="phUserInfo" runat="server" Visible="true">

        <div class="row">

            <div class="col-md-6">

               <fieldset>
                    <legend>New Account</legend>
                    <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" ></Rock:RockTextBox>
                    <dl id="availabilityMessageRow">
                        <dt></dt>
                        <dd><div id="availabilityMessage" class="alert"/></dd>
                    </dl>
                    <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" TextMode="Password" ValidateRequestMode="Disabled" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirmation" Required="true" TextMode="Password" ValidateRequestMode="Disabled" ></Rock:RockTextBox>
                    <asp:CompareValidator ID="covalPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="Password and Confirmation do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>

                </fieldset>

            </div>

            <div class="col-md-6">

                <fieldset>
                    <legend>Your Information</legend> 
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
                    <Rock:RockDropDownList ID="ddlGender" runat="server" Label="Gender">
                        <asp:ListItem Text="" Value="U"></asp:ListItem>
                        <asp:ListItem Text="Male" Value="M"></asp:ListItem>
                        <asp:ListItem Text="Female" Value="F"></asp:ListItem>
                    </Rock:RockDropDownList>
                    <Rock:BirthdayPicker ID="bdaypBirthDay" runat="server" Label="Birthday" />
               </fieldset>

               <asp:Panel ID="pnlPhoneNumbers" runat="server">
                   <fieldset>
                        <legend>Phone Numbers</legend>
                        <asp:Repeater ID="rPhoneNumbers" runat="server">
                                    <ItemTemplate>
                                        <div runat="server" id="PhoneGroup" class="form-group phonegroup clearfix">
                                            <div class="control-label col-sm-1 phonegroup-label"><%# Eval("NumberTypeValue.Value")  %>&nbsp;</div>
                                            <div class="controls col-sm-11 phonegroup-number">
                                                <div class="row margin-l-sm">
                                                    <div class="col-sm-7">
                                                        <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' 
                                                            Number='<%# Eval("NumberFormatted")  %>'  />
                                                    </div>    
                                                    <div class="col-sm-5 margin-t-sm">
                                                        <div class="row">
                                                            <div class="col-xs-6">
                                                                <asp:CheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' CssClass="js-sms-number" />
                                                            </div>
                                                            <div class="col-xs-6">
                                                                <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                                            </div>
                                                        </div>
                                                    </div>

                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                        </fieldset>
                   </asp:Panel>
                    
                    <asp:Panel ID="pnlAddress" runat="server">
                        <fieldset>
                            <legend>Address</legend>
                            <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" />
                        </fieldset>
                    </asp:Panel>

                <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
            </div>
        
        </div>

        <div class="actions">
            <asp:Button ID="btnUserInfoNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnUserInfoNext_Click" />
        </div>

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
        </div>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>
