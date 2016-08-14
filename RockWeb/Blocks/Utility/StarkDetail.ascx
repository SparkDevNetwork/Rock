<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" Visible="false">
        
           <Rock:CategoryPicker ID="catTest" runat="server" EntityTypeId="49" EntityTypeQualifierColumn="EntityTypeId" />
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
