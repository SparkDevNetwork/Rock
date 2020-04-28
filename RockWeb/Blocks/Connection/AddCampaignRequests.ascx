<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddCampaignRequests.ascx.cs" Inherits="RockWeb.Blocks.Connection.AddCampaignRequests" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-12">
                <asp:LinkButton ID="btnAddCampaignRequests" runat="server" OnClick="btnAddCampaignRequests_Click" CssClass="btn btn-default btn-sm margin-b-md pull-right">
            <i class="fa fa-plus"></i>
            Campaign Requests
                </asp:LinkButton>
            </div>
        </div>

        <Rock:ModalDialog ID="mdAddCampaignRequests" runat="server" Title="Get Additional Requests" SaveButtonText="Assign" CancelLinkVisible="true" ValidationGroup="vgAddCampaignRequests" OnSaveClick="mdAddCampaignRequests_SaveClick">
            <Content>
                <Rock:RockLiteral ID="lCampaignConnectionItemSingle" runat="server" Label="Campaign" />
                <Rock:RockDropDownList ID="ddlCampaignConnectionItemsMultiple" runat="server" Label="Campaign" AutoPostBack="true" OnSelectedIndexChanged="ddlCampaignConnectionItem_SelectedIndexChanged" /> 
                <Rock:NotificationBox ID="nbAddConnectionRequestsMessage" runat="server" Visible="False" />

                <Rock:NumberBox ID="nbNumberOfRequests" runat="server" Label="Number of Requests" NumberType="Integer" Required="true" MinimumValue="0"  ValidationGroup="vgAddCampaignRequests"/>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
