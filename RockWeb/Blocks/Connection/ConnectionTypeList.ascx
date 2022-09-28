<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionTypeList.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i> Connection Types</h1>

                <div class="panel-labels">
                    <a href="/CampaignConfiguration" class="btn btn-xs btn-default mr-1">Connection Campaigns</a>
                    <asp:LinkButton ID="lbAddConnectionType" runat="server" CssClass="btn btn-action btn-xs btn-square" OnClick="lbAddConnectionType_Click" CausesValidation="false" Title="Add Connection Type"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gConnectionType" runat="server" RowItemText="Connection Type" OnRowSelected="gConnectionType_Edit" TooltipField="Id">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="OpportunityCount" HeaderText="Opportunity Count" SortExpression="OpportunityCount" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gConnectionType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
