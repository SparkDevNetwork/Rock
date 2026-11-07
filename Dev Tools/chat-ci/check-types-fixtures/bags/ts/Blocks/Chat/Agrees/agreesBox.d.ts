/** A box whose declaration agrees with it. */
export type AgreesBox = {
    /** Gets or sets the campuses. */
    campuses?: Record<string, number | null>[] | null;

    errorMessage?: string | null;

    navigationUrls?: Record<string, string> | null;

    /** Gets or sets the organization name. */
    organizationName?: string | null;

    rsvpReminderOffsetDays: number;
};
