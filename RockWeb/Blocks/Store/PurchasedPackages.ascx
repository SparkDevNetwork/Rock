<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PurchasedPackages.ascx.cs" Inherits="RockWeb.Blocks.Store.PurchasedPackages" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gift"></i> Purchased Packages</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lPurchasedProducts" runat="server" />

                <asp:Repeater ID="rptPurchasedProducts" runat="server" OnItemDataBound="rptPurchasedProducts_ItemDataBound" OnItemCommand="rptPurchasedProducts_ItemCommand">
                    <ItemTemplate>
                        <div class="purchasedpackage row">
                            <div class="col-md-4">
                                <div class="margin-b-md" style="
                                    background: url('<%# Eval( "PackageIconBinaryFile.ImageUrl" ) %>') no-repeat center;
                                    width: 100%;
                                    height: 140px;">
                                    </div>
                            </div>
                            <div class="col-md-6 margin-b-md">
                                <h1><%# Eval( "Name" ) %></h1>

                                <div class="clearfix margin-b-sm">
                                    <div class="pull-left"><strong>Purchased: </strong><br><%# string.Format("{0:M/d/yyyy}", Eval("PurchasedDate"))%></div>
                                    <div class="pull-right"><strong>Purchased by: </strong><br><%# Eval( "Purchaser" ) %></div>
                                </div>

                                <%# Eval( "Description" ) %>

                                <p class="margin-t-md">
                                    <asp:LinkButton ID="lbPackageDetails" runat="server" CssClass="btn btn-default btn-sm margin-b-sm" CommandName="PackageDetails" CommandArgument='<%#Eval("Id") %>'>Package Details</asp:LinkButton>
                                </p>
                            </div>
                            <div class="col-md-2 purchasedpackage-install">
                                <asp:LinkButton ID="lbInstall" runat="server" CssClass="btn btn-primary margin-b-md" CommandName="Install" CommandArgument='<%#Eval("Id") %>'>Install</asp:LinkButton>
                            
                                <asp:Literal ID="lVersionNotes" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
