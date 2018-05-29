<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupArchivedList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupArchivedList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-archive"></i>
                    Archived Groups
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfList" runat="server" OnDisplayFilterValue="gfList_DisplayFilterValue" OnClearFilterClick="gfList_ClearFilterClick" OnApplyFilterClick="gfList_ApplyFilterClick">
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:GroupTypePicker ID="gtpGroupTypeFilter" runat="server" Label="Group Type" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" RowItemText="archived group">
                        <Columns>
                            <Rock:RockBoundField DataField="GroupType.Name" HeaderText="Group Type" SortExpression="GroupType.Name"/>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:DateField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                            <Rock:DateField DataField="ArchivedDateTime" HeaderText="Archived" SortExpression="ArchivedDateTime" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right"/>
                            <Rock:PersonField DataField="ArchivedByPersonAlias" HeaderText="Archived By" SortExpression="ArchivedByPersonAlias.Person.LastName, ArchivedByPersonAlias.Person.NickName" />
                            <Rock:LinkButtonField CssClass="btn btn-default btn-sm" ID="btnUnarchive" Text="<i class='fa fa-undo' title='Unarchive'></i>" OnClick="btnUnarchive_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
