<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeDetail" %>

<asp:UpdatePanel ID="upPledgeDetails" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="False">
            <asp:HiddenField ID="hfPledgeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> <asp:Literal ID="lActionTitle" runat="server"/></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" IncludeBusinesses="true"/>
                            <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="True"/>
            
                            <Rock:CurrencyBox ID="tbAmount" runat="server" Label="Total Amount" Required="True" />
                        </div>

                        <div class="col-md-6">
                            <Rock:DateRangePicker ID="dpDateRange" runat="server" Label="Date Range" />
                            <Rock:DataDropDownList ID="ddlFrequencyType" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="PledgeFrequencyValue" Label="Payment Schedule" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>