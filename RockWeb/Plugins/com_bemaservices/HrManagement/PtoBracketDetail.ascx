<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoBracketDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoBracketDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%= btnHideDialog.ClientID %>').click();
    }

    function updateField(obj) {
        $("#<%= tbPublicName.ClientID %>").val($(obj).val());
    }
</script>

<asp:UpdatePanel ID="upnlPtoBracketDetail" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfPtoBracketId" runat="server" />

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

                    <Rock:NotificationBox ID="nbInvalidPtoTypes" runat="server" NotificationBoxType="Danger" Visible="false" Heading="Pto Types" />

                    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbMinimumYears" runat="server" SourceTypeName="com.bemaservices.HrManagement.Model.PtoBracket, com.bemaservices.HrManagement" PropertyName="MinimumYears" OnBlur='updateField(this)' />
                                <Rock:DataTextBox ID="tbMaximumYears" runat="server" SourceTypeName="com.bemaservices.HrManagement.Model.PtoBracket, com.bemaservices.HrManagement" PropertyName="MaximumYears" OnBlur='updateField(this)' />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <%--<Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </Rock:PanelWidget>--%>

                        <Rock:PanelWidget ID="wpPtoBracketTypes" runat="server" Title="Annual PTO Allocations">
                            <h4>Annual PTO Allocations Configuration</h4>
							<div class="grid">
								<Rock:Grid ID="gPtoBracketTypeConfigs" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="PTO Type" ShowConfirmDeleteDialog="false">
									<Columns>
										<Rock:RockBoundField DataField="PtoTypeName" HeaderText="PTO Type" />
										<Rock:RockBoundField DataField="DefaultHours" HeaderText="Default Hours Accured" />
                                        <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                                        <Rock:EditField OnClick="gPtoBracketTypeConfigs_Edit" />
										<Rock:DeleteField OnClick="gPtoBracketTypeConfigs_Delete" />
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

        <Rock:ModalDialog ID="dlgPtoTypeConfigDetails" runat="server" ValidationGroup="PtoTypeConfig" SaveButtonText="Add" OnSaveClick="dlgPtoTypeConfigDetails_SaveClick" Title="PTO Allocation Configuration">
            <Content>
                <asp:ValidationSummary ID="valPtoTypeConfig" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupConfig" />
                <asp:HiddenField ID="hfGroupConfigGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="PTO Type" OnSelectedIndexChanged="ddlPtoType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="PtoTypeConfig" EnhanceForLongLists="false" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="ddlDefaultHours" runat="server" Label="Default Hours" Help="The number of hours the person will accru on an annual basis for this PTO Type." Required="true" ValidationGroup="PtoTypeConfig" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
