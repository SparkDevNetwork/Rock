(function ($) {
    /*
     * Setup the TOC for the book, normally on the left of the page.
     */
    function setupBookTableOfContents(options) {
        options = options || {};
        var $toc = this;

        /* For any node marked as a group, add a chevron and collapse the three */
        $toc.find('li.tree-group').each(function () {
            $(this).children('.title').prepend($('<i class="fa fa-chevron-right fa-fw"></i>'));

            $(this).children('ul').addClass('collapse');
        });

        /* For any item marked as a leaf node, prepend an empty icon to match spacing. */
        $toc.find('li.tree-item').each(function () {
            $(this).children('.title').prepend($('<i class="fa fa-fw"></i>'));
        });

        /* When a group node is clicked, expand or collapse it's tree. */
        $toc.find('li.tree-group > .title').on('click', function () {
            var isExpanded = $(this).find('i.fa').hasClass('fa-chevron-down');

            if (isExpanded) {
                $(this).find('i.fa').removeClass('fa-chevron-down').addClass('fa-chevron-right');
                $(this).next('ul').collapse('hide');
            }
            else {
                $(this).find('i.fa').removeClass('fa-chevron-right').addClass('fa-chevron-down');
                $(this).next('ul').collapse('show');
            }
        });

        /* If we have a current article id, make it visible by expanding it's tree. */
        if (options.currentArticleId !== null) {
            var $item = $('li.tree-item[data-article-id="' + options.currentArticleId + '"]');
            $item.addClass('active');
            $item.parents('li.tree-group').children('ul').addClass('in');
            $item.parents('li.tree-group').find('> .title i.fa').removeClass('fa-chevron-right').addClass('fa-chevron-down');
        }

        /* Handle resize events to keep the TOC the proper width. */
        var $bottomElement = $('.end-of-doc-marker');
        console.log('bottom', $bottomElement);

        $toc.width($toc.parent().width());
        updateStickyHeight($toc.find('.book-toc-container'), $toc, $bottomElement);

        $(window).resize(function () {
            $toc.width($toc.parent().width());
            updateStickyHeight($toc.find('.book-toc-container'), $toc, $bottomElement);
        });

        if ($bottomElement !== undefined) {
            $(window).on('scroll', function () {
                updateStickyHeight($toc.find('.book-toc-container'), $toc, $bottomElement);
            });
        }
    }

    /*
     * Setup the alerts in the article by converting [!TIP] style quotes into
     * something that looks a bit nicer to the user.
     */
    function setupAlerts(options) {
        options = options || {};
        var types = {
            TIP: { title: 'Tip', class: 'alert-success', icon: 'fa fa-lightbulb' },
            NOTE: { title: 'Note', class: 'alert-info', icon: 'fa fa-info-circle' },
            IMPORTANT: { title: 'Important', class: 'alert-info', icon: 'fa fa-exclamation-circle' },
            WARNING: { title: 'Warning', class: 'alert-warning', icon: 'fa fa-exclamation-triangle' },
            CAUTION: { title: 'Caution', class: 'alert-danger', icon: 'fa fa-times-circle' }
        };
        var pattern = /^\[\!(TIP|NOTE|IMPORTANT|WARNING|CAUTION)\]/;

        /* Find all block quotes in this article. */
        this.find('blockquote').each(function () {
            var $p = $(this).children().first();

            /* First node should be a paragraph. */
            if ($p.get(0).nodeName !== 'P') {
                return;
            }

            /* And also match our pattern */
            var match = pattern.exec($p.text());
            if (match === null) {
                return;
            }

            var $container = $('<p></p>');
            var type = types[match[1]];

            /* Remove the tip tag from the paragraph text. */
            $p.get(0).firstChild.nodeValue = $p.get(0).firstChild.nodeValue.replace(match[0], '').trimStart();

            /* Setup the new container to look pretty. */
            $container.addClass('alert').addClass(type.class);
            $container.append($('<p class="alert-title"><i class="' + type.icon + '"></i> ' + type.title + '</p>'));
            $container.append($(this).children());

            /* Replace the blockquote with the new prettified containter. */
            $(this).replaceWith($container);
        });
    }

    /*
     * Setup the quick links, which is the articles TOC, normally on the right
     * of the page.
     */
    function setupQuickLinks(options) {
        options = options || {};

        if (options.articleSelector === undefined) {
            return;
        }

        $toc = this;

        /* Build the TOC items. */
        Toc.init({
            $nav: this,
            $scope: $(options.articleSelector)
        });

        /* We need to reference this by Id, so generate one if it doesn't have one. */
        if ($toc.attr('id') === undefined) {
            $toc.attr('id', uuidv4());
        }

        /* Spy on scrolling so we can highlight the correct anchor. */
        $('body').scrollspy({
            target: '#' + this.attr('id')
        });

        /* Handle resize events to keep the TOC the proper width. */
        var $bottomElement = $('.end-of-doc-marker');

        $toc.width($toc.parent().width());
        updateStickyHeight($toc.children('ul').first(), $toc, $bottomElement);

        $(window).resize(function () {
            $toc.width($toc.parent().width());
            updateStickyHeight($toc.children('ul').first(), $toc, $bottomElement);
        });

        if ($bottomElement !== undefined) {
            $(window).on('scroll', function () {
                updateStickyHeight($toc.children('ul').first(), $toc, $bottomElement);
            });
        }
    }

    /*
     * Perform syntax highlighting of code blocks.
     */
    function setupSyntaxHighlight(options) {
        var languageTable = {
            applescript: 'AppleScript',
            aspnet: 'ASP.Net',
            bash: 'Bash',
            basic: 'Basic',
            c: 'C',
            coffee: 'Coffee',
            coffeescript: 'Coffee Script',
            cpp: 'C++',
            cs: 'C#',
            csharp: 'C#',
            eiffel: 'Eiffel',
            fsharp: 'F#',
            go: 'Go',
            html: 'HTML',
            java: 'Java',
            javascript: 'JavaScript',
            js: 'JavaScript',
            json: 'JSON',
            markdown: 'Markdown',
            markup: 'Markup',
            mathml: 'MathML',
            md: 'Markdown',
            objectivec: 'Objective-C',
            ocaml: 'OCaml',
            pascal: 'Pascal',
            perl: 'Perl',
            php: 'PHP',
            plsql: 'PL/SQL',
            powershell: 'PowerShell',
            py: 'Python',
            python: 'Python',
            rb: 'Ruby',
            regex: 'Regular Expression',
            ruby: 'Ruby',
            sass: 'SASS',
            scss: 'SCSS',
            shell: 'Shell',
            smalltalk: 'Smalltalk',
            sql: 'SQL',
            svg: 'SVG',
            swift: 'Swift',
            ts: 'TypeScript',
            typescript: 'TypeScript',
            vb: 'Visual Basic',
            'visual-basic': 'Visual Basic',
            xaml: 'XAML',
            xml: 'XML',
            yaml: 'YAML',
            yml: 'YAML'
        };

        $(this).find('pre > code[class*="language-"]').each(function () {
            /* Make sure it's a valid language. */
            var pattern = /language\-([a-zA-Z\-]+)/;
            var match = pattern.exec($(this).attr('class'));
            if (match === null) {
                return;
            }

            var language = match[1];
            var lang = languageTable[language] || language;

            /* Setup a toolbar to be displayed above the code block. */
            var $tb = $('<div class="code-toolbar"><span class="language">' + lang + '</span><span class="copy">Copy</span></div >');
            $tb.insertBefore($(this).parent());

            /* Setup the copy button. */
            var $code = $(this);
            var cb = window.ClipboardJS || window.Clipboard; /* Support Rock < v9.0 */
            var clip = new cb($tb.find('.copy').get(0), {
                text: function () {
                    return $code.text();
                }
            });
            clip.on('success', function () {
                $tb.find('.copy').addClass('success');
                setTimeout(function () {
                    $tb.find('.copy').removeClass('success');
                }, 100);
            });

        });

        /* Ensure Prism has the XAML language defined. */
        if (Prism.languages.xaml === undefined) {
            Prism.languages.xaml = Prism.languages.extend('xml', {});
        }

        /* Highlight everything in this article. */
        Prism.highlightAllUnder(this.get(0));
    }

    /*
     * Generate a UUID.
     */
    function uuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    /*
     * Updates the height of a sticky element in relation to the $bottom
     * element, which should be somewhere below it in the view tree.
     */
    function updateStickyHeight($target, $top, $bottom) {
        var viewportBottom = $(window).scrollTop() + $(window).height();
        var offsetBottom = Math.max(viewportBottom - ($bottom !== undefined ? $bottom.offset().top : 0), 0);

        var height = $(window).height() - ($top.offset().top - $(window).scrollTop()) - offsetBottom;

        height = Math.max(200, height);

        $target.css('height', height);
    }

    /*
     * Process an action for a single jQuery element.
     */
    function processAction(action, options) {
        if (action === "book-toc") {
            setupBookTableOfContents.apply(this, [options]);
        }
        else if (action === 'alerts') {
            setupAlerts.apply(this, [options]);
        }
        else if (action === 'syntax-highlight') {
            setupSyntaxHighlight.apply(this, [options]);
        }
        else if (action === 'quick-links') {
            setupQuickLinks.apply(this, [options]);
        }
        else if (action === 'url-links') {
            setupUrlLinks.apply(this, [options]);
        }
    }

    /*
     * Declare the jQuery plugin.
     */
    $.fn.Rockumentation = function (action, options) {
        if (action === undefined) {
            /* Initialize the article */
            this.Rockumentation('alerts')
                .Rockumentation('syntax-highlight');

            if (this.length === 1) {
                /* Initialize the book TOC */
                $('.book-toc').Rockumentation('book-toc', {
                    currentArticleId: this.data('article-id')
                });

                /* Initialize the in-article TOC */
                $('.article-toc').Rockumentation('quick-links', {
                    articleSelector: 'article'
                });

                /* Initialize the search. */
                var $searchContainer = $('.js-search-field');
                if ($searchContainer.length === 1) {
                    var versionId = $searchContainer.data('version-id');
                    var pageId = $searchContainer.data('page-id');

                    var $searchField = $('<input />').appendTo($searchContainer);
                    $searchField.typeahead({
                        name: 'articles',
                        limit: 10,
                        remote: '/api/BBM_Rockumentation_Utility/Search?versionId=' + versionId + '&pageId=' + pageId + '&q=%QUERY',
                        valueKey: 'Title'
                    })
                        .on('typeahead:selected', function (_field, result) {
                            window.location = result.Url;
                        });

                    $searchContainer.find('.twitter-typeahead').css('width', '100%');
                    $searchContainer.find('.tt-hint').hide();
                    $searchContainer.find('.tt-query').addClass('form-control')
                        .css('width', '100%')
                        .css('background-color', '')
                        .attr('placeholder', 'Search');
                }
            }
        }
        else {
            return this.each(function () {
                processAction.apply($(this), [action, options]);
            });
        }
    };
})(jQuery);
