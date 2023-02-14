export type ChangePasswordEvent = {
    code: string;
    password: string;
};

export type ConfirmAccountEvent = {
    code: string;
};

export type DeleteAccountEvent = {
    code: string;
};

export type ShowChangePasswordViewEvent = {
    code: string;
};

export type ShowDeleteConfirmationViewEvent = {
    code: string;
};