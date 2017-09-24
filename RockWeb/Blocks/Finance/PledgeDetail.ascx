<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeDetail" %>

<asp:UpdatePanel ID="upPledgeDetails" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="False">
            <asp:HiddenField ID="hfPledgeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> <asp:Literal ID="lActionTitle" runat="server"/></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbInvalid" runat="server" NotificationBoxType="Danger" Visible="false" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" IncludeBusinesses="true" OnSelectPerson="ppPerson_SelectPerson"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:DateRangePicker ID="dpDateRange" runat="server" Label="Date Range" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlGroup" runat="server" Visible="false" DataTextField="Name" DataValueField="GroupId" />
                            <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="True"/>
                            <Rock:CurrencyBox ID="tbAmount" runat="server" Label="Total Amount" MinimumValue="0" Required="True" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlFrequencyType" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="PledgeFrequencyValue" Label="Payment Schedule" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="attributes">
                                <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>