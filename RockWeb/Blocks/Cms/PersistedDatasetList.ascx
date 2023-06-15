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
                            <Rock:RockBoundField DataField="TimeToBuildMS" HeaderText="Time to Build (ms)" NullDisplayText="-" DataFormatString="{0:F0}" SortExpression="TimeToBuildMS" />
                            <Rock:RockBoundField DataField="Size" HeaderText="Result Size (KB)" NullDisplayText="-" DataFormatString="{0:N0}" SortExpression="TimeToBuildMS" />
                            <Rock:DateTimeField DataField="LastRefreshDateTime" HeaderText="Last Refresh" NullDisplayText="-" SortExpression="LastRefreshDateTime" />
                            <Rock:LinkButtonField HeaderText="Refresh" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-refresh'></i>" ToolTip="Rebuild the persisted dataset" OnClick="lbRefresh_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" OnDataBound="btnRefresh_DataBound" />
                            <Rock:LinkButtonField HeaderText="Preview" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-search'></i>" ToolTip="Preview the generated JSON " OnClick="lbPreview_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            <Rock:DeleteField OnClick="gList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <Rock:ModalDialog ID="mdPreview" runat="server" Title="Preview the Dataset JSON" OnCancelScript="">
                <Content>
                    <Rock:NotificationBox ID="nbPreviewMessage" runat="server" />
                    <Rock:NotificationBox ID="nbPreviewMaxLengthWarning" NotificationBoxType="Info" runat="server" />
                    <Rock:RockLiteral ID="lPreviewJson" runat="server" Label="Build Script output" CssClass="js-preview-json" ViewStateMode="Disabled" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
