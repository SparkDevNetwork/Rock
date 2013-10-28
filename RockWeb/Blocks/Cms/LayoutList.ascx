<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutList.ascx.cs" Inherits="RockWeb.Blocks.Cms.LayoutList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hfSiteId" />
        <div id="pnlLayouts" runat="server">
            <h4>Layouts</h4>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gLayouts" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gLayouts_Edit">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:TemplateField HeaderText="Layout File" SortExpression="FileName">
                        <ItemTemplate><%# GetFilePath( Eval( "FileName" ).ToString() ) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:SecurityField TitleField="Name" />
                    <Rock:DeleteField OnClick="DeleteLayout_Click" />
                </Columns>
            </Rock:Grid>
            <Rock:NotificationBox runat="server" NotificationBoxType="Info" Title="Note" 
                Text="Rock will ensure that at least one layout entry exists for each layout file in the site's theme folder.  If all the the layouts for a layout file are deleted, Rock will automatically and immediately recreate a new layout entry." />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
