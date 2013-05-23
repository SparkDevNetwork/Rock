<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Relationships.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Relationships" %>
<section class="widget relationships">
    <header class="clearfix">
        <h4 class="pull-left"><asp:PlaceHolder ID="phGroupTypeIcon" runat="server"></asp:PlaceHolder> <asp:Literal ID="lGroupName" runat="server"></asp:Literal></h4>
        <asp:PlaceHolder ID="phEditActions" runat="server">
            <div class="actions pull-right">
                <a class="edit" href=""><i class="icon-pencil"></i></a>
                <a class="edit" href=""><i class="icon-plus"></i></a>
            </div>
        </asp:PlaceHolder>
    </header>
    <div class="widget-content">
        <ul class="personlist">
            <asp:Repeater ID="rGroupMembers" runat="server">
                <ItemTemplate>
                    <li>
                        <Rock:PersonLink runat="server" 
                            PersonId='<%# Eval("PersonId") %>' 
                            PersonName='<%# Eval("Person.FullName") %>' 
                            Role='<%# ShowRole ? Eval("GroupRole.Name") : "" %>' 
                            PhotoId='<%# Eval("Person.PhotoId") %>'
                        />
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>

</section>
