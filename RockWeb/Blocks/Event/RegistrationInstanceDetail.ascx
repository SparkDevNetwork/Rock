<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceDetail" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="wizard">
            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardTemplate" runat="server" OnClick="lbTemplate_Click" CausesValidation="false">
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-clipboard"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardTemplateName" runat="server" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-file-o"></i>
                </div>
                <div class="wizard-item-label">
                    <asp:Literal ID="lWizardInstanceName" runat="server" />
                </div>
            </div>

            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-group"></i>
                </div>
                <div class="wizard-item-label">
                    Registration
                </div>
            </div>

            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-user"></i>
                </div>
                <div class="wizard-item-label">
                    Registrant
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfRegistrationInstanceId" runat="server" />
            <asp:HiddenField ID="hfRegistrationTemplateId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading panel-follow">
                    <h1 class="panel-title">
                        <i class="fa fa-file-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                        <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Type" />
                    </div>
                    <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <Rock:RegistrationInstanceEditor ID="rieDetails" runat="server" />

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lName" runat="server" Label="Name" />
                                <Rock:RockLiteral ID="lMaxAttendees" runat="server" Label="Maximum Attendees" />
                                <Rock:RockLiteral ID="lWorkflowType" runat="server" Label="Registration Workflow" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lCost" runat="server" Label="Cost" />
                                <Rock:RockLiteral ID="lMinimumInitialPayment" runat="server" Label="Minimum Initial Payment" />
                                <Rock:RockLiteral ID="lDefaultPaymentAmount" runat="server" Label="Default Payment Amount" />
                                <Rock:RockLiteral ID="lAccount" runat="server" Label="Account" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lStartDate" runat="server" Label="Registration Starts" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lEndDate" runat="server" Label="Registration Ends" />
                            </div>
                        </div>

                        <Rock:RockLiteral ID="lDetails" runat="server" Label="Details"></Rock:RockLiteral>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-instance-has-payments" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-instance" OnClick="btnDelete_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <asp:LinkButton ID="btnSendPaymentReminder" runat="server" Text="Send Payment Reminders" CssClass="btn btn-default" OnClick="btnSendPaymentReminder_Click" Visible="false" />
                                <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm btn-square" Text="<i class='fa fa-clone'></i>" OnClick="btnCopy_Click" />
                            </span>
                        </div>
                    </fieldset>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
