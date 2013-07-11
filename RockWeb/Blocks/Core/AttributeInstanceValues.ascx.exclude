<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeInstanceValues.ascx.cs" Inherits="RockWeb.Blocks.Core.AttributeInstanceValues" %>
<div class="attribute-instance-values">

    <h4 ><asp:Literal ID="lAttributeName" runat="server"></asp:Literal></h4>
    <ul>
    <asp:ListView id="lvAttributeValues" runat="server" ItemPlaceholderID="valuePlaceHolder">
        <LayoutTemplate>
            <li id="valuePlaceHolder" runat="server"></li>
        </LayoutTemplate>
        <ItemTemplate>
            <li>
                <asp:PlaceHolder ID="phDisplayValue" runat="server"/>
                <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" CommandName="Edit" >Edit</asp:LinkButton>
                <asp:LinkButton ID="lbDelete" runat="server" CssClass="delete" CommandName="Delete" >Delete</asp:LinkButton>
            </li>
        </ItemTemplate>
        <EditItemTemplate>
            <li>
                <asp:PlaceHolder ID="phEditValue" runat="server"/>
                <asp:LinkButton ID="lbSave" runat="server" CssClass="save" CommandName="Update">Save</asp:LinkButton>
                <asp:LinkButton ID="lbCancel" runat="server" CssClass="cancel" CommandName="Cancel">Cancel</asp:LinkButton>
            </li>
        </EditItemTemplate>
        <InsertItemTemplate>
            <li runat="server" class="insert-value">
                <asp:PlaceHolder ID="phInsertValue" runat="server"/>
                <asp:LinkButton ID="lbSave" runat="server" CssClass="save" CommandName="Insert">Add</asp:LinkButton>
            </li>
        </InsertItemTemplate>
    </asp:ListView>
    </ul>

</div>
