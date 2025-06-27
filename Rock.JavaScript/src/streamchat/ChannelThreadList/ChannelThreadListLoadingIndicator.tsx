import React from 'react';

import type { ThreadManagerState } from 'stream-chat';

import { LoadingIndicator as DefaultLoadingIndicator, useChatContext, useComponentContext, useStateStore } from 'stream-chat-react';

const selector = (nextValue: ThreadManagerState) => ({
    isLoadingNext: nextValue.pagination.isLoadingNext,
});

export const CustomThreadListLoadingIndicator = () => {
    const { LoadingIndicator = DefaultLoadingIndicator } = useComponentContext();
    const { client } = useChatContext();
    const { isLoadingNext } = useStateStore(client.threads.state, selector);

    if (!isLoadingNext) return null;

    return (
        <div className='str-chat__thread-list-loading-indicator'>
            <LoadingIndicator />
        </div>
    );
};