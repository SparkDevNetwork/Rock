<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarsTransactions.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Stars.StarsTransactions" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlSearch" CssClass="panel panel-block" runat="server" >
            
                        <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Star Transactions</h1>
            </div>
           
            <div class="panel-body">
                
                <div class="col-md-12">
                             
                    <Rock:Grid ID="gStars" runat="server" AllowSorting="true" AllowPaging="True" EmptyDataText="No Data Found" RowClickEnabled="True" OnRowSelected="gStars_OnRowSelected" >
                        <Columns>
                            <asp:BoundField DataField="TransactionDateTime" HeaderText="Date" />
                            <asp:BoundField DataField="Value" HeaderText="Stars" />
                            <asp:BoundField DataField="Note" HeaderText="Notes" />
                        </Columns>
                    </Rock:Grid>
                    
                      

                  </div>  </div>
            </asp:Panel>
        
        
        


    </ContentTemplate>
</asp:UpdatePanel>
