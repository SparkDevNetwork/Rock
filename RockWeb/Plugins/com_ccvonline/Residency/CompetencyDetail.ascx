<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/bootstrap.css" visible="false" />
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/theme.css" visible="false" />

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                            <Rock:DataTextBox ID="tbGoals" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Goals" TextMode="MultiLine" Rows="12" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lblPeriod" runat="server" Label="Period" />
                            <Rock:RockLiteral ID="lblTrack" runat="server" Label="Track" />

                            <Rock:PersonPicker ID="ppTeacherOfRecord" runat="server" Label="Teacher of Record" />
                            <Rock:PersonPicker ID="ppFacilitator" runat="server" Label="Facilitator" />
                            <Rock:DataTextBox ID="tbCreditHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="CreditHours" />
                            <Rock:DataTextBox ID="tbSupervisionHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="SupervisionHours" />
                            <Rock:DataTextBox ID="tbImplementationHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="ImplementationHours" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

            <asp:HiddenField ID="hfCompetencyId" runat="server" />
            <asp:HiddenField ID="hfTrackId" runat="server" />

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
