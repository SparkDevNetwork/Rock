<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduleDetail" %>

<asp:UpdatePanel ID="upScheduleDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfScheduleId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div id="pnlEditDetails" runat="server" class="well">
                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <div class="row-fluid">
                        <div class="span6">

                            <Rock:DataTextBox ID="tbScheduleName" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbScheduleDescription" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                            <Rock:ScheduleBuilder ID="sbSchedule" runat="server" LabelText="Edit Schedule" OnSaveSchedule="sbSchedule_SaveSchedule" />

                        </div>
                        <div class="span6">

                            <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.Schedule" LabelText="Category" Required="true" />
                            <div class="control-group">
                                <div class="control-label">Enable Check-in</div>
                                <div class="controls">
                                    <Rock:NumberBox ID="nbStartOffset" runat="server" NumberType="Integer" CssClass="input-mini" />
                                    Minutes Before Start
                                </div>
                            </div>

                            <div class="control-group">
                                <div class="control-label">Until</div>
                                <div class="controls">
                                    <Rock:NumberBox ID="nbEndOffset" runat="server" NumberType="Integer" CssClass="input-mini" />
                                    Minutes After Start
                                </div>
                            </div>
                        </div>

                    </div>

                    <Rock:HelpBlock ID="hbSchedulePreview" runat="server" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <div id="pnlViewDetails" runat="server">

                <fieldset>
                    <legend>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </legend>
                    <div class="well">
                        <div class="row-fluid">
                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                        </div>
                        <div class="row-fluid">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        </div>
                    </div>
                </fieldset>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
