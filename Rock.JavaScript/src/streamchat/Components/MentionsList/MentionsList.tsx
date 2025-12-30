import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useChatContext } from 'stream-chat-react';
import type { ChannelFilters, MessageResponse } from 'stream-chat';
import { MessageSearchResultItem } from '../MessageSearch/MessageSearchResultItem';

interface MentionsListProps {
    cid?: string;
}

// each result from client.search has this shape
interface SearchResult {
    message: MessageResponse;
}

export const MentionsList: React.FC<MentionsListProps> = ({
    cid,
}) => {
    const [results, setResults] = useState<SearchResult[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [offset, setOffset] = useState(0);
    const [hasMore, setHasMore] = useState(true);
    const limit = 10;
    const { client, channel } = useChatContext('MentionsList');
    const observer = useRef<IntersectionObserver | null>(null);

    const handleLoadMore = useCallback(async () => {
        const newOffset = offset + limit;
        setOffset(newOffset);
        await fetchResults(newOffset);
    }, [offset, limit]);

    const lastItemRef = useCallback(
        (node: HTMLLIElement | null) => {
            if (loading) return;
            if (observer.current) observer.current.disconnect();
            observer.current = new window.IntersectionObserver(entries => {
                if (entries[0].isIntersecting && hasMore && !loading) {
                    handleLoadMore();
                }
            });
            if (node) observer.current.observe(node);
        },
        [loading, hasMore, handleLoadMore]
    );

    if (!channel) {
        return <div className="message-search-container">No active channel found.</div>;
    }

    const fetchResults = async (offsetValue: number) => {
        setLoading(true);
        setError(null);

        let channelFilters: ChannelFilters = {};
        if (cid) {
            channelFilters = { cid: { $eq: cid } };
        }

        try {
            const response = await client.search(
                channelFilters,
                { "mentioned_users.id": { $contains: client.userID! } },
                { limit, offset: offsetValue, sort: [{ created_at: -1 }] }
            );
            // TypeScript knows response.results is SearchResult[]
            const messages = response.results;

            if (offsetValue === 0) {
                setResults(messages);
            } else {
                setResults(prev => [...prev, ...messages]);
            }

            setHasMore(messages.length === limit);
        } catch (err) {
            setError('Failed to search messages.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchResults(0);
        setOffset(0);
    }, [cid]);

    return (
        <div className="message-search-container">

            {error && <div className="error">{error}</div>}
            {!loading && !error && results.length === 0 && (
                <div className="message-search-no-results">
                    <div className="message-search-no-results-inner">
                        <i className="fas fa-search" aria-hidden="true" />
                        <span>No results found.</span>
                    </div>
                </div>
            )}

            {results.length > 0 && (
                <ul className="message-search-results">
                    {results.map(({ message }, idx) => {
                        const isLast = idx === results.length - 1;
                        return (
                            <MessageSearchResultItem
                                key={message.id}
                                message={message}
                                ref={isLast ? lastItemRef : undefined}
                            />
                        );
                    })}
                </ul>
            )}
        </div>
    );
};
