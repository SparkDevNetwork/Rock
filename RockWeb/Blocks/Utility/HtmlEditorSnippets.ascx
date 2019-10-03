<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlEditorSnippets.ascx.cs" Inherits="RockWeb.Blocks.Utility.HtmlEditorSnippets" %>

<asp:Panel ID="pnlModalHeader" runat="server" Visible="false">
    <h3 class="modal-title">
        <asp:Literal ID="lTitle" runat="server"></asp:Literal>
        <span class="js-cancel-button cursor-pointer pull-right" style="opacity: .5">&times;</span>
    </h3>

</asp:Panel>
<asp:UpdatePanel runat="server" ID="upSnippets">
    <ContentTemplate>
        <style>
            .truncated-text {
                max-width: 300px;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
            }
        </style>
        <div class="snippets-wrapper clearfix">
            <Rock:Grid ID="gSnippets" runat="server" AllowSorting="true" RowItemText="snippet" TooltipField="Description" DataKeyNames="Id" >
                <Columns>
                    <Rock:ReorderField Visible="false" />
                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <Rock:RockBoundField DataField="Content" HeaderText="Content" SortExpression="Content" ItemStyle-CssClass="truncated-text" ItemStyle-Width="70%" />
                    <Rock:EditField ID="efEdit" OnClick="efEdit_Click" />
                    <Rock:LinkButtonField CssClass="btn btn-default btn-sm fa fa-clone" OnClick="lbfCopy_Click" HeaderStyle-HorizontalAlign="Center" />
                    <Rock:DeleteField ID="dfDelete" OnClick="Delete_Click" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Panel ID="pnlModalFooterActions" CssClass="modal-footer" runat="server" Visible="false">
    <div class="row">
        <div class="actions">
            <a class="btn btn-primary js-ok-button">OK</a>
        </div>
    </div>
</asp:Panel>
