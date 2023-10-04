<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.Rockumentation.BookList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDocBookList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Books</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:GridFilter ID="gfDocBook" runat="server" OnApplyFilterClick="gfDocBook_ApplyFilterClick" OnClearFilterClick="gfDocBook_ClearFilterClick" OnDisplayFilterValue="gfDocBook_DisplayFilterValue">
                        <Rock:RockTextBox ID="tbTitleFilter" runat="server" Label="Title" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gDocBook" runat="server" RowItemText="Book" AllowSorting="true" TooltipField="Description" OnRowSelected="gDocBook_RowSelected" OnGridRebind="gDocBook_GridRebind" OnRowDataBound="gDocBook_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:RockBoundField DataField="Slug" HeaderText="Slug" SortExpression="Slug" />
                            <Rock:DeleteField OnClick="gDocBook_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>