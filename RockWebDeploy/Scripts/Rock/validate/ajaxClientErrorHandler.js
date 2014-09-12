(function ($) {
    'use strict';
    var ajaxRequestBegin = function (sender, args) {
            $(".ajax-error").hide();
        },
        ajaxRequestEnd = function (sender, args) {
            if (args.get_error() != undefined && args.get_error().httpStatusCode == '500') {
                var errorMessage = args.get_error().message;
                var errorName = args.get_error().name;
                args.set_errorHandled(true);
                $(".ajax-error-message").text(errorMessage.replace(errorName + ":", ""));
                $(".ajax-error").show();
            }
            else if (args.get_response() != undefined && args.get_response().get_timedOut() == true) {
                args.set_errorHandled(true);
                $(".ajax-error-message").text("Request timed out.  Please try again later.");
                $(".ajax-error").show();
            }
        };

    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(ajaxRequestBegin);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxRequestEnd);
    });
}(jQuery));