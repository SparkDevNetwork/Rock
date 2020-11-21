<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HistoryLog.ascx.cs" Inherits="RockWeb.Blocks.Core.HistoryLog" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lHeading" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlDateAdded" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.History" />
                        <Rock:PersonPicker ID="ppWhoFilter" runat="server" Label="Who" />
                        <Rock:RockTextBox ID="tbSummary" runat="server" Label="Summary Contains" />
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gHistory" runat="server" AllowSorting="true" RowItemText="Change" >
                        <Columns>
                            <Rock:RockBoundField DataField="Category.Name" SortExpression="Category.Name" HeaderText="Category" />
                            <asp:HyperLinkField DataTextField="CreatedByPersonName" DataNavigateUrlFields="CreatedByPersonId" SortExpression="CreatedByPersonName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Who" />
                            <Rock:ListDelimitedField DataField="HistoryList" Delimiter="<br />" HeaderText="Did" HtmlEncode="false" SortExpression="Verb,ValueName" />
                            <Rock:RockBoundField DataField="FormattedCaption" HeaderText="What" SortExpression="Caption" HtmlEncode="false" />
                            <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" HeaderText="When" FormatAsElapsedTime="true" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
