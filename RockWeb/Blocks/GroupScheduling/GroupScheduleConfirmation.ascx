<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleConfirmation.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduleConfirmation" %>

<asp:UpdatePanel ID="upnContent" runat="server">
    <ContentTemplate>
          <asp:HiddenField ID="hfSelectedPersonId" runat="server" />
        <Rock:NotificationBox ID="nbError" runat="server" Visible="false" Title="Sorry" NotificationBoxType="Warning" Text="You are not authorized to view this confirmation." />
        <Rock:NotificationBox ID="nbSaveDeclineReasonMessage" runat="server" Visible="false" Title="Thank You" NotificationBoxType="Success" Text="Thank you for letting us know the reason." />
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

                    <asp:Panel ID="pnlSelectConfirmationOption" runat="server" CssClass="pending-confirmations" Visible="false">
                        <asp:Repeater ID="rptSelectedConfirmations" runat="server" OnItemDataBound="rptSelectedConfirmations_ItemDataBound">
                            <ItemTemplate>
                                <asp:Literal ID="litSectionBreak" runat="server" Visible="false"><hr /></asp:Literal>
                                <h5><asp:Literal ID="litDate" runat="server" Visible="false" /></h5>
                                <p>
                                    <asp:Literal ID="litGroupName" runat="server" /><br />
                                    <asp:Literal ID="litLocation" runat="server" />
                                </p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <hr />
                        <div>
                            <asp:Button ID="btnAcceptAll" CssClass="btn btn-success" runat="server" Text="Accept All" OnClick="btnAcceptAll_Click" />
                            <asp:Button ID="btnDeclineAll" CssClass="btn btn-danger" runat="server" Text="Decline All" OnClick="btnDeclineAll_Click" />
                            <a href="/scheduletoolbox">Customize in Schedule Toolbox</a>
                        </div>
                    </asp:Panel>

                    <Rock:ModalDialog ID="mdDeclineAll" runat="server"
                        Title="Declined Reason"
                        ValidationGroup="DeclineAll"
                        OnSaveClick="mdDeclineAll_SaveClick"
                        SaveButtonText="Save"
                        Visible="false">
                        <Content>
                            <p>Your feedback on declining this schedule would be valuable to us.</p>
                            <Rock:RockDropDownList ID="ddlDeclineAllReason" DataValueField="Id" DataTextField="Value" runat="server" Label="Reason for Decline" ValidationGroup="DeclineAll" />
                            <Rock:RockTextBox ID="tbDeclineAllNote" runat="server" Label="Please elaborate on why you cannot attend." TextMode="MultiLine" ValidationGroup="DeclineAll" />
                        </Content>
                    </Rock:ModalDialog>


                    <asp:Panel ID="pnlDeclineReason" runat="server" CssClass="decline-reason margin-b-lg"  Visible="false">
                        <div class="row">
                            <div class="col-md-3">
                                <Rock:RockDropDownList ID="ddlDeclineReason" DataValueField="Id" DataTextField="Value" runat="server" Label="Decline Reason" Visible="false" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <Rock:DataTextBox ID="dtbDeclineReasonNote" runat="server"  TextMode="MultiLine" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.Attendance, Rock" PropertyName="Note" Visible="false" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <asp:Button ID="btnSubmitDeclineReason" CssClass="btn btn-primary" runat="server" Text="Submit" OnClick="btnSubmitDeclineReason_Click" Visible="false" />
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
                                            <asp:LinkButton ID="btnConfirmAttend" runat="server" CssClass="btn btn-xs btn-success" Text="Attend" OnClick="btnConfirmAttend_Click" />
                                            <asp:LinkButton ID="btnDeclineAttend" runat="server" CssClass="btn btn-xs btn-danger" Text="Decline" OnClick="btnDeclineAttend_Click" />
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
        <asp:PostBackTrigger ControlID="btnSubmitDeclineReason" />
    </Triggers>
</asp:UpdatePanel>
