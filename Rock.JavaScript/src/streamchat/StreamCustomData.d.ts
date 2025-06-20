import "stream-chat";
import type { DefaultChannelData, DefaultUserData } from "stream-chat-react";

declare module "stream-chat" {
    interface CustomChannelData extends DefaultChannelData {
        rock_always_shown?: boolean;
        rock_campus_id?: number;
        rock_leaving_allowed?: boolean;
    }

    interface CustomUserData extends DefaultUserData {
        rock_open_direct_message_allowed?: boolean;
        rock_badges?: [any];
        rock_profile_public?: boolean;
    }
}