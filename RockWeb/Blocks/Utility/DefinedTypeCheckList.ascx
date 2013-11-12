<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypeCheckList.ascx.cs" Inherits="RockWeb.Blocks.Utility.DefinedTypeCheckList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" CssClass="defined-type-checklist">
            <Rock:RockCheckBoxList ID="cblList" runat="server" AutoPostBack="true" DataTextField="Name" DataValueField="Id" 
                OnSelectedIndexChanged="cblList_SelectedIndexChanged" RepeatDirection="Vertical" ></Rock:RockCheckBoxList>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
