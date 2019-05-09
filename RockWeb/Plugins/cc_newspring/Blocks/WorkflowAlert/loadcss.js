var loadCSS = function () {
    var relPath = '/plugins/cc_newspring/blocks/workflowalert/styles/styles.css';
    var styleLink = $('<link>').attr('rel', 'stylesheet').attr('href', relPath);
    $('head').append(styleLink);
}();
