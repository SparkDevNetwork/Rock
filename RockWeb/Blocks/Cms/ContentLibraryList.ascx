<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentLibraryList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentLibraryList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"> Content Libraries</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfFilter" runat="server">
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gContentLibrary" runat="server" EmptyDataText="No Libraries Found" RowItemText="Library" AllowSorting="true" TooltipField="Description" OnRowSelected="gContentLibrary_Edit">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Sources" HeaderText="Sources" SortExpression="Sources" />
                            <Rock:RockBoundField DataField="LastIndexItemCount" HeaderText="Item Count" SortExpression="LastIndexItemCount" />
                            <Rock:DeleteField OnClick="gContentLibrary_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
