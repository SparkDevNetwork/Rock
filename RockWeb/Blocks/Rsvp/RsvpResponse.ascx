<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RsvpResponse.ascx.cs" Inherits="RockWeb.Blocks.RSVP.RSVPResponse" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <asp:Panel ID="pnlHeading" runat="server" CssClass="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-user-check"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="RSVP for Event" />
                </h1>
            </asp:Panel>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbNotAuthorized" runat="server" Visible="false" Title="Sorry" NotificationBoxType="Warning" Text="You are not authorized to view this invitation." />
                <asp:Panel ID="pnl404" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbNotFound" runat="server" NotificationBoxType="Warning" Visible="false" Heading="Not Found">
                        Sorry, this RSVP could not be found.
                    </Rock:NotificationBox>
                    <Rock:NotificationBox ID="nbExpired" runat="server" NotificationBoxType="Warning" Visible="false" Heading="Invitation Expired">
                        Sorry, this event RSVP is no longer active.
                    </Rock:NotificationBox>
                </asp:Panel>

                <asp:Panel ID="pnlForm" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="rtbFirstName" runat="server" Label="First Name" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="rtbLastName" runat="server" Label="Last Name" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:EmailBox ID="rebEmail" runat="server" Label="Email Address" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSingle_Choice" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-sm-12">
                            <asp:PlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbAccept_Single" runat="server" AccessKey="a" ToolTip="Alt+A" Text="Accept" CssClass="btn btn-primary" OnClick="lbAccept_Single_Click"  />
                        <asp:LinkButton ID="lbDecline_Single" runat="server" AccessKey="d" ToolTip="Alt+D" Text="Decline" CssClass="btn btn-default" CausesValidation="false" OnClick="lbDecline_Single_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSingle_Accept" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbAccept" runat="server" NotificationBoxType="Success" />
                </asp:Panel>

                <asp:Panel ID="pnlSingle_Decline" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:NotificationBox ID="nbDecline" runat="server" NotificationBoxType="Warning" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlDeclineReasons" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockRadioButtonList ID="rrblDeclineReasons" runat="server" Label="Please enter a reason below:" Required="true" DataTextField="Value" DataValueField="Id" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockTextBox ID="rtbDeclineNote" runat="server" MaxLength="255" Label="Note" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:HiddenField ID="hfDeclineReason_OccurrenceId" runat="server" />
                            <asp:LinkButton ID="lbSaveDeclineReason" runat="server" AccessKey="s" ToolTip="Alt+S" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveDeclineReason_Click" />
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlDeclineReasonConfirmation" runat="server" Visible="false">
                        <Rock:NotificationBox ID="nbDeclineReasonSaved" runat="server" NotificationBoxType="Success" Text="Saved." />
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlMultiple_Choice" runat="server" Visible="false">

                    <asp:Repeater ID="rptrValues" runat="server" OnItemDataBound="rptrValues_ItemDataBound">
                        <ItemTemplate>
                            <div class="defined-type-checklist">
                                <article class="panel panel-widget checklist-item">
                                    <header class="panel-heading clearfix">
                                        <asp:HiddenField ID="hfOccurrenceId" runat="server" Value='<%# Eval("OccurrenceId") %>' />
                                        <Rock:RockCheckBox ID="rcbAccept" runat="server" Text='<%# Eval("Title") %>' CssClass="rsvp-list-input" />
                                    </header>
                                    <div class="checklist-description panel-body" style="display: none;">
                                        <asp:PlaceHolder ID="phOccurrenceAttributes" runat="server" />
                                    </div>
                                </article>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                    <Rock:NotificationBox ID="nbNoOccurrencesSelected" runat="server" NotificationBoxType="Warning" Text="Please select at least one occurrence to accpt." Visible="false" />
                    <div class="actions">
                        <asp:LinkButton ID="lbAccept_Multiple" runat="server" AccessKey="a" ToolTip="Alt+A" Text="Accept" CssClass="btn btn-primary" OnClick="lbAccept_Multiple_Click"  />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlMultiple_Accept" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbAcceptMultiple" runat="server" NotificationBoxType="Success" />
                </asp:Panel>

            </div>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>
