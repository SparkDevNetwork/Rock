<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlContent.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContent" %>

<asp:UpdatePanel runat="server" class="html-content-block">
<ContentTemplate>

    <asp:Literal ID="lPreText" runat="server"></asp:Literal><asp:Literal ID="lHtmlContent" runat="server"></asp:Literal><asp:Literal ID="lPostText" runat="server"></asp:Literal>

    <asp:PlaceHolder ID="phEditContent" runat="server" Visible="false">

        <div id="html-content-editor-<%=BlockInstance.Id %>" class="modal hide fade">
            <div class="modal-header">
                <a href="#" class="close">&times;</a>
                <h3>HTML Content</h3>
            </div>
            <div id="html-content-versions-<%=BlockInstance.Id %>" style="display:none">
                <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" >
                    <Columns>
                        <asp:TemplateField SortExpression="Version" HeaderText="Version">
                            <ItemTemplate>
                                <a html-id='<%# Eval("Id") %>' class="html-content-show-version-<%=BlockInstance.Id %>" href="#"><%# Eval("Version") %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ModifiedDateTime" HeaderText="Modified" SortExpression="ModifiedDateTime" />
                        <asp:BoundField DataField="ModifiedByPerson" HeaderText="By" SortExpression="ModifiedByPerson" />
                        <Rock:BoolField DataField="Approved" HeaderText="Approved" SortExpression="Approved" />
                        <asp:BoundField DataField="ApprovedByPerson" HeaderText="By" SortExpression="ApprovedByPerson" />
                        <asp:BoundField DataField="StartDateTime" DataFormatString="MM/dd/yy" HeaderText="Start" SortExpression="StartDateTime" />
                        <asp:BoundField DataField="ExpireDateTime" DataFormatString="MM/dd/yy" HeaderText="Expire" SortExpression="ExpireDateTime" />
                    </Columns>
                </Rock:Grid>
                <div class="modal-footer">
                    <button id="html-content-versions-cancel-<%=BlockInstance.Id %>" class="btn secondary">Cancel</button>
                </div>
            </div>
            <div id="html-content-edit-<%=BlockInstance.Id %>">
                <asp:Panel ID="pnlVersioningHeader" runat="server" class="html-content-edit-header">
                    <asp:HiddenField ID="hfVersion" runat="server" />
                    <a id="html-content-version-<%=BlockInstance.Id %>">Version <asp:Literal ID="lVersion" runat="server"></asp:Literal></a>
                    Start: <asp:TextBox ID="tbStartDate" runat="server"></asp:TextBox>
                    Expire: <asp:TextBox ID="tbExpireDate" runat="server"></asp:TextBox>
                    <asp:CheckBox ID="cbApprove" runat="server" TextAlign="Left" Text="Approve" />
                </asp:Panel>
                <div class="modal-body">
                    <asp:TextBox ID="txtHtmlContentEditor" CssClass="html-content-editor" TextMode="MultiLine" runat="server"></asp:TextBox>
                </div>
                <div class="modal-footer">
                    <asp:CheckBox ID="cbOverwriteVersion" runat="server" TextAlign="Right" Text="don't save a new version" />
                    <input id="btnCancel" runat="server" type="button" class="btn secondary" value="Cancel" />
                    <asp:Button ID="btnSaveContent" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSaveContent_Click" />
                </div>
            </div>
        </div>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>



