import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useChatContext } from 'stream-chat-react';
import type { MessageResponse } from 'stream-chat';
import { MessageSearchResultItem } from './MessageSearchResultItem';

interface SearchProps {
    directMessagingChannelType?: string;
    disabled?: boolean;
    exitSearchOnInputBlur?: boolean;
    placeholder?: string;
    debounceDelay?: number;
}

// each result from client.search has this shape
interface SearchResult {
    message: MessageResponse;
}

export const MessageSearch: React.FC<SearchProps> = ({
    directMessagingChannelType = 'messaging',
    disabled,
    exitSearchOnInputBlur,
    placeholder,
    debounceDelay = 400,
}) => {
    const [searchTerm, setSearchTerm] = useState('');
    const [submittedTerm, setSubmittedTerm] = useState('');
    const [results, setResults] = useState<SearchResult[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [offset, setOffset] = useState(0);
    const [hasMore, setHasMore] = useState(true);
    const limit = 10;
    const { client, channel } = useChatContext('MessageSearch');
    const debounceTimeout = useRef<NodeJS.Timeout | null>(null);
    const observer = useRef<IntersectionObserver | null>(null);

    const handleLoadMore = useCallback(async () => {
        const newOffset = offset + limit;
        setOffset(newOffset);
        await fetchResults(submittedTerm, newOffset);
    }, [offset, limit, submittedTerm]);

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

    const fetchResults = async (term: string, offsetValue: number) => {
        if (term.trim() === '') return;
        if (offsetValue === 0 && term === submittedTerm) return;

        setLoading(true);
        setError(null);

        try {
            const response = await client.search(
                { cid: { $eq: channel.cid } },
                term,
                { limit, offset: offsetValue, sort: [{ created_at: -1 }] }
            );
            // TypeScript knows response.results is SearchResult[]
            const messages = response.results;

            if (offsetValue === 0) {
                setResults(messages);
                setSubmittedTerm(term);
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
        if (debounceTimeout.current) clearTimeout(debounceTimeout.current);

        if (searchTerm.trim() === '') {
            setResults([]);
            setSubmittedTerm('');
            setHasMore(true);
            setOffset(0);
            return;
        }

        debounceTimeout.current = setTimeout(() => {
            setOffset(0);
            setHasMore(true);
            fetchResults(searchTerm, 0);
        }, debounceDelay);

        return () => {
            if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        };
    }, [searchTerm]);

    const handleInputBlur = () => {
        if (exitSearchOnInputBlur) {
            if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
            fetchResults(searchTerm, 0);
        }
    };

    return (
        <div className="message-search-container">
            <div className="message-search-header">
                <label className="message-search-input-label">
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={e => setSearchTerm(e.target.value)}
                        placeholder={placeholder || 'Search messages...'}
                        disabled={disabled}
                        onBlur={handleInputBlur}
                    />
                </label>
            </div>

            {error && <div className="error">{error}</div>}
            {!loading && !error && submittedTerm && results.length === 0 && (
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
                                searchTerm={searchTerm}
                                ref={isLast ? lastItemRef : undefined}
                            />
                        );
                    })}
                </ul>
            )}
        </div>
    );
};
