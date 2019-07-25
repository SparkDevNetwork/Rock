<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BadgeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.BadgeList" %>

<asp:UpdatePanel ID="upBadgeList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> Badge List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gBadge" runat="server" AllowSorting="false" OnRowSelected="gBadge_Edit">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="EntityType.FriendlyName" HeaderText="Type" SortExpression="EntityType.FriendlyName" />
                            <Rock:RockBoundField DataField="BadgeComponentEntityType.FriendlyName" HeaderText="Component" SortExpression="BadgeComponentEntityType.FriendlyName" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gBadge_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
