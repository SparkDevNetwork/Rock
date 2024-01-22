document.addEventListener(
  "DOMContentLoaded",
  function () {
    let collapsables = $('[data-toggle="collapse"]');
    collapsables.each(function (i) {
      $(this).bind("click", function () {
        let icon = $(this).find(".fa-plus, .fa-minus");
        let hasClass = icon.hasClass("fa-plus");
        if (hasClass) {
          icon.removeClass("fa-plus");
          icon.addClass("fa-minus");
        } else {
          icon.addClass("fa-plus");
          icon.removeClass("fa-minus");
        }
      });
    });
  },
  false
);
