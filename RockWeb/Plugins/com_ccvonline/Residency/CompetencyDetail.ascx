<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upCompetencyDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyId" runat="server" />
            <asp:HiddenField ID="hfTrackId" runat="server" />

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Name" CssClass="input-xlarge" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" CssClass="input-xlarge" />
                            <Rock:DataTextBox ID="tbGoals" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="Goals"
                                TextMode="MultiLine" Rows="12" CssClass="input-xlarge" />
                        </div>
                        <div class="span6">
                            <Rock:RockLiteral ID="lblPeriod" runat="server" Label="Period" />
                            <Rock:RockLiteral ID="lblTrack" runat="server" Label="Track" />

                            <Rock:PersonPicker ID="ppTeacherOfRecord" runat="server" Label="Teacher of Record" />
                            <Rock:PersonPicker ID="ppFacilitator" runat="server" Label="Facilitator" />
                            <Rock:DataTextBox ID="tbCreditHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="CreditHours" CssClass="input-mini" />
                            <Rock:DataTextBox ID="tbSupervisionHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="SupervisionHours" CssClass="input-mini" />
                            <Rock:DataTextBox ID="tbImplementationHours" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Competency, com.ccvonline.Residency" PropertyName="ImplementationHours" CssClass="input-mini" />
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Competency - Projects
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>

            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
