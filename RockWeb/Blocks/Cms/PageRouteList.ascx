<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageRouteList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageRouteList" %>

<asp:UpdatePanel ID="upPageRoutes" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i> Route List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gPageRoutes" runat="server" AllowSorting="true" RowItemText="Route" OnRowSelected="gPageRoutes_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Route" HeaderText="Route" SortExpression="Route" />
                            <Rock:RockBoundField DataField="PageName" HeaderText="Page Name" SortExpression="PageName" />
                            <Rock:RockBoundField DataField="PageId" HeaderText="Page Id" SortExpression="PageId" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gPageRoutes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>


