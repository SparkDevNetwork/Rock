<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialAccountSearch.ascx.cs" Inherits="RockWeb.Blocks.Finance.FinancialAccountSearch" %>

<div class="grid">
    <Rock:Grid ID="gAccounts" runat="server" EmptyDataText="No Accounts Found" OnRowDataBound="gAccounts_RowDataBound">
        <Columns>
            <Rock:RockTemplateField HeaderText="Account Name">
                <ItemTemplate>
                    <div class="container-fluid">
                        <div class="col-xs-12">
                            <div class="row">
                                <asp:LinkButton ID="lnkAccount" runat="server" OnCommand="lnkAccount_Command" CommandName="AccountClick" CommandArgument='<%#Bind("Id") %>'>
                                    <asp:Label runat="server" ID="lblAccountName" Text='<%#Bind("Name") %>'></asp:Label>
                                </asp:LinkButton>
                            </div>
                            <asp:Panel ID="pnlPublicName" runat="server" class="row">
                                <h6 class="text-muted text-normal">
                                    <asp:Label runat="server" ID="lblPublicName" Text='<%#Bind("PublicName") %>'></asp:Label></h6>
                            </asp:Panel>
                            <div class="row">
                                <h6 class="text-muted text-normal">
                                    <asp:Label runat="server" ID="lblAccountPath" Text='<%#Bind("Path") %>'></asp:Label></h6>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </Rock:RockTemplateField>
            <Rock:RockBoundField
                HeaderText="GL Code"
                DataField="GlCode"
                SortExpression="GlCode" />
            <Rock:RockTemplateField ID="rtfAccountType" HeaderText="Account Type">
                <ItemTemplate>
                    <div>
                        <asp:Label runat="server" ID="lblAccountType" Text='<%#Bind("AccountType") %>'></asp:Label>
                    </div>
                </ItemTemplate>
            </Rock:RockTemplateField>
            <Rock:RockTemplateField ID="rtfAccountDescription" HeaderText="Description">
                <ItemTemplate>
                    <div>
                        <asp:Label runat="server" ID="lblDescription" Text='<%#Bind("Description") %>'></asp:Label>
                    </div>
                </ItemTemplate>
            </Rock:RockTemplateField>
            <Rock:RockBoundField
                HeaderText="Campus"
                DataField="Campus"
                SortExpression="Campus" />
        </Columns>
    </Rock:Grid>
</div>
