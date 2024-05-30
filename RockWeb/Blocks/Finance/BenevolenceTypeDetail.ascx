<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceTypeDetail" %>
<asp:UpdatePanel ID="upnlDevice" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" />

        <%-- Edit Device Details --%>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block js-type-panel" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-desktop"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" CssClass="js-inactivetype-label" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbDuplicateDevice" runat="server" NotificationBoxType="Warning" Title="Sorry" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BenevolenceType, Rock" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-1">
                            </div>
                        <div class="col-md-2">
                            <Rock:RockCheckBox ID="cbShowFinancialResults" runat="server" CssClass="js-isactivetype" Label="Show Financial Results" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" CssClass="js-isactivetype" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BenevolenceType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceLavaTemplate" runat="server" SourceTypeName="Rock.Model.BenevolenceType, Rock" PropertyName="RequestLavaTemplate" EditorMode="Lava" EditorTheme="Rock" EditorHeight="200" Label="Request Lava Template"
                                Help="Used to show personalized resources or instructions based on the information on the request. The request will be provided in the <strong>'BenevolenceRequest'</strong> merge field." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="numberBoxMaxDocuments" runat="server" Label="Maximum Number of Documents" Help="The maximum number of documents that can be added to a request." Required="true"></Rock:NumberBox>
                        </div>
                        <div class="col-md-6">

                        </div>
                    </div>

                    <div class="form-group">
                        <h4>Workflows</h4>
                        <div class="panel-body">
                            <div class="grid grid-panel">
                                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                                <Rock:Grid ID="gBenevolenceTypeWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow" ShowConfirmDeleteDialog="false" OnRowDataBound="gBenevolenceTypeWorkflows_RowDataBound">
                                    <Columns>
                                        <Rock:RockBoundField DataField="WorkflowTypeName" HeaderText="Workflow Type" HtmlEncode="false" />
                                        <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                        <Rock:EditField ID="efBenevolenceType" OnClick="gBenevolenceTypeWorkflows_Edit" />
                                        <Rock:DeleteField ID="dfBenevolenceType" OnClick="gBenevolenceTypeWorkflows_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>

                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <!-- Modal Dialogs -->
        <asp:Button ID="btnHideDialog" runat="server" Style="display: none" OnClick="btnHideDialog_Click" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdWorkflowDetails" runat="server" Title="Select Workflow" SaveButtonText="Add" OnSaveClick="mdWorkflowDetails_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="WorkflowDetails">
            <Content>

                <asp:HiddenField ID="hfWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valWorkflowDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="WorkflowDetails" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When"
                            OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="WorkflowDetails">
                            <asp:ListItem Value="0" Text="Request Started" />
                            <asp:ListItem Value="1" Text="Status Changed" />
                            <asp:ListItem Value="2" Text="Caseworker Assigned" />
                            <asp:ListItem Value="3" Text="Manual" />
                        </Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wpWorkflowType" runat="server" Label="Workflow Type" Required="true" ValidationGroup="WorkflowDetails" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" ValidationGroup="WorkflowDetails" />
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" ValidationGroup="WorkflowDetails" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <script>
            function clearActiveDialog() {
                $('#<%=mdWorkflowDetails.ClientID %>').val('');
            };

            Sys.Application.add_load(function () {
                function setIsActiveControls(activeCheckbox) {

                    var $inactiveLabel = $(activeCheckbox).closest(".js-type-panel").find('.js-inactivetype-label');
                    if ($(activeCheckbox).is(':checked')) {
                        $inactiveLabel.hide();
                    }
                    else {
                        $inactiveLabel.show();
                    }
                }

                $('.js-isactivetype').on('click', function () {
                    setIsActiveControls(this);
                });

                $('.js-isactivetype').each(function (i) {
                    setIsActiveControls(this);
                });

            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
