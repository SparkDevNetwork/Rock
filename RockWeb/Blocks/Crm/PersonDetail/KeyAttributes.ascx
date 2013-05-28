<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KeyAttributes.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.KeyAttributes" %>

<section class="widget bookmarkattributes attributeholder">
    <header class="clearfix">
        <h4 class="pull-left"><i class="icon-bookmark"></i> Bookmarked Attributes</h4> 
        <div class="actions pull-right">
            <a class="edit" href=""><i class="icon-pencil"></i></a>
            <a class="edit" href=""><i class="icon-cog"></i></a>
        </div></header>
    <div class="widget-content">
        <ul>
            <asp:Repeater ID="rAttributes" runat="server">
                <ItemTemplate>
                    <li><strong><%# Eval("Name") %></strong> <%# Eval("Value") %></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
                            
</section>
