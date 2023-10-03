import { NotificationMessageBag } from "@Obsidian/ViewModels/Blocks/Core/NotificationMessageList/notificationMessageBag";

export type ExpandedNotificationMessageBag = NotificationMessageBag & {
    isVisible: boolean;
};
