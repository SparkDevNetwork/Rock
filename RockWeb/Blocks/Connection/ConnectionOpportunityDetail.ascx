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
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfConnectionOpportunityId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lIcon" runat="server" />
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:NotificationBox ID="nbIncorrectOpportunity" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The opportunity selected does not belong to the selected connection type." />

                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save opportunities for the configured connection type." />

                    <Rock:NotificationBox ID="nbInvalidGroupTypes" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Groups" />

                    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="Name" OnBlur='updateField(this)' />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbPublicName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="PublicName" />
                            </div>
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="IconCssClass" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                            </div>
                            <div class="col-md-6">
                                <div class="well">
                                    <h4>Placement Group Configuration</h4>
                                    <Rock:RockDropDownList ID="ddlGroupType" runat="server" Label="Group Type" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" Help="The group type that the user will be placed in" />
                                    <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Group Member Role" Help="The role that the person will hold after being connected" />
                                    <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" Help="The Status of the person upon being connected" />
                                    <Rock:Toggle ID="tglUseAllGroupsOfGroupType" runat="server" Label="Use All Groups Of This Type" ButtonSizeCssClass="btn btn-sm" OnText="Yes" OffText="No" OnCheckedChanged="tglUseAllGroupsOfGroupType_CheckedChanged" Help="All groups of this group type are used for this opportunity" />
                                </div>
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityGroups" runat="server" Title="Groups">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityGroups" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityGroups_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityConnectorGroups" runat="server" Title="Connector Groups">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityConnectorGroups" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Campus Connector Group" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Group" HeaderText="Group" />
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:EditField OnClick="gConnectionOpportunityConnectorGroups_Edit" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityConnectorGroups_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityWorkflow" runat="server" Title="Workflows">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow" OnRowDataBound="gConnectionOpportunityWorkflows_RowDataBound">
                                    <Columns>
                                        <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" HtmlEncode="false" />
                                        <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                        <Rock:EditField OnClick="gConnectionOpportunityWorkflows_Edit" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityWorkflows_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
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

                <asp:ValidationSummary ID="valWorkflowDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="WorkflowDetails" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" DataTextField="Name" DataValueField="Id" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupDetails" runat="server" ValidationGroup="GroupDetails" SaveButtonText="Add" OnSaveClick="dlgGroupDetails_SaveClick" Title="Select Group">
            <Content>
                <asp:ValidationSummary ID="valGroupDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="GroupDetails" />
                <Rock:NotificationBox ID="nbInvalidGroupType" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Group Type" />
                <Rock:GroupPicker ID="gpOpportunityGroup" runat="server" Label="Select Group" ValidationGroup="GroupDetails" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectorGroupDetails" runat="server" ValidationGroup="ConnectorGroup" SaveButtonText="Add" OnSaveClick="dlgConnectorGroupDetails_SaveClick" Title="Select Group">
            <Content>
                <asp:ValidationSummary ID="valConnectorGroup" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ConnectorGroup" />
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
