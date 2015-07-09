(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.followingsToggler = (function () {

        var exports = {
            // initialize a script that will set the following status via REST
            // entityTypeId is the EntityType, and entityId is the .Id for the associated entity
            // personId and personAliasId are the person that is doing the following/un-following
            initialize: function ($followingDiv, entityTypeId, entityId, personId, personAliasId) {
                $followingDiv.click(function () {
                    if ($followingDiv.hasClass('following')) {

                        $.ajax({
                            type: 'DELETE',
                            url: Rock.settings.get('baseUrl') + 'api/followings/' + entityTypeId + '/' + entityId + '/' + personId,
                            success: function (data, status, xhr) {
                                $followingDiv.removeClass('following');
                                $followingDiv.attr("data-original-title", 'Click to follow.');
                            },
                        });

                    } else {
                        var following = {
                            EntityTypeId: entityTypeId,
                            EntityId: entityId,
                            PersonAliasId: personAliasId
                        };

                        $.ajax({
                            type: 'POST',
                            contentType: 'application/json',
                            data: JSON.stringify(following),
                            url: Rock.settings.get('baseUrl') + 'api/followings',
                            statusCode: {
                                201: function () {
                                    $followingDiv.addClass('following');
                                    $followingDiv.attr("data-original-title", 'following');
                                }
                            }
                        });
                    }
                })
            }
        };

        return exports;
    }());
}(jQuery));