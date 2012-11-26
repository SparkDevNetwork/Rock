function ConfirmDelete(e, nameText) {
    e.preventDefault();
    bootbox.confirm('Are you sure you want to delete this ' + nameText + '?', function (result) {
        {
            if (result) {
                {
                    eval(e.target.href);
                }
            }
        }
    }
    );
}