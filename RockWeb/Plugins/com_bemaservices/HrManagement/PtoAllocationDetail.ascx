<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoAllocationDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoAllocationDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upPtoAllocation" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeleteConfirm" runat="server" CssClass="panel panel-body" Visible="false">
            <Rock:NotificationBox ID="nbDeleteConfirm" runat="server" NotificationBoxType="Warning" Text="Deleting a Pto Allocation will permently delete the allocation and all related pto requests. Are you sure you want to delete the Pto Allocation?" />
            <asp:LinkButton ID="btnDeleteConfirm" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
            <asp:LinkButton ID="btnDeleteCancel" runat="server" Text="Cancel" CssClass="btn btn-primary" OnClick="btnDeleteCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfPtoAllocationId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlAllocationId" runat="server" LabelType="Default" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" Visible="false" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valPtoAllocationDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <dl class="margin-b-md">
                                <Rock:TermDescription ID="tdPerson" runat="server" Term="Person" />
                                <Rock:TermDescription ID="tdPtoType" runat="server" Term="Pto Type" />
                                <Rock:TermDescription ID="tdDateRange" runat="server" Term="Date" />
                            </dl>
                        </div>
                    </div>

                    <p class="description">
                        <asp:Literal ID="lPtoAllocationDescription" runat="server"></asp:Literal>
                    </p>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" />
                            <Rock:RockDropDownList ID="ddlPtoType" runat="server" Label="PTO Type" SourceTypeName="com.bemaservices.HrManagement.Model.PtoAllocation, com.bemaservices.HrManagement" PropertyName="PtoType" />
                            <Rock:DatePicker ID="dtpStartDate" runat="server" Label="Start Date" Required="true" RequiredErrorMessage="Start Date is required" />
                            <Rock:RockDropDownList ID="ddlPtoAccrualSchedule" runat="server" Label="Schedule" Visible="false" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:NumberBox ID="tbHours" runat="server" NumberType="Integer" Label="Hours" MinimumValue="1"/>
                            <Rock:DatePicker ID="dtpEndDate" runat="server" Label="End Date" />
                        </div>
                    </div>
                    
                    <Rock:DataTextBox ID="tbNote" runat="server" Label="Note" SourceTypeName="com.bemaservices.HrManagement.Model.PtoAllocation, com.bemaservices.HrManagement" TextMode="MultiLine" Rows="4" PropertyName="Note"/>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />


    </ContentTemplate>
</asp:UpdatePanel>
