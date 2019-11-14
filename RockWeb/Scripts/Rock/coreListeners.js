Sys.Application.add_load(function() {
    $(function() {
        $('[data-toggle="tooltip"]').tooltip();
    });

    var stickyHeaders = document.querySelector('.js-sticky-headers');

    if (stickyHeaders !== null) {
        var navbarFixedTop = document.querySelector('.navbar-fixed-top');
        if (navbarFixedTop !== null &&  getComputedStyle(navbarFixedTop)[position] === 'fixed') {
            stickyHeaders.stickyTableHeaders({ fixedOffset: navbarFixedTop });
        } else {
            stickyHeaders.stickyTableHeaders();
        }
    }
});

$(document).on('enabledStickiness.stickyTableHeaders', function(event) {
    var navbarFixedTop = document.querySelector('.navbar-fixed-top');

    $(window).on('resize', function(event) {
        if (document.querySelector('.navbar-fixed-top') !== null && getComputedStyle(navbarFixedTop)['position'] === 'fixed') {
            $('.js-sticky-headers').stickyTableHeaders({ fixedOffset: $('.navbar-fixed-top') });
        } else {
            $('.js-sticky-headers').stickyTableHeaders();
        }
        $('.table-responsive').each(stickyRepaint);
    });
    let offsetLeft = $(event.target).offset().left;
    let twidth = $(event.target).parent('.table-responsive').width();
    $(event.target)
        .find('.tableFloatingHeaderOriginal')
        .css({ left: offsetLeft, right: $(window).width() - offsetLeft - twidth, clip: 'rect(0px, ' + (twidth + 2) + 'px, 140px, 0px)' });

    $(event.target)
        .parent('.table-responsive')
        .scroll(stickyRepaint);
});

function stickyRepaint() {
    let twidth = $(this).width();
    let scroll = $(this).scrollLeft();
    let offsetLeft = $(this).offset().left;
    $('.tableFloatingHeaderOriginal').css({
        left: offsetLeft - scroll,
        clip: 'rect(0px, ' + (scroll + twidth) + 'px, 140px, ' + scroll + 'px)'
    });
}
