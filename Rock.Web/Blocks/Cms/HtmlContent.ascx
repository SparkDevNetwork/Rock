<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HtmlContent.ascx.cs" Inherits="Rock.Web.Blocks.Cms.HtmlContent" %>
<script> 
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

        $('#html-content-editor-<%=BlockInstance.Id %>').dialog({
            autoOpen: false,
            width: 880,
            height: 500,
            title: 'HTML Content Editor',
            closeOnEscape: true,
            modal: true,
            close: function (event, ui) {
                $('#html-content-editor-<%=BlockInstance.Id %>').find('textarea.html-content-editor').ckeditorGet().destroy();
            },
            open: function (event, ui) {
                $(this).parent().appendTo("form");
                $('#html-content-editor-<%=BlockInstance.Id %> textarea.html-content-editor').ckeditor(ckoptionsAdv).end();
            }
        });

        $('#block-instance-id-<%=BlockInstance.Id %> .block-configuration a.edit').click(function (ev) {
            $('#html-content-editor-<%=BlockInstance.Id %>').dialog('open');
            return false;
        });
    });

    Sys.Application.add_load(function () {

        $('#html-content-editor-<%=BlockInstance.Id %> .save').click(function () {
            $('#html-content-editor-<%=BlockInstance.Id %>').dialog('close');
        });

        $('#html-content-editor-<%=BlockInstance.Id %> .cancel').click(function () {
            $('#html-content-editor-<%=BlockInstance.Id %>').dialog('close');
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

    <div id="html-content-editor-<%=BlockInstance.Id %>" class="html-content-editor-panel">
        <asp:UpdatePanel ID="upContentDialog" UpdateMode="Conditional" ChildrenAsTriggers="true" runat="server">
            <ContentTemplate>
                <asp:TextBox ID="txtHtmlContentEditor" CssClass="html-content-editor" TextMode="MultiLine" runat="server"></asp:TextBox>
                
                <div class="editor-buttons">
                    <asp:Button ID="btnSaveContent" runat="server" meta:resourcekey="btnSaveContent" Text="Save" CssClass="save" OnClick="btnSaveContent_Click" />
                    <input id="btnCancel" runat="server" meta:resourcekey="btnCancel" type="button" class="cancel" value="Cancel" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</div>


