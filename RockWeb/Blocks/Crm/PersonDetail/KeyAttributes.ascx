<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KeyAttributes.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.KeyAttributes" %>

<section class="panel panel-persondetails bookmarkattributes attributeholder">
    <div class="panel-heading rollover-container clearfix">
        <h3 class="panel-title pull-left"><i class="fa fa-bookmark"></i> Bookmarked Attributes</h3> 
        <div class="actions rollover-item pull-right">
            <a class="edit" href=""><i class="fa fa-pencil"></i></a>
            <a class="edit" href=""><i class="fa fa-cog"></i></a>
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
