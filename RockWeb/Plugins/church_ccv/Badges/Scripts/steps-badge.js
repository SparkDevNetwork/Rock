Sys.Application.add_load(function () {

    var firstName = 'Ted';
    var servingConnectionPageId = '971';
    
    var servingTeamGroupTypeId = 23;

    var connectGroupTypeId = 111;
    var connectionGroupRegistrationPage = '111';

    var groupDetailPageId = '113';
    var groupListPageId = '1010';

    /*

    remove before flight
    + change guid (search for all locations)
    + merge in the first name
    + merge in person id

    */

    $.ajax({
        type: 'GET',
        url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/StepsBar/8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4',
        statusCode: {
            200: function (data, status, xhr) {

                var $badge = $('.js-badge-group-steps.badge-id-15');

                // baptism
                if (data.BaptismResult.BaptismStatus == 2) {
                    var baptismRegistrationDate = new Date(data.BaptismResult.BaptismRegistrationDate);
                    var baptismRegistrationDateFormatted = (baptismRegistrationDate.getMonth() + 1) + '/' + baptismRegistrationDate.getDate() + '/' + baptismRegistrationDate.getFullYear();

                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' is registered to be baptised on ' + baptismRegistrationDateFormatted );
                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').addClass('step-partial');
                } else if (data.BaptismResult.BaptismStatus == 1) {
                    var baptismDate = new Date(data.BaptismResult.BaptismDate);
                    var baptismDateFormatted = (baptismDate.getMonth() + 1) + '/' + baptismDate.getDate() + '/' + baptismDate.getFullYear();

                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' was baptised on ' + baptismDateFormatted + '');
                }

                // member
                if (data.MembershipResult.IsMember) {
                    var membershipDate = new Date(data.MembershipResult.MembershipDate);
                    var membershipDateFormatted = (membershipDate.getMonth() + 1) + '/' + membershipDate.getDate() + '/' + membershipDate.getFullYear();

                    $badge.find('.badge-membership').removeClass('step-nottaken');
                    $badge.find('.badge-membership').attr('data-original-title', firstName + ' became a member on ' + membershipDateFormatted + '');
                }

                // worship
                if (data.IsWorshipper) {
                    $badge.find('.badge-worship').removeClass('step-nottaken');
                    $badge.find('.badge-worship').attr('data-toggle', 'tooltip');
                    $badge.find('.badge-worship').attr('data-original-title', firstName + ' is an eRA');
                    $badge.find('.badge-worship').attr('data-container', 'body');
                    $badge.find('.badge-worship').tooltip();
                }

                // connect
                if (data.ConnectionResult.ConnectionStatus != 1) {

                    // create content for popover
                    var popoverContent = firstName + " is in the following connection groups: <ul styling='padding-left: 20px;'>";

                    $.each(data.ConnectionResult.Groups, function (index, group) {

                        popoverContent = popoverContent + "<li><a href='/page/" + groupDetailPageId + "?GroupId=" + group.GroupId + "'>" + group.GroupName + "</a></li>";

                        // only display 2
                        if (index == 1) {
                            return false;
                        }
                    });

                    var popoverContent = popoverContent + "</ul>"

                    // check for more than two groups
                    if (data.ConnectionResult.Groups.length > 2) {
                        var moreCount = data.ConnectionResult.Groups.length - 2;

                        if (moreCount == 1) {
                            popoverContent = popoverContent + "<p>and <a href='/page/" + groupListPageId + "?PersonId=" + '4' + "&GroupTypeId=" + connectGroupTypeId + "'> " + moreCount + " other</a></p>";
                        }
                        else {
                            popoverContent = popoverContent + "<p>and <a href='/page/" + groupListPageId + "?PersonId=" + '4' + "&GroupTypeId=" + connectGroupTypeId + "'> " + moreCount + " others</a></p>";
                        }
                    }

                    var popoverContent = popoverContent + "<p class='margin-b-none'><a href='/page/" + connectionGroupRegistrationPage + "?PersonGuid=8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4' class='btn btn-primary btn-block btn-xs'>Find Group</a></p>";

                    $badge.find('.badge-connect').removeClass('step-nottaken');
                    $badge.find('.badge-connect').attr('data-toggle', 'popover');
                    $badge.find('.badge-connect').attr('data-container', 'body');
                    $badge.find('.badge-connect').attr('data-content', popoverContent);
                    $badge.find('.badge-connect').attr('data-original-title', firstName + ' is in a connection group &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');

                    var servingPopoverIsOpen = false;

                    $badge.find('.badge-connect').popover({
                        html: true,
                        placement: 'top',
                        trigger: 'manual'
                    });

                    // disable the anchor tag
                    $badge.find('.badge-connect').on("click", function (e) {
                        e.preventDefault();
                    });


                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find('.badge-connect').on('click', function () {
                        if (servingPopoverIsOpen) {
                            $badge.find('.badge-connect').popover('hide');
                            servingPopoverIsOpen = false;
                            $badge.find('.badge-connect').attr('data-original-title', firstName + ' is in a connection group &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');
                        }
                        else {
                            $badge.find('.badge-connect').attr('data-original-title', '');
                            $badge.find('.badge-connect').popover('show');
                            servingPopoverIsOpen = true;
                            $badge.find('.badge-connect').tooltip('hide');
                        }
                    });
                }

                // tithing
                if (data.IsTithing) {
                    $badge.find('.badge-tithe').removeClass('step-nottaken');
                }

                // serving
                if (data.ServingResult.IsServing) {

                    // create content for popover
                    var popoverContent = firstName + " is on the following serving teams: <ul styling='padding-left: 20px;'>";

                    $.each(data.ServingResult.Groups, function (index, group) {
                        
                        popoverContent = popoverContent + "<li><a href='/page/" + groupDetailPageId + "?GroupId=" + group.GroupId + "'>" + group.GroupName + "</a></li>";

                        // only display 2
                        if (index == 1) {
                            return false;
                        }
                    });

                    var popoverContent = popoverContent + "</ul>"

                    // check for more than two groups
                    if (data.ServingResult.Groups.length > 2) {
                        var moreCount = data.ServingResult.Groups.length - 2;

                        if (moreCount == 1) {
                            popoverContent = popoverContent + "<p>and <a href='/page/" + groupListPageId + "?PersonId=" + '4' + "&GroupTypeId=" + servingTeamGroupTypeId + "'> " + moreCount + " other</a></p>";
                        }
                        else {
                            popoverContent = popoverContent + "<p>and <a href='/page/" + groupListPageId + "?PersonId=" + '4' + "&GroupTypeId=" + servingTeamGroupTypeId + "'> " + moreCount + " others</a></p>";
                        }
                    }

                    var popoverContent = popoverContent + "<p class='margin-b-none'><a href='/page/" + servingConnectionPageId + "?PersonGuid=8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4' class='btn btn-primary btn-block btn-xs'>Connect</a></p>";

                    $badge.find('.badge-serve').removeClass('step-nottaken');
                    $badge.find('.badge-serve').attr('data-toggle', 'popover');
                    $badge.find('.badge-serve').attr('data-container', 'body');
                    $badge.find('.badge-serve').attr('data-content', popoverContent);
                    $badge.find('.badge-serve').attr('data-original-title', firstName + ' is serving &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');

                    var servingPopoverIsOpen = false;

                    $badge.find('.badge-serve').popover({
                        html: true,
                        placement: 'top',
                        trigger: 'manual'
                    });
                    
                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find('.badge-serve').on('click', function () {
                        if (servingPopoverIsOpen) {
                            $badge.find('.badge-serve').popover('hide');
                            servingPopoverIsOpen = false;
                            $badge.find('.badge-serve').attr('data-original-title', firstName + ' is serving &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');
                        }
                        else {
                            $badge.find('.badge-serve').attr('data-original-title', '');
                            $badge.find('.badge-serve').popover('show');
                            servingPopoverIsOpen = true;
                            $badge.find('.badge-serve').tooltip('hide');
                        }
                    });
                }

                

                // sharing
                // not implemented yet

                // coaching
                if (data.CoachingResult.IsCoaching) {

                    // create content for popover
                    var popoverContent = firstName + " is in the following coaching groups: <ul styling='padding-left: 20px;'>";

                    $.each(data.CoachingResult.Groups, function (index, group) {

                        popoverContent = popoverContent + "<li><a href='/page/" + groupDetailPageId + "?GroupId=" + group.GroupId + "'>" + group.GroupName + "</a></li>";

                        // only display 2
                        if (index == 1) {
                            return false;
                        }
                    });

                    var popoverContent = popoverContent + "</ul>"

                    // check for more than two groups
                    if (data.CoachingResult.Groups.length > 2) {
                        var moreCount = data.CoachingResult.Groups.length - 2;

                        if (moreCount == 1) {
                            popoverContent = popoverContent + "<p>and " + moreCount + " other (see groups tab for details)</p>";
                        }
                        else {
                            popoverContent = popoverContent + "<p>and " + moreCount + " others (see groups tab for details)</p>";
                        }
                    }

                    $badge.find('.badge-coach').removeClass('step-nottaken');
                    $badge.find('.badge-coach').attr('data-toggle', 'popover');
                    $badge.find('.badge-coach').attr('data-container', 'body');
                    $badge.find('.badge-coach').attr('data-content', popoverContent);
                    $badge.find('.badge-coach').attr('data-original-title', firstName + ' is in a coaching group &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');

                    var servingPopoverIsOpen = false;

                    $badge.find('.badge-coach').popover({
                        html: true,
                        placement: 'top',
                        trigger: 'manual'
                    });

                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find('.badge-coach').on('click', function () {
                        if (servingPopoverIsOpen) {
                            $badge.find('.badge-coach').popover('hide');
                            servingPopoverIsOpen = false;
                            $badge.find('.badge-coach').attr('data-original-title', firstName + ' is in a coaching group &nbsp;&nbsp;<i class="fa fa-mouse-pointer"></i>');
                        }
                        else {
                            $badge.find('.badge-coach').attr('data-original-title', '');
                            $badge.find('.badge-coach').popover('show');
                            servingPopoverIsOpen = true;
                            $badge.find('.badge-coach').tooltip('hide');
                        }
                    });
                }
            }
        },
    });
});