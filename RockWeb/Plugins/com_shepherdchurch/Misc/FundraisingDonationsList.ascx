<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingDonationsList.ascx.cs" Inherits="Plugins.com_shepherdchurch.Misc.FundraisingDonationsList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-money"></i> Fundraising Donations</h3>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gDonations" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Donations Found" PersonIdField="DonorId" RowItemText="Donations" DataKeyNames="DonorId" ExportSource="ColumnOutput" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="DonorName" HeaderText="Donor" SortExpression="Donor.LastName, Donor.NickName" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="Address" HeaderText="Donor Address" HtmlEncode="false" ExcelExportBehavior="IncludeIfVisible" />
                            <Rock:RockBoundField DataField="Email" HeaderText="Donor Email" SortExpression="Email" ExcelExportBehavior="IncludeIfVisible" />
                            <Rock:RockBoundField DataField="ParticipantName" HeaderText="Participant" SortExpression="Participant.Person.LastName, Participant.Person.NickName" HtmlEncode="false" />
                            <Rock:DateField DataField="Date" HeaderText="Date" HeaderStyle-HorizontalAlign="Right" SortExpression="Date" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:CurrencyField DataField="Amount" HeaderText="Amount" HeaderStyle-HorizontalAlign="Right" SortExpression="Amount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>