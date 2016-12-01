<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PeriodDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.PeriodDetail" %>

<asp:UpdatePanel ID="upPeriodDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="nbCloneMessage" runat="server" />
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="church.ccv.Residency.Model.Period, church.ccv.Residency" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="church.ccv.Residency.Model.Period, church.ccv.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                            <Rock:DateRangePicker ID="dpStartEndDate" runat="server" Label="Date Range "/>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <asp:Literal ID="lblDescription" runat="server" />

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

            </div>

            <asp:HiddenField ID="hfPeriodId" runat="server" />
            <asp:HiddenField ID="hfCloneFromPeriodId" runat="server" />


            

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
