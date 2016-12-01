(function ($) {
    'use strict';
    var ajaxRequestBegin = function (sender, args) {
        $(".ajax-error").hide();
    },
        ajaxRequestEnd = function (sender, args) {
            if (args.get_error() != undefined && args.get_error().httpStatusCode == '500') {
                var errorName = args.get_error().name;
                var errorMessage = args.get_error().message;
                errorMessage = errorMessage.replace(errorName + ":", "");
                var response = args.get_response();
                if (response) {

                    // if we got responseData (probably from Error.aspx.cs), use that as the error output
                    var responseData = response.get_responseData();
                    errorMessage = responseData;
                }

                args.set_errorHandled(true);
                $(".ajax-error-message").html(errorMessage);
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