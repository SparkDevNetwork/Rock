<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PickSomething.ascx.cs" Inherits="RockWeb.Blocks.Utility.PickSomething" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <label><asp:Literal ID="lHeaderText" runat="server" />
        </label>

        <Rock:RockDropDownList ID="ddlPickSomething" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPickSomething_SelectedIndexChanged">
            <asp:ListItem Text="Ketchup" />
            <asp:ListItem Text="Pickle" />
            <asp:ListItem Text="Cheese" />
            <asp:ListItem Text="Lettuce" />
        </Rock:RockDropDownList>
    </ContentTemplate>
</asp:UpdatePanel>
