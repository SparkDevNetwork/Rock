<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SavedAccountList.ascx.cs" Inherits="RockWeb.Blocks.Finance.SavedAccountList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> Payment Accounts</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSavedAccounts" DisplayType="Light" runat="server" AutoGenerateColumns="False" AllowSorting="false" AllowPaging="false" RowItemText="Payment Account" >
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name"/>
                            <Rock:RockBoundField DataField="AccountNumber" HeaderText="Account Number" />
                            <Rock:RockBoundField DataField="AccountType" HeaderText="Account Type" />
                            <Rock:DeleteField OnClick="gSavedAccounts_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>