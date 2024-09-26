
var postbackCount = 0;

$(window).bind('pageshow', function (e) {if ( e.originalEvent.persisted ){$('#updateProgress').hide();}});


function pageResize() {
    window.parent.resizeDynamicModal();

    // create an Observer instance
    const resizeObserver = new ResizeObserver(entries => {
        window.parent.resizeDynamicModal()
    });

    // start observing a DOM node
    resizeObserver.observe(document.body)
}

// DOMContentLoaded end
document.addEventListener('DOMContentLoaded', function () {
    Sys.Application.add_load(function () {
        // increment postback
        postbackCount++;

        var header = document.querySelector(".pjs-main-container");
        if (window.location.hash) {
            header.classList.add("headroom--not-top");
        }
        var headroom = new Headroom(header, {
            tolerance: {
                down : 5,
                up : 5
            },
            offset : 120
        });
        headroom.init();

        // if .block-instance.group-finder
        var groupFinder = $('.block-instance.group-finder');
        if (groupFinder.length) {
            var openFilters = JSON.parse(sessionStorage.getItem('openFilters')) || [];
            // get id that ends with _pnlSearch
            var groupFinderSearch = groupFinder.find('[id$=_pnlSearch]').addClass('filters visibility-hidden styled-scroll');
            var groupFinderResults = groupFinder.find('[id$=_pnlResults]').addClass('results');
            groupFinderResults.find('div:first').removeClass('margin-v-sm').addClass('lava-output');
            var groupFinderActions = groupFinderSearch.find('.actions');

            // add filter toggle button
            var filterButton = `<a href="#" class="btn btn-outline-primary toggle-filters-button js-toggle-filters-button mb-3 d-md-none"><svg class="filter-icon" width="15" height="12" viewBox="0 0 15 12" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M1 1H14" stroke="white" stroke-width="2" stroke-linecap="round"/><path d="M4 5.5H11" stroke="white" stroke-width="2" stroke-linecap="round"/><path d="M6 10.5H9" stroke="white" stroke-width="2" stroke-linecap="round"/></svg> Filters</a>`
            groupFinderSearch.parent().prepend(filterButton);

            //add event listener to open filters button
            $('.js-toggle-filters-button').on('click', (e) => {
                e.preventDefault();
                groupFinderSearch.toggleClass('active');
            });

            // insert html

            // prepend search input to groupFinderResults
            groupFinderResults.prepend(groupFinderSearchInput);


            groupFinderSearch.find('.form-group').wrapAll('<div class="facets"><div class="filter-list"></div></div>');

            var groupFinderSearchInput = `<div class="group-search-input-wrap js-group-search-input-wrap"><input type="search" class="js-filter-items-search group-search-input form-control" placeholder="Search" name="q" /></div>`;


            // // get text of control-label and add to parent .form-group as class
            groupFinderSearch.find('.control-label').each(function () {
                var formGroup = $(this).parent('.form-group'),
                    labelClass = $(this).text().toLowerCase().replace(/&/g, '').replace(/\s+/g, '-')
                formGroup.addClass("filter filter-" + labelClass);
                formGroup.attr("data-filter", labelClass)

                if ( labelClass === "group-experience" ) {
                    if ( openFilters !== null && openFilters.indexOf("more-filters") > -1 ) {
                        formGroup.nextAll('.form-group').wrapAll('<div class="form-group filter filter-more-filters open" data-filter="more-filters"><div class="control-wrapper"></div></div>');
                    } else {
                        formGroup.nextAll('.form-group').wrapAll('<div class="form-group filter filter-more-filters" data-filter="more-filters"><div class="control-wrapper" style="display:none;"></div></div>');
                    }
                }

                // if openFilters is not null
                if (openFilters !== null && openFilters.length > 0) {
                    // if labelClass is in openFilters
                    if (openFilters.indexOf(labelClass) > -1) {
                        formGroup.addClass("open");
                    } else {
                        if ( labelClass === "campuses" || labelClass === "group-type" || labelClass == "group-experience" ) {
                            formGroup.find('.control-wrapper').hide();
                        }
                    }
                } else {
                    if ( labelClass === "campuses" || labelClass === "group-type") {
                        formGroup.addClass("open");
                    }
                    if ( labelClass == "group-experience" ) {
                        formGroup.find('.control-wrapper').hide();
                    }
                }
            });

            let filterGender = ($('[id$="_pnlSearch"] .filter-list .filter-gender'));
            $('[id$="_pnlSearch"] .filter-list .filter-gender').remove();
            $('.filter-more-filters > .control-wrapper').append(filterGender);

            // shorten the text to three letters
            groupFinderSearch.find('.filter-day-of-week .checkbox-inline .label-text').each(function () {
                var dayText = $(this).text().substring(0, 3);
                $(this).text(dayText);
            });

            $('.card-group .card-meta .day').each(function () {
                var dayText = $(this).text().substring(0, 3);
                $(this).text(dayText);
            });

            // add html <label for="filter-more-filters">More Filters</label> inside .filter-more-filters
            groupFinderSearch.find('.filter-more-filters').prepend('<label class="control-label" for="filter-more-filters">More Filters</label>');

            // get
            groupFinderSearch.find('.filter-group-type .label-text').each(function () {
                $(this).addClass("label-" + $(this).text().toLowerCase().replace(/&/g, '').replace(/\s+/g, '-'));
            });

            groupFinderSearch.find('.actions .btn-primary').addClass("align-items-center").text("Filter").prepend(`<svg class="filter-icon mr-2" width="15" height="12" viewBox="0 0 15 12" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M1 1H14" stroke="white" stroke-width="2" stroke-linecap="round"/><path d="M4 5.5H11" stroke="white" stroke-width="2" stroke-linecap="round"/><path d="M6 10.5H9" stroke="white" stroke-width="2" stroke-linecap="round"/></svg>`)
            // change text of .btn-primary to "Search"

            $('.filters .filter-list > .filter > .control-label').on('click', function () {
                const $filter = $(this).parent('.filter');
                const $openFilters = $('.filters .filter-list > .filter.open');
                $openFilters.removeClass('open').find('> .control-wrapper').slideUp();
                $filter.toggleClass('open').find('> .control-wrapper').slideToggle();
                const openFilter = new Array($filter.attr('data-filter'));
                sessionStorage.setItem('openFilters', JSON.stringify(openFilter));
            });
        }

        if ($('.js-filter-items').length) {
            var filterItems = [];

            $('.js-filter-items-search').on('keyup', function () {
                if ($('.js-filter-items > .panel-group').length > 0) {
                    filterItems = $('.js-filter-items .panel-group > .panel');
                } else {
                    filterItems = $('.js-filter-item');
                }
                // get value of js-filter-items-search
                var searchValue = $(this).val().toLowerCase();
                // loop through .js-filter-items
                filterItems.each(function () {
                    // get value of .js-filter-items
                    var filterValue = $(this).text().toLowerCase();
                    // if filterValue does not contain searchValue
                    if (filterValue.indexOf(searchValue) > -1) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });
                // if all .js-filter-items are hidden
                if ($('.js-filter-item:visible').length == 0) {
                    // show .js-filter-items-empty
                    $('.js-filter-items-empty').show();
                } else {
                    // hide .js-filter-items-empty
                    $('.js-filter-items-empty').hide();
                }

            });
        }

        $('.js-launch-modal').click(function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            var modalTitle = $(this).data('modal-title');
            var modalConfirm = $(this).data('modal-close-confirmation');
            var closeMessage = $(this).data('modal-close-message');
            document.body.style.setProperty('--scrollbar-width', Rock.controls.util.getScrollbarWidth() + 'px');
            doDynamicModal(url, modalConfirm, modalTitle, closeMessage);
        });

        // if js-filter-items
        if ($('.js-filter-items').length) {
            var filterItems = [];

            $('.js-filter-items-search').on('keyup', function () {
                if ($('.js-filter-items > .panel-group').length > 0) {
                    filterItems = $('.js-filter-items .panel-group > .panel');
                } else {
                    filterItems = $('.js-filter-item');
                }
                // get value of js-filter-items-search
                var searchValue = $(this).val().toLowerCase();
                // loop through .js-filter-items
                filterItems.each(function () {
                    // get value of .js-filter-items
                    var filterValue = $(this).text().toLowerCase();
                    // if filterValue does not contain searchValue
                    if (filterValue.indexOf(searchValue) > -1) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });
                // if all .js-filter-items are hidden
                if ($('.js-filter-item:visible').length == 0) {
                    // show .js-filter-items-empty
                    $('.js-filter-items-empty').show();
                } else {
                    // hide .js-filter-items-empty
                    $('.js-filter-items-empty').hide();
                }

            });
        }

        if (postbackCount > 1) {
            document.body.classList.add("has-postback");
        }

        if (self !== top) {
            // if any obsidian-block-loading exists in vanilla js
            if (document.body.classList.contains('obsidian-loading')) {
                // add observer to body to observe data-obsidian-pending-blocks
                var targetNode = document.body;
                var config = { attributes: true, childList: false, subtree: false };
                var callback = function (mutationsList, observer) {
                    for (var mutation of mutationsList) {
                        if (mutation.attributeName === 'data-obsidian-pending-blocks') {
                            if (targetNode.getAttribute('data-obsidian-pending-blocks') === '0' && !targetNode.classList.contains('obsidian-loading')) {
                                // run function once and disconnect observer
                                if (typeof iResized === 'undefined') {
                                    window.iResized = true;
                                    pageResize();
                                }
                                observer.disconnect();
                            }
                        }
                    }
                };

                var observer = new MutationObserver(callback);
                observer.observe(targetNode, config);
            } else {
                pageResize();
            }

            if (document.querySelectorAll('.js-workflow-entry-message-notification-box.alert-success,.alert.alert-success>.js-notification-text,.js-allow-modal-close').length > 0) {
                window.parent.allowCloseModal();
            }

            var jsCloseModal = document.querySelectorAll('.js-close-modal');
            if (jsCloseModal.length > 0) {
                for (var i = 0; i < jsCloseModal.length; i++) {
                    jsCloseModal[i].addEventListener('click', function (e) {
                        e.preventDefault();
                        window.parent.closeModal();
                    }, false);
                }
            }
        }

        var screenSize = 'desktop';
        var windowWidth = $(window).width();

        if ($(window).width() < 992) {
            screenSize = 'mobile';
            filterElementsOnResize(screenSize);
        }

        $(window).on('resize', function () {
            windowWidth = $(window).width();

            if (screenSize == 'desktop' && windowWidth < 992 ) {
                screenSize = 'mobile'
                console.log(screenSize);
                filterElementsOnResize(screenSize);

            } else if (screenSize == 'mobile' && windowWidth > 991) {
                screenSize = 'desktop'
                console.log(screenSize);
                filterElementsOnResize(screenSize);
            }
        });
    });
}, false);

window.resizeDynamicModal = function (e) {
    resizeDynamicIframe();
}

window.closeModal = function () {
    $('#dynamicModal').modal('hide');
}

window.allowCloseModal = function () {
    $('.js-close-confirmation').off('.dynamicModal').removeClass('js-close-confirmation').attr('data-dismiss', 'modal');
}

function doDynamicModal(url, closeConfirmation, modalTitle, closeConfirmationMessage) {
    if (url) {
        // Append querystring to url if it doesn't already exist
        if (url.indexOf('?') === -1) {
            url += '?modal=true';
        } else {
            url += '&modal=true';
        }
        var html = '<div class="modal modal-lg fade" id="dynamicModal" tabindex="-1" role="dialog">';
        html += '<div class="modal-dialog" role="document">';
        html += '<div class="modal-content">';
        html += '<div class="absolute-close">';
        if (closeConfirmation && closeConfirmation === true) {
            html += '<button type="button" class="close js-close-confirmation" aria-label="Close">✕</button>';
        } else {
            html += '<button type="button" class="close" data-dismiss="modal" aria-label="Close">✕</button>';
        }
        html += '</div>';
        if (modalTitle && modalTitle !== '') {
            html += '<div class="modal-header border-0"><h4 class="modal-title">' + modalTitle + '</h4></div>';
        }
        html += '<div class="modal-body p-0">';
        html += '<div id="modalLoading" class="w-100 bg-white d-none" style="height:2000px;"></div>';
        html += '<iframe loading="eager" src="' + url + '" frameborder="0" id="dynamicModalIframe" class="wc-modal-iframe w-100" style="display:block;opacity: 0;height:200px;" scrolling="no"></iframe>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        $('body').append(html);
        if (closeConfirmation && closeConfirmation === true) {
            $('#dynamicModal').modal({ backdrop: 'static', keyboard: false }).modal('show');
        } else {
            $('#dynamicModal').modal().modal('show');
        }
        $('#dynamicModalIframe').on('load', function (e) {
            //resizeDynamicIframe(this, $('#dynamicModal'));
            $(this).delay(100).animate({ opacity: "1" }, 600);

        });

        $('#dynamicModal').on('destroyed.modalmanager', function (e) {
            $('#dynamicModal').remove();
        });

        if (closeConfirmation && closeConfirmation === true) {
            if (closeConfirmationMessage && closeConfirmationMessage !== '') {
                var confirmationMessage = closeConfirmationMessage;
            } else {
                var confirmationMessage = "Are you sure you want to exit the form? Your data will be lost if you haven't submitted it.";
            }

            $('.js-close-confirmation').on('click.dynamicModal', function () { //Close Button on Form Modal to trigger Warning Modal
                bootbox.confirm({
                    message: confirmationMessage,
                    className: "modal-confirmation",
                    buttons: {
                        confirm: {
                            label: 'Yes, Exit',
                            className: 'btn-primary'
                        },
                        cancel: {
                            label: 'No, Stay',
                            className: 'btn-gray'
                        }
                    },
                    callback: function (result) {
                        if (result === true) {
                            $('#dynamicModal').modal('hide');
                        }
                    }
                });
            });
        }
    }
}

function resizeDynamicIframe(el, modal) {
    if (!el) {
        el = document.getElementById('dynamicModalIframe');
    }

    el.style.height = el.contentWindow.document.documentElement.offsetHeight + 'px';

    var modalOverflow = $(window).height() - 10 < $('#dynamicModal').height();
    if (modalOverflow) {
        $('#dynamicModal')
            .css('margin-top', 0)
            .addClass('modal-overflow');
    } else {
        $('#dynamicModal')
            .css('margin-top', 0 - $('#dynamicModal').height() / 2)
            .removeClass('modal-overflow');
    }
}


$(document).ready(() => {
    // set up mega menu button
    var megaMenuButton = $('.js-mega-menu-button');
    var megaMenuBurger = megaMenuButton.find('.hamburger-menu');

    $(megaMenuButton).on('click', ()=> {
      megaMenuBurger.toggleClass('close');

      if (megaMenuBurger.hasClass('close')) {
        $('body').css('overflow','hidden');

      } else {
        $('body').css('overflow','auto');
      }
    });

    webFormReplaceLabelsWithPlaceholders();
});


function filterElementsOnResize(screenSize) {
    var groupFinder = $('.block-instance.group-finder');
    var groupFinderSearch = groupFinder.find('[id$=_pnlSearch]').addClass('filters visibility-hidden styled-scroll');
    var groupFinderSearchInput = $('.js-group-search-input-wrap');
    var groupFinderActions = groupFinderSearch.find('.actions');


    if (screenSize == 'mobile') {
        // add search input to facets
        groupFinderSearch.find('.facets').prepend(groupFinderSearchInput);

        //remove action buttons from above .filters to inside so can be hidden on mobile with filters toggle button.
        groupFinderActions.detach();
        groupFinderSearch.find('.facets').append(groupFinderActions);




    } else if (screenSize == 'desktop') {
        $(groupFinderSearchInput).detach();
        groupFinderActions.detach();
        groupFinderSearch.prepend(groupFinderActions);


    }
}


function webFormReplaceLabelsWithPlaceholders () {
    var form = $('.js-label-replace');
    var inputs = form.find('input[type="text"], textarea');
    var selects = form.find('select');

    inputs.each((i, obj) => {
        // get label
        var inputLabel = $(obj).parents('.form-group').find('label');
        var labelText = $(inputLabel).text();

        // remove label
        $(inputLabel).detach();

        //remove tooltips
        $(inputLabel).find('[data-toggle="tooltip"').detach();

        $(obj).attr('placeholder',labelText);

    });

    selects.each((i, obj) => {
        // get label
        var selectLabel = $(obj).parents('.form-group').find('label');
        var labelText = $(selectLabel).text();

        // remove label
        $(selectLabel).detach();

        //remove tooltips
        $(selectLabel).find('[data-toggle="tooltip"').detach();

        $(obj).find('option:first-child').text(labelText);

    });


}





/*!
 * headroom.js v0.12.0 - Give your page some headroom. Hide your header until you need it
 * Copyright (c) 2020 Nick Williams - http://wicky.nillia.ms/headroom.js
 * License: MIT
 */

!function(t,n){"object"==typeof exports&&"undefined"!=typeof module?module.exports=n():"function"==typeof define&&define.amd?define(n):(t=t||self).Headroom=n()}(this,function(){"use strict";function t(){return"undefined"!=typeof window}function d(t){return function(t){return t&&t.document&&function(t){return 9===t.nodeType}(t.document)}(t)?function(t){var n=t.document,o=n.body,s=n.documentElement;return{scrollHeight:function(){return Math.max(o.scrollHeight,s.scrollHeight,o.offsetHeight,s.offsetHeight,o.clientHeight,s.clientHeight)},height:function(){return t.innerHeight||s.clientHeight||o.clientHeight},scrollY:function(){return void 0!==t.pageYOffset?t.pageYOffset:(s||o.parentNode||o).scrollTop}}}(t):function(t){return{scrollHeight:function(){return Math.max(t.scrollHeight,t.offsetHeight,t.clientHeight)},height:function(){return Math.max(t.offsetHeight,t.clientHeight)},scrollY:function(){return t.scrollTop}}}(t)}function n(t,s,e){var n,o=function(){var n=!1;try{var t={get passive(){n=!0}};window.addEventListener("test",t,t),window.removeEventListener("test",t,t)}catch(t){n=!1}return n}(),i=!1,r=d(t),l=r.scrollY(),a={};function c(){var t=Math.round(r.scrollY()),n=r.height(),o=r.scrollHeight();a.scrollY=t,a.lastScrollY=l,a.direction=l<t?"down":"up",a.distance=Math.abs(t-l),a.isOutOfBounds=t<0||o<t+n,a.top=t<=s.offset[a.direction],a.bottom=o<=t+n,a.toleranceExceeded=a.distance>s.tolerance[a.direction],e(a),l=t,i=!1}function h(){i||(i=!0,n=requestAnimationFrame(c))}var u=!!o&&{passive:!0,capture:!1};return t.addEventListener("scroll",h,u),c(),{destroy:function(){cancelAnimationFrame(n),t.removeEventListener("scroll",h,u)}}}function o(t){return t===Object(t)?t:{down:t,up:t}}function s(t,n){n=n||{},Object.assign(this,s.options,n),this.classes=Object.assign({},s.options.classes,n.classes),this.elem=t,this.tolerance=o(this.tolerance),this.offset=o(this.offset),this.initialised=!1,this.frozen=!1}return s.prototype={constructor:s,init:function(){return s.cutsTheMustard&&!this.initialised&&(this.addClass("initial"),this.initialised=!0,setTimeout(function(t){t.scrollTracker=n(t.scroller,{offset:t.offset,tolerance:t.tolerance},t.update.bind(t))},100,this)),this},destroy:function(){this.initialised=!1,Object.keys(this.classes).forEach(this.removeClass,this),this.scrollTracker.destroy()},unpin:function(){!this.hasClass("pinned")&&this.hasClass("unpinned")||(this.addClass("unpinned"),this.removeClass("pinned"),this.onUnpin&&this.onUnpin.call(this))},pin:function(){this.hasClass("unpinned")&&(this.addClass("pinned"),this.removeClass("unpinned"),this.onPin&&this.onPin.call(this))},freeze:function(){this.frozen=!0,this.addClass("frozen")},unfreeze:function(){this.frozen=!1,this.removeClass("frozen")},top:function(){this.hasClass("top")||(this.addClass("top"),this.removeClass("notTop"),this.onTop&&this.onTop.call(this))},notTop:function(){this.hasClass("notTop")||(this.addClass("notTop"),this.removeClass("top"),this.onNotTop&&this.onNotTop.call(this))},bottom:function(){this.hasClass("bottom")||(this.addClass("bottom"),this.removeClass("notBottom"),this.onBottom&&this.onBottom.call(this))},notBottom:function(){this.hasClass("notBottom")||(this.addClass("notBottom"),this.removeClass("bottom"),this.onNotBottom&&this.onNotBottom.call(this))},shouldUnpin:function(t){return"down"===t.direction&&!t.top&&t.toleranceExceeded},shouldPin:function(t){return"up"===t.direction&&t.toleranceExceeded||t.top},addClass:function(t){this.elem.classList.add.apply(this.elem.classList,this.classes[t].split(" "))},removeClass:function(t){this.elem.classList.remove.apply(this.elem.classList,this.classes[t].split(" "))},hasClass:function(t){return this.classes[t].split(" ").every(function(t){return this.classList.contains(t)},this.elem)},update:function(t){t.isOutOfBounds||!0!==this.frozen&&(t.top?this.top():this.notTop(),t.bottom?this.bottom():this.notBottom(),this.shouldUnpin(t)?this.unpin():this.shouldPin(t)&&this.pin())}},s.options={tolerance:{up:0,down:0},offset:0,scroller:t()?window:null,classes:{frozen:"headroom--frozen",pinned:"headroom--pinned",unpinned:"headroom--unpinned",top:"headroom--top",notTop:"headroom--not-top",bottom:"headroom--bottom",notBottom:"headroom--not-bottom",initial:"headroom"}},s.cutsTheMustard=!!(t()&&function(){}.bind&&"classList"in document.documentElement&&Object.assign&&Object.keys&&requestAnimationFrame),s});
