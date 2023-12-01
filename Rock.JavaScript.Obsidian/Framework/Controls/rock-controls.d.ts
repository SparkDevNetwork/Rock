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
            MediaPlayer: {
                AllPlayers: unknown[]
            }
        }
    }
}