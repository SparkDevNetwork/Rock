<%@ Control Language="C#" AutoEventWireup="false" CodeFile="HtmlContent.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContent" %>
<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        Rock.controls.htmlContentEditor.initialize({
            blockId: <%= CurrentBlock.Id %>,
            behaviorId: '<%= mpeContent.BehaviorID %>',
            hasBeenModified: <%= HtmlContentModified.ToString().ToLower() %>,
            versionId: '<%= hfVersion.ClientID %>',
            startDateId: '<%= tbStartDate.ClientID %>',
            expireDateId: '<%= tbExpireDate.ClientID %>',
            ckEditorId: '<%= edtHtmlContent.ClientID %>',
            approvalId: '<%= cbApprove.ClientID %>'
        });
    });
</script>
<asp:UpdatePanel runat="server" class="html-content-block">
<ContentTemplate>

    <asp:Literal ID="lPreText" runat="server"></asp:Literal><asp:Literal ID="lHtmlContent" runat="server"></asp:Literal><asp:Literal ID="lPostText" runat="server"></asp:Literal>

    <asp:HiddenField ID="hfAction" runat="server" />
    <asp:Button ID="btnDefault" runat="server" Text="Show" style="display:none"/>
    <asp:Panel ID="pnlContentEditor" runat="server" CssClass="rock-modal" style="display:none">

        <div class="modal-header">
            <a id="aClose" runat="server" href="#" class="close">&times;</a>
            <h3>HTML Content</h3>
            <asp:PlaceHolder ID="phCurrentVersion" runat="server"><a id="html-content-version-<%=CurrentBlock.Id %>" 
                class="html-content-version-label">Version <asp:Literal ID="lVersion" runat="server"></asp:Literal></a></asp:PlaceHolder>
        </div>

        <div id="html-content-versions-<%=CurrentBlock.Id %>" style="display:none">
            <div class="modal-body">
                <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" >
                    <Columns>
                        <asp:TemplateField SortExpression="Version" HeaderText="Version">
                            <ItemTemplate>
                                <a data-html-id='<%# Eval("Id") %>' class="html-content-show-version-<%=CurrentBlock.Id %>" href="#">Version <%# Eval("Version") %></a>
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
            </div>
            <div class="modal-footer">
                <button id="html-content-versions-cancel-<%=CurrentBlock.Id %>" class="btn">Cancel</button>
            </div>
        </div>

        <div id="html-content-edit-<%=CurrentBlock.Id %>">
            <asp:panel ID="pnlVersioningHeader" runat="server" class="html-content-edit-header">
                <asp:HiddenField ID="hfVersion" runat="server" />
                Start: <asp:TextBox ID="tbStartDate" runat="server" CssClass="date-picker"></asp:TextBox>
                Expire: <asp:TextBox ID="tbExpireDate" runat="server" CssClass="date-picker"></asp:TextBox>
                <div class="html-content-approve inline-form"><asp:CheckBox ID="cbApprove" runat="server" TextAlign="Right" Text="Approve" /></div>
            </asp:panel>
            <div class="modal-body">
                <Rock:CKEditorControl ID="edtHtmlContent" runat="server" Visible="false"/>
            </div>
            <div class="modal-footer">
                <span class="inline-form">
                    <asp:CheckBox ID="cbOverwriteVersion" runat="server" TextAlign="Right" Text="don't save a new version" />
                </span>
                <asp:LinkButton ID="lbCancel" runat="server" cssclass="btn" Text="Cancel" />
                <asp:LinkButton ID="lbOk" runat="server" cssclass="btn btn-primary" Text="Save" />
            </div>
        </div>

        <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click" Text="Save" style="display: none;" CssClass="save-button" />

    </asp:Panel>
    
    <asp:ModalPopupExtender ID="mpeContent" runat="server" BackgroundCssClass="modal-backdrop"  
        PopupControlID="pnlContentEditor" CancelControlID="lbCancel" OkControlID="lbOk" TargetControlID="btnDefault"></asp:ModalPopupExtender>

</ContentTemplate>
</asp:UpdatePanel>



