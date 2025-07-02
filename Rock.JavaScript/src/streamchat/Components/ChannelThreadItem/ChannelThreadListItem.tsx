import React, { createContext, useContext } from 'react';

import type { Thread } from 'stream-chat';
import { ChannelThreadListItemUI, ThreadListItemUIProps } from './ChannelThreadListItemUI';

export type ChannelThreadListItemProps = {
    thread: Thread;
    threadListItemUIProps?: ThreadListItemUIProps;
};

const ChannelThreadListItemContext = createContext<Thread | undefined>(undefined);

export const useChannelThreadListItemContext = () => useContext(ChannelThreadListItemContext);

export const ChannelThreadListItem = ({
    thread,
    threadListItemUIProps,
}: ChannelThreadListItemProps) => {

    return (
        <ChannelThreadListItemContext.Provider value={thread}>
            <ChannelThreadListItemUI {...threadListItemUIProps} />
        </ChannelThreadListItemContext.Provider>
    );
};