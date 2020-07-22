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
                      <Rock:GridFilter ID="gFilter" runat="server">
                          <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gPageRoutes" runat="server" AllowSorting="true" RowItemText="Route" OnRowSelected="gPageRoutes_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Route" HeaderText="Route" SortExpression="Route" />
                            <Rock:RockBoundField DataField="Page.Layout.Site.Name" HeaderText="Site" SortExpression="Page.Layout.Site.Name" />
                            <Rock:RockBoundField DataField="Page.InternalName" HeaderText="Page Name" SortExpression="Page.InternalName" />
                            <Rock:RockBoundField DataField="Page.Id" HeaderText="Page Id" SortExpression="Page.Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="IsGlobal" HeaderText="Global Route" SortExpression="IsGlobal" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>


