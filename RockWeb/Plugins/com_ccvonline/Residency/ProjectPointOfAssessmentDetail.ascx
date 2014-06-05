<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectPointOfAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectPointOfAssessmentDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upProjectPointOfAssessmentDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            
            <asp:HiddenField ID="hfProjectId" runat="server" />
            <asp:HiddenField ID="hfProjectPointOfAssessmentId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:LabeledDropDownList ID="ddlPointOfAssessmentTypeValue" runat="server" LabelText="Point of Assessment Type" DataTextField="Name" DataValueField="Id" />
                <Rock:LabeledText ID="lblAssessmentOrder" runat="server" LabelText="Assessment #" />
                <Rock:DataTextBox ID="tbAssessmentText" runat="server" SourceTypeName="com.ccvonline.Residency.Model.ProjectPointOfAssessment, com.ccvonline.Residency" PropertyName="AssessmentText" TextMode="MultiLine" Rows="3" CssClass="input-xxlarge"/>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
