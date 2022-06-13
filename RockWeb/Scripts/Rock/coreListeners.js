Sys.Application.add_load(function() {
    $(function() {
        $('[data-toggle="tooltip"]').tooltip();
    });

    stickyTableHeaderInit()
});

$(document).on('enabledStickiness.stickyTableHeaders', function(event) {
    $(window).on('resize', function(event) {
        stickyTableHeaderInit();

        $('.table-responsive').each(stickyRepaint);
    });
    var offsetLeft = $(event.target).offset().left;
    var twidth = $(event.target).parent('.table-responsive').width();
    $(event.target)
        .find('.tableFloatingHeaderOriginal')
        .css({ left: offsetLeft, right: $(window).width() - offsetLeft - twidth, clip: 'rect(0px, ' + (twidth + 2) + 'px, 300px, 0px)' });

    $(event.target)
        .parent('.table-responsive')
        .scroll(stickyRepaint);
});

function stickyTableHeaderInit() {
    var stickyHeaders = document.querySelector('.js-sticky-headers');

    if (stickyHeaders) {
        var navbarFixedTop = document.querySelector('.navbar-fixed-top');

        $(stickyHeaders).each(function() {
            var table = $(this);
            if (table.parents('.page-fullscreen-capable').length || table.parents('.is-fullscreen').length) {
                table.stickyTableHeaders({scrollableArea: table.parents('.panel-block')});
            } else {
                if ($(navbarFixedTop).length && $(navbarFixedTop).css('position') === 'fixed') {
                    table.stickyTableHeaders({ fixedOffset: $(navbarFixedTop) });
                } else {
                    table.stickyTableHeaders();
                }
            }
        });
    }
}

function stickyRepaint() {
    var twidth = $(this).width();
    var scroll = $(this).scrollLeft();
    var offsetLeft = $(this).offset().left;
    $('.tableFloatingHeaderOriginal').css({
        left: offsetLeft - scroll,
        clip: 'rect(0px, ' + (scroll + twidth) + 'px, 300px, ' + scroll + 'px)'
    });
}
