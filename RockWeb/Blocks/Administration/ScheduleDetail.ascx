<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ScheduleDetail" %>

<asp:UpdatePanel ID="upScheduleDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfScheduleId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div id="pnlEditDetails" runat="server">

                <div class="banner">
                    <h1>
                        <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>


                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbScheduleName" runat="server" SourceTypeName="Rock.Model.Schedule, Rock" PropertyName="Name" />
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

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <div id="pnlViewDetails" runat="server">

                <div class="banner">
                    <h1>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                </div>

                <div class="row">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                </div>
                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" OnClick="btnEdit_Click" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-action btn-sm" OnClick="btnDelete_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
