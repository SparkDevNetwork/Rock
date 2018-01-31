<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingTypeContextPicker.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingTypeContextPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <ul class="nav navbar-nav contextsetter givingtype-context-setter">
            <li class="dropdown">

                <div class="btn-group">
                    <asp:Repeater runat="server" ID="rptGivingTypes" OnItemCommand="rptGivingTypes_ItemCommand">
                        <ItemTemplate>
                    
                                <asp:LinkButton ID="btnPersonOrBusiness" CssClass='<%# Eval("ButtonClass") %>' runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                    
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </li>
        </ul>
    </ContentTemplate>
</asp:UpdatePanel>