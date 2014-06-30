<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutBlockList.ascx.cs" Inherits="RockWeb.Blocks.Cms.LayoutBlockList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField runat="server" ID="hfLayoutId" />
            <div id="pnlBlocks" runat="server">
                <h4>Blocks</h4>
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                <div class="grid">
                    <Rock:Grid ID="gLayoutBlocks" runat="server" DisplayType="Full">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:TemplateField HeaderText="Type" >
                                <ItemTemplate>
                                    <%# Eval("BlockType.Name") %><br />
                                    <small><%# Eval("BlockType.Path") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Zone" HeaderText="Zone" SortExpression="Zone" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%# CreateConfigIcon( Eval("Id").ToString() ) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="DeleteBlock_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
