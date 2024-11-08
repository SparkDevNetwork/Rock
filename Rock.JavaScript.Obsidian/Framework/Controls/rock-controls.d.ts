declare class MediaPlayer {
    constructor(playerSelector: string, options: Record<string, unknown>);
    on(event: string, cb: (args?) => void): void;
    off(event: string, cb?: (args?) => void): void;
    percentWatched: number;
}

/* eslint-disable @typescript-eslint/naming-convention */
interface Window {
    Rock: {
        controls: {
            datePicker: {
                initialize: (args: Record<string, unknown>) => void
            },
            yearPicker: {
                initialize: (args: Record<string, unknown>) => void
            },
            mediaplayer: {
                initialize: (args: Record<string, unknown>) => void
            }
        },
        UI: {
            MediaPlayer: typeof MediaPlayer
        }
    }
}
