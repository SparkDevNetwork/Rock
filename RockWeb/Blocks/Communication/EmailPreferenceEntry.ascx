<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailPreferenceEntry.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailPreferenceEntry" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <div class="email-error"></div>
        <div class="radio margin-t-sm margin-b-md">
            <Rock:RockRadioButton ID="rbUnsubscribe" runat="server" Text="Option 1" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
            <div id="divUnsubscribeLists" runat="server" class="margin-l-lg margin-t-sm" style="display: none">
                <Rock:RockCheckBoxList ID="cblUnsubscribeFromLists" runat="server" Label="" />
            </div>
        </div>
        <div class="radio">
            <Rock:RockRadioButton ID="rbUpdateEmailAddress" runat="server" Text="Option 2" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
        </div>
        <div id="divUpdateEmail" runat="server" style="display: none">
            <Rock:EmailBox ID="tbEmail" runat="server" Placeholder="Email" Label="Email" AllowMultiple="false" CssClass="input-width-xxl" />
        </div>
        <div class="radio" style="margin-top: -5px">
            <Rock:RockRadioButton ID="rbEmailPreferenceEmailAllowed" runat="server" Text="Option 3" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
        </div>
        <div class="radio">
            <Rock:RockRadioButton ID="rbEmailPreferenceNoMassEmails" runat="server" Text="Option 4" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
        </div>
        <div class="radio">
            <Rock:RockRadioButton ID="rbEmailPreferenceDoNotEmail" runat="server" Text="Option 5" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
        </div>
        <div class="radio">
            <Rock:RockRadioButton ID="rbNotInvolved" runat="server" Text="Option 6" GroupName="EmailPreference" DisplayInline="false" CssClass="js-email-radio-option" />
        </div>

        <div id="divNotInvolved" runat="server" style="display: none">
            <Rock:RockDropDownList ID="ddlInactiveReason" runat="server" Label="Reason" />
            <Rock:Switch ID="cbInactFamily" runat="server" Label="Inactivate all individuals in my family" TextAlign="Right" Checked="false" />
            <Rock:RockTextBox ID="tbInactiveNote" runat="server" Label="More Info (optional)" TextMode="MultiLine" Rows="3" MaxLength="1000" />
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClientClick="return fnValidateForm();" OnClick="btnSubmit_Click" />
        </div>
        <br />
        <Rock:NotificationBox ID="nbEmailPreferenceSuccessMessage" runat="server" NotificationBoxType="Success" Visible="false" />
        <Rock:NotificationBox ID="nbUnsubscribeSuccessMessage" runat="server" NotificationBoxType="Success" Visible="false" />

        <script>
            Sys.Application.add_load(function () {
                function toggleVisibility() {
                    if ($('#<%=rbNotInvolved.ClientID%>').is(':checked')) {
                        $('#<%=divNotInvolved.ClientID%>').slideDown('fast');
                        $('#<%=divUnsubscribeLists.ClientID%>').slideUp('fast');
                        $('#<%=divUpdateEmail.ClientID%>').slideUp('fast');

                    } else if ($('#<%=rbUnsubscribe.ClientID%>').is(':checked')) {
                        $('#<%=divNotInvolved.ClientID%>').slideUp('fast');
                        $('#<%=divUnsubscribeLists.ClientID%>').slideDown('fast');
                        $('#<%=divUpdateEmail.ClientID%>').slideUp('fast');

                    } else if ($('#<%=rbUpdateEmailAddress.ClientID%>').is(':checked')) {
                        $('#<%=divNotInvolved.ClientID%>').slideUp('fast');
                        $('#<%=divUnsubscribeLists.ClientID%>').slideUp('fast');
                        $('#<%=divUpdateEmail.ClientID%>').slideDown('fast');

                    }
                    else {
                        $('#<%=divNotInvolved.ClientID%>').slideUp('fast');
                        $('#<%=divUnsubscribeLists.ClientID%>').slideUp('fast');
                        $('#<%=divUpdateEmail.ClientID%>').slideUp('fast');
                    }
                }

                $('.js-email-radio-option').on('click', function () {
                    toggleVisibility();
                });

                toggleVisibility();
            });

            function fnValidateForm() {
                //validate text box is not empty
                if ($('#<%=tbEmail.ClientID%>').val() == "" && $('#<%=rbUpdateEmailAddress.ClientID%>').is(':checked')) {
                    $('.email-error').html('<div class="alert alert-danger">Email is required.</div>')
                    return false;
                }
                // your other validation goes here
                ///...

                // Finally return true if all validation are success
                return true;
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
