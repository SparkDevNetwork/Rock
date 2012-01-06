<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlContent.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContent" %>
<script type="text/javascript"> 

    $(document).ready(function () {

        var ckoptionsBasic = {
            langCode: 'en',
            skin: 'kama',
            width: 820,
            resize_enabled: false,
            startupFocus: true,
            toolbar: [
                        ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord'],
                        ['Bold', 'Italic', 'Underline'],
                        ['Image', 'Link', 'Unlink', 'Anchor', 'Table'],

                        ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                        ['NumberedList', 'BulletedList']
            ]
        };

        var ckoptionsAdv = {
            langCode: 'en',
            skin: 'kama',
            width: '100%',
            resize_enabled: false,
            startupFocus: true,
            toolbar: [
                        ['Source'], ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Find', 'Replace'],
                        ['Bold', 'Italic', 'Underline', 'Strike', '-', 'Subscript', 'Superscript'],
                        ['Image', 'Link', 'Unlink', 'Anchor', 'Table', 'HorizontalRule'],

                        ['Undo', 'Redo', '-', 'SelectAll', 'RemoveFormat'],
                        ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                        ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', 'Blockquote', 'CreateDiv'],
                        ['Format'],
                        ['TextColor', 'BGColor'],
                        ['Font', 'FontSize'], ['Maximize', 'ShowBlocks']
            ]
        };

        $('#html-content-editor-<%=BlockInstance.Id %>').modal({
            backdrop: true,
            keyboard: true
        });
        
        $('#html-content-editor-<%=BlockInstance.Id %>').bind('shown', function() {
            $(this).appendTo($('form'));
            $('#html-content-editor-<%=BlockInstance.Id %> textarea.html-content-editor').ckeditor(ckoptionsAdv).end();
        });

        $('#html-content-editor-<%=BlockInstance.Id %>').bind('hide', function () {
            $('#html-content-editor-<%=BlockInstance.Id %>').find('textarea.html-content-editor').ckeditorGet().destroy();
        });

        $('#bid_<%=BlockInstance.Id %> .block-configuration a.edit').click(function (ev) {
            $('#html-content-editor-<%=BlockInstance.Id %>').modal('show');
            return false;
        });
    });

    Sys.Application.add_load(function () {

        $('#html-content-editor-<%=BlockInstance.Id %> .btn').click(function () {
            $('#html-content-editor-<%=BlockInstance.Id %>').modal('hide');
        });

    });

</script>

<div id="html-content-view-<%=BlockInstance.Id %>" class="html-content-block">
    
    <asp:UpdatePanel ID="pnlContent" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSaveContent" EventName="Click" />
        </Triggers>
        <ContentTemplate>
            <asp:Literal ID="lPreText" runat="server"></asp:Literal><asp:Literal ID="lHtmlContent" runat="server"></asp:Literal><asp:Literal ID="lPostText" runat="server"></asp:Literal>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="upContentDialog" UpdateMode="Conditional" ChildrenAsTriggers="true" runat="server">
    <ContentTemplate>

        <div id="html-content-editor-<%=BlockInstance.Id %>"" class="modal hide fade">
            <div class="modal-header">
                <a href="#" class="close">&times;</a>
                <h3>HTML Content</h3>
            </div>
            <div class="modal-body">
                <asp:TextBox ID="txtHtmlContentEditor" CssClass="html-content-editor" TextMode="MultiLine" runat="server"></asp:TextBox>
            </div>
            <div class="modal-footer">
                <input id="btnCancel" runat="server" type="button" class="btn secondary" value="Cancel" />
                <asp:Button ID="btnSaveContent" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSaveContent_Click" />
            </div>
        </div>

    </ContentTemplate>
    </asp:UpdatePanel>

</div>


