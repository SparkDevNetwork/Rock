<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SequenceMapEditor.ascx.cs" Inherits="RockWeb.Blocks.Sequences.SequenceMapEditor" %>

<asp:UpdatePanel ID="upMapEditor" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ol"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" />

                <div class="margin-all-md well">
                    <Rock:SlidingDateRangePicker runat="server" ID="sdrpDateRange" EnabledSlidingDateRangeUnits="Day, Week, Month, Year" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                    <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-primary" ToolTip="Refresh" OnClick="btnRefresh_Click"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                </div>

                <Rock:RockCheckBoxList ID="cblCheckboxes" runat="server" RepeatDirection="Vertical" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
