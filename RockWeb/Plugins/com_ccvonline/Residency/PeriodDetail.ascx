<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PeriodDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.PeriodDetail" %>

<asp:UpdatePanel ID="upPeriodDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfPeriodId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Period, com.ccvonline.Residency" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.ccvonline.Residency.Model.Period, com.ccvonline.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        <Rock:DateRangePicker ID="dpStartEndDate" runat="server" Label="Date Range "/>
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
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </div>

            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
