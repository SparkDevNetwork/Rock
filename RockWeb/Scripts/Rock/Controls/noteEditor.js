(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    /** JS helper for the NoteEditor control */
    Rock.controls.noteEditor = (function () {
        var exports = {
            /** initializes the JavasSript for the noteEditor control */
            initialize: function (options) {

                if (!options.id) {
                    throw 'id is required';
                }

                var self = this;

                var $control = $('#' + options.id);

                if ($control.length == 0) {
                    return;
                }

                self.$noteEditor = $control;

                this.initializeEventHandlers();

                if (options.isEditing) {
                    var $noteContainer = self.$noteEditor.closest('.js-notecontainer');

                    if (options.currentNoteId) {
                        // editing an existing note
                        var $displayedNote = $noteContainer.find("[data-note-id='" + options.currentNoteId + "']");

                        // move note editor and display in place of the readonly version of note
                        self.$noteEditor.detach();
                        $displayedNote.parent('.js-note').prepend(self.$noteEditor);
                        self.$noteEditor.fadeIn();

                        $displayedNote.hide();
                    }
                    else if (options.parentNoteId) {
                        // new reply to a note
                        var $replyToNote = $noteContainer.find("[data-note-id='" + options.parentNoteId + "']");

                        // move note editor as a child note of the note we are replying to
                        self.$noteEditor.detach();
                        $replyToNote.append(self.$noteEditor)
                        self.$noteEditor.slideDown().find('textarea').trigger("focus");
                    }
                    else {
                        // new note, so just show it 
                        self.$noteEditor.fadeIn();
                    }
                }
            },

            /**  */
            initializeEventHandlers: function () {
                // Initialize NoteEditor and NoteContainer events
                var self = this;
                var $noteContainer = self.$noteEditor.closest('.js-notecontainer');

                $('.js-addnote,.js-editnote,.js-replynote', $noteContainer).on('click', function (e) {
                    var addNote = $(this).hasClass('js-addnote');
                    var editNote = $(this).hasClass('js-editnote');
                    var replyNote = $(this).hasClass('js-replynote');
                    var cancelNote = $(this).hasClass('js-editnote-cancel');
                    var deleteNote = $(this).hasClass('js-removenote');

                    var sortDirection = $noteContainer.data('sortdirection');
                    var $noteEditor = $noteContainer.find('.js-note-editor');
                    var $currentNote = $(false);
                    $noteEditor.detach();

                    // clear out any previously entered stuff
                    $noteEditor.find('.js-parentnoteid').val('');
                    $noteEditor.find('textarea').val('');
                    $noteEditor.find('input:checkbox').prop('checked', false);
                    $noteEditor.find('.js-notesecurity').hide();

                    var $noteprivateInput = $noteEditor.find('.js-noteprivate');
                    $noteprivateInput.parent().show();

                    if (addNote) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        var postbackJs = $noteContainer.find(".js-add-postback").attr('href');
                        window.location = postbackJs;
                        return;
                    }
                    else {
                        $currentNote = $(this).closest('.js-noteviewitem');
                        var currentNoteId = $currentNote.data('note-id');

                        if (replyNote) {
                            $noteContainer.find('.js-currentnoteid').val(currentNoteId);
                            $noteEditor.find('.js-parentnoteid').val(currentNoteId);

                            e.preventDefault();
                            e.stopImmediatePropagation();
                            var postbackJs = $noteContainer.find(".js-reply-to-postback").attr('href');
                            window.location = postbackJs;
                            return;
                        }
                        else if (editNote) {
                            $noteContainer.find('.js-currentnoteid').val(currentNoteId);

                            e.preventDefault();
                            e.stopImmediatePropagation();
                            var postbackJs = $noteContainer.find(".js-edit-postback").attr('href');
                            window.location = postbackJs;
                            return;
                        }
                    }
                });

                $('.js-notesecurity', $noteContainer).on('click', function (e) {
                    var $securityBtn = $(this);
                    var entityTypeId = $securityBtn.data('entitytype-id');
                    var title = $securityBtn.data('title');
                    var currentNoteId = $securityBtn.data('entity-id');
                    var securityUrl = Rock.settings.get('baseUrl') + "Secure/" + entityTypeId + "/" + currentNoteId + "?t=" + title + "&pb=&sb=Done";
                    Rock.controls.modal.show($securityBtn, securityUrl);
                });

                $('.js-editnote-cancel', $noteContainer).on('click', function (e) {
                    var $noteEditor = $noteContainer.find('.js-note-editor');
                    $noteEditor.slideUp();

                    // show any notedetails that might have been hidden when doing the editing
                    $noteEditor.parent().find('.js-noteviewitem').slideDown();
                });

                $('.js-removenote', $noteContainer).on('click', function (e) {
                    var $currentNote = $(this).closest('.js-noteviewitem');
                    var currentNoteId = $currentNote.attr('data-note-id');
                    $noteContainer.find('.js-currentnoteid').val(currentNoteId);

                    e.preventDefault();
                    e.stopImmediatePropagation();
                    var postbackJs = $noteContainer.find(".js-delete-postback").attr('href');
                    return Rock.dialogs.confirm('Are you sure you want to delete this note?', function (result) {
                        if (result) {
                            window.location = postbackJs;
                        }
                    });
                });

                $('.js-expandreply', $noteContainer).on('click', function (e) {
                    var $currentNote = $(this).closest('.js-note');
                    var $childNotesContainer = $currentNote.find('.js-childnotes').first();
                    $childNotesContainer.slideToggle(function (x) {

                        // get a list of noteIds that have their child items visible, so that we can maintain that expansion after a postback
                        var expandedNoteIds = $(this).closest('.js-notecontainer').find('.js-noteviewitem:visible').map(function () {
                            var $noteItem = $(this).closest('.js-note');
                            var $childNotesExpanded = $noteItem.find('.js-childnotes:first').is(':visible');
                            if ($childNotesExpanded) {
                                return $(this).attr('data-note-id');
                            }
                            else {
                                return null;
                            }
                        }).get().join();

                        var $expandedNoteIdsHiddenField = $noteContainer.find('.js-expandednoteids');
                        $expandedNoteIdsHiddenField.val(expandedNoteIds);
                    });
                });
            }
        }

        return exports
    }());
}(jQuery));
