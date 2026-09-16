// Mirrors Rock.Configuration.ConnectedServices.OneTimeBoostStatus.
export const OneTimeBoostStatus = {
    Complete: 0,
    Pending: 1,
    Declined: 2,
    Error: 3,
} as const;

export type OneTimeBoostStatus = typeof OneTimeBoostStatus[keyof typeof OneTimeBoostStatus];
