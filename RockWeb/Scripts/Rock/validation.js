$(document).ready(function (e) {

    // Monitors validation controls to watch for them becoming invalid.  Once
    // They are invalid, the validated control's parent <dl> tag will have the 'error'
    // css class added.  This requires that the validation control's display be set to
    // dynamic.  This script will also clear the error text display by the control (requires
    // that a validation summary control be used.

    $("span.validation-error").bind("DOMAttrModified propertychange", function (e) {

        // Exit early if IE because it throws this event lots more
        if (e.originalEvent.propertyName && e.originalEvent.propertyName != "isvalid") return;

        var controlToValidate = $("#" + this.controltovalidate);
        var validators = controlToValidate.attr("Validators");
        if (validators == null) return;

        var isValid = true;
        $(validators).each(function () {
            if (this.isvalid !== true) {
                isValid = false;
            }
        });

        this.innerHTML = '';

        if (isValid) {
            controlToValidate.parents($('dl')).removeClass("error");
        } else {
            controlToValidate.parents($('dl')).addClass("error");
        }
    });

});
