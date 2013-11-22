<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypeCheckList.ascx.cs" Inherits="RockWeb.Blocks.Utility.DefinedTypeCheckList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" CssClass="defined-type-checklist">
            
            <asp:Literal ID="lPreText" runat="server"></asp:Literal>

            <asp:Literal ID="lTitle" runat="server"></asp:Literal>
            <asp:Literal ID="lDescription" runat="server"></asp:Literal>
            <asp:Repeater ID="rptrValues" runat="server">
                <ItemTemplate>
                    <asp:Panel ID="pnlValue" runat="server">
                        <article class="panel panel-widget checklist-item">
                            <asp:HiddenField ID="hfValue" runat="server" Value='<%# Eval("Id") %>' />
                            <header class="panel-heading clearfix">
                                <Rock:RockCheckBox ID="cbValue" runat="server" AutoPostBack="true" />
                                <div class='btn btn-action btn-xs pull-right checklist-desc-toggle'><i class='fa fa-chevron-down'></i></div>
                            </header>
                            <div class="checklist-description panel-body" style="display: none;">
                                <%# Eval("Description") %>
                            </div>
                        </article>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Literal ID="lPostText" runat="server"></asp:Literal>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
