$(document).ready(function() {

    // fade in page
    $("#content-box").css("display", "none");
    $("#content-box").fadeIn(1000);

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
    var urlregex = new RegExp('^https?:\/\/[a-z0-9-\.]+\.[a-z]{2,4}\/?[^\s<>\#%"\,\{\}\\|\\\^\[\]`]+?$');
    return urlregex.test(textval);
}

