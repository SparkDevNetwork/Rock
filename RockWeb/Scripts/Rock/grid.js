function confirmDelete(e, nameText) {
    e.preventDefault();
    bootbox.dialog('Are you sure you want to delete this ' + nameText + '?',
        [
            {
                "label": "OK", "class": "btn-primary", "callback": function () {
                    eval(e.target.href)
                }
            },
            {
                "label": "Cancel", "class": "btn-secondary"
            }
        ]);
}