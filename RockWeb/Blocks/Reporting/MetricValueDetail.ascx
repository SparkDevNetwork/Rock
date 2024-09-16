<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueDetail" %>

<asp:UpdatePanel ID="upMetricValueDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />
            <asp:HiddenField ID="hfMetricValueId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-signal"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlMetricValueType" runat="server" Label="Type" />
                            <Rock:DatePicker ID="dpMetricValueDateTime" runat="server" Label="Value Date" Required="true" />

                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="nbValueRequired" runat="server" NotificationBoxType="Warning" Visible="false" />
                        </div>
                        <asp:PlaceHolder ID="phMetricValuePartitions" runat="server" />
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="tbYValue" runat="server" Label="Value" NumberType="Double" />

                            <%-- Hide X Value for now until we implement XValue (In most cases, the Metric.Xaxis is ignored and MetricValueDateTime is used as the X-Axis --%>
                            <Rock:DataTextBox ID="tbXValue" runat="server" SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="XValue" Visible="false" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbNote" runat="server" SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="Note" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" data-shortcut-key="s" ToolTip="Alt+s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" data-shortcut-key="c" ToolTip="Alt+c" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
