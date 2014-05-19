(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {

        // Monitors validation controls to watch for them becoming invalid.  Once
        // They are invalid, the validated control's parent <dl> tag will have the 'error'
        // css class added.  This requires that the validation control's display be set to
        // dynamic.  This script will also clear the error text display by the control (requires
        // that a validation summary control be used.

        if ('MutationObserver' in window) {

            var MutationObserver = window.MutationObserver || window.WebKitMutationObserver;
            var myObserver = new MutationObserver(mutationHandler);
            var obsConfig = { attributes: true };

            $("span.validation-error").each(function () {
                myObserver.observe(this, obsConfig);
            });

        }
        else {

            $("span.validation-error").bind("DOMAttrModified propertychange", function (e) {

                // Exit early if IE because it throws this event lots more
                if (e.originalEvent.propertyName && e.originalEvent.propertyName != "isvalid") return;

                var controlToValidate = $("#" + this.controltovalidate);
                if (controlToValidate == null) return;

                var isValid = true;

                $(controlToValidate).each(function () {
                    $(this.Validators).each(function () {
                        if (this.isvalid !== true) {
                            isValid = false;
                        }
                    });
                });

                this.innerHTML = '';

                if (isValid) {
                    controlToValidate.parents('div.form-group').removeClass("has-error");
                } else {
                    controlToValidate.parents('div.form-group').addClass("has-error");
                }
            });

        }

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

