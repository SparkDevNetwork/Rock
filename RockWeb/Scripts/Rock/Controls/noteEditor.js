(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {

        // Initialize NoteEditor and NoteContainer events
        $('.js-notecontainer .js-addnote,.js-editnote,.js-replynote').click(function (e) {
            var addNote = $(this).hasClass('js-addnote');
            var editNote = $(this).hasClass('js-editnote');
            var replyNote = $(this).hasClass('js-replynote');
            var cancelNote = $(this).hasClass('js-editnote-cancel');
            var deleteNote = $(this).hasClass('js-removenote');


            var $noteContainer = $(this).closest('.js-notecontainer');
            var sortDirection = $noteContainer.data('sortdirection');
            var $noteEditor = $noteContainer.find('.js-note-editor');
            var $currentNote = $(false);
            $noteEditor.detach();

            // clear out any previously entered stuff
            $noteEditor.find('.js-parentnoteid').val('');
            $noteEditor.find('textarea').val('');
            $noteEditor.find('input:checkbox').prop('checked', false);
            $noteEditor.find('.js-notesecurity').hide();

            if (addNote) {
                // display the 'noteEditor' as a new note
                var $noteList = $noteContainer.find('.js-notelist').first();
                if (sortDirection == 'Ascending') {
                    $noteList.append($noteEditor);
                } else {
                    $noteList.prepend($noteEditor);
                }
            }
            else {
                $currentNote = $(this).closest('.js-noteviewitem');
                var currentNoteId = $currentNote.data('note-id');
                var currentNoteNoteTypeId = $currentNote.data('notetype-id');

                if (replyNote) {
                    // display the 'noteEditor' as a reply to the current note
                    $noteEditor.find('.js-parentnoteid').val(currentNoteId);

                    // restrict note replies to use the notetype of the parent note (if there is only one notetype option there won't be a $noteTypeInput)
                    var $noteTypeInput = $noteEditor.find('.js-notenotetype');
                    if ($noteTypeInput.length) {
                        $noteTypeInput.val(currentNoteNoteTypeId)
                        $noteTypeInput.hide();
                    }
                    else {
                        $noteTypeInput.show();
                    }

                    $currentNote.append($noteEditor)
                }
                else if (editNote) {
                    // display the 'noteEditor' in place of the currentNote
                  $.get(Rock.settings.get('baseUrl') + 'api/notes/GetNoteEditData?noteId=' + currentNoteId, function (noteData) {
                        $noteEditor.find('.js-parentnoteid').val(noteData.ParentNoteId);
                        $noteEditor.find('.js-notetext').val(noteData.Text);
                        $noteEditor.find('.js-noteprivate').prop('checked', noteData.IsPrivateNote);
                        $noteEditor.find('.js-notealert').prop('checked', noteData.IsAlert);

                      var date = new Date(noteData.CreatedDateTime);
                      var timeWithouthSecond = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

                      var $datetimepicker = $noteEditor.find('.js-notecreateddate').find('input');
                      $datetimepicker.first().val(date.toLocaleDateString());
                      $datetimepicker.last().val(timeWithouthSecond);


                        var $noteTypeInput = $noteEditor.find('.js-notenotetype');

                        // noteType dropdown will only be rendered if more than one notetype is pickable
                        if ($noteTypeInput.length) {

                            // it is possible that we are editing a note that has a notetype that is not user selectable, so it won't be in the dropdown.  In that case, just hide the picker keep the current notetype
                            var $unselectableNoteType = $noteEditor.find('.js-has-unselectable-notetype');
                            if ($noteTypeInput.find('option[value=' + noteData.NoteTypeId + ']').length) {

                                $unselectableNoteType.val('false');
                                $noteTypeInput.show();
                            }
                            else {
                                // indicate that this not has an unselectable notetype so that postback will keep the value that it had
                                $unselectableNoteType.val('true');

                                $noteTypeInput.hide();
                            }

                            $noteTypeInput.val(noteData.NoteTypeId);
                        }


                        $noteEditor.find('.js-noteid').val(currentNoteId);

                        var $securityBtn = $noteEditor.find('.js-notesecurity');
                        $securityBtn.data('entity-id', currentNoteId);
                        $securityBtn.show();

                        e.preventDefault();
                        $currentNote.parent('.js-note').prepend($noteEditor);
                    });
                }
            }

            if (editNote) {
                // hide the readonly details of the note that we are editing then show the editor
                $noteEditor.fadeIn();
                $currentNote.hide();
            }
            else {
                // slide the noteeditor into view
                $noteEditor.slideDown().find('textarea').focus();
            }
        });

        $('.js-notecontainer .js-notesecurity').click(function (e) {
            var $securityBtn = $(this);
            var entityTypeId = $securityBtn.data('entitytype-id');
            var title = $securityBtn.data('title');
            var currentNoteId = $securityBtn.data('entity-id');
            var securityUrl = Rock.settings.get('baseUrl') + "Secure/" + entityTypeId + "/" + currentNoteId + "?t=" + title + "&pb=&sb=Done";
            Rock.controls.modal.show($securityBtn, securityUrl);
        });

        $('.js-notecontainer .js-editnote-cancel').click(function (e) {
            var $noteContainer = $(this).closest('.js-notecontainer');
            var $noteEditor = $noteContainer.find('.js-note-editor');
            $noteEditor.slideUp();

            // show any notedetails that might have been hidden when doing the editing
            $noteEditor.parent().find('.js-noteviewitem').slideDown();
        });

        $('.js-notecontainer .js-removenote').click(function (e) {
            var $currentNote = $(this).closest('.js-noteviewitem');
            var currentNoteId = $currentNote.attr('data-note-id');
            var $noteContainer = $(this).closest('.js-notecontainer');
            $noteContainer.find('.js-currentnoteid').val(currentNoteId);

            e.preventDefault();
            e.stopImmediatePropagation();
            var postbackJs = $noteContainer.find(".js-delete-postback").attr('href');
            return Rock.dialogs.confirm('Are you sure you want to delete this note?', function () {
                window.location = postbackJs;
            });
        });

        $('.js-expandreply').click(function (e) {
            var $noteContainer = $(this).closest('.js-notecontainer');

            var $currentNote = $(this).closest('.js-note');
            var $childNotesContainer = $currentNote.find('.js-childnotes:first');
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
    });
}(Sys));
