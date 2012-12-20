var $moveLink;

function saveBlockMove() {

    // The current block's id
    var blockId = $('#modal-block-move_panel').attr('block-instance');

    // The new zone selected
    var zoneName = $('#block-move-zone').val();

    // Get the current block instance object
    $.ajax({
        type: 'GET',
        contentType: 'application/json',
        dataType: 'json',
        url: rock.baseUrl + 'api/blocks/' + blockId,
        success: function (getData, status, xhr) {

            // Update the new zone
            getData.Zone = zoneName;

            // Set the appropriate parent value (layout or page)
            if ($('#block-move-Location_0').attr('checked') == 'checked') {
                getData.Layout = null;
                getData.PageId = rock.pageId;
            }
            else {
                getData.Layout = rock.layout;
                getData.PageId = null;
            }

            // Save the updated block instance
            $.ajax({
                type: 'PUT',
                contentType: 'application/json',
                dataType: 'json',
                data: JSON.stringify(getData),
                url: rock.baseUrl + 'api/blocks/move/' + blockId,
                success: function (data, status, xhr) {

                    // Get a reference to the block instance's container div
                    var $source = $('#bid_' + blockId);

                    // Get a reference to the new zone's container
                    var $target = $('#zone-' + $('#block-move-zone').val());

                    // Update the move anchor with the new zone name
                    $moveLink.attr('zone', $('#block-move-zone').val());

                    // If the block instance's parent is the page, move it to the new zone as the last
                    // block in that zone.  If the parent is the layout, insert it as the last layout
                    // block (prior to any page block's
                    if ($('#block-move-Location_0').attr('checked') == true) {
                        $target.append($source);
                        $moveLink.attr('zoneloc', 'Page');
                        $source.attr('zoneLoc', 'Page');
                    }
                    else {
                        if ($('#' + $target.attr('id') + '>[zoneLoc="Layout"]').length > 0)
                            $source.insertAfter($('#' + $target.attr('id') + '>[zoneLoc="Layout"]:last'));
                        else
                            $target.append($source);
                        $moveLink.attr('zoneloc', 'Layout');
                        $source.attr('zoneLoc', 'Layout');
                    }

                },
                error: function (xhr, status, error) {
                    alert('PUT ' + status + ' [' + error + ']: ' + xhr.responseText);
                }
            });
        },
        error: function (xhr, status, error) {
            alert('GET ' + status + ' [' + error + ']: ' + xhr.responseText);
        }
    });

}

$(document).ready(function () {

    // Bind the click event of the block move anchor tag
    $('a.block-move').click(function () {

        // Get a reference to the anchor tag for use in the dialog success function
        $moveLink = $(this);

        // Add the current block's id as an attribute of the move dialog's save button
        $('#modal-block-move_panel').attr('block-instance', $(this).attr('href'));

        // Set the dialog's zone selection select box value to the block's current zone 
        $('#block-move-zone').val($(this).attr('zone'));

        // Set the dialog's parent option to the current zone's parent (either the page or the layout)
        if ($(this).attr('zoneloc') == 'Page') {
            $('#block-move-Location_1').removeAttr('checked');
            $('#block-move-Location_0').attr('checked', 'checked');
        }
        else {
            $('#block-move-Location_0').removeAttr('checked');
            $('#block-move-Location_1').attr('checked', 'checked');
        }

        // Show the popup block move dialog
        $find('modal-block-move').show();

        return false;

    });


    // Bind the block instance delete anchor
    $('a.block-delete').click(function () {

        if (confirm('Are you sure you want to delete this block?')) {

            var blockId = $(this).attr('href');

            // delete the block instance
            $.ajax({
                type: 'DELETE',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'api/blocks/' + blockId,
                success: function (data, status, xhr) {

                    // Remove the block instance's container div
                    $('#bid_' + blockId).remove();

                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });

        }

        // Cancel the default action of the delete anchor tag
        return false;

    });

    // Bind the page's block config anchor to toggle the display
    // of each block's container and config options
    $('#cms-admin-footer .block-config').click(function () {
        $('.zone-configuration').hide();
        $('.zone-instance').removeClass('outline');
        $('.block-configuration').toggle();
        $('.block-instance').toggleClass('outline');
        return false;
    });

    // Bind the page's zone config anchor to toggle the display
    // of each zone's container and config options
    $('#cms-admin-footer .page-zones').click(function () {
        $('.block-configuration').hide();
        $('.block-instance').removeClass('outline');
        $('.zone-instance').toggleClass('outline');
        $('.zone-configuration').toggle();
        return false;
    });

    // Bind the block configure icon so that edit icons are displayed on hover
    $(".block-configuration").hover(function () {
        var barWidth = $('.block-configuration-bar', this).outerWidth() + 45 + 'px';
        $(this).stop().animate({ width: barWidth }, 200).css({ 'z-index': '10' });
    }, function () {
        $(this).stop().animate({ width: '24px' }, 200).css({ 'z-index': '1' });
    });

    // Bind the zone configure icon so that edit icons are displayed on hover
    $(".zone-configuration").hover(function () {
        var barWidth = $('.zone-configuration-bar', this).width() + 45 + 'px';
        $(this).stop().animate({ width: barWidth }, 200).css({ 'z-index': '10' });
    }, function () {
        $(this).stop().animate({ width: '24px' }, 200).css({ 'z-index': '1' });
    });
});
