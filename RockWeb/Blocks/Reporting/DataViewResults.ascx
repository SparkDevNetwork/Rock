<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewResults.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewResults" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-table"></i>
                    Results
                </h1>

                <div class="panel-labels">
                    <asp:LinkButton ID="btnToggleResults" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnToggleResults_Click" />
                </div>
            </div>
            <asp:Panel ID="pnlResultsGrid" runat="server">
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbGridError" runat="server" NotificationBoxType="Warning" />
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gDataViewResults" runat="server" AllowSorting="true" EmptyDataText="No Results" />
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
