import { newGuid } from "@Obsidian/Utility/guid";
import { ComputedRef, inject, provide } from "vue";
import { ExpandedNoteBag, NoteOptions } from "./types.partial";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

/**
 * Converts an input string to a CSS class name.
 *
 * @param input The text to be converted.
 *
 * @returns A string safe for use as a CSS class.
 */
export function toCssClass(input: string): string {
    if (input.trim() === "") {
        return "";
    }

    input = input.toLowerCase();
    input = input.replace(/[^a-z_0-9]/g, "-");
    input = input.replace(/-+/g, "-");

    if (!input[0].match("^-?[_a-z]+[_a-z0-9-]*")) {
        input = `-x-${input}`;
    }

    return input;
}

/**
 * Sorts a collection of notes in place. This should only be called if it is
 * likely that the order will change.
 *
 * @param notes The array of notes to be sorted.
 * @param descending `true` if the notes should be sorted in descending order.
 */
export function sortNotes(notes: ExpandedNoteBag[], descending: boolean): void {
    notes.sort((a, b) => {
        if (a.isAlert && !b.isAlert) {
            return -1;
        }
        else if (!a.isAlert && b.isAlert) {
            return 1;
        }

        if (a.isPinned && !b.isPinned) {
            return -1;
        }
        else if (!a.isPinned && b.isPinned) {
            return 1;
        }

        const aDate = a.createdDateTime
            ? RockDateTime.parseISO(a.createdDateTime)
            : null;
        const bDate = b.createdDateTime
            ? RockDateTime.parseISO(b.createdDateTime)
            : null;

        if (!descending) {
            if (aDate && !bDate) {
                return -1;
            }
            else if (!aDate && bDate) {
                return 1;
            }
            else if (aDate && bDate) {
                if (aDate.isEarlierThan(bDate)) {
                    return -1;
                }
                else if (aDate.isLaterThan(bDate)) {
                    return 1;
                }
            }
        }
        else {
            if (aDate && !bDate) {
                return -1;
            }
            else if (!aDate && bDate) {
                return 1;
            }
            else if (aDate && bDate) {
                if (aDate.isLaterThan(bDate)) {
                    return -1;
                }
                else if (aDate.isEarlierThan(bDate)) {
                    return 1;
                }
            }
        }

        return 0;
    });
}

/**
 * Inserts a new note into the collection in the proper sorted position.
 *
 * @param expandedNote The note to be inserted.
 * @param notes The collection of existing notes.
 * @param descending Whether to treat the list of notes as descending or not.
 */
export function insertSortedPosition(expandedNote: ExpandedNoteBag, notes: ExpandedNoteBag[], descending: boolean): void {
    if (descending) {
        let index = 0;

        // If not an alert, put it at the top of the list after all
        // other alerts.
        if (!expandedNote.isAlert) {
            while (index < notes.length && notes[index].isAlert) {
                index++;
            }
        }

        // If not pinned to top, put it at the top of the list after all
        // other pinned to top and alerts.
        if (!expandedNote.isPinned) {
            while (index < notes.length && notes[index].isPinned) {
                index++;
            }
        }

        notes.splice(index, 0, expandedNote);
    }
    else {
        let index = 0;

        // If an alert and pinned item, put it at as the last alert and pinned item.
        if (expandedNote.isAlert && expandedNote.isPinned) {
            while (index < notes.length && (notes[index].isAlert && notes[index].isPinned)) {
                index++;
            }
        }
        // If an alert but not a pinned item, put it at as the last alert item.
        else if (expandedNote.isAlert && !expandedNote.isPinned) {
            while (index < notes.length && notes[index].isAlert) {
                index++;
            }
        }
        // If not an alert but a pinned item, put it at as the last item.
        else if (!expandedNote.isAlert && expandedNote.isPinned) {
            while (index < notes.length && (notes[index].isAlert || notes[index].isPinned)) {
                index++;
            }
        }
        else {
            index = notes.length;
        }

        notes.splice(index, 0, expandedNote);
    }
}

// #region Note Options

const noteOptionsSymbol = Symbol();

/**
 * Makes the note options object available to child components.
 *
 * @param options The options to be provided to child note components.
 */
export function provideNoteOptions(options: ComputedRef<NoteOptions>): void {
    provide(noteOptionsSymbol, options);
}

/**
 * Gets the note options object for this tree of notes or throws an error if
 * it is not available.
 *
 * @returns An instance of {@link NoteOptions}.
 */
export function useNoteOptions(): ComputedRef<NoteOptions> {
    const options = inject<ComputedRef<NoteOptions>>(noteOptionsSymbol);

    if (!options) {
        throw new Error("Invalid note options state.");
    }

    return options;
}

// #endregion

// #region Note Emitter

const noteEmitterSymbol = Symbol();

type NoteEventNames = "beginEdit" | "endEdit";

/**
 * Creats a new {@link NoteEventEmitter} and provides it to child components.
 *
 * @returns The emitter that was created.
 */
export function provideEmitter(): NoteEventEmitter {
    const emitter = new NoteEventEmitter();

    provide(noteEmitterSymbol, emitter);

    return emitter;
}

/**
 * Uses a previously provided {@link NoteEventEmitter} from the notes component.
 *
 * @returns The emitter.
 */
export function useEmitter(): NoteEventEmitter {
    return inject<NoteEventEmitter>(noteEmitterSymbol) ?? new NoteEventEmitter();
}

interface INoteEventTarget {
    id: string;

    handler: () => void;
}

/**
 * A special class that handles emitting events related to the notes block.
 */
export class NoteEventEmitter {
    private readonly events: Record<string, INoteEventTarget[]> = {};

    /**
     * Creates a new subscriber that can then subscribe to events.
     *
     * @returns A new subscriber key.
     */
    public subscribe(): string {
        return newGuid();
    }

    /**
     * Unsubscribes a subscriber from all events.
     *
     * @param subscriberKey The subscriber to be unsubscribed.
     */
    public unsubscribe(subscriberKey: string): void {
        const keys = Object.keys(this.events);

        for (const key of keys) {
            const targets = this.events[key];

            if (!targets) {
                continue;
            }

            for (let i = 0; i < targets.length;) {
                if (targets[i].id === subscriberKey) {
                    targets.splice(i, 1);
                }
                else {
                    i++;
                }
            }
        }
    }

    /**
     * Registers a callback for a specific event.
     *
     * @param event The event to be registered to.
     * @param subscriberKey The identifier of the subscriber.
     * @param handler The function to call when the event is fired.
     */
    public on(event: NoteEventNames, subscriberKey: string, handler: () => void): void {
        if (!this.events[event]) {
            this.events[event] = [];
        }

        this.events[event].push({
            id: subscriberKey,
            handler
        });
    }

    /**
     * Unregisters a callback for a specific event.
     *
     * @param event The event to be unregistered from.
     * @param subscriberKey The identifier of the subscriber.
     */
    public off(event: NoteEventNames, subscriberKey: string): void {
        const targets = this.events[event];

        if (!targets) {
            return;
        }

        for (let i = 0; i < targets.length;) {
            if (targets[i].id === subscriberKey) {
                targets.splice(i, 1);
            }
            else {
                i++;
            }
        }
    }

    /**
     * Emits a single event to all subscribers except the one specified.
     *
     * @param event The event name to be emitted.
     * @param exceptSubscriberKey The identifier of the subscriber that will not receive the event.
     */
    public emit(event: NoteEventNames, exceptSubscriberKey: string): void {
        const targets = this.events[event];

        if (!targets) {
            return;
        }

        for (let i = 0; i < targets.length; i++) {
            if (targets[i].id !== exceptSubscriberKey) {
                targets[i].handler();
            }
        }
    }
}

// #endregion
