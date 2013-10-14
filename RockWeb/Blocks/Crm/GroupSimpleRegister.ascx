<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupSimpleRegister.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupSimpleRegister" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlInputInfo" runat="server">


                <fieldset>
                    <Rock:RockTextBox ID="txtFirstName" runat="server" Placeholder="First Name" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtLastName" runat="server" Placeholder="Last Name" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtEmail" runat="server" Placeholder="Email" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                </fieldset>

                <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="GroupSimpleRegister" />
                <Rock:NotificationBox ID="nbError" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                <div id="divActions" runat="server" class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server"  Visible="false">

                <Rock:NotificationBox ID="nbSuccess" runat="server" Title="Thank-you" NotificationBoxType="Success"></Rock:NotificationBox>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


