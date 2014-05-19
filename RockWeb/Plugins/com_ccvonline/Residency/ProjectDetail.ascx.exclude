<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfProjectId" runat="server" />
            <asp:HiddenField ID="hfCompetencyId" runat="server" />

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Project, com.ccvonline.Residency" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Project, com.ccvonline.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" CssClass="input-xxlarge" />
                    <Rock:LabeledText ID="lblPeriod" runat="server" LabelText="Period" />
                    <Rock:LabeledText ID="lblTrack" runat="server" LabelText="Track" />
                    <Rock:LabeledText ID="lblCompetency" runat="server" LabelText="Competency" />
                    <Rock:DataTextBox ID="tbMinAssessmentCountDefault" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Project, com.ccvonline.Residency" PropertyName="MinAssessmentCountDefault"
                        LabelText="Default # of Assessments" Help="Set this to specify the default minimum number of assessments of this project that a person must complete." CssClass="input-mini" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Project - Points of Assessment
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>

            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
