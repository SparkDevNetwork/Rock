<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LinkOrganization.ascx.cs" Inherits="RockWeb.Blocks.Store.LinkOrganization" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-link"></i> Link Organization</h1>
            </div>
            <div class="panel-body">

                <h1>Welcome to the Rock Store!</h1>

                <p>
                    In order for us to get the store ready for your first download 
                    we need to do a bit of configuration. The first step is to match
                    your Rock install to a registered organzation so we can remember
                    your purchases. 
                </p>
                
                <asp:Panel ID="pnlAuthenicate" runat="server">
                    <div class="alert alert-info">
                        <strong>Which Password?</strong> Use the username and password below that you created
                        on the <a href="http://www.rockrms.com">Rock RMS website</a>.
                    </div>

                    <Rock:RockTextBox ID="txtUsername" runat="server" Label="Rock RMS Username" />
                    <Rock:RockTextBox ID="txtPassword" TextMode="Password" runat="server" Label="Rock RMS Password" />

                    <asp:Button ID="btnRetrieveOrganization" CssClass="btn btn-primary" runat="server" OnClick="btnRetrieveOrganization_Click" Text="Retrieve Organization" />
                </asp:Panel>

                <asp:Panel ID="pnlSelectOrganization" runat="server" Visible="false">
                    
                    <div class="alert alert-info"><strong>Success!</strong> We found multiple organizations tied to your account. Select the organization Rock should
                        use for remembering purchases.
                    </div>

                    <Rock:RockRadioButtonList ID="rblOrganizations" AutoPostBack="true" OnSelectedIndexChanged="rblOrganizations_SelectedIndexChanged"  runat="server"></Rock:RockRadioButtonList>
                    
                    <asp:Button ID="btnSelectOrganization" runat="server" CssClass="btn btn-primary" Text="Select Organization" OnClick="btnSelectOrganization_Click" Enabled="false" />
                    <asp:Button ID="btnSelectOrganizationCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnSelectOrganizationCancel_Click" />
                </asp:Panel>

                <asp:Literal ID="lMessages" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
