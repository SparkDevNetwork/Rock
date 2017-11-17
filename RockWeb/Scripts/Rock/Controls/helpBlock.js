(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {
        $('a.help').click(function (e) {
            e.preventDefault();
            $(this).siblings('div.alert-info').slideToggle(function () {
                Rock.controls.modal.updateSize(this);
            });
            $(this).siblings('a.warning').insertAfter($(this));
            $(this).siblings('div.alert-warning').slideUp();
        });
    });
}(Sys));