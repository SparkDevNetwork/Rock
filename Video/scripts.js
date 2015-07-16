var Ooyala = function () {
    var loadStyles = function () {
        var relPath = '../plugins/cc_newspring/Blocks/Video/Styles/styles.css';
        var styleLink = $('<link>').attr('rel', 'stylesheet').attr('href', relPath);
        $('head').append(styleLink);
    };

    return {
        init: function () {
            loadStyles();
        }
    };
}();

$(document).ready(Ooyala.init);