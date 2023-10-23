import { ConfirmAccountViewType } from "@Obsidian/Enums/Blocks/Security/ConfirmAccount/confirmAccountViewType";
import { ConfirmAccountAccountConfirmationViewOptionsBag } from "@Obsidian/ViewModels/Blocks/Security/ConfirmAccount/confirmAccountAccountConfirmationViewOptionsBag";
import { ConfirmAccountChangePasswordViewOptionsBag } from "@Obsidian/ViewModels/Blocks/Security/ConfirmAccount/confirmAccountChangePasswordViewOptionsBag";
import { ConfirmAccountDeleteConfirmationViewOptionsBag } from "@Obsidian/ViewModels/Blocks/Security/ConfirmAccount/confirmAccountDeleteConfirmationViewOptionsBag";
import { ConfirmAccountAlertViewOptionsBag } from "@Obsidian/ViewModels/Blocks/Security/ConfirmAccount/confirmAccountAlertViewOptionsBag";
import { ConfirmAccountContentViewOptionsBag } from "@Obsidian/ViewModels/Blocks/Security/ConfirmAccount/confirmAccountContentViewOptionsBag";

type AccountConfirmationView = {
    type: typeof ConfirmAccountViewType.AccountConfirmation;
    options: ConfirmAccountAccountConfirmationViewOptionsBag;
};

type AlertView = {
    type: typeof ConfirmAccountViewType.Alert;
    options: ConfirmAccountAlertViewOptionsBag;
};

type DeleteConfirmationView = {
    type: typeof ConfirmAccountViewType.DeleteConfirmation;
    options: ConfirmAccountDeleteConfirmationViewOptionsBag;
};

type ChangePasswordView = {
    type: typeof ConfirmAccountViewType.ChangePassword;
    options: ConfirmAccountChangePasswordViewOptionsBag;
};

type ContentView = {
    type: typeof ConfirmAccountViewType.Content;
    options: ConfirmAccountContentViewOptionsBag;
};

export type ConfirmAccountView = AccountConfirmationView | AlertView | DeleteConfirmationView | ChangePasswordView | ContentView;
