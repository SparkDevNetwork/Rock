<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersistedDatasetList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersistedDatasetList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-database"></i>
                    Persisted Dataset List
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="AccessKey" HeaderText="AccessKey" SortExpression="AccessKey" />
                            <Rock:DateTimeField DataField="LastRefreshDateTime" HeaderText="Last Refresh" SortExpression="LastRefreshDateTime" />
                            <Rock:LinkButtonField HeaderText="Refresh" CssClass="btn btn-default btn-sm fa fa-refresh" ToolTip="Rebuild the persisted dataset" OnClick="lbRefresh_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" OnDataBound="btnRefresh_DataBound" />
                            <Rock:DeleteField OnClick="gList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
