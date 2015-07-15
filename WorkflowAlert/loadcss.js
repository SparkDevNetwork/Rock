var loadCSS = function () {
    var relPath = '../plugins/cc_newspring/workflowalert/Styles/styles.css';
    var styleLink = $('<link>').attr('rel', 'stylesheet').attr('href', relPath);
    $('head').append(styleLink);
}();