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

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbScheduleName" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Name" Required="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbScheduleDescription" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbStartOffset" Label="Enable Check-in" AppendText="Mins Before Start" runat="server" NumberType="Integer" CssClass="input-width-lg" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.Schedule" Label="Category" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbEndOffset" Label="Close Check-in" AppendText="Mins After Start&nbsp;&nbsp;" runat="server" NumberType="Integer" CssClass="input-width-lg" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" OnSaveSchedule="sbSchedule_SaveSchedule" />
                        </div>
                    </div>

                    <asp:Literal ID="lPreview" runat="server" Text="Preview" />
                    <Rock:HelpBlock ID="hbSchedulePreview" runat="server" />


                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </fieldset>
                </div>

            </div>

            <div id="pnlViewDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-calendar"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <div class="row">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>

                    <fieldset>
                        <div class="row">
                            <div class="col-md-12">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClientClick="Rock.dialogs.confirmDelete(event, 'schedule');" OnClick="btnDelete_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        </div>
                    </fieldset>

                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
