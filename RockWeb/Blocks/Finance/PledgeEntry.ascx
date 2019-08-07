<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
        <Rock:NotificationBox ID="nbInvalid" runat="server" NotificationBoxType="Danger" Visible="false" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlAddPledge" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"></h1>
            </div>
            <div class="panel-body">
                <fieldset>

                    <Rock:RockLiteral ID="lName" runat="server" Label="Name" />
                    <Rock:RockDropDownList ID="ddlGroup" runat="server" Visible="false" DataTextField="Name" DataValueField="GroupId" />

                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />

                    <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                    <Rock:CurrencyBox ID="tbTotalAmount" runat="server" Label="Total Pledge Amount" MinimumValue="0" Required="true" Help="The total amount that you are pledging. If you intend to give $100 monthly for one year, enter $1,200." />
                    <Rock:RockDropDownList ID="ddlFrequency" runat="server" Label="Gift Frequency" Help="How often you expect to be making gifts towards the total amount." />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" AccessKey="s" runat="server" ToolTip="Alt+s" Text="Save" OnClick="btnSave_Click" CssClass="btn btn-primary" />
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
