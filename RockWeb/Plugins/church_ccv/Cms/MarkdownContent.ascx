<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarkdownContent.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Cms.MarkdownContent" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="js-markdown-content">
            <span class="js-markdown-view rollover-container">
                <div class="js-markdown-view">
                    <asp:Literal ID="lView" runat="server" />
                </div>

                <div class="actions rollover-item">
                    <a class="edit btn btn-action btn-xs js-md-editbutton"><i class="fa fa-pencil"></i>&nbsp;Edit Markdown</a>
                </div>
            </span>
            
            <a href="#mdedit"></a>
            <div class="js-md-edit" style="display: none">
                <Rock:MarkdownEditor ID="mdEdit" CssClass="js-markdown-editor" Rows="10" runat="server" Font-Names="Courier New" ValidateRequestMode="Disabled" />
                <div class="margin-t-sm">
                    <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary btn-xs" Text="Save" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-default btn-xs" Text="Cancel" OnClick="btnCancel_Click" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            $(document).ready(function () {
                Sys.Application.add_load(function () {

                    $('.js-md-editbutton').click(function () {
                        $(this).closest('.js-markdown-content').find('.js-markdown-view').hide();
                        $(this).closest('.js-markdown-content').find('.js-md-edit').show();

                        // size the text editor to the size of the text content
                        var $mdEditor = $(this).closest('.js-markdown-content').find('.js-markdown-editor');
                        $mdEditor.height('1px')
                        $mdEditor.height($mdEditor[0].scrollHeight);

                        // scroll to the top of the editor
                        window.scrollTo(0, $mdEditor.position().top);
                    });
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
