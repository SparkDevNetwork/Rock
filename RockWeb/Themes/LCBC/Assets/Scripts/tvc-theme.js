// jquery ready

$(document).ready(function () {
    $('#site-navigation').on('show.bs.collapse', function () {
        $('body').addClass('nav-open');
    }).on('hide.bs.collapse', function () {
        $('body').removeClass('nav-open');
    });

    // toggle search
    $('.js-search-toggle').off("click").on("click", function (e) {
        e.preventDefault();
        var searchInput = $('.site-search-wrapper input');
        var search = searchInput.val().trim();
        if ($('body').hasClass('search-open') && search.length > 0) {
            window.location = '/search?Q=' + search;
        } else {
            if (!$('body').hasClass('search-open')) {
                $('body').addClass('search-open');
                $('.site-search-wrapper').addClass('active');

                $(document).bind("click.searchOpen", function (e) {
                    if ($(e.target).closest('.site-search-wrapper').length === 0) {
                        closeSearch();
                    }
                });
                $('.site-search-wrapper input').bind("keypress.searchOpen", function (e) {
                    if (e.which == 13) {
                        e.preventDefault();
                        var search = $(this).val().trim();
                        if (search.length > 0) {
                            window.location = '/search?Q=' + search;
                        }
                    }
                });
            } else {
                closeSearch();
            }
        }
    });

    function closeSearch() {
        $('.site-search-wrapper').removeClass('active');
        $('body').removeClass('search-open');
        $(document).unbind(".searchOpen");
    }


    $('.mobile-site-search-wrapper input').bind("keypress", function (e) {
        if (e.which == 13) {
            e.preventDefault();
            var search = $(this).val().trim();
            if (search.length > 0) {
                window.location = '/search?Q=' + search;
            }
        }
    });

    $('.js-launch-modal').click(function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        var modalTitle = $(this).data('modal-title');
        var modalConfirm = $(this).data('modal-close-confirmation');
        var closeMessage = $(this).data('modal-close-message');
        doDynamicModal(url, modalConfirm, modalTitle, closeMessage);
    });
});
var postbackCount = 0;
document.addEventListener('DOMContentLoaded', function () {
    Sys.Application.add_load(function () {
        // increment postback
        postbackCount++;
        if (postbackCount > 1) {
            document.body.classList.add("has-postback");
        }

        if (self !== top) {
            window.parent.resizeDynamicModal();

            // create an Observer instance
            const resizeObserver = new ResizeObserver(entries => {
                window.parent.resizeDynamicModal()
            });

            // start observing a DOM node
            resizeObserver.observe(document.body)

            if ( document.querySelectorAll('.js-workflow-entry-message-notification-box.alert-success,.alert.alert-success>.js-notification-text,.js-allow-modal-close').length > 0) {
                window.parent.allowCloseModal();
            }

            var jsCloseModal = document.querySelectorAll('.js-close-modal');
            if (jsCloseModal.length > 0) {
                for (var i = 0 ; i < jsCloseModal.length; i++) {
                    jsCloseModal[i].addEventListener('click', function (e) {
                        e.preventDefault();
                        window.parent.closeModal();
                    }, false);
                }
            }
        }
        outlinedInput();
    });
}, false);

window.resizeDynamicModal = function(e) {
    resizeDynamicIframe();
}

window.closeModal = function() {
    $('#dynamicModal').modal('hide');
}

window.allowCloseModal = function() {
    $('.js-close-confirmation').off('.dynamicModal').removeClass('js-close-confirmation').attr('data-dismiss', 'modal');
}


function outlinedInput() {
    // if class exists on page
    var outlinedContainer = document.getElementsByClassName('outlined-input');
    if (outlinedContainer.length > 0) {
        // loop through checkboxes in container
        for (var i = 0; i < outlinedContainer.length; i++) {
            var outlined = outlinedContainer[i];
            // find all checkboxes in container
            var checkboxes = outlined.querySelectorAll('input[type="radio"], input[type="checkbox"]');
            // loop through checkboxes
            for (var j = 0; j < checkboxes.length; j++) {
                var checkbox = checkboxes[j];
                // select checkbox parent
                var parent = checkbox.closest('.checkbox, .radio, .checkbox-inline, .radio-inline');
                // if checkbox is checked
                if (checkbox.checked) {
                    // add class to parent
                    parent.classList.add('outline-checked');
                }

                // add event listener to checkbox
                checkbox.addEventListener('change', function () {
                    var parent = this.closest('.checkbox, .radio, .checkbox-inline, .radio-inline');
                    // // if checkbox is checked
                    if (this.checked) {
                        // add class to container
                        parent.classList.add('outline-checked');
                    } else {
                        // remove class from container
                        parent.classList.remove('outline-checked');
                    }

                    // if input is a radio button
                    if (this.type === 'radio') {
                        // get all radio buttons with the same name
                        var radios = document.querySelectorAll('input[name="' + this.name + '"]');
                        // loop through radio buttons
                        for (var k = 0; k < radios.length; k++) {
                            var radio = radios[k];
                            // if radio button is not the same as the one that was clicked
                            if (radio !== this) {
                                // remove class from radio button
                                radio.closest('.checkbox, .radio, .checkbox-inline, .radio-inline').classList.remove('outline-checked');
                            }
                        }
                    }
                });
            }
        }
    }
}

function doDynamicModal(url, closeConfirmation, modalTitle, closeConfirmationMessage) {
    if (url) {
        // Append querystring to url if it doesn't already exist
        if (url.indexOf('?') === -1) {
            url += '?modal=true';
        } else {
            url += '&modal=true';
        }
        html = '<div class="modal fade" id="dynamicModal" tabindex="-1" role="dialog">';
        html += '<div class="modal-dialog" role="document">';
        html += '<div class="modal-content">';
        html += '<div class="absolute-close">';
        if (closeConfirmation && closeConfirmation === true) {
            html += '<button type="button" class="close js-close-confirmation" aria-label="Close"><i class="tvcico fa-times"></i></button>';
        } else {
            html += '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><i class="tvcico fa-times"></i></button>';
        }
        html += '</div>';
        if (modalTitle && modalTitle !== '') {
            html += '<div class="modal-header border-0">';
            html += '<h4 class="modal-title">' + modalTitle + '</h4>';
            html += '</div>';
        }
        html += '<div class="modal-body p-0">';
        html += '<div id="modalLoading" class="w-100 bg-white d-none" style="height:2000px;"></div>';
        html += '<iframe loading="eager" src="' + url + '" frameborder="0" id="dynamicModalIframe" class="tvc-modal-iframe w-100" style="display:block;opacity: 0;height:200px;" scrolling="no"></iframe>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        $('body').append(html);
        if (closeConfirmation && closeConfirmation === true) {
            $('#dynamicModal').modal({backdrop: 'static', keyboard: false}).modal('show');
        } else {
            $('#dynamicModal').modal().modal('show');
        }
        $('#dynamicModalIframe').on('load', function(e) {


            resizeDynamicIframe(this, $('#dynamicModal'));
            $(this).delay(100).animate({opacity: "1"}, 600);
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

// get iframe