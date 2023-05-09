
// jquery ready
var postbackCount = 0;

function scrollInteractions() {
    var fromTop = $(window).scrollTop();
    if (fromTop > 400) {
        $("body").addClass("scrolled-down").removeClass("scrolled-up");
    } else {
        if ($("body").hasClass("scrolled-down")) {
            $("body").addClass("scrolled-up");
            $("body").removeClass("scrolled-down");
        }
    }
}

function navInteractions() {
    if (window.innerWidth > 992) {
        $('.navbar-nav .dropdown').each(function () {
            if ($(this).find('a[data-bs-toggle]').length > 0) {
                // this on mouseover
                let el_link = $(this).find('a[data-bs-toggle]');
                el_link.off('.nav');
                $(this).off('.nav').on('mouseover.nav', function (e) {
                    el_link.addClass('active').next().addClass('show');
                });
                // this on mouseleave
                $(this).on('mouseleave.nav', function (e) {
                    el_link.removeClass('active').next().removeClass('show');
                });
            }
        });
    } else {
        $('.navbar-nav .dropdown').each(function () {
            $(this).off('.nav').find('a[data-bs-toggle]').off('.nav').on('click.nav', function (e) {
                e.preventDefault();
                // if next element is dropdown menu
                if ( $(this).next().hasClass('dropdown-menu') ) {
                    // slide toggle any dropdowns that were open
                    $('.navbar-nav .dropdown .dropdown-menu').not($(this).next()).slideUp().closest('.dropdown').removeClass('dropdown-open');
                    // slide toggle current dropdown
                    $(this).next().slideToggle().closest('.dropdown').toggleClass('dropdown-open');
                }
            });
        });
    }
}

function serviceCountdown() {
    if (document.getElementById('serviceCountdown')) {
        var second = 1000,
        minute = second * 60,
        hour = minute * 60,
        day = hour * 24;
        var serviceCountdown = document.getElementById('serviceCountdown');
        var serviceCountdownTime = serviceCountdown.getAttribute('data-countdown-time');
        
        if (serviceCountdownTime !== null) {
            // get data-countdown-time attribute from #serviceCountdown
            var countDown = new Date(serviceCountdownTime).getTime();
            var x = setInterval(function() {
                var now = new Date().getTime();
                var distance = countDown - now;

                // if distance is 60 minutes or less, show the countdown in minutes
                if (distance < 0) {
                    clearInterval(x);
                    document.getElementById('countdown-wrapper').innerText = serviceCountdown.getAttribute('data-live-text');
                    // if watchnow-livebtn exists, replace href with live stream url
                    var watchNowLiveBtn = document.getElementById('watchnow-livebtn');
                    if (watchNowLiveBtn) {
                        watchNowLiveBtn.href = watchNowLiveBtn.getAttribute('data-live-url');
                        watchNowLiveBtn.innerHTML = `<span class="label label-danger">New</span> Watch Now!`;
                    }
                } else if (distance <= 60 * minute) {
                    // add class live to serviceCountdown
                    serviceCountdown.classList.add('live');
                    document.getElementById('olm').innerText = ('0' + Math.floor((distance % (hour)) / (minute)) + 'm').substr(-3),
                    document.getElementById('ols').innerText = ('0' + Math.floor((distance % (minute)) / second) + 's').substr(-3);
                }
            }, second);
        } else {
            var watchNowLiveBtn = document.getElementById('watchnow-livebtn');
            if (watchNowLiveBtn) {
                watchNowLiveBtn.href = watchNowLiveBtn.getAttribute('data-live-url');
                watchNowLiveBtn.innerHTML = `<span class="label label-danger">LIVE</span> Watch Now!`;
            }
        }
    };
}

// DOMContentLoaded end
document.addEventListener('DOMContentLoaded', function () {
    Sys.Application.add_load(function () {
        // increment postback
        postbackCount++;

        // duplicate .navbar-static-top and add .clone class
        $('#site-nav').clone().insertAfter('#site-nav').addClass('clone').removeAttr('id').find('.navbar-collapse').removeAttr('id');
        navInteractions();
        $(window).on('resize', function () {
            navInteractions();
        });

        scrollInteractions();
        $(window).on("scroll", function () {
            scrollInteractions();
        });

        serviceCountdown();

        // if .js-alertbar
        if ($('.js-alertbar').length) {
            $(".js-alertbar").each(function () {
                var notificationbarId = "#" + this.id;
                var notificationbarClose = $(".js-alertbar-close");
                var notificationbarName = this.getAttribute("data-alertbar-name");
                var notificationbarValue = this.getAttribute("data-alertbar-value");
                var alertCookie = Cookies.get(notificationbarName)

                if (alertCookie) {
                    $(' .js-alertbar').hide();
                } else {
                    $(notificationbarClose).on("click", function () {
                        var expireIn = 2 / 48; Cookies.set(notificationbarName, notificationbarValue, {
                            expires: expireIn
                        });
                        $(notificationbarId).removeClass("js-active").addClass("d-none");
                    })
                }
            });
            $('.js-alertbar-close').on('click', function () {
                var alertHeight = $(".alertbar").height();
                $('.js-alertbar').animate({ 'marginTop': alertHeight * -1 }, 800);
                $('body').css("margin-top", "0");
            });
        }

        $('.js-launch-modal').click(function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            var modalTitle = $(this).data('modal-title');
            var modalConfirm = $(this).data('modal-close-confirmation');
            var closeMessage = $(this).data('modal-close-message');
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

        $('.navbar-collapse').on('hidden.bs.collapse', function (e) {
            $(e.target).find('.dropdown-menu').removeAttr('style');
            $(e.target).find('.dropdown').removeClass('dropdown-open');
        });

        $('.js-collapse-nav').on('click', function (e) {
            $('#main-navcol').collapse('hide');
        });


        // copyUrl are items with the class .btn-copy-url
        var copyUrl = document.querySelector('.btn-copy-url');
        if (copyUrl) {
            // add click event listener to copyUrl
            copyUrl.addEventListener('click', function(e) {
                e.preventDefault();
                var url = window.location.href;
                var textArea = document.createElement("textarea");
                textArea.value = url;
                document.body.appendChild(textArea);
                textArea.select();
                    try {
                        var successful = document.execCommand('copy');
                        var msg = successful ? 'successful' : 'unsuccessful';
                        console.log('Copying text command was ' + msg);
                        textArea.remove();
                        // if successful, update the button tooltip to show success
                        if (successful) {
                            copyUrl.setAttribute('data-original-title', 'Copied!');
                            copyUrl.setAttribute('data-placement', 'top');
                            copyUrl.setAttribute('data-toggle', 'tooltip');
                            copyUrl.setAttribute('title', 'Copied!');
                            copyUrl.setAttribute('data-trigger', 'manual');
                            $(copyUrl).tooltip('show');
                            setTimeout(function() {
                                $(copyUrl).tooltip('hide');
                                copyUrl.setAttribute('data-original-title', 'Copy URL');
                                copyUrl.setAttribute('title', 'Copy URL');
                                copyUrl.setAttribute('data-trigger', 'hover');
                            }, 1500);
                        }
                    } catch (err) {
                        console.log('Oops, unable to copy');
                        textArea.remove();
                    }
                }
            ); 
        }

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
        var html = '<div class="modal fade" id="dynamicModal" tabindex="-1" role="dialog">';
        html += '<div class="modal-dialog" role="document">';
        html += '<div class="modal-content">';
        html += '<div class="absolute-close">';
        if (closeConfirmation && closeConfirmation === true) {
            html += '<button type="button" class="close js-close-confirmation" aria-label="Close"><i class="fa fa-times"></i></button>';
        } else {
            html += '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><i class="fa fa-times"></i></button>';
        }
        html += '</div>';
        if (modalTitle && modalTitle !== '') {
            html += '<div class="modal-header border-0"><h4 class="modal-title">' + modalTitle + '</h4></div>';
        }
        html += '<div class="modal-body p-0">';
        html += '<div id="modalLoading" class="w-100 bg-white d-none" style="height:2000px;"></div>';
        html += '<iframe loading="eager" src="' + url + '" frameborder="0" id="dynamicModalIframe" class="lcbc-modal-iframe w-100" style="display:block;opacity: 0;height:200px;" scrolling="no"></iframe>';
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
            resizeDynamicIframe(this, $('#dynamicModal'));
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
    console.log(el.contentWindow.document.documentElement.offsetHeight + 'px');
    el.style.height = el.contentWindow.document.documentElement.offsetHeight + 'px';

    var modalOverflow = $(window).height() - 10 < $('#dynamicModal').height();
    console.log(modalOverflow);
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

/*! js-cookie v3.0.1 | MIT */
!function(e,t){"object"==typeof exports&&"undefined"!=typeof module?module.exports=t():"function"==typeof define&&define.amd?define(t):(e=e||self,function(){var n=e.Cookies,o=e.Cookies=t();o.noConflict=function(){return e.Cookies=n,o}}())}(this,(function(){"use strict";function e(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var o in n)e[o]=n[o]}return e}return function t(n,o){function r(t,r,i){if("undefined"!=typeof document){"number"==typeof(i=e({},o,i)).expires&&(i.expires=new Date(Date.now()+864e5*i.expires)),i.expires&&(i.expires=i.expires.toUTCString()),t=encodeURIComponent(t).replace(/%(2[346B]|5E|60|7C)/g,decodeURIComponent).replace(/[()]/g,escape);var c="";for(var u in i)i[u]&&(c+="; "+u,!0!==i[u]&&(c+="="+i[u].split(";")[0]));return document.cookie=t+"="+n.write(r,t)+c}}return Object.create({set:r,get:function(e){if("undefined"!=typeof document&&(!arguments.length||e)){for(var t=document.cookie?document.cookie.split("; "):[],o={},r=0;r<t.length;r++){var i=t[r].split("="),c=i.slice(1).join("=");try{var u=decodeURIComponent(i[0]);if(o[u]=n.read(c,u),e===u)break}catch(e){}}return e?o[e]:o}},remove:function(t,n){r(t,"",e({},n,{expires:-1}))},withAttributes:function(n){return t(this.converter,e({},this.attributes,n))},withConverter:function(n){return t(e({},this.converter,n),this.attributes)}},{attributes:{value:Object.freeze(o)},converter:{value:Object.freeze(n)}})}({read:function(e){return'"'===e[0]&&(e=e.slice(1,-1)),e.replace(/(%[\dA-F]{2})+/gi,decodeURIComponent)},write:function(e){return encodeURIComponent(e).replace(/%(2[346BF]|3[AC-F]|40|5[BDE]|60|7[BCD])/g,decodeURIComponent)}},{path:"/"})}));