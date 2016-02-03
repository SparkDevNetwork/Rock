Sys.Application.add_load(function () {

    var firstName = 'Ted';

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

                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' is registered to be baptised on ' + baptismRegistrationDateFormatted + ' .');
                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').addClass('step-partial');
                } else if (data.BaptismResult.BaptismStatus == 1) {
                    var baptismDate = new Date(data.BaptismResult.BaptismDate);
                    var baptismDateFormatted = (baptismDate.getMonth() + 1) + '/' + baptismDate.getDate() + '/' + baptismDate.getFullYear();

                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' was baptised on ' + baptismDateFormatted + '.');
                }

                // member
                if (data.MembershipResult.IsMember) {
                    var membershipDate = new Date(data.MembershipResult.MembershipDate);
                    var membershipDateFormatted = (membershipDate.getMonth() + 1) + '/' + membershipDate.getDate() + '/' + membershipDate.getFullYear();

                    $badge.find('.badge-membership').removeClass('step-nottaken');
                    $badge.find('.badge-membership').attr('data-original-title', firstName + ' became a member on ' + membershipDateFormatted + '.');
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

                // tithing
                if (data.IsTithing) {
                    $badge.find('.badge-tithe').removeClass('step-nottaken');
                }

                // serving

                // sharing
                // not implemented yet

                // coaching
            }
        },
    });
});