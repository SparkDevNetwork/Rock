<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueDetail" %>

<asp:UpdatePanel ID="upMetricValueDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfMetricValueId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlMetricValueType" runat="server" Label="Type" />
                        <Rock:DataTextBox ID="tbXValue" runat="server" SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="XValue" />
                        <Rock:NumberBox ID="tbYValue" runat="server" Label="Y Value" NumberType="Double" />
                        <Rock:DataTextBox ID="tbNote" runat="server" SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="Note" TextMode="MultiLine" Rows="3" />
                        <Rock:DateTimePicker ID="dtpMetricValueDateTime" runat="server" Label="Value Date/Time" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                    </div>
                </div>

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
