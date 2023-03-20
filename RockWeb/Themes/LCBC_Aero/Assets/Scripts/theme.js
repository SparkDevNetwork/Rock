document.addEventListener('DOMContentLoaded', function () {
    Sys.Application.add_load(function() {
        if (document.querySelector('.namesearch')) {
            setTimeout(function () {
                document.querySelector('.namesearch').focus()
            }, 100)
        }
    });
});