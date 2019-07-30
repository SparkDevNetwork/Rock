<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSVPResponse.ascx.cs" Inherits="RockWeb.Blocks.RSVP.RSVPResponse" %>

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
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbFirstName" runat="server" Label="First Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbLastName" runat="server" Label="Last Name" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:EmailBox ID="rebEmail" runat="server" Label="Email Address" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSingle_Choice" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-12">
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
                    <Rock:NotificationBox ID="nbDecline" runat="server" NotificationBoxType="Warning" />
                    <asp:Panel ID="pnlDeclineReasons" runat="server" Visible="false">
                        <Rock:RockRadioButtonList ID="rrblDeclineReasons" runat="server" Label="Please enter a reason below:" />
                    </asp:Panel>

                </asp:Panel>

                <asp:Panel ID="pnlMultiple_Choice" runat="server" Visible="false">
                    <asp:PlaceHolder ID="phOccurrences" runat="server" />
                    
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
