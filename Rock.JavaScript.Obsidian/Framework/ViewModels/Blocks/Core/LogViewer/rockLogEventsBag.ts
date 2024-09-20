import { DateTime } from "luxon";

export type RockLogEventsBag =  {
    dateTime?: DateTime | null;

    level?: string | null;

    domain?: string | null;

    message?: string | null;

    exception?: string | null;
};