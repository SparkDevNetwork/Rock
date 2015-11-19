<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupHierarchyMap.ascx.cs" Inherits="com.reallifeministries.GroupHierarchyMap" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <Rock:GroupPicker runat="server" ID="gpGroup" />
        
        <asp:Panel runat="server" ID="pnlGroups" EnableViewState="false">

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>