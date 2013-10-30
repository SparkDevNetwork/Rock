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

            $.ajax({
                url: restUrl,
                statusCode: {
                    404: function () {
                        var result = confirm('A tag called "' + tagName + '" does not exist. Do you want to create a new personal tag?');
                        if (result) {
                            tagList.addTag(tagName);
                        } else {
                            $('#' + tagList.controlId).removeTag(tagName);
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
            restUrl += '/' + tagName;

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
            restUrl += '/' + tagName;

            if (tagList.entityQualifierColumn) {
                restUrl += '/' + tagList.entityQualifierColumn;
            }

            if (tagList.entityQualifierValue) {
                restUrl += '/' + tagList.entityQualifierValue;
            }

            $.ajax({
                type: 'DELETE',
                url: restUrl,
                error: function (xhr, status, error) {
                    console.log('RemoveTag() status: ' + status + ' [' + error + ']: ' + xhr.responseText);
                }
            });
        };

        TagList.prototype.initialize = function () {
            var autoCompleteUrl = Rock.settings.get('baseUrl') + 'api/tags/availablenames';
            autoCompleteUrl += '/' + this.entityTypeId;
            autoCompleteUrl += '/' + this.currentPersonId;
            autoCompleteUrl += '/' + this.entityGuid;

            if (this.entityQualifierColumn) {
                autoCompleteUrl += '/' + this.entityQualifierColumn;    
            }
            
            if (this.entityQualifierValue) {
                autoCompleteUrl += '/' + this.entityQualifierValue;
            }

            

            $('ul.ui-autocomplete').css({ 'width': '300px' });

            $('#' + this.controlId).tagsInput({
                autocomplete_url: function (request, response) {
                    $.ajax({
                        url: autoCompleteUrl,
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
                exports.tagLists[options.controlId] = tagList;
                tagList.initialize();
            }
        };

        return exports;
    }());
}(jQuery));