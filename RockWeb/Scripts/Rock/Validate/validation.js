(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {
          
        var MutationObserver = window.MutationObserver || window.WebKitMutationObserver;
        var myObserver = new MutationObserver(mutationHandler);
        var obsConfig = { attributes: true };

        $("span.validation-error").each(function () {
            myObserver.observe(this, obsConfig);
        });

        function mutationHandler(mutationRecords) {
            mutationRecords.forEach(function (mutation) {

                var controlToValidate = $("#" + mutation.target.controltovalidate);

                var isValid = true;

                $(controlToValidate).each(function () {
                    $(this.Validators).each(function () {
                        if (this.isvalid !== true) {
                            isValid = false;
                        }
                    });
                });

                mutation.target.innerHTML = '';

                if (isValid) {
                    controlToValidate.parents('div.form-group').removeClass("has-error");
                } else {
                    controlToValidate.parents('div.form-group').addClass("has-error");
                }
            })
        }

    });
}(Sys));

