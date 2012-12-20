<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupTreeView" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
    <ContentTemplate>
        <div class="treeview-back">
            <h3><asp:Literal ID="ltlTreeViewTitle" runat="server" /></h3>
            <div id="treeviewGroups" class="groupTreeview" >
                <asp:PlaceHolder runat="server" ID="phGroupTree" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
