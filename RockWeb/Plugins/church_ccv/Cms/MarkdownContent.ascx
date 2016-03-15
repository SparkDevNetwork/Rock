<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarkdownContent.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Cms.MarkdownContent" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="js-markdown-content">
            <span class="js-markdown-view">
                <asp:Literal ID="lContent" runat="server" />
            </span>

            <div class="js-markdown-block">
                <asp:Panel ID="pnlEdit" runat="server" CssClass="js-md-edit" Style="display: none">
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:MarkdownEditor ID="mdEdit" Rows="10" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary btn-xs" Text="Save" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-default btn-xs" Text="Cancel" OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>

        <script type="text/javascript">
            $(document).ready(function () {
                Sys.Application.add_load(function () {

                    $('.js-markdown-view').dblclick(function () {
                        $(this).closest('.js-markdown-content').find('.js-markdown-view').hide();
                        $(this).closest('.js-markdown-content').find('.js-md-edit').show();
                    });
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
