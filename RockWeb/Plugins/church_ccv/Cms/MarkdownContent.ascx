<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarkdownContent.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Cms.MarkdownContent" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="js-markdown-content ">
            <span class="js-markdown-view rollover-container">
                <div class="js-markdown-content">
                    <asp:Literal ID="lContent" runat="server" />
                </div>

                <div class="actions rollover-item">
                    <a class="edit btn btn-action btn-xs js-md-editbutton"><i class="fa fa-pencil"></i>&nbsp;Edit Markdown</a>
                </div>
            </span>

            <div class="js-markdown-block">
                <asp:Panel ID="pnlEdit" runat="server" CssClass="js-md-edit" Style="display: none">
                    <Rock:MarkdownEditor ID="mdEdit" CssClass="js-markdown-editor" Rows="10" runat="server" />
                    <div class="margin-t-sm">
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary btn-xs" Text="Save" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-default btn-xs" Text="Cancel" OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>

        <script type="text/javascript">
            $(document).ready(function () {
                Sys.Application.add_load(function () {

                    $('.js-md-editbutton').click(function () {
                        var contentHeight = $(this).closest('.js-markdown-content').height();
                        if (contentHeight < 200) {
                            contentHeight = 200;
                        }

                        $(this).closest('.js-markdown-content').find('.js-markdown-editor').height(contentHeight)
                        $(this).closest('.js-markdown-content').find('.js-markdown-view').hide();
                        $(this).closest('.js-markdown-content').find('.js-md-edit').show();
                    });
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
