<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkUpdateRequests.ascx.cs" Inherits="RockWeb.Blocks.Connection.BulkUpdateRequests" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfSelectedItems" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Bulk Update Connection Requests</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlEntry" runat="server">

                    <Rock:NotificationBox ID="nbBulkUpdateNotification" runat="server" Visible="false"></Rock:NotificationBox>

                    <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <Rock:RockControlWrapper ID="rcwBulkUpdateCampuses" runat="server" Visible="false" Label="Campus Filter" CssClass="well" Help="Only connection requests for this campus will be affected by the Update.">
                        <Rock:RockCheckBoxList ID="cblBulkUpdateCampuses" runat="server" RepeatDirection="Horizontal" AutoPostBack="true"></Rock:RockCheckBoxList>
                    </Rock:RockControlWrapper>

                    <div class="row fade-inactive">

                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlOpportunity" runat="server" Label="Opportunity" AutoPostBack="true" OnSelectedIndexChanged="ddlOpportunity_SelectedIndexChanged" EnhanceForLongLists="true" />
                            <Rock:RockDropDownList ID="ddlState" runat="server" Label="State" AutoPostBack="true" OnSelectedIndexChanged="ddlState_SelectedIndexChanged" />
                        </div>

                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DatePicker ID="dpFollowUpDate" runat="server" Label="Follow-up Date" AllowPastDateSelection="false" Visible="false" />
                        </div>

                    </div>

                    <div class="row fade-inactive">
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwBulkUpdateConnector" runat="server" Label="Connector">
                                <div>
                                    <Rock:RockRadioButton ID="rbBulkUpdateCurrentConnector" runat="server" CssClass="js-transfer-connector" Text="Keep Current Connector" GroupName="BulkUpdateOpportunityConnector" Checked="true" />
                                </div>
                                <div>
                                    <Rock:RockRadioButton ID="rbBulkUpdateDefaultConnector" runat="server" CssClass="js-transfer-connector" Text="Use Default Connector" GroupName="BulkUpdateOpportunityConnector" />
                                </div>
                                <div>
                                    <Rock:RockRadioButton ID="rbBulkUpdateSelectConnector" runat="server" CssClass="js-transfer-connector" Text="Select Connector" GroupName="BulkUpdateOpportunityConnector" />
                                    <Rock:RockDropDownList ID="ddlBulkUpdateOpportunityConnector" CssClass="ml-4" runat="server" Style="display: none" />
                                </div>
                                <div>
                                    <Rock:RockRadioButton ID="rbBulkUpdateNoConnector" runat="server" CssClass="js-transfer-connector" Text="No Connector" GroupName="BulkUpdateOpportunityConnector" />
                                </div>
                            </Rock:RockControlWrapper>
                        </div>

                        <div class="col-md-6">
                            <Rock:WorkflowTypePicker ID="wtpLaunchWorkflow" runat="server" Label="Launch Workflow" ValidationGroup="vgAlertDetails"
                                Help="Will launch a workflow of this type for each connection request passing in the Connection Request to the Workflow with the key of ‘Request’." />
                        </div>

                    </div>

                    <hr class="mt-5" />

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbAddActivity" runat="server" Text="Add Activity" OnCheckedChanged="cbAddActivity_CheckedChanged" AutoPostBack="true" />
                        </div>
                    </div>

                    <div id="dvActivity" runat="server" visible="false">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlActivityType" runat="server" Label="Activity Type" />
                                <asp:CustomValidator ID="cvSelection" runat="server" OnServerValidate="cvSelection_ServerValidate" Display="None" ErrorMessage="You have not selected an activity type" />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlActivityConnector" runat="server" Label="Connector" />
                            </div>
                        </div>

                        <div class="row">
                            <Rock:RockTextBox runat="server" ID="tbActivityNote" Label="Note" TextMode="MultiLine" Rows="4" FormGroupCssClass="col-md-12" />
                        </div>
                    </div>

                    <div class="actions text-right">
                        <asp:LinkButton ID="btnBulkRequestsUpdateSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Update" CssClass="btn btn-primary" OnClick="btnBulkRequestUpdateSave_Click"></asp:LinkButton>
                        <asp:LinkButton ID="btnBulkRequestUpdateCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnBulkRequestUpdateCancel_Click"></asp:LinkButton>
                    </div>

                </asp:Panel>

            </div>

        </div>

        <Rock:ModalDialog ID="mdConfirmUpdateRequests" runat="server" Title="Confirm Update" SaveButtonText="Confirm" CancelLinkVisible="true" OnSaveClick="btnConfirm_Click" SaveButtonCausesValidation="false">
            <Content>
                <asp:Panel ID="pnlConfirm" runat="server" CssClass="js-panel-confirm">

                      <asp:PlaceHolder ID="phConfirmation" runat="server" />

                  </asp:Panel>

                  <asp:Panel ID="pnlStatusUpdate" runat="server" Visible="true">

                      <Rock:NotificationBox ID="nbStatusUpdate" runat="server" NotificationBoxType="Success"></Rock:NotificationBox>

                </asp:Panel>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        Rock.controls.fullScreen.initialize();
        // Transfer mode: when user selects "Select Connector" show the connector picker
        var syncTransferConnectorControls = function () {
            var selectedOptionIsSelectConnector = $(this).is('#<%= rbBulkUpdateSelectConnector.ClientID %>');
            $("#<%=ddlBulkUpdateOpportunityConnector.ClientID%>").toggle(selectedOptionIsSelectConnector);
        };

        $('#<%= upnlContent.ClientID %> .js-transfer-connector').on('click', syncTransferConnectorControls);
        $("#<%=ddlBulkUpdateOpportunityConnector.ClientID%>").toggle($('#<%=rbBulkUpdateSelectConnector.ClientID%>').is(":checked"));
    });
</script>
