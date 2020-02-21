(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {
        $('a.warning').on('click', function (e) {
            e.preventDefault();
            $(this).siblings('div.alert-warning').slideToggle(function () {
                Rock.controls.modal.updateSize(this);
            });
            $(this).siblings('div.alert-info').slideUp();
        });
    });
}(Sys));
