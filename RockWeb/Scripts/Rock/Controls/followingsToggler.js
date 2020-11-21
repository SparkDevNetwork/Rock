(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.followingsToggler = (function () {

        var exports = {
            // initialize a script that will set the following status via REST
            // entityTypeId is the EntityType, and entityId is the .Id for the associated entity
            // personId and personAliasId are the person that is doing the following/un-following
            // callback is optional and called on follow or unfollow events with a param indicating
            // if the person is now following
            initialize: function ($followingDiv, entityTypeId, entityId, personId, personAliasId, callback) {
                var hasCallback = typeof callback === 'function';

                $followingDiv.on('click', function () {
                    if ($followingDiv.hasClass('following')) {

                        $.ajax({
                            type: 'DELETE',
                            url: Rock.settings.get('baseUrl') + 'api/followings/' + entityTypeId + '/' + entityId + '/' + personId,
                            success: function (data, status, xhr) {
                                $followingDiv.removeClass('following');

                                // update the tooltip (if one was configured)
                                if ($followingDiv.attr('data-original-title')) {
                                    $followingDiv.attr('data-original-title', 'Click to follow');
                                }

                                if (hasCallback) {
                                    callback(false, $followingDiv, entityTypeId, entityId, personId, personAliasId);
                                }
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

                                    // update the tooltip (if one was configured)
                                    if ($followingDiv.attr('data-original-title')) {
                                        $followingDiv.attr('data-original-title', 'Currently following');
                                    }

                                    if (hasCallback) {
                                        callback(true, $followingDiv, entityTypeId, entityId, personId, personAliasId);
                                    }
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
