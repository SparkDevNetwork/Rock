<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunityDetail.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ConnectionOpportunityDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlConnectionOpportunityList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfConnectionOpportunityId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save group with the selected group type and/or parent group." />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbPublicName" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="PublicName" />
                            </div>
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="IconCssClass" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ConnectionOpportunity, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupType" runat="server" Label="Group Type" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" />
                                <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Group Member Role" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" />
                            </div>
                            <div class="col-md-6">
                                <Rock:GroupPicker ID="gpConnectorGroup" runat="server" Label="Connector Group" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:Toggle ID="tglUseAllGroupsOfGroupType" runat="server" Label="Use All Groups Of This Type" ButtonSizeCssClass="btn btn-sm" OnText="Yes" OffText="No" OnCheckedChanged="tglUseAllGroupsOfGroupType_CheckedChanged" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpConnectionOpportunityAttributes" runat="server" Title="Opportunity Attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpConnectionOpportunityGroups" runat="server" Title="Groups">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityGroups" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityGroups_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>
                        <Rock:PanelWidget ID="wpConnectionOpportunityCampuses" runat="server" Title="Campus Connector Groups">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityCampuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Campus Connector Group">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:RockBoundField DataField="Group" HeaderText="Group" />
                                        <Rock:EditField OnClick="gConnectionOpportunityCampuses_Edit" />
                                        <Rock:DeleteField OnClick="gConnectionOpportunityCampuses_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>
                        <Rock:PanelWidget ID="wpConnectionTypeWorkflow" runat="server" Title="Inherited Workflows">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionTypeWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow">
                                    <Columns>
                                        <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                                        <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>
                        <Rock:PanelWidget ID="wpConnectionOpportunityWorkflow" runat="server" Title="Workflows">
                            <div class="grid">
                                <Rock:Grid ID="gConnectionOpportunityWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow">
                                    <Columns>
                                        <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
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

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgConnectionOpportunityWorkflow" runat="server" Title="Campus Select" OnSaveClick="dlgConnectionOpportunityWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ConnectionOpportunityWorkflow">
            <Content>

                <asp:HiddenField ID="hfAddConnectionOpportunityWorkflowGuid" runat="server" />

                <asp:ValidationSummary ID="valConnectionOpportunityWorkflowSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ConnectionOpportunityWorkflow" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When" DataTextField="Name" DataValueField="Id" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" DataTextField="Name" DataValueField="Id" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlFrom" runat="server" Label="From" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTo" runat="server" Label="To" Visible="false" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgConnectionOpportunityGroups" runat="server" ValidationGroup="ConnectionOpportunityGroup" SaveButtonText="Add" OnSaveClick="btnAddConnectionOpportunityGroup_Click" Title="Select Group">
            <Content>
                <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Select Group" ValidationGroup="ConnectionOpportunityGroup" />
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="dlgConnectionOpportunityCampuses" runat="server" ValidationGroup="ConnectionOpportunityGroup" SaveButtonText="Add" OnSaveClick="btnAddConnectionOpportunityCampus_Click" Title="Select Group">
            <Content>
                <asp:HiddenField ID="hfAddConnectionOpportunityCampusGuid" runat="server" />
                <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" ValidationGroup="ConnectionOpportunityGroup" Required="true" />
                <Rock:GroupPicker ID="gpGroup" runat="server" Label="Connector Group" ValidationGroup="ConnectionOpportunityGroup" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
