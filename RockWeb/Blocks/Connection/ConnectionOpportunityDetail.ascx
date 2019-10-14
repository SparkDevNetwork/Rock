<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunityDetail.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionOpportunityDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%= btnHideDialog.ClientID %>').click();
    }

    function updateField(obj) {
        $("#<%= tbPublicName.ClientID %>").val($(obj).val());
    }
</script>

<asp:UpdatePanel ID="upnlConnectionOpportunityDetail" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfConnectionOpportunityId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lIcon" runat="server" />
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:NotificationBox ID="nbIncorrectOpportunity" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The opportunity selected does not belong to the selected connection type." />

                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save opportunities for the configured connection type." />

                    <Rock:NotificationBox ID="nbInvalidGroupTypes" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Groups" />

                    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="Name" OnBlur='updateField(this)' />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:HtmlEditor ID="htmlSummary" runat="server" Label="Summary" Help="The content to display as a public summary for this opportunity (i.e. in search results)." />
                        <Rock:HtmlEditor ID="htmlDescription" runat="server" Label="Details" Help="The complete details of this opportunity." />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbPublicName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="PublicName" />
                            </div>
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                            </div>
                            <div class="col-md-6">
                                <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                                <Rock:RockControlWrapper ID="rcwCampus" runat="server" Label="Campuses">
                                    <div class="controls">
                                        <asp:CheckBoxList ID="cblCampus" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal"
                                            AutoPostBack="true" OnSelectedIndexChanged="cblCampus_SelectedIndexChanged" />
                                    </div>
                                </Rock:RockControlWrapper>
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityGroups" runat="server" Title="Placement Groups">
                            <h4>Placement Group Configuration</h4>
							<div class="grid">
								<Rock:Grid ID="gConnectionOpportunityGroupConfigs" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Type" ShowConfirmDeleteDialog="false">
									<Columns>
										<Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" />
										<Rock:RockBoundField DataField="GroupMemberRoleName" HeaderText="Group Member Role" />
										<Rock:EnumField DataField="GroupMemberStatus" HeaderText="Group Member Status" />
                                        <Rock:BoolField DataField="UseAllGroupsOfType" HeaderText="Use All Groups of This Type" />
                                        <Rock:EditField OnClick="gConnectionOpportunityGroupConfigs_Edit" />
										<Rock:DeleteField OnClick="gConnectionOpportunityGroupConfigs_Delete" />
									</Columns>
								</Rock:Grid>
                            </div>
                            <h4>Placement Groups</h4>
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityGroups" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="GroupName" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" />
                                        <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityGroups_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityConnectorGroups" runat="server" Title="Connector Groups">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="grid">
                                        <Rock:Grid ID="gConnectionOpportunityConnectorGroups" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Campus Connector Group" ShowConfirmDeleteDialog="false">
                                            <Columns>
                                                <Rock:RockBoundField DataField="GroupName" HeaderText="Group" />
                                                <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" />
                                                <Rock:EditField OnClick="gConnectionOpportunityConnectorGroups_Edit" />
                                                <Rock:DeleteField OnClick="gConnectionOpportunityConnectorGroups_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <asp:ListView ID="lvDefaultConnectors" runat="server">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfDefaultConnector" runat="server" />
                                            <Rock:RockDropDownList ID="ddlDefaultConnector" runat="server" DataValueField="Key" DataTextField="Value" />
                                        </ItemTemplate>
                                    </asp:ListView>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityWorkflow" runat="server" Title="Workflows">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow"
                                    ShowConfirmDeleteDialog="false" OnRowDataBound="gConnectionOpportunityWorkflows_RowDataBound">
                                    <Columns>
                                        <Rock:RockBoundField DataField="WorkflowTypeName" HeaderText="Workflow Type" HtmlEncode="false" />
                                        <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                        <Rock:EditField OnClick="gConnectionOpportunityWorkflows_Edit" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityWorkflows_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s"  Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Button ID="btnHideDialog" runat="server" Style="display: none" OnClick="btnHideDialog_Click" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgWorkflowDetails" runat="server" Title="Select Workflow" OnSaveClick="dlgWorkflowDetails_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="WorkflowDetails">
            <Content>

                <asp:HiddenField ID="hfWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valWorkflowDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="WorkflowDetails" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When"
                            OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="WorkflowDetails" >
                            <asp:ListItem Value="0" Text="Request Started" />
                            <asp:ListItem Value="8" Text="Request Assigned" />
                            <asp:ListItem Value="7" Text="Request Transferred" />
                            <asp:ListItem Value="1" Text="Request Connected" />
                            <asp:ListItem Value="5" Text="Placement Group Assigned" />
                            <asp:ListItem Value="2" Text="Status Changed" />
                            <asp:ListItem Value="3" Text="State Changed" />
                            <asp:ListItem Value="4" Text="Activity Added" />
                            <asp:ListItem Value="6" Text="Manual" />
                        </Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wpWorkflowType" runat="server" Label="Workflow Type" Required="true" ValidationGroup="WorkflowDetails"/>
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

        <Rock:ModalDialog ID="dlgGroupDetails" runat="server" ValidationGroup="GroupDetails" SaveButtonText="Add" OnSaveClick="dlgGroupDetails_SaveClick" Title="Select Group">
            <Content>
                <asp:ValidationSummary ID="valGroupDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupDetails" />
                <Rock:NotificationBox ID="nbInvalidGroupType" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Group Type" />
                <Rock:GroupPicker ID="gpOpportunityGroup" runat="server" Label="Select Group" AllowMultiSelect="true" ValidationGroup="GroupDetails" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupConfigDetails" runat="server" ValidationGroup="GroupConfig" SaveButtonText="Add" OnSaveClick="dlgGroupConfigDetails_SaveClick" Title="Placement Group Configuration">
            <Content>
                <asp:ValidationSummary ID="valGroupConfig" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupConfig" />
                <asp:HiddenField ID="hfGroupConfigGuid" runat="server" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlGroupType" runat="server" Label="Group Type" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" Help="The group type that the user will be placed in" Required="true" ValidationGroup="GroupConfig" EnhanceForLongLists="true" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Group Member Role" Help="The role that the person will hold after being connected" Required="true" ValidationGroup="GroupConfig" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" Help="The Status of the person upon being connected" Required="true" ValidationGroup="GroupConfig" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:Toggle ID="tglUseAllGroupsOfGroupType" runat="server" Label="Use All Groups of This Type" ButtonSizeCssClass="btn btn-sm" OnText="Yes" OffText="No" Help="All groups of this group type are used for this opportunity" />
                    </div>
                </div>

                <div id="divUseGroupsOfTypeNote" runat="server" class="alert alert-info" style="display:none">
                    Note: Setting the 'Use All Groups of This Type' option will remove any placement groups of this group type that have been specifically configured for this opportunity type.
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectorGroupDetails" runat="server" ValidationGroup="ConnectorGroup" SaveButtonText="Add" OnSaveClick="dlgConnectorGroupDetails_SaveClick" Title="Select Group">
            <Content>
                <asp:ValidationSummary ID="valConnectorGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="ConnectorGroup" />
                <asp:HiddenField ID="hfConnectorGroupGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpGroup" runat="server" Label="Connector Group" ValidationGroup="ConnectorGroup" Required="true"  />
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" ValidationGroup="ConnectorGroup"/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
