<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlAddPledge" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h3 class="panel-title"></h3>
            </div>
            <div class="panel-body">
                <fieldset>
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    <Rock:RockTextBox ID="tbEmail" runat="server" Label="Email" TextMode="Email" Required="true" />

                    <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                    <Rock:ButtonDropDownList ID="bddlFrequency" runat="server" CssClass="btn btn-primary" Label="Frequency" />
                    <Rock:CurrencyBox ID="tbTotalAmount" runat="server" Label="Total Amount" Required="true" />


                    <div class="actions">
                        <asp:LinkButton ID="btnSave" AccessKey="s" runat="server" Text="Save" OnClick="btnSave_Click" CssClass="btn btn-primary" />

                    </div>

                    <Rock:RockLiteral ID="lNote" runat="server" />
                </fieldset>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlConfirm" runat="server" CssClass="panel panel-block" Visible="false">

            <div class="panel-heading">
                <h3 class="panel-title"></h3>
            </div>
            <div class="panel-body">
                <fieldset>
                    <Rock:NotificationBox ID="nbDuplicatePledgeWarning" runat="server" NotificationBoxType="Warning" />
                    <div class="actions">
                        <asp:LinkButton ID="btnConfirm" runat="server" Text="Add Pledge" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click"  CssClass="btn" />
                    </div>
                </fieldset>
            </div>

        </asp:Panel>

        <asp:Literal ID="lReceipt" runat="server" Text="" />

    </ContentTemplate>
</asp:UpdatePanel>
