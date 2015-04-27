$(document).ready(function() {

    // fade in page
    $("#content-box").not(".no-fade").css("display", "none");
    $("#content-box").not(".no-fade").fadeIn(1000);

    // reusable show password control
    // checkbox toggle should have class of '.show-password'
    // password input should have class of '.password-field'
    $('body').on('click', '.show-password', function (e) {
        field = $(this).closest('.form-group').find('.password-field');;
        if (field.attr('type') == "text") { new_type = "password"; } else { new_type = "text"; }
        new_field = field.clone();
        new_field.attr("id", field.attr('id'));
        new_field.attr("type", new_type);
        field.replaceWith(new_field);
    });
});


//
// Validation Helpers

// validates required form fields
function validateForm(panel) {
    var formValid = true;

    // ensure that all values were provided
    $(panel).find(".required-field").each(function (index, value) {
        if (this.value.length == 0) {
            $(this).closest('.form-group').addClass('has-error');
            formValid = false;
        } else {
            $(this).closest('.form-group').removeClass('has-error');
        }
    });


    if (formValid) {
        return true;
    } else {
        return false;
    }
}

// validates urls 
function validateURL(textval) {
    var urlregex = new RegExp(
          "^" +
            // protocol identifier
            "(?:(?:https?)://)" +
            // user:pass authentication
            "(?:\\S+(?::\\S*)?@)?" +
            "(?:" +
              // IP address exclusion
              // private & local networks

              // IP address dotted notation octets
              // excludes loopback network 0.0.0.0
              // excludes reserved space >= 224.0.0.0
              // excludes network & broacast addresses
              // (first & last IP address of each class)
              "(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])" +
              "(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}" +
              "(?:\\.(?:[1-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))" +
            "|" +
              // host name
              "(?:(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z\\u00a1-\\uffff0-9]+)" +
              // domain name
              "(?:\\.(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z\\u00a1-\\uffff0-9]+)*" +
              // TLD identifier
              "(?:\\.(?:[a-z\\u00a1-\\uffff]{2,}))" +
            ")" +
            // port number
            "(?::\\d{2,5})?" +
            // resource path
            "(?:/\\S*)?" +
          "$", "i");
    return urlregex.test(textval);
}

