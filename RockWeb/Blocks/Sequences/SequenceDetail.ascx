<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SequenceDetail.ascx.cs" Inherits="RockWeb.Blocks.Sequences.SequenceDetail" %>

<asp:UpdatePanel ID="upSequenceDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ol"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSequenceDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lSequenceDescription" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:LinkButton ID="btnMapEditor" runat="server" Text="Map Editor" CssClass="btn btn-default" OnClick="btnMapEditor_Click" CausesValidation="false" />
                            <asp:LinkButton ID="btnExclusions" runat="server" Text="Exclusions" CssClass="btn btn-default" OnClick="btnExclusions_Click" CausesValidation="false" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                            <asp:LinkButton runat="server" ID="btnRebuild" CausesValidation="false" OnClick="btnRebuild_Click" CssClass="btn btn-danger btn-sm margin-l-md" Text="Rebuild" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6 col-sm-3">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                        <div class="col-sm-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:DatePicker ID="rdpStartDate" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="StartDate" Label="Start Date" />
                        </div>
                        <div class="col-sm-3 col-xs-6">
                            <Rock:RockCheckBox CssClass="col-sm-6" ID="cbEnableAttendance" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="EnableAttendance" Label="Enable Attendance" Checked="true" Text="Yes" />
                        </div>
                        <div class="col-sm-3 col-xs-6">
                            <Rock:RockCheckBox CssClass="col-sm-6" ID="cbRequireEnrollment" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" PropertyName="RequiresEnrollment" Label="Require Enrollment" Checked="false" Text="Yes" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlStructureType" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" Label="Structure Type" PropertyName="StructureType" DataTextField="Text" DataValueField="Value" OnSelectedIndexChanged="ddlStructureType_SelectedIndexChanged" AutoPostBack="true" />
                            <Rock:GroupPicker ID="gpStructureGroupPicker" runat="server" Visible="false" />
                            <Rock:GroupTypePicker ID="gtpStructureGroupTypePicker" runat="server" Visible="false" />
                            <Rock:DefinedValuePicker ID="dvpStructureGroupTypePurposePicker" runat="server" Visible="false" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlFrequencyOccurrence" runat="server" SourceTypeName="Rock.Model.Sequence, Rock" Label="Frequency" PropertyName="OccurrenceFrequency" DataTextField="Text" DataValueField="Value" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
