<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageMap.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageMap" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfExpandedIds" runat="server" ClientIDMode="Static" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sitemap"></i><asp:Literal runat="server" Text=" Page Map" /></h1>
            </div>
            <div class="panel-body">

                <a href="#" class="btn btn-action btn-xs pull-right" onclick="$('.js-auth-roles').toggleClass('hidden')" >Show Auth</a>
                <div id="pages" style="display:none">
                    <asp:Literal ID="lPages" runat="server" ViewStateMode="Disabled" ></asp:Literal>
                </div>

            </div>
            
        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {

                var $selectedId = $('#hfSelectedId');

                $('#pages')
                    .on('rockTree:selected', function (e, id) {

                        var $li = $(this).find('[data-id="' + id + '"]'),
                            rockTree = $(this).data('rockTree'),
                            modelType,
                            action,
                            i;
                        
                        // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                        var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                            return $(this).attr('data-id')
                        }).get().join(',');

                        $('#hfExpandedIds').val(expandedDataIds);

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

                $('#pages').show();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

