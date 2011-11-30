$(document).ready(function () {

    $('#divZoneSelect').dialog({
        title: 'Move Block To',
        autoOpen: false,
        width: 290,
        height: 300,
        modal: true
    })

    $('#modalDiv').dialog({
        autoOpen: false,
        width: 580,
        height: 600,
        modal: true
    })

    $('a.show-iframe-dialog').click(function () {

        $('#modalIFrame').attr('src', $(this).attr('href'));

        $('#modalDiv').bind('dialogclose', function (event, ui) {
            if ($(this).attr('instance-id') != undefined)
                $('#blck-cnfg-trggr-' + $(this).attr('instance-id')).click();
            $('#modalDiv').unbind('dialogclose');
            $('#modalIFrame').attr('src', '');
        });

        if ($(this).attr('title') != undefined)
            $('#modalDiv').dialog('option', 'title', $(this).attr('title'));

        $('#modalDiv').dialog('open');

        return false;

    });

    $('div.zone-instance').sortable({
        appendTo: 'body',
        connectWith: 'div.zone-instance',
        handle: 'a.block-move',
        opacity: 0.6,
        start: function (event, ui) {
            var start_pos = ui.item.index();
            ui.item.data('start_pos', start_pos);
            $('div.zone-instance').addClass('outline');
        },
        stop: function (event, ui) {
            $('div.zone-instance').removeClass('outline');
        }
    }).disableSelection();

    $('a.blockinstance-move').click(function () {

        var $moveLink = $(this);

        $('#btnSaveZoneSelect').attr('blockInstance', $(this).attr('href'));
        $('#ddlZones').val($(this).attr('zone'));
        if ($(this).attr('zoneloc') == 'Page') {
            $('#rblLocation_1').removeAttr('checked');
            $('#rblLocation_0').attr('checked', 'checked');
        }
        else {
            $('#rblLocation_0').removeAttr('checked');
            $('#rblLocation_1').attr('checked', 'checked');
        }

        $('#divZoneSelect').dialog('open');

        $('#btnSaveZoneSelect').click(function () {

            $('#divZoneSelect').dialog('close');

            var blockInstanceId = $(this).attr('blockinstance');
            var zoneName = $('#ddlZones').val();

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: 'http://localhost:6229/RockWeb/api/Cms/BlockInstance/' + blockInstanceId,
                success: function (getData, status, xhr) {

                    getData.Zone = zoneName;
                
                    if ($('#rblLocation_0').attr('checked') == true) {
                        getData.Layout = null;
                        getData.PageId = rock.pageId;
                    }
                    else {
                        getData.Layout = rock.layout;
                        getData.PageId = null;
                    }

                    $.ajax({
                        type: 'PUT',
                        contentType: 'application/json',
                        dataType: 'json',
                        data: JSON.stringify( getData ),
                        url: 'http://localhost:6229/RockWeb/api/Cms/BlockInstance/Move/' + blockInstanceId,
                        success: function (data, status, xhr) {

                            var $source = $('#bid_' + blockInstanceId);
                            var $target = $('#zone-' + $('#ddlZones').val());

                            $moveLink.attr('zone', $('#ddlZones').val());

                            if ($('#rblLocation_0').attr('checked') == true) {
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
                            alert(status + ' ' + error + ' ' + xhr.responseText);
                        }
                    });
                },
                error: function (xhr, status, error) {
                    alert(status + ' ' + error + ' ' + xhr.responseText);
                }
            });

            $(this).unbind('click');

        });

        return false;
    });


    $('a.blockinstance-delete').click(function () {
        var elementId = $(this).attr('href');
        if (confirm('Are you sure? (element: ' + elementId + ')')) {
            alert('block instance delete logic goes here!');
        }
        return false;
    });

    $('#cms-admin-footer .block-config').click(function () {
        $('.block-configuration').toggle();
        $('.block-instance').toggleClass('outline');
        return false;
    });

    $('#cms-admin-footer .page-zones').click(function () {
        $('.zone-configuration').toggle();
        $('.zone-instance').toggleClass('outline');
        return false;
    });

});
