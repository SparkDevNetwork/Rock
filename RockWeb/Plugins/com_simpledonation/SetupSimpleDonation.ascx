<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SetupSimpleDonation.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationSetup" %>

<style type="text/css">
    .sd-content {
        flex: 1;
    }

    .sd-icon {
        font-size: 3rem;
        margin-right: 1.5rem;
    }

    .sd-setup-row {
        display: flex;
        flex-direction: row;
        align-items: flex-start;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Configure Simple Donation</h1>
            </div>

            <div class="panel-body">
                <div class="person-exists sd-setup-row" id="divPersonExists" runat="server">
                    <div class="sd-setup-row">
                        <i class="fa fa-thumbs-up sd-icon"></i>

                        <p class="sd-content">Everything is set up correctly!</p>
                    </div>
                </div>

                <div class="create-person" id="divCreatePerson" runat="server">
                    <div class="sd-setup-row">
                        <i class="fa fa-info-circle sd-icon"></i>

                        <p class="sd-content">To complete your setup of Simple Donation, you'll need a new API user in Rock. Click the button below to automate that process.</p>
                    </div>

                    <Rock:BootstrapButton CssClass="btn btn-primary margin-v-sm" runat="server" ID="btnCreateUser" DataLoadingText="Create Simple Donation User" OnClick="btnCreateUser_Click" Text="Create Simple Donation User" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
