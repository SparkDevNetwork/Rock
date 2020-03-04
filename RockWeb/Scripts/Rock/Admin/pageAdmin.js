// if the admin footer is rendered, watch for alt keypresses for page config, block config, etc
Sys.Application.add_load(function () {
    if ($('.js-cms-admin-footer').length) {
        $(window).on('keydown', function (event) {
            if (event.altKey) {
                if (event.keyCode == 80) {
                    //'Alt-P' page config
                    window.location = $('.js-page-properties').attr('href');
                } else if (event.keyCode == 66) {
                    // Alt-B block config
                    window.location = $('.js-block-config').attr('href');
                } else if (event.keyCode == 76) {
                    // Alt-L child pages
                    window.location = $('.js-page-child-pages').attr('href');
                } else if (event.keyCode == 90) {
                    // Alt-Z page zones
                    window.location = $('.js-page-zones').attr('href');
                }
            }
        });
    }
});


(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.admin = Rock.admin || {};

    Rock.admin.pageAdmin = (function () {
        var $moveLink,
            _saveBlockMove = function () {
                // The current block's id
                var blockId = $moveLink.attr('href');

                // The new zone selected
                var zoneName = $('#block-move-zone').val();

                // Get the current block instance object
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    dataType: 'json',
                    url: Rock.settings.get('baseUrl') + 'api/blocks/' + blockId,
                    success: function (getData, status, xhr) {

                        // Update the new zone
                        getData.Zone = zoneName;
                        var blockLocation = $('#block-move-Location :checked').val()

                        // Set the appropriate parent value (site, layout or page)
                        switch (blockLocation) {
                            case "Site":
                                getData.SiteId = Rock.settings.get('siteId');
                                getData.LayoutId = null;
                                getData.PageId = null;
                                break;
                            case "Layout":
                                getData.SiteId = null;
                                getData.LayoutId = Rock.settings.get('layoutId');
                                getData.PageId = null;
                                break;
                            case "Page":
                                getData.SiteId = null;
                                getData.LayoutId = null;
                                getData.PageId = Rock.settings.get('pageId');
                                break;
                        }

                        // Save the updated block instance
                        $.ajax({
                            type: 'PUT',
                            contentType: 'application/json',
                            dataType: 'json',
                            data: JSON.stringify(getData),
                            url: Rock.settings.get('baseUrl') + 'api/blocks/move/' + blockId,
                            success: function (data, status, xhr) {

                                // Get a reference to the block instance's container div
                                var $source = $('#bid_' + blockId);

                                // Get a reference to the new zone's container
                                var $target = $('#zone-' + zoneName.toLowerCase());

                                // Update the move anchor with the new zone name
                                $moveLink.attr('data-zone', zoneName);

                                // If the block instance's parent is the page, move it to the new zone as the last
                                // block in that zone.  If the parent is the layout, insert it as the last layout
                                // block (prior to any page block's
                                if ($('#block-move-Location_0').prop('checked')) {
                                    $target.append($source);
                                    $moveLink.attr('data-zone-location', 'Page');
                                    $source.attr('data-zone-location', 'Page');
                                }
                                else {
                                    if ($('#' + $target.attr('id') + '>[data-zone-location="Layout"]').length > 0)
                                        $source.insertAfter($('#' + $target.attr('id') + '>[data-zone-location="Layout"]').last());
                                    else
                                        $target.append($source);
                                    $moveLink.attr('data-zone-location', 'Layout');
                                    $source.attr('data-zone-location', 'Layout');
                                }

                            },
                            error: function (xhr, status, error) {
                                alert(status + ' [' + error + ']: ' + xhr.responseText);
                            }
                        });
                    },
                    error: function (xhr, status, error) {
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }
                });
            },

            // toggle the display of each block's container and config options
            _showBlockConfig = function () {
                $('.zone-configuration').hide();
                $('.zone-instance').removeClass('outline');
                $('body').removeClass('zone-highlight');
                $('.block-configuration').toggle();
                $('.block-instance').toggleClass('outline');
                $('body').toggleClass('block-highlight');

                // Bind the block configure icon so that edit icons are displayed on hover
                $(".block-configuration").on("mouseenter", function (e) {
                    var barWidth = $('.block-configuration-bar', this).outerWidth() + 45 + 'px';
                    $(this).stop(true, true).animate({ width: barWidth }, 200).css({ 'z-index': '1049' });
                }).on("mouseleave", function () {
                    $(this).stop(true, true).delay(500).animate({ width: '26px' }, 500).css({ 'z-index': '1000' });
                });

                // Bind the block instance delete anchor (ensure it is only bound once)
                $('a.block-delete').off('click').on('click', function (a, b, c) {
                    var blockId = $(this).attr('href');

                    Rock.dialogs.confirm('Are you sure you want to delete this block?', function (result) {

                        if (result) {

                            // delete the block instance
                            $.ajax({
                                type: 'DELETE',
                                contentType: 'application/json',
                                dataType: 'json',
                                url: Rock.settings.get('baseUrl') + 'api/blocks/' + blockId,
                                success: function (data, status, xhr) {

                                    // Remove the block instance's container div
                                    $('#bid_' + blockId).remove();

                                },
                                error: function (xhr, status, error) {
                                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                                }
                            });
                        }

                    });

                    // Cancel the default action of the delete anchor tag
                    return false;

                });

                // Bind the click event of the block move anchor tag
                $('a.block-move').off('click').on('click', function () {

                    // Get a reference to the anchor tag for use in the dialog success function
                    $moveLink = $(this);

                    // Set the dialog's zone selection select box value to the block's current zone
                    $('#block-move-zone').val($(this).attr('data-zone'));

                    // Set the dialog's parent option to the current zone's parent (either the page or the layout)
                    var blockLocation = $(this).attr('data-zone-location');
                    $('#block-move-Location input[value=' + blockLocation + ']').prop('checked', true);

                    $('.js-modal-block-move .js-subtitle').text($(this).attr('data-blockname'));

                    // Show the popup block move dialog
                    $('.js-modal-block-move .modal').modal('show');

                    return false;

                });
            },
            _showPageZones = function () {
                $('.block-configuration').hide();
                $('.block-instance').removeClass('outline');
                $('body').removeClass('block-highlight');
                $('.zone-instance').toggleClass('outline');
                $('body').toggleClass('zone-highlight');
                $('.zone-configuration').toggle();

                // Bind the zone configure icon so that edit icons are displayed on hover
                $(".zone-configuration").on("mouseenter", function () {
                    var barWidth = $('.zone-configuration-bar', this).width() + 45 + 'px';
                    $(this).stop(true, true).animate({ width: barWidth }, 200).css({ 'z-index': '1049' });
                }).on("mouseleave", function () {
                    $(this).stop(true, true).delay(500).animate({ width: '26px' }, 500).css({ 'z-index': '1000' });
                });
            },
            exports = {
                saveBlockMove: function () {
                    _saveBlockMove();
                },
                showBlockConfig: function () {
                    _showBlockConfig();
                },
                showPageZones: function () {
                    _showPageZones();
                }
            };

        return exports;
    }());
}(jQuery));
