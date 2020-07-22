; (function () {
  function updateKeyValues(e) {
    var $span = e.closest('span.value-list');
    var newValue = '';

    var valueDelimiters = ['|', ','];
    $span.children('span.value-list-rows').first().children('div.controls-row').each(function (index) {
      var value = $(this).children('.js-value-list-input').first().val();

      valueDelimiters.forEach(function (v, i, a) {
        var re = new RegExp('\\' + v, 'g');
        if (value.indexOf(v) > -1) {
          value = value.replace(re, encodeURIComponent(v));
        }
      });
      newValue += value + '|'
    });

    $span.children('input').first().val(newValue);
  }

  Sys.Application.add_load(function () {

    $('a.value-list-add').on('click', function (e) {
      e.preventDefault();
      var $ValueList = $(this).closest('.value-list');
      var newValuePickerHtml = $ValueList.find('.js-value-list-html').val()
      $ValueList.find('.value-list-rows').append(newValuePickerHtml);
      updateKeyValues($(this));
      Rock.controls.modal.updateSize($(this));
    });

    $(document).on('click', 'a.value-list-remove', function (e) {
      e.preventDefault();
      var $rows = $(this).closest('span.value-list-rows');
      $(this).closest('div.controls-row').remove();
      updateKeyValues($rows);
      Rock.controls.modal.updateSize($(this));
    });

    $(document).on('change', '.js-value-list-input', function (e) {
      updateKeyValues($(this));
    });
  });
})();
