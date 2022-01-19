var contentSlug = function () {
    var _selectors = {
        btnAdd: '#lbAdd',
        slugSection: '.js-slugs',
        slugRow: '.js-slug-row',
        btnSlugSave: '.js-slug-save',
        inputSlug: '.js-slug-input',
        slugId: '.js-slug-id',
        inputGroup: '.js-input-slug-group',
        slugLiteral: '.js-slug-literal',
        btnEdit: '.js-slug-edit',
        btnDelete: '.js-slug-remove'
    };

    var _contentSlugSelector;
    var _contentChannelItemSelector;
    var _saveSlug;
    var _uniqueSlug;
    var _removeSlug;
    var _txtTitle;

    function init(settings) {
        contentChannelItemSelector = $(settings.contentChannelItem);
        _saveSlug = settings.SaveSlug;
        _uniqueSlug = settings.UniqueSlug;
        _contentSlugSelector = settings.contentSlug;
        _removeSlug = settings.RemoveSlug;
        _txtTitle = settings.txtTitle;
        subscribeToEvents();
        setDelete();
        subscribeToTitle();
    }
    function subscribeToTitle() {
        $(_txtTitle).unbind('focusout');
        $(_txtTitle).focusout(function (e) {
            e.preventDefault();
            e.stopPropagation();
            var inputSlug = $(this).val();
            var rowLength = $(_selectors.slugRow).length;
            if (rowLength === 0 && inputSlug !== '') {
                var html = '<div class="form-group rollover-container js-slug-row">' +
                    '<input id="slugId" class="js-slug-id" type="hidden" value="" />' +
                    '</div >';
                $(_selectors.btnAdd).before(html);
                var row = $(_selectors.slugSection).find(_selectors.slugRow);
                if ($(contentChannelItemSelector).val() === "0") {
                    uniqueSlug(inputSlug, row);
                    $(_selectors.btnAdd).hide();
                } else {
                    saveSlug(inputSlug, '', row);
                }
            }
        });
    }
    function subscribeToEvents() {
        $(_selectors.btnAdd).unbind('click');
        $(_selectors.btnAdd).on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            var html = '<div class="form-group rollover-container js-slug-row">' +
                '<label class="js-input-slug-warning text-danger hidden">Invalid Characters Entered</label>' +
                '<input id="slugId" class="js-slug-id" type="hidden" value="" />' +
                '<div class="input-group js-input-slug-group input-group-edit">' +
                '<input class="form-control js-slug-input" name="slugInput" />' +
                '<span class="input-group-addon">' +
                '<a class="js-slug-save" href="#"><i class="fa fa-check"></i></a>' +
                '</span>' +
                '</div >' +
                '</div >';
            $(_selectors.btnAdd).before(html);
            if ($(contentChannelItemSelector).val() === "0") {
                $(_selectors.btnAdd).hide();
            }
            subscribeToEvents();
        });

        $(_selectors.inputSlug).unbind('keyup').on("keyup", function (e) {
            this.value = this.value.toLowerCase();
        });

        $(_selectors.btnSlugSave).unbind('click');
        $(_selectors.btnSlugSave).on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var inputSlug = row.find(_selectors.inputSlug).val();
            var slugId = row.find(_selectors.slugId).val();

            // make sure inputSlug has only valid characters
            var regex = new RegExp("[^a-zA-Z0-9-]");
            if (regex.test(inputSlug) === true) {
                $('.js-input-slug-warning').removeClass('hidden');
                return;
            }

            $('.js-input-slug-warning').addClass('hidden');

            if (inputSlug !== '') {
                if ($(contentChannelItemSelector).val() === "0") {
                    uniqueSlug(inputSlug, row);
                } else {
                    saveSlug(inputSlug, slugId, row);
                }
            }
        });

        $(_selectors.btnEdit).unbind('click');
        $(_selectors.btnEdit).on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var slug = row.find(_selectors.slugLiteral).html();
            setSlugEdit(slug, row);
        });

        $(_selectors.btnDelete).unbind('click');
        $(_selectors.btnDelete).on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var slugId = row.find(_selectors.slugId).val();
            if (slugId !== '') {
                removeSlug(slugId, row);
            } else {
                $(_contentSlugSelector).val('');
                $(row).remove();
                $(_selectors.btnAdd).show();
            }
        });
    }
    function setDelete() {
        var count = $(_selectors.slugRow).length;
        if (count <= 1) {
            $(_selectors.btnDelete).hide();
        } else {
            $(_selectors.btnDelete).show();
        }
    }

    function removeSlug(slugId, row) {
        $.ajax({
            url: _removeSlug.restUrl + _removeSlug.restParams.replace('{id}', slugId),
            type: 'DELETE',
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                $(row).remove();
                setDelete();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
    }
    function setSlugEdit(slug, row) {

        $(row).children().not(_selectors.slugId).remove();
        var html =
            '<div class="input-group js-input-slug-group input-group-edit">' +
            '<input class="form-control js-slug-input" Value="' + slug + '" />' +
            '<span class="input-group-addon">' +
            '<a class="js-slug-save" href="#"><i class="fa fa-check"></i></a>' +
            '</span>' +
            '</div >';
        $(row).find(_selectors.slugId).after(html);
        subscribeToEvents();
    }
    function setSlugDetail(slug, slugId, row) {
        $(row).find(_selectors.slugId).val(slugId);
        if (slugId === '') {
            $(_contentSlugSelector).val(slug);
        }
        $(row).find(_selectors.inputGroup).remove();
        var html = '<span class="js-slug-literal">' + slug + '</span>' +
            '<div class="rollover-item control-actions pull-right">' +
            '<a class="js-slug-edit margin-r-md" href="#"><i class="fa fa-pencil"></i></a>' +
            '<a class="js-slug-remove" href="#"><i class="fa fa-close"></i></a>' +
            '</div >';
        $(row).find(_selectors.slugId).after(html);
        subscribeToEvents();
    }

    function saveSlug(slug, slugId, row) {
        $.ajax({
            url: _saveSlug.restUrl + _saveSlug.restParams.replace('{slug}', encodeURI($.trim(slug))).replace('{contentChannelItemSlugId?}', slugId),
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                setSlugDetail(data.Slug, data.Id, row);
                setDelete();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
    }
    function uniqueSlug(slug, row) {
        var encodedSlug = encodeURIComponent(slug.replace('&', '').replace('|',''));
        $.ajax({
            url: _uniqueSlug.restUrl + _uniqueSlug.restParams.replace('{slug}', encodedSlug),
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                setSlugDetail(data, '', row);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
    }
    return {
        init: init
    };

}();
