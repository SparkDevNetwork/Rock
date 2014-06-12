<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectPointOfAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectPointOfAssessmentDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/bootstrap.css" visible="false" />
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/theme.css" visible="false" />

<asp:UpdatePanel ID="upProjectPointOfAssessmentDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfProjectId" runat="server" />
            <asp:HiddenField ID="hfProjectPointOfAssessmentId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="row">
                <div class="col-md-12">
                    <Rock:RockDropDownList ID="ddlPointOfAssessmentTypeValue" runat="server" Label="Point of Assessment Type" DataTextField="Name" DataValueField="Id" />
                    <Rock:RockLiteral ID="lblAssessmentOrder" runat="server" Label="Assessment #" />
                    <Rock:DataTextBox ID="tbAssessmentText" runat="server" SourceTypeName="com.ccvonline.Residency.Model.ProjectPointOfAssessment, com.ccvonline.Residency" PropertyName="AssessmentText" TextMode="MultiLine" Rows="3" />
                    <Rock:RockCheckBox ID="cbIsPassFail" runat="server" Label="Graded as Pass/Fail" />
                </div>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
