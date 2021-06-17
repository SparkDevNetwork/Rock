<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LinkOrganization.ascx.cs" Inherits="RockWeb.Blocks.Store.LinkOrganization" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-link"></i>Link Organization</h1>
            </div>
            <div class="panel-body">

                <h1 class="mt-0">Store Configuration</h1>

                <p>
                    In order for us to get the store ready for your first download
                    we need to do a bit of configuration. The first step is to match
                    your Rock install to a registered organization so we can remember
                    your purchases.
                </p>

                <asp:Panel ID="pnlAuthenicate" runat="server">
                    <div class="alert alert-info">
                        <strong>Which Password?</strong> Use the username and password below that you created
                        on the <a href="https://www.rockrms.com">Rock RMS website</a>.
                    </div>

                    <Rock:RockTextBox ID="txtUsername" runat="server" Label="Rock RMS Username" />
                    <Rock:RockTextBox ID="txtPassword" TextMode="Password" runat="server" Label="Rock RMS Password" />

                    <asp:Button ID="btnRetrieveOrganization" CssClass="btn btn-primary" runat="server" OnClick="btnRetrieveOrganization_Click" Text="Retrieve Organization" />
                </asp:Panel>

                <asp:Panel ID="pnlSelectOrganization" runat="server" Visible="false">

                    <div class="alert alert-info">
                        <strong>Success!</strong> We found multiple organizations tied to your account. Select the organization Rock should
                        use for remembering purchases.
                    </div>

                    <Rock:RockRadioButtonList ID="rblOrganizations" AutoPostBack="true" OnSelectedIndexChanged="rblOrganizations_SelectedIndexChanged" runat="server"></Rock:RockRadioButtonList>

                    <div class="margin-t-md">
                        <asp:Button ID="btnSelectOrganization" runat="server" CssClass="btn btn-primary" Text="Select Organization" OnClick="btnSelectOrganization_Click" Enabled="false" />
                        <asp:Button ID="btnSelectOrganizationCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnSelectOrganizationCancel_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlAverageWeeklyAttendance" runat="server" Visible="false">
                    <div class="alert alert-info">
                        <p><strong>Confirm:</strong>
                        Please confirm the size of your organization below. This will update your organization size on the Spark server which impacts your suggested donation
                        amount and the relative pricing of Rock Shop plugins.</p>
                        <br />
                        <p>Rock runs on integrity so we rely on the information below being accurate.</p>
                    </div>

                    <Rock:NumberBox runat="server" ID="nbAverageWeeklyAttendance" Label="Average Weekend Attendance" Required="true" NumberType="Integer" />

                    <div class="margin-t-md">
                        <asp:Button ID="btnSaveAttendance" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveAttendance_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlComplete" runat="server" Visible="false">
                    <asp:Literal ID="lCompleteMessage" runat="server" />

                    <div class="margin-t-md">
                        <asp:Button ID="btnContinue" runat="server" CssClass="btn btn-primary" Text="Continue" OnClick="btnContinue_Click" />
                    </div>
                </asp:Panel>

                <asp:Literal ID="lMessages" runat="server" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
