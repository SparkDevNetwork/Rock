<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoBracketDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoBracketDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%= btnHideDialog.ClientID %>').click();
    }
</script>

<asp:UpdatePanel ID="upnlPtoBracketDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfPtoBracketId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">
                        <i class='fa fa-clock'></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:NotificationBox ID="nbInvalidPtoTypes" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="At least one PTO Type needs to be configured."/>

                    <Rock:NotificationBox ID="nbIncorrectTier" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The bracket selected does not belong to the selected PTO Tier." />

                    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbMinimumYears" runat="server" SourceTypeName="com.bemaservices.HrManagement.Model.PtoBracket, com.bemaservices.HrManagement" PropertyName="MinimumYear" OnBlur='updateField(this)' />
                                <Rock:DataTextBox ID="tbMaximumYears" runat="server" SourceTypeName="com.bemaservices.HrManagement.Model.PtoBracket, com.bemaservices.HrManagement" PropertyName="MaximumYear" OnBlur='updateField(this)' />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <%--<Rock:PanelWidget ID="wpAttributes" runat="server" Title="Opportunity Attributes">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </Rock:PanelWidget>--%>

                        <Rock:PanelWidget ID="wpPtoBracketTypes" runat="server" Title="PTO Allocations">
							<div class="grid">
								<Rock:Grid ID="gPtoBracketTypes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="PTO Type" ShowConfirmDeleteDialog="false">
									<Columns>
										<Rock:RockBoundField DataField="PtoType.Name" HeaderText="PTO Type" />
										<Rock:RockBoundField DataField="DefaultHours" HeaderText="Default Hours Allocated" />
                                        <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                                        <Rock:EditField OnClick="gPtoBracketTypes_Edit" />
										<Rock:DeleteField OnClick="gPtoBracketTypes_Delete" />
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

        <Rock:ModalDialog ID="dlgPtoBracketTypeDetails" runat="server" ValidationGroup="PtoBracketType" SaveButtonText="Add" OnSaveClick="dlgPtoBracketTypeDetails_SaveClick" Title="PTO Allocation Configuration">
            <Content>
                <asp:ValidationSummary ID="valPtoBracketType" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupConfig" />
                <asp:HiddenField ID="hfPtoBracketTypeId" runat="server" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="PTO Type" Required="true" ValidationGroup="PtoBracketType" EnhanceForLongLists="false" />
                    </div>
                    <div class="col-md-4">
                        <Rock:NumberBox ID="tbDefaultHours" runat="server" Label="Default Hours" Help="The number of hours the person will accru on an annual basis for this PTO Type." Required="true" ValidationGroup="PtoBracketType" NumberType="Integer" MinimumValue="1" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBox ID="cbBracketTypeIsActive" runat="server" Label="Active" ValidationGroup="PtoBracketType" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
