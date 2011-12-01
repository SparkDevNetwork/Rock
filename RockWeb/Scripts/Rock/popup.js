$(document).ready(function () {

    // Wire up the iframe div as a popup dialog
    $('#modalDiv').dialog({
        autoOpen: false,
        width: 580,
        height: 600,
        modal: true
    });

    // Bind the click event for all of the links that use the iframe to show the 
    // modal div/iframe
    $('a.show-iframe-dialog').click(function () {

        // Use the anchor tag's href attribute as the source for the iframe
        $('#modalIFrame').attr('src', $(this).attr('href'));

        // 
        $('#modalDiv').bind('dialogclose', function (event, ui) {
            if ($(this).attr('instance-id') != undefined)
                $('#blck-cnfg-trggr-' + $(this).attr('instance-id')).click();
            $('#modalDiv').unbind('dialogclose');
            $('#modalIFrame').attr('src', '');
        });

        // Use the anchor tag's title attribute as the title of the dialog box
        if ($(this).attr('title') != undefined)
            $('#modalDiv').dialog('option', 'title', $(this).attr('title'));

        // popup the dialog box
        $('#modalDiv').dialog('open');

        // Cancel the default behavior of the anchor tag
        return false;

    });

});
