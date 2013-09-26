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
                        // TODO: There could be a possiblity that a block and a page could have the same id
                        // May need to interrogate the selected node on the rockTree to make sure the right
                        // one gets selected.
                        var $li = $(this).find('[data-id="' + id + '"]'),
                            modelType = $li.attr('data-model'),
                            action = $li.find('a').first().attr('href');

                        switch (modelType) {
                            case 'Page':
                                window.location = action;
                                break;
                            case 'Block':
                                action = action.substring(action.indexOf('javascript: '));
                                console.log(action);
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

