<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteMap.ascx.cs" Inherits="SiteMap" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <div id="pages">
                <asp:Literal ID="lPages" runat="server"></asp:Literal>
            </div>
        </asp:Panel>

        <script>
            $(function () {
                $('#pages')
                    .on('rockTree:selected', function (e, id) {
                        var $li = $(this).find('[data-id="' + id + '"]'),
                            rockTree = $(this).data('rockTree'),
                            modelType,
                            action,
                            i;
                        
                        if ($li.length > 1) {
                            for (i = 0; i < $li.length; i++) {
                                if (!rockTree.selectedNodes[0].name === $li.find('span').text()) {
                                    $li = $li[i];
                                    break;
                                }
                            }
                        }

                        modelType = $li.attr('data-model');
                        action = $li.find('a').first().attr('href');

                        switch (modelType) {
                            case 'Page':
                                window.location = action;
                                break;
                            case 'Block':
                                action = action.substring(action.indexOf('javascript: '));
                                eval(action);
                                break;
                        }
                    })
                    .rockTree({
                        mapping: {
                            include: [ 'model' ]
                        }
                    });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

