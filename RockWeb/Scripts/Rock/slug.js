var contentSlug = function () {
    var _selectors = {
        btnAdd: '#lbAdd',
        slugSection: '.js-slugs',
        slugRow:'.js-slug-row'
    };

    var contentChannelItemSelector;
    function init(settings) {
        contentChannelItemSelector = $(settings.contentChannelItem);
        subscribeToAddNew();

    }
    function subscribeToAddNew() {
        $(_selectors.btnAdd).unbind('click');
        $(_selectors.btnAdd).click(function (e) {

            var html = '<div class="row margin-l-sm margin-b-sm rollover-container js-slug-row clearfix">' +
                '<input id="slugId" type="hidden" value="0" />' +
                '<div class="input-group">'+
                '<input class="form-control js-slug-input" />'+
                '<span class="input-group-addon">'+
                '<a><i class="fa fa-check"></i></a>'+
                '</span>'+
                '</div >' +
                '</div >';
            $(_selectors.btnAdd).before(html)
            if ($(contentChannelItemSelector).val() === "0") {
                $(_selectors.btnAdd).hide();
            }
        });
    }
    return {
        init: init
    };

}();