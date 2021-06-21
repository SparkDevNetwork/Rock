<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.ScheduleDetail" %>

<asp:UpdatePanel ID="upScheduleDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfScheduleId" runat="server" />

            <div id="pnlEditDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-calendar"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbScheduleName" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>

                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbScheduleDescription" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>

                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbStartOffset" Label="Enable Check-in" AppendText="mins before start" runat="server" NumberType="Integer" CssClass="input-width-lg" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.Schedule" Label="Category" Required="true" />
                            <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" OnSaveSchedule="sbSchedule_SaveSchedule" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbEndOffset" Label="Close Check-in" AppendText="mins after start" runat="server" NumberType="Integer" CssClass="input-width-lg" />
                            <div class="attributes">
                                <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                    <asp:Literal ID="lPreview" runat="server" Text="Preview" />
                    <Rock:HelpBlock ID="hbSchedulePreview" runat="server" />


                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </div>

            <div id="pnlViewDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-calendar"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    </div>
                </div>

                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbExclusions" runat="server" NotificationBoxType="Warning" Heading="Exclusions" Visible="false" />

                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:DynamicPlaceHolder ID="phDisplayAttributes" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />

                            <asp:HiddenField ID="hfHasAttendanceHistory" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClientClick="confirmScheduleDelete(event);" OnClick="btnDelete_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        </div>
                    </fieldset>

                </div>

            </div>

            <script type="text/javascript">
                function confirmScheduleDelete(e)
                {
                    e.preventDefault();

                    var confirmMsg = 'Are you sure you want to delete this schedule?';
                    if ($('#<%=hfHasAttendanceHistory.ClientID%>').val() == "1") {
                        confirmMsg = 'This schedule has attendance history. If you delete this, the attendance history will no longer be associated with the schedule. ' + confirmMsg;
                    }

                    Rock.dialogs.confirm(confirmMsg, function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                }
            </script>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
