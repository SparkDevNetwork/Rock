(function ($) {
    $(document).ready(function () {
        SearchAccounts();
    });
}(jQuery));

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SearchAccounts);

function SearchAccounts() {
    (function ($) {
        $('#rtbSearchBox').keyup(function () {

            var rex = new RegExp($(this).val(), 'i');
            $('#searchCollapse .btn').hide();
            $('#searchCollapse .btn').filter(function () {
                return rex.test($(this).text());
            }).show();

            if ($(this).val().length == 0) {
                $('#searchCollapse .btn').hide();
            }

        })

    }(jQuery));
}