import React, { useEffect } from 'react';
import type { ComputeItemKey, VirtuosoProps } from 'react-virtuoso';
import { Virtuoso } from 'react-virtuoso';

import type { Thread, ThreadManagerState } from 'stream-chat';
import { useChatContext, useComponentContext, useStateStore, ThreadListItem as DefaultThreadListItem, useThreadsViewContext, useChat, useChannelActionContext } from 'stream-chat-react';
import { CustomThreadListLoadingIndicator } from './ChannelThreadListLoadingIndicator';
import { ChannelThreadListUnseenThreadsBanner as DefaultThreadListUnseenThreadsBanner } from './ChannelThreadListUnseenBanner';

const selector = (nextValue: ThreadManagerState) => ({ threads: nextValue.threads });

const computeItemKey: ComputeItemKey<Thread, unknown> = (_, item) => item.id;

type ThreadListProps = {
    virtuosoProps?: VirtuosoProps<Thread, unknown>;
};

export const useThreadList = () => {
    const { client } = useChatContext();

    useEffect(() => {
        const handleVisibilityChange = () => {
            if (document.visibilityState === 'visible') {
                client.threads.activate();
            }
            if (document.visibilityState === 'hidden') {
                client.threads.deactivate();
            }
        };

        handleVisibilityChange();

        document.addEventListener('visibilitychange', handleVisibilityChange);
        return () => {
            client.threads.deactivate();
            document.removeEventListener('visibilitychange', handleVisibilityChange);
        };
    }, [client]);
};

export const ChannelThreadList = ({ virtuosoProps }: ThreadListProps) => {
    const { client, channel } = useChatContext();
    const {
        ThreadListEmptyPlaceholder = CustomThreadListEmptyPlaceholder,
        ThreadListItem = DefaultThreadListItem,
        ThreadListLoadingIndicator = CustomThreadListLoadingIndicator,
        ThreadListUnseenThreadsBanner = DefaultThreadListUnseenThreadsBanner,
    } = useComponentContext();
    const { threads } = useStateStore(client.threads.state, selector);

    // Define a new filtered thread list that filters to threads that belong to the current channel
    const filteredThreads = threads.filter(thread => thread.channel?.id === channel?.id);

    const { activeThread, setActiveThread } = useThreadsViewContext();
    const { openThread } = useChannelActionContext();

    console.log(activeThread);
    useThreadList();

    return (
        <div className='str-chat__thread-list-container'>
            {/* TODO: allow re-load on stale ThreadManager state */}
            <ThreadListUnseenThreadsBanner />
            <Virtuoso
                atBottomStateChange={(atBottom) => atBottom && client.threads.loadNextPage()}
                className='str-chat__thread-list'
                components={{
                    EmptyPlaceholder: ThreadListEmptyPlaceholder,
                    Footer: ThreadListLoadingIndicator,
                }}
                computeItemKey={computeItemKey}
                data={filteredThreads}
                itemContent={(_, thread) => <ThreadListItem thread={thread} threadListItemUIProps={
                    {
                        onClick: () => {
                            console.log('Thread clicked:', thread);
                            setActiveThread(thread);
                            openThread(thread.state.getLatestValue().parentMessage);
                        },
                    }
                } />}
                // TODO: handle visibility (for a button that scrolls to the unread thread)
                // itemsRendered={(items) => console.log({ items })}
                {...virtuosoProps}
            />
        </div>
    );
};



export const CustomThreadListEmptyPlaceholder = () => (
    <div className='str-chat__thread-list-empty-placeholder'>
        <i className="fas fa-comments" />
        {/* TODO: translate */}
        No threads here yet...
    </div>
);
