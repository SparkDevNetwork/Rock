<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationList.ascx.cs" Inherits="RockWeb.Blocks.Core.CommunicationList" %>

<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:GridFilter ID="rFilter" runat="server">
                
            <Rock:RockTextBox ID="tbSubject" runat="server" Label="Subject" />
            <Rock:ComponentPicker ID="cpChannel" runat="server" ContainerType="Rock.Communication.ChannelContainer, Rock" Label="Channel" />
            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
            <Rock:PersonPicker ID="ppSender" runat="server" Label="Created By" />
            <Rock:RockTextBox ID="tbContent" runat="server" Label="Content" />
        </Rock:GridFilter>

        <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" OnRowSelected="gCommunication_RowSelected" OnRowDataBound="gCommunication_RowDataBound" >
            <Columns>
                <asp:BoundField DataField="Communication.Subject" SortExpression="Communication.Subject" HeaderText="Subject" />
                <asp:BoundField DataField="Communication.ChannelEntityType.FriendlyName" SortExpression="Communication.ChannelEntityType.FriendlyName" HeaderText="Channel" />
                <asp:BoundField DataField="Communication.Sender.FullName" SortExpression="Communication.Sender.FullName" HeaderText="Created By" />
                <asp:BoundField DataField="Communication.Reviewer.FullName" SortExpression="Communication.Reviewer.FullName" HeaderText="Reviewed By" />
                <Rock:DateTimeField DataField="Communication.ReviewedDateTime" SortExpression="Communication.ReviewedDateTime" HeaderText="Date Reviewed" />
                <Rock:EnumField DataField="Communication.Status" SortExpression="Communication.Status" HeaderText="Communication Status" />
                <asp:TemplateField HeaderText="Recipients" SortExpression="Recipients">
                    <ItemTemplate>
                        <Rock:Badge ID="bPending" runat="server" ToolTip="Pending" BadgeType="None"><%# ((int)Eval("PendingRecipients")).ToString("N0") %></Rock:Badge>
                        <Rock:Badge ID="bSuccess" runat="server" ToolTip="Successful" BadgeType="Success"><%# ((int)Eval("SuccessRecipients")).ToString("N0") %></Rock:Badge>
                        <Rock:Badge ID="bWarning" runat="server" ToolTip="Cancelled" BadgeType="Warning"><%# ((int)Eval("CancelledRecipients")).ToString("N0") %></Rock:Badge>
                        <Rock:Badge ID="bFailed" runat="server" Tooltip="Failed" BadgeType="Important"><%# ((int)Eval("FailedRecipients")).ToString("N0") %></Rock:Badge>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="gCommunication_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
