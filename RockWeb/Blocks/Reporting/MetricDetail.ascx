<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfMetricId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Title" />
                        <Rock:DataTextBox ID="tbSubtitle" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Subtitle" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="IconCssClass" />
                        <Rock:CategoryPicker ID="cpMetricCategories" runat="server" AllowMultiSelect="true" Label="Categories" />
                        <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" AutoPostBack="true" OnSelectedIndexChanged="ddlSourceType_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbXAxisLabel" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="XAxisLabel" />
                        <Rock:DataTextBox ID="tbYAxisLabel" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="YAxisLabel" />
                        <Rock:RockCheckBox ID="cbIsCumulative" runat="server" Label="Cumulative" Help="Helps to calculate year to date metrics."/>
                        <Rock:PersonPicker ID="ppStewardPerson" runat="server" Label="Steward" Help="Person responsible for overseeing the metric and meeting the goals established." />
                        <Rock:PersonPicker ID="ppAdminPerson" runat="server" Label="Administrator" Help="Person responsible for entering the metric values."/>
                    </div>
                </div>

                <div class="row">
                    <asp:Panel ID="pnlSourceTypeSql" runat="server">
                        <Rock:CodeEditor ID="ceSourceSql" runat="server" Label="Source SQL" EditorMode="Sql" />
                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" ShowDuration="false" ShowScheduleFriendlyTextAsToolTip="true" />
                        <Rock:RockLiteral ID="ltLastRunDateTime" runat="server" Label="Last Run" />
                    </asp:Panel>
                    <Rock:RockDropDownList ID="ddlDataView" runat="server" Label="Source DataView" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-action pull-right" />
                </div>

            </fieldset>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
