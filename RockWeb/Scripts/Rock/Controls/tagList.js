(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.tagList = (function () {
        var TagList = function (options) {
                this.controlId = options.controlId;
                this.entityTypeId = options.entityTypeId;
                this.currentPersonId = options.currentPersonId;
                this.entityGuid = options.entityGuid;
                this.entityQualifierColumn = options.entityQualifierColumn;
                this.entityQualifierValue = options.entityQualifierValue;
                this.preventTagCreation = options.preventTagCreation;
                this.delaySave = options.delaySave;
            },
            exports;

        TagList.prototype.verifyTag = function (tagName) {
            // Since `verifyTag` is being called by the jQuery lib, `this`
            // is the current TagList DOM element. Get the HTML ID and fetch
            // from the cache.
            var tagList = exports.tagLists[$(this).attr('id')],
                restUrl = Rock.settings.get('baseUrl') + 'api/tags';
            restUrl += '/' + tagList.entityTypeId;
            restUrl += '/' + tagList.currentPersonId;
            
            if (tagList.entityQualifierColumn) {
                restUrl += '/' + tagList.entityQualifierColumn;
            }

            if (tagList.entityQualifierValue) {
                restUrl += '/' + tagList.entityQualifierValue;
            }

            restUrl += '?name=' + encodeURIComponent(tagName);


            $.ajax({
                url: restUrl,
                statusCode: {
                    404: function () {
                        if (tagList.preventTagCreation) {
                            $('#' + tagList.controlId).removeTag(tagName);
                        }
                        else {
                            Rock.dialogs.confirm('A tag called "' + $('<div/>').text(tagName).html() + '" does not exist. Do you want to create a new personal tag?', function (result) {
                                if (result) {
                                    tagList.addTag(tagName);
                                } else {
                                    $('#' + tagList.controlId).removeTag(tagName);
                                }
                            });
                        }
                    },
                    200: function () {
                        tagList.addTag(tagName);
                    }
                }
            });
        };

        TagList.prototype.addTag = function (tagName) {
            // `addTag` is invoked by `verifyTag` on an instance of a `TagList` object.
            // `this` is the current TagList instance in scope.

            var tagList = this,
                restUrl = Rock.settings.get('baseUrl') + 'api/taggeditems'
            restUrl += '/' + tagList.entityTypeId;
            restUrl += '/' + tagList.currentPersonId;
            restUrl += '/' + tagList.entityGuid;
            restUrl += '?name=' + encodeURIComponent(tagName);

            // only attempt to add tag if an entityGuid exists
            if (!tagList.delaySave) {
                if (tagList.entityQualifierColumn) {
                    restUrl += '/' + tagList.entityQualifierColumn;
                }

                if (tagList.entityQualifierValue) {
                    restUrl += '/' + tagList.entityQualifierValue;
                }

                $.ajax({
                    type: 'POST',
                    url: restUrl,
                    error: function (xhr, status, error) {
                        console.log('AddTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
                    }
                });
            }
        };

        TagList.prototype.removeTag = function (tagName) {
            // Since `removeTag` is being called by the jQuery lib, `this`
            // is the current TagList DOM element. Get the HTML ID and fetch
            // from the cache.
            var tagList = exports.tagLists[$(this).attr('id')],
                restUrl = Rock.settings.get('baseUrl') + 'api/taggeditems';
            restUrl += '/' + tagList.entityTypeId;
            restUrl += '/' + tagList.currentPersonId;
            restUrl += '/' + tagList.entityGuid;

            // only attempt to remove tag if an entityGuid exists
            if (!tagList.delaySave) {
                if (tagList.entityQualifierColumn) {
                    restUrl += '/' + tagList.entityQualifierColumn;
                }

                if (tagList.entityQualifierValue) {
                    restUrl += '/' + tagList.entityQualifierValue;
                }

                restUrl += '?name=' + encodeURIComponent(tagName);

                $.ajax({
                    type: 'DELETE',
                    url: restUrl,
                    context: {
                        tagName: tagName,
                        tagsInput: this
                    },
                    error: function (xhr, status, error) {
                        if (xhr && xhr.status == 404) {
                            // already deleted
                            return;
                        }

                        Rock.dialogs.alert("Unable to remove tag: " + error);

                        // put the tag back in (in alpha order, case-insensitive)
                        var tagsCommaList = $(this.tagsInput).val() + ',' + this.tagName

                        tagsCommaList = tagsCommaList.split(",").sort(function (a, b) {
                            return a.toLowerCase().localeCompare(b.toLowerCase());
                        }).join(",");

                        $(this.tagsInput).importTags(tagsCommaList);
                        return false;
                    }
                });
            }

        };

        TagList.prototype.initialize = function () {

            var autoCompleteUrlPrefix = Rock.settings.get('baseUrl') + 'api/tags/availablenames';
            autoCompleteUrlPrefix += '/' + this.entityTypeId;
            autoCompleteUrlPrefix += '/' + this.currentPersonId;

            var autoCompleteUrlSuffix = '/' + this.entityGuid;
            if (this.entityQualifierColumn) {
                autoCompleteUrlSuffix += '/' + this.entityQualifierColumn;
            }
            if (this.entityQualifierValue) {
                autoCompleteUrlSuffix += '/' + this.entityQualifierValue;
            }

            $('ul.ui-autocomplete').css({ 'width': '300px' });

            $('#' + this.controlId).tagsInput({
                autocomplete_url: function (request, response) {
                    $.ajax({
                        url: autoCompleteUrlPrefix + '/' + request.term + autoCompleteUrlSuffix,
                        dataType: 'json',
                        success: function (data, status, xhr) {
                            response($.map(data, function (item) {
                                return {
                                    value: item.Name,
                                    class: !item.OwnerId ? 'system' : 'personal'
                                };
                            }));
                        },
                        error: function (xhr, status, error) {
                            console.log('availablenames status: ' + status + ' [' + error + ']: ' + xhr.reponseText);
                        }
                    });
                },
                autoCompleteAppendTo: 'div.tag-wrap',
                autoCompleteMessages: {
                    noResults: function () {},
                    results: function () {}
                },
                height: 'auto',
                width: '100%',
                interactive: true,
                defaultText: 'add tag',
                removeWithBackspace: false,
                onAddTag: this.verifyTag,
                onRemoveTag: this.removeTag
            });
        };

        exports = {
            tagLists: {},
            initialize: function (options) {
                if (!options.controlId) throw 'controlId must be set.';
                if (!options.entityTypeId) throw 'entityTypeId must be set.';
                if (!options.currentPersonId) throw 'currentPersonId must be set';
                if (!options.entityGuid) throw 'entityGuid must be set';
                
                var tagList = new TagList(options);
                tagList.options = options;
                exports.tagLists[options.controlId] = tagList;
                tagList.initialize();
            }
        };

        return exports;
    }());
}(jQuery));