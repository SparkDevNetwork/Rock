function closeModal() {
    $('#modal-popup_iframe').attr('src', '');
    $find('modal-popup').hide();
}

$(document).ready(function () {

    // Bind the click event for all of the links that use the iframe to show the 
    // modal div/iframe
    $('a.show-modal-iframe').click(function () {

        var $primaryBtn = $('#modal-popup_panel div.modal-footer a.btn.primary');
        if ($(this).attr('primary-button') !== undefined) {
            $primaryBtn.text($(this).attr('primary-button'));
        } else {
            $primaryBtn.text('Save');
        }
        if ($primaryBtn.text() !== '') {
            $primaryBtn.show();
        } else {
            $primaryBtn.hide();
        }

        var $secondaryBtn = $('#modal-popup_panel div.modal-footer a.btn.secondary');
        if ($(this).attr('secondary-button') !== undefined) {
            $secondaryBtn.text($(this).attr('secondary-button'));
        } else {
            $secondaryBtn.text('Cancel');
        }
        if ($secondaryBtn.text() !== '') {
            $secondaryBtn.show();
        } else {
            $secondaryBtn.hide();
        }

        // Use the anchor tag's href attribute as the source for the iframe
        $('#modal-popup_iframe').attr('src', $(this).attr('href'));

        // If the anchor tag specifies a modal height, set the dialog's height
        if ($(this).attr('height') != undefined)
            $('#modal-popup_panel div.modal-body').css('height', $(this).attr('height'));
        else
            $('#modal-popup_panel div.modal-body').css('height', '');

        // Use the anchor tag's title attribute as the title of the dialog box
        if ($(this).attr('title') != undefined)
            $('#modal-popup_panel h3').html($(this).attr('title') + ' <small></small>');

        // popup the dialog box
        $find('modal-popup').show();

        // Cancel the default behavior of the anchor tag
        return false;

    });

});
