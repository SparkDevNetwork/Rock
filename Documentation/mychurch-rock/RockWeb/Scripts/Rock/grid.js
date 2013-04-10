function confirmDelete(e, nameText) {
    e.preventDefault();
    bootbox.dialog('Are you sure you want to delete this ' + nameText + '?',
        [
            {
                "label": "OK", "class": "btn-primary", "callback": function () {
                    var postbackJs = e.target.href;
                    if (postbackJs == null) {
                        postbackJs = e.target.parentElement.href;
                    }
                    // need to do unescape because firefox might put %20 instead of spaces
                    postbackJs = unescape(postbackJs);
                    eval(postbackJs)
                }
            },
            {
                "label": "Cancel", "class": "btn-secondary"
            }
        ]);
}