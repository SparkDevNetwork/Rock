import React, { createContext, useContext } from 'react';
import type { Thread } from 'stream-chat';
import { ChannelThreadListItemUI, ThreadListItemUIProps } from './ChannelThreadListItemUI';

/**
 * ChannelThreadListItemProps
 *
 * Props for the ChannelThreadListItem component.
 * @property thread - The thread object to display
 * @property threadListItemUIProps - Optional props to pass to the UI component
 */
export type ChannelThreadListItemProps = {
    thread: Thread;
    threadListItemUIProps?: ThreadListItemUIProps;
};

/**
 * ChannelThreadListItemContext
 *
 * React context for sharing the current thread object with descendant components.
 * Initialized as undefined to enforce usage within a provider.
 */
const ChannelThreadListItemContext = createContext<Thread | undefined>(undefined);

/**
 * useChannelThreadListItemContext
 *
 * Custom hook to access the current thread from context.
 *
 * @returns {Thread | undefined} The current thread object
 */
export const useChannelThreadListItemContext = () => useContext(ChannelThreadListItemContext);

/**
 * ChannelThreadListItem
 *
 * Provides the current thread object to its descendants via context and renders the UI component for the thread list item.
 *
 * - Wraps ChannelThreadListItemUI in a context provider for thread access.
 * - Passes any additional UI props to the UI component.
 *
 * @param thread - The thread object to display
 * @param threadListItemUIProps - Optional props for the UI component
 * @returns {JSX.Element} The rendered thread list item
 */
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