import React from "react";
import { useMemo } from "react";
import {
    MessageActions,
    defaultMessageActionSet,
    MessageActionsProps,
} from "stream-chat-react/experimental";

export const CustomMessageActions: React.FC<MessageActionsProps> = (props) => {
    // const filteredActionSet = useMemo(
    //     () =>
    //         defaultMessageActionSet.filter(
    //             (action) => action.type !== "pin"
    //         ),
    //     []
    // );

    return (
        <MessageActions />
    );
};
