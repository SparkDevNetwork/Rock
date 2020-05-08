<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SavedAccountList.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.SavedAccountList" %>
<style>
	td {
	font-size:12px;
	}
	#bid_972 {
	position: inherit !important;
	}
</style>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> Payment Accounts</h1>
            </div>
            <div class="panel-body">
				<style>
					thead {
						display:none
					}
				</style>
				<p>Below are payment methods that you have saved in the past. Click the red 'x' to delete an account. You may add a new payment method the next time you create a new gift.</p> <hr />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSavedAccounts"  cssclass=""  runat="server" AutoGenerateColumns="False" AllowSorting="false" AllowPaging="false" RowItemText="Payment Account" >
                        <Columns>
						<Rock:DeleteField OnClick="gSavedAccounts_Delete" />
                            <Rock:RockBoundField DataField="AccountName" HeaderText="Name"  />
   
                            
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>