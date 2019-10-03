var RockSnippets = function (context) {
    var ui = $.summernote.ui;

    var snippetData = [];

    var updateSnippets = function () {
        $.get('/api/HtmlContents/Snippets', function (data) {
            // Remove any existing snippets
            $("button.rock-snippets").next('div.dropdown-menu').find(".rock-snippet").remove();
            snippetData = [];

            if (data.length) {
                $("button.rock-snippets").next('div.dropdown-menu').append("<li class=\"rock-snippet\" style=\"font-weight: bold; color: #999;padding-left: 20px;cursor: default;\" onclick=\"event.stopPropagation(); event.preventDefault();\">My Snippets</li>");

                // Add each snippet
                $.each(data, function (key, snippet) {
                    snippetData.push({ "Name": snippet.Name, "Content": snippet.Content });
                    $("button.rock-snippets").next('div.dropdown-menu').append('<li class="rock-snippet"><a href="#" data-value="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;' + snippet.Name + '">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;' + snippet.Name + '</a></li>');
                });
            }
        });
    };
    var settings = {
        className: 'dropdown-template dropdown-menu-right',
        items: [
            "Save Content as New Snippet",
            "Manage Existing Snippets"
        ],
        click: function (event) {
            event.preventDefault();

            var $button = $(event.target);
            var value = $button.data('value').trim();
            if (value === "Save Content as New Snippet") {
                bootbox.prompt("Please enter a name for this snippet:", function (result) {
                    if (result !== null) {
                        data = { "Name": result, "Content": context.invoke("code") };
                        $.post('/api/HtmlContents/AddSnippet',
                            data,
                            function (result) {
                                updateSnippets();
                            }
                        );
                    }
                });
            }
            else if (value.startsWith('Manage Existing Snippets')) {

                context.invoke('editor.saveRange');
                var iframeUrl = Rock.settings.get('baseUrl') + "htmleditorplugins/rocksnippets";
                iframeUrl += "?modalMode=1";
                iframeUrl += "&title=Manage Snippets";

                Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

                $modalPopupIFrame = Rock.controls.modal.getModalPopupIFrame();

                $modalPopupIFrame.load(function () {

                    $modalPopupIFrame.contents().off('click');

                    $modalPopupIFrame.contents().on('click', '.js-ok-button, .js-cancel-button', function () {
                        Rock.controls.modal.close();
                        updateSnippets();
                    });
                    $modalPopupIFrame.height(675);
                    $modalPopupIFrame[0].scrolling = "yes";
                    
                });

            } else {
                var node = document.createElement('span');

                // Find and output the snippet content
                $.each(snippetData, function (key, snippet) {
                    if (snippet.Name === value) {
                        node.innerHTML = snippet.Content;
                    }
                });
                context.invoke('editor.insertNode', node);
            }
        }
    };

    // create button
    var button = ui.buttonGroup([
        ui.button({
            className: 'dropdown-toggle rock-snippets',
            contents: '<i class="fa fa-cut" /> <span class="caret"></span>',
            tooltip: 'Snippets',
            data: {
                toggle: 'dropdown'
            }
        }),
        ui.dropdown(settings)
    ]);

    updateSnippets();

    return button.render();   // return button as jquery object 

};
