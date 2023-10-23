import { newGuid } from "@Obsidian/Utility/guid";
import { inject, provide } from "vue";

export const noteOptionsSymbol = Symbol();

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
