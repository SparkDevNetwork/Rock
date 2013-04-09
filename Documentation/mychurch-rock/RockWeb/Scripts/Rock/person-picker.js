(function ($) {
    var proto = $.ui.autocomplete.prototype

    $.extend(proto, {
        _renderItem: function (ul, item) {

            if (this.options.html) {

                // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                var listItem = document.createElement("li");
                listItem.className = "rock-picker-select-item";
                if (!item.IsActive) {
                    listItem.className += " inactive";
                }
                listItem.setAttribute('data-person-id', item.Id);
                listItem.innerHTML = '<label><input type="radio" id="' + item.Id + '" name="person-id" value="' + item.Id + '">' + item.Name + '</label>'
                        + item.PickerItemDetailsHtml;
                var myResultSection = $('#' + this.options.appendTo);
                return myResultSection.append(listItem);
            }
            else {
                return $("<li></li>")
                        .data("item.autocomplete", item)
                        .append($("<a></a>")["text"](item.label))
                        .appendTo(ul);
            }
        }
    });

})(jQuery);

$('a.rock-picker').click(function (e) {
    e.preventDefault();
    $(this).next('.rock-picker').show();
});

$('.rock-picker-select').on("click", ".rock-picker-select-item", function (e) {
    var selectedItem = $(this).attr('data-person-id');

    // hide other open details
    $('.rock-picker-select-item-details').each(function (index) {
        var currentItem = $(this).parent().attr('data-person-id');

        if (currentItem != selectedItem) {
            $(this).slideUp();
        }
    });

    $(this).find('.rock-picker-select-item-details:hidden').slideDown();
});