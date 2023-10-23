import { Component } from "vue";
import { NoteBag } from "@Obsidian/ViewModels/Blocks/Core/Notes/noteBag";
import { NoteTypeBag } from "@Obsidian/ViewModels/Blocks/Core/Notes/noteTypeBag";

/**
 * Expanded note bag that contains some additional details that will be
 * dynamically added to the object at run-time.
 */
export type ExpandedNoteBag = NoteBag & {
    /** The note type of this note. */
    noteType: NoteTypeBag;

    /** The ordered child notes of this note. */
    childNotes: ExpandedNoteBag[];
};

/**
 * Options that describe the current configuration of the block. This will be
 * provided to all child UI components.
 */
export type NoteOptions = {
    /**
     * We need to provide/inject the note component so that the container can
     * access it without causing a recursive reference when bundling.
     */
    noteComponent: Component;

    /** The note types that are configured on the block settings. */
    noteTypes: NoteTypeBag[];

    /** The user-selectable note types configured on the block settings. */
    selectableNoteTypes: NoteTypeBag[];

    /** `true` if notes should be displayed in descending order. */
    isDescending: boolean;

    /** `true` if the is alert option should be displayed when editing notes. */
    showAlert: boolean;

    /**
     * `true` if the created date time override should be displayed when
     * editing notes.
     */
    showCreateDate: boolean;

    /** `true` if the note type panel header should be displayed. */
    showNoteTypeHeading: boolean;

    /** `true` if the private option should be displayed when editing notes. */
    showPrivate: boolean;

    /** `true` if the security button should be displayed when editing notes. */
    showSecurity: boolean;

    /** `true` if adding new notes is available. */
    showAdd: boolean;

    /** `true` if the person avatar should be shown when viewing notes. */
    showAvatar: boolean;

    /** Contains the URL of the current person's avatar. */
    avatarUrl: string | undefined | null;

    /** `true` if the notes block is in light mode. */
    isLightMode: boolean;

    /** `true` if replies should be auto expanded. */
    autoExpandReplies: boolean;
};
