<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TvPageList.ascx.cs" Inherits="RockWeb.Blocks.Tv.TvPageList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdWarning" runat="server" />
        <asp:Panel ID="pnlBlock" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tv"></i> 
                    <asp:Literal ID="lBlockTitle" runat="server" Text="Page List" />
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPages" runat="server" RowItemText="Page" OnGridRebind="gPages_GridRebind" OnRowSelected="gPages_RowSelected" OnGridReorder="gPages_GridReorder" OnRowDataBound="gPages_RowDataBound">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="InternalName" SortExpression="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Description" SortExpression="Description" HeaderText="Description" />
                            <Rock:BoolField DataField="DisplayInNav" SortExpression="DisplayInNav" HeaderText="Display In Nav" />
                            <Rock:SecurityField TitleField="InternalName" />
                            <Rock:DeleteField OnClick="gPages_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>