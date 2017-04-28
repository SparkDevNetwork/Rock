<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestDetail" %>

<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>

       <Rock:ModalAlert ID="maWarning" runat="server" />

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-cloud-upload"></i>
                        <asp:Literal ID="lActionTitle" runat="server" /></h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlCategory" Text="Category..." LabelType="Type" runat="server" />
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <asp:HiddenField ID="hfPrayerRequestId" runat="server" />

                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <!-- Edit -->
                    <div id="pnlEditDetails" runat="server">

                        <fieldset>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:PersonPicker ID="ppRequestor" runat="server" Label="Requested By" EnableSelfSelection="true" OnSelectPerson="ppRequestor_SelectPerson" />
                                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                    <Rock:DatePicker ID="dpExpirationDate" Text="Expires On" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="ExpirationDate" />
                                </div>

                                <div class="col-md-3">
                                    <Rock:RockCheckBox ID="cbIsActive" Label="Active" Text="Yes" runat="server" />
                                    <Rock:HighlightLabel ID="hlblFlaggedMessage" IconCssClass="fa fa-flag" LabelType="warning" runat="server" Visible="false" ToolTip="re-approve the request to clear the flags" />
                                    <Rock:RockControlWrapper ID="rcwOptions" runat="server" Label="Options">
                                        <Rock:RockCheckBox ID="cbIsPublic" Text="Public" runat="server" />
                                        <Rock:RockCheckBox ID="cbIsUrgent" Text="Urgent" runat="server" />
                                        <Rock:RockCheckBox ID="cbAllowComments" Text="Allow Comments" runat="server" />
                                    </Rock:RockControlWrapper>
                                </div>
                                <div class="col-md-3">

                                    <div class="form-group" id="divStatus" runat="server">
                                        <label class="control-label">Status</label>
                                        <div class="form-control-static">
                                            <asp:HiddenField ID="hfApprovedStatus" runat="server" />
                                            <asp:Panel ID="pnlStatus" runat="server" CssClass="toggle-container">
                                                <div class="btn-group btn-toggle">
                                                    <a class="btn btn-xs <%=ApprovedCss%>" data-status="true" data-active-css="btn-success">Approved</a>
                                                    <a class="btn btn-xs <%=PendingCss%>" data-status="false" data-active-css="btn-warning">Pending</a>
                                                </div>
                                            </asp:Panel>
                                        </div>
                                    </div>

                                </div>

                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:CategoryPicker ID="catpCategory" runat="server" Label="Category" Required="true" EntityTypeName="Rock.Model.PrayerRequest" />
                                    <Rock:DataTextBox ID="dtbText" runat="server" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" Label="Request" CssClass="field span12" TextMode="MultiLine" Rows="4" />
                                    <Rock:DataTextBox ID="dtbAnswer" runat="server" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Answer" Label="Answer" CssClass="field span12" TextMode="MultiLine" Rows="4" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <div class="attributes">
                                        <asp:PlaceHolder ID="phAttributes" runat="server" />
                                    </div>
                                </div>
                            </div>

                        </fieldset>

                        <div class="actions">
                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                        </div>

                    </div>

                    <!-- Read only -->
                    <fieldset id="fieldsetViewDetails" runat="server">

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="pull-right">
                            <Rock:HighlightLabel ID="hlblFlaggedMessageRO" LabelType="warning" runat="server" Visible="false" IconCssClass="fa fa-flag" ToolTip="To clear the flags you'll have to re-approve this request." />
                            <Rock:HighlightLabel ID="hlblUrgent" LabelType="Danger" runat="server" Visible="false" IconCssClass="fa fa-exclamation-circle" Text="Urgent" />
                            <Rock:Badge ID="badgePrayerCount" runat="server"></Rock:Badge>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <asp:Literal ID="lMainDetails" runat="server" />

                                <asp:PlaceHolder ID="phDisplayAttributes" runat="server" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                            <asp:LinkButton ID="lbCancelView" runat="server" Text="Back" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                            <asp:LinkButton ID="lbDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="lbDelete_Click" />
                        </div>

                    </fieldset>
                </div>
            </div>
        </asp:Panel>
        <hr />
    </ContentTemplate>
</asp:UpdatePanel>
