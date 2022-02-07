<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakTypeDetail" %>

<asp:UpdatePanel ID="upStreakTypeDetail" runat="server">
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
                <asp:ValidationSummary ID="valStreakTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-lg-4">
                            <asp:Literal ID="lStreakTypeDescription" runat="server" />
                        </div>
                        <div class="col-lg-8 text-right">
                            <asp:LinkButton ID="btnAchievements" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnAchievements_Click" CausesValidation="false"><i class="fa fa-medal"></i> Achievements</asp:LinkButton>
                            <asp:LinkButton ID="btnMapEditor" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnMapEditor_Click" CausesValidation="false"><i class="fa fa-calendar-check"></i> Map Editor</asp:LinkButton>
                            <asp:LinkButton ID="btnExclusions" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnExclusions_Click" CausesValidation="false"><i class="fa fa-calendar-times"></i> Exclusions</asp:LinkButton>
                            <asp:LinkButton ID="btnRebuild" runat="server" CausesValidation="false" OnClick="btnRebuild_Click" CssClass="btn btn-danger btn-xs"><i class="fa fa-redo-alt"></i> Rebuild</asp:LinkButton>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security btn-square" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6 col-sm-3">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                        <div class="col-sm-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:DatePicker ID="rdpStartDate" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="StartDate" Label="Start Date"
                                Help="This defines the first day (or week) that a person could engage with this Streak Type and receive an affirmative mark towards their streak. The start date cannot be changed once the Streak Type is created except when rebuilding the entire Streak Type." />
                        </div>
                        <div class="col-sm-3 col-xs-6">
                            <Rock:RockCheckBox CssClass="col-sm-6" ID="cbEnableAttendance" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="EnableAttendance" Label="Sync Linked Activity" Checked="true" Text="Yes"
                                Help="If enabled, this streak type will create linked activity records (interaction or attendance) when the Mark Engagement API endpoints are used. Furthermore, when linked activity records are added, this streak type will mark the appropriate engagement bit." />
                        </div>
                        <div class="col-sm-3 col-xs-6">
                            <Rock:RockCheckBox CssClass="col-sm-6" ID="cbRequireEnrollment" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" PropertyName="RequiresEnrollment" Label="Require Enrollment" Checked="false" Text="Yes"
                                Help="If enabled, this Streak Type will not allow new Streaks to be tracked unless the person has been explicitly enrolled. Implicit enrollment occurs when Sync Linked Activity is enabled, and a linked activity record is created." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlStructureType" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" Label="Linked Activity" PropertyName="StructureType" DataTextField="Text" DataValueField="Value" OnSelectedIndexChanged="ddlStructureType_SelectedIndexChanged" AutoPostBack="true"
                                Help="This specifies what linked activity types this Streak Type may interface with.  Something very broad like “Attendance: Any” could help measure general engagement with the church.  Alternatively, something narrower like a Group Type Purpose could measure more specific engagement points." />
                            <Rock:GroupPicker ID="gpStructureGroupPicker" runat="server" Visible="false" />
                            <Rock:GroupTypePicker ID="gtpStructureGroupTypePicker" runat="server" Visible="false" />
                            <Rock:DefinedValuePicker ID="dvpStructureDefinedValuePicker" runat="server" Visible="false" />
                            <Rock:InteractionChannelInteractionComponentPicker ID="icicComponentPicker" runat="server" Visible="false" />
                            <Rock:InteractionChannelPicker ID="icChannelPicker" runat="server" Visible="false" />
                            <div class="row">
                                <div class="col-sm-6 col-xs-12">
                                    <Rock:AccountPicker ID="apStructureAccountPicker" runat="server" Visible="false" />
                                </div>
                                <div class="col-sm-6 col-xs-12">
                                    <Rock:RockCheckBox ID="cbIncludeChildAccounts" runat="server" Visible="false" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlFrequencyOccurrence" runat="server" SourceTypeName="Rock.Model.StreakType, Rock" Label="Frequency" PropertyName="OccurrenceFrequency" DataTextField="Text" DataValueField="Value" OnSelectedIndexChanged="ddlFrequencyOccurrence_SelectedIndexChanged" AutoPostBack="true"
                                Help="Daily measuring is great for something like Bible Reading where the ideal is daily engagement. Weekly measurements are better suited for activities such as a small group where the ideal is weekly engagement." />
                            <Rock:DayOfWeekPicker ID="dowPicker" runat="server" Label="Day of Week Start" Help="Allows this weekly streak type to calculate streaks based off a custom first day of the week setting. Leave this blank to use the system setting." />
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
