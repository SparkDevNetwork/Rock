<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupSimpleRegister.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupSimpleRegister" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlInputInfo" runat="server" CssClass="panel panel-default">
            <div class="panel-body">

                <fieldset>
                    <Rock:RockTextBox ID="txtFirstName" runat="server" Label="First" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtLastName" runat="server" Label="Last" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtEmail" runat="server" Label="Email" Required="true" ValidationGroup="GroupSimpleRegister" ></Rock:RockTextBox>
                </fieldset>

                <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="GroupSimpleRegister" />
                <Rock:NotificationBox ID="nbError" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                <div id="divActions" runat="server" class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="panel panel-default" Visible="false">
            <div class="panel-body">

                <Rock:NotificationBox ID="nbSuccess" runat="server" Title="Thank-you" NotificationBoxType="Success"></Rock:NotificationBox>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


