<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KeyAttributes.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.KeyAttributes" %>

<section class="panel panel-default bookmarkattributes attributeholder">
    <div class="panel-heading clearfix">
        <h3 class="panel-title pull-left"><i class="icon-bookmark"></i> Bookmarked Attributes</h3> 
        <div class="actions pull-right">
            <a class="edit" href=""><i class="icon-pencil"></i></a>
            <a class="edit" href=""><i class="icon-cog"></i></a>
        </div></div>
    <div class="panel-body">
        <ul>
            <asp:Repeater ID="rAttributes" runat="server">
                <ItemTemplate>
                    <li><strong><%# Eval("Name") %></strong> <%# Eval("Value") %></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
                            
</section>
