<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleConfirmation.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupScheduleConfirmation" %>

<asp:UpdatePanel ID="upnContent" runat="server">
    <ContentTemplate>
          <asp:HiddenField ID="hfSelectedPersonId" runat="server" />
        <Rock:NotificationBox ID="nbError" runat="server" Visible="false" Title="Sorry" NotificationBoxType="Warning" Text="You are not authorized to view this confirmation" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lBlockTitleIcon" runat="server" />
                    <asp:Literal ID="lBlockTitle" Visible="true" runat="server" />
                </h1>
            </div>

                <div class="panel-body">
                    <div class="confirmation-message">
                    <asp:Literal ID="lResponse" runat="server" Visible="false" />
                    </div>
                    <asp:Panel ID="pnlDeclineReason" runat="server" CssClass="decline-reason margin-b-lg"  Visible="false">
                        <div class="row">
                            <div class="col-md-3">
                                <Rock:RockDropDownList ID="ddlDeclineReason" DataValueField="Id" DataTextField="Value"  runat="server" Label="Decline Reason" Visible="false" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <Rock:DataTextBox ID="dtbDeclineReasonNote" runat="server"  TextMode="MultiLine" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.Attendance, Rock" PropertyName="Note" Visible="false" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <asp:Button ID="btnSubmit" CssClass="btn btn-primary" runat="server" Text="Submit" OnClick="btnSubmit_Click" Visible="false" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlPendingConfirmation" runat="server" CssClass="pending-confirmations" Visible="false">
                        <span class="control-label">
                            <asp:Literal runat="server" ID="lPendingConfirmations" Text="Pending Confirmations" />
                        </span>
                        <table class="table table-borderless"><tbody>
                        <asp:Repeater ID="rptPendingConfirmations" runat="server" OnItemDataBound="rptPendingConfirmations_ItemDataBound">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:Literal ID="lPendingOccurrenceDetails" runat="server" />
                                    </td>
                                    <td>
                                        <asp:Literal ID="lPendingOccurrenceTime" runat="server" />
                                    </td>
                                    <td>
                                        <div class="actions">
                                            <asp:LinkButton ID="btnConfirmAttending" runat="server" CssClass="btn btn-xs btn-success" Text="Attending" OnClick="btnConfirmAttending_Click" />
                                            <asp:LinkButton ID="btnDeclineAttending" runat="server" CssClass="btn btn-xs btn-danger" Text="Decline" OnClick="btnDeclineAttending_Click" />
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                        </tbody></table>
                    </asp:Panel>
                </div>

        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnSubmit" />
    </Triggers>
</asp:UpdatePanel>
