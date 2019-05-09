<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingDonationList.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingDonationList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-money"></i> Fundraising Donations</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gDonations" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Donations Found" PersonIdField="DonorId" RowItemText="Donations" DataKeyNames="DonorId" ExportSource="ColumnOutput" OnRowDataBound="gDonations_RowDataBound" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockLiteralField HeaderText="Donor" SortExpression="Donor.LastName, Donor.NickName" />
                            <Rock:RockBoundField DataField="Address" HeaderText="Donor Address" HtmlEncode="false" ExcelExportBehavior="IncludeIfVisible" />
                            <Rock:RockBoundField DataField="Donor.Email" HeaderText="Donor Email" SortExpression="Donor.Email" ExcelExportBehavior="IncludeIfVisible" />
                            <Rock:RockLiteralField HeaderText="Participant" SortExpression="Participant.Person.LastName, Participant.Person.NickName" ExcelExportBehavior="IncludeIfVisible" />
                            <Rock:DateField DataField="Date" HeaderText="Date" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="Date" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:CurrencyField DataField="Amount" HeaderText="Amount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="Amount" ExcelExportBehavior="IncludeIfVisible" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>