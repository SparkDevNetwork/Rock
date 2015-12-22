<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyMemberEntry.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.FamilyMemberEntry" %>

<asp:UpdatePanel ID="upnlNewAccount" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <asp:Panel ID="pnlDetails" runat="server">
            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true"></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true"></Rock:RockTextBox>
            <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender">
                <asp:ListItem Text="Male" Value="Male" />
                <asp:ListItem Text="Female" Value="Female" />
                <asp:ListItem Text="Unknown" Value="Unknown" />
            </Rock:RockRadioButtonList>
            <Rock:BirthdayPicker ID="bdaypBirthDay" runat="server" Label="Birthday" />

            <div class="actions">
                <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
