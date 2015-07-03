<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <div class="panel-body">

                 <asp:Panel ID="pnlDetails" runat="server">

                    <div class="form-horizontal">
                        <asp:Literal ID="lFutureSend" runat="server"></asp:Literal>
                        <div class="row margin-b-lg">
                            <div class="col-md-6">
                                <asp:Literal ID="lCreatedBy" runat="server"></asp:Literal>
                            </div>
                            <div class="col-md-6 text-right">
                                <asp:Literal ID="lApprovedBy" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>

                    <div class="recipient-status row">
                        <asp:Panel id="pnlPending" runat="server">
                            <a id="aPending" runat="server" class="btn btn-lg btn-block btn-default">
                                <asp:Literal ID="lPending" runat="server"></asp:Literal><br />
                                <small>Pending</small></a>
                        </asp:Panel>
                        <asp:Panel id="pnlDelivered" runat="server">
                            <a id="aDelivered" runat="server" class="btn btn-lg btn-block btn-info">
                                <asp:Literal ID="lDelivered" runat="server"></asp:Literal><br />
                                <small>Delivered</small></a>
                        </asp:Panel>
                        <asp:Panel id="pnlOpened" runat="server">
                            <a id="aOpened" runat="server" class="btn btn-lg btn-block btn-success" disabled="disabled">
                                <asp:Literal ID="lOpened" runat="server"></asp:Literal><br />
                                <small>Opened</small></a>
                        </asp:Panel>
                        <asp:Panel id="pnlFailed" runat="server">
                            <a id="aFailed" runat="server" class="btn btn-lg btn-block btn-danger">
                                <asp:Literal ID="lFailed" runat="server"></asp:Literal><br />
                                <small>Failed</small></a>
                        </asp:Panel>
                        <asp:Panel id="pnlCancelled" runat="server">
                            <a id="aCancelled" runat="server" class="btn btn-lg btn-block btn-warning">
                                <asp:Literal ID="lCancelled" runat="server"></asp:Literal><br />
                                <small>Cancelled</small></a>
                        </asp:Panel>
                    </div>

                    <asp:HiddenField ID="hfActiveRecipient" runat="server" />

                    <section id="sPending" runat="server" class="js-communication-recipients panel panel-widget">
                        <header class="panel-heading clearfix">Pending Recipients</header>
                        <div class="panel-body">
                            <div class="grid">
                                <Rock:Grid ID="gPending" runat="server" AllowSorting="true">
                                    <Columns>
                                        <Rock:PersonField HeaderText="Name" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:RockBoundField HeaderText="Note" DataField="StatusNote" SortExpression="StatusNote" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </section>

                    <section id="sDelivered" runat="server" class="js-communication-recipients panel panel-widget">
                        <header class="panel-heading clearfix">Delivered Recipients</header>
                        <div class="panel-body">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gDelivered" runat="server" AllowSorting="true" ShowActionRow="false">
                                    <Columns>
                                        <Rock:PersonField HeaderText="Name" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:EnumField HeaderText="Status" DataField="Status" SortExpression="Status" />
                                        <Rock:RockBoundField HeaderText="Note" DataField="StatusNote" SortExpression="StatusNote" />
                                        <Rock:DateTimeField HeaderText="Opened" DataField="OpenedDateTime" SortExpression="OpenedDateTime" />
                                        <Rock:RockBoundField HeaderText="Client" DataField="OpenedClient" SortExpression="OpenedClient" />
                                        <Rock:RockBoundField HeaderText="Message ID" DataField="UniqueMessageId" SortExpression="UniqueMessageId" ColumnPriority="DesktopLarge" />
                                        <Rock:RockBoundField HeaderText="Activity" ItemStyle-CssClass="wrap-contents" DataField="ActivityListHtml" HtmlEncode="false" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </section>

                    <section id="sFailed" runat="server" class="js-communication-recipients panel panel-widget">
                        <header class="panel-heading clearfix">Failed Recipients</header>
                        <div class="panel-body">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gFailed" runat="server" AllowSorting="true">
                                    <Columns>
                                        <Rock:PersonField HeaderText="Name" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:RockBoundField HeaderText="Note" DataField="StatusNote" SortExpression="StatusNote" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </section>

                    <section id="sCancelled" runat="server" class="js-communication-recipients panel panel-widget">
                        <header class="panel-heading clearfix">Cancelled Recipients</header>
                        <div class="panel-body">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gCancelled" runat="server" AllowSorting="true">
                                    <Columns>
                                        <Rock:PersonField HeaderText="Name" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:RockBoundField HeaderText="Note" DataField="StatusNote" SortExpression="StatusNote" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </section>

                    <section id="sOpened" runat="server" class="js-communication-recipients panel panel-widget">
                        <header class="panel-heading clearfix">Opened Recipients</header>
                        <div class="panel-body">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gOpened" runat="server" AllowSorting="true">
                                    <Columns>
                                        <Rock:PersonField HeaderText="Name" DataField="PersonAlias.Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:RockBoundField HeaderText="Note" DataField="StatusNote" SortExpression="StatusNote" />
                                        <Rock:DateTimeField HeaderText="Opened" DataField="OpenedDateTime" SortExpression="OpenedDateTime" />
                                        <Rock:RockBoundField HeaderText="Client" DataField="OpenedClient" SortExpression="OpenedClient" ColumnPriority="Desktop" />
                                        <Rock:RockBoundField HeaderText="Message ID" DataField="UniqueMessageId" SortExpression="UniqueMessageId" ColumnPriority="DesktopLarge" />
                                        <Rock:RockBoundField HeaderText="Activity" DataField="ActivityListHtml" HtmlEncode="false" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </section>

                    <Rock:PanelWidget ID="wpMessageDetails" runat="server" Title="Message Details">
                        <asp:Literal ID="lDetails" runat="server" />
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpEvents" runat="server" Title="Activity" Expanded="false">
                        <div class="grid grid-panel">
                            <Rock:Grid ID="gActivity" runat="server" AllowSorting="true" RowItemText="Activity">
                                <Columns>
                                    <Rock:DateTimeField HeaderText="Date" DataField="ActivityDateTime" SortExpression="ActivityDateTime" />
                                    <Rock:PersonField HeaderText="Person" DataField="CommunicationRecipient.PersonAlias.Person" 
                                        SortExpression="CommunicationRecipient.PersonAlias.Person.LastName,CommunicationRecipient.PersonAlias.Person.NickName" />
                                    <Rock:RockBoundField HeaderText="Activity" DataField="ActivityType" SortExpression="ActivityType" />
                                    <Rock:EnumField HeaderText="Details" ItemStyle-CssClass="wrap-contents" DataField="ActivityDetail" SortExpression="ActivityDetail" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-success" OnClick="btnApprove_Click" />
                        <asp:LinkButton ID="btnDeny" runat="server" Text="Deny" CssClass="btn btn-danger" OnClick="btnDeny_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel Send" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                        <asp:LinkButton ID="btnCopy" runat="server" Text="Copy Communication" CssClass="btn btn-link" OnClick="btnCopy_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlResult" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
                    <br />
                    <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
                </asp:Panel>

            </div>
        </div>

       

        <script>
            $('.js-date-rollover').tooltip();
        </script>

    </ContentTemplate>
</asp:UpdatePanel>


