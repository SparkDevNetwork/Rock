import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useChatContext } from 'stream-chat-react';
import type { Channel, ChannelFilters, UserResponse } from 'stream-chat';
import { DirectoryChannelResultItem } from './DirectoryChannelResultItem';
import { UserSearchResultItem } from './UserSearchResultItem';
import { useDirectoryContext } from './DirectoryContext';

interface DirectoryProps { }

const ChannelSearchResults: React.FC<{
    results: Channel[];
    lastItemRef: (node: HTMLLIElement | null) => void;
    handleClick: (channel: Channel) => void;
}> = ({ results, lastItemRef, handleClick }) => (
    <ul className="directory-search-results" style={{ listStyle: 'none' }}>
        <li className="directory-search-result-header">
            <div className="directory-search-result-header-cell directory-search-result-item-name-cell">Name</div>
            <div className="directory-search-result-header-cell directory-search-result-item-members-cell">Members</div>
            <div className="directory-search-result-header-cell directory-search-result-item-last-message-cell">Last Message At</div>
        </li>
        {results.map((channel, idx) => {
            const isLast = idx === results.length - 1;
            return (
                <DirectoryChannelResultItem
                    key={channel.id}
                    channel={channel}
                    ref={isLast ? lastItemRef : undefined}
                    onClick={() => handleClick(channel)}
                />
            );
        })}
    </ul>
);

const UserSearchResults: React.FC<{
    results: UserResponse[];
}> = ({ results }) => (
    <ul className="directory-search-results" style={{ listStyle: 'none' }}>
        <li className="directory-search-result-header">
            <div className="directory-search-result-header-cell directory-search-result-item-name-cell">Name</div>
            <div className="directory-search-result-header-cell directory-search-result-item-members-cell">Last Active At</div>
        </li>
        {results.map((user) => (
            <UserSearchResultItem user={user} key={user.id} />
        ))}
    </ul>
);

/**
 * Directory component for displaying a list of channels.
 * This will likely be expanded in the future to include more functionality.
 * For now, think of it as a "channel search".
 * This is a placeholder component that can be expanded with actual content.
 *
 * @returns {JSX.Element} The rendered Directory component.
 */
export const Directory: React.FC<DirectoryProps> = () => {
    type DirectoryContextType = 'channels' | 'users' | 'teams';
    const [context, setContext] = useState<DirectoryContextType>('channels');

    const [searchTerm, setSearchTerm] = useState('');
    const [submittedTerm, setSubmittedTerm] = useState('');
    const [channelResults, setChannelResults] = useState<Channel[]>([]);
    const [userResults, setUserResults] = useState<UserResponse[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [offset, setOffset] = useState(0);
    const [hasMore, setHasMore] = useState(true);
    const limit = 20;
    const { client, setActiveChannel } = useChatContext('Directory');
    const { setShowDirectory } = useDirectoryContext();
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

    // Fetch results based on context (channels or users)
    const fetchResults = async (term: string, offsetValue: number) => {
        setLoading(true);
        setError(null);
        if (context === 'channels') {
            const filter: ChannelFilters = {
                rock_public: true,
                ...(term.trim() !== '' && { name: { $autocomplete: term } })
            };
            try {
                const channels = await client.queryChannels(
                    filter,
                    { last_message_at: -1 },
                    { limit, offset: offsetValue }
                );
                if (offsetValue === 0) {
                    setChannelResults(channels);
                    setSubmittedTerm(term);
                } else {
                    setChannelResults(prev => [...prev, ...channels]);
                }
                setHasMore(channels.length === limit);
            } catch (err) {
                setError('Failed to search channels.');
            } finally {
                setLoading(false);
            }
        } else if (context === 'users') {
            try {
                const response = await client.queryUsers(
                    term.trim() ? { name: { $autocomplete: term } } : {},
                    { id: 1 },
                    { limit, offset: offsetValue }
                );
                const users = response.users || [];
                if (offsetValue === 0) {
                    setUserResults(users);
                    setSubmittedTerm(term);
                } else {
                    setUserResults(prev => [...prev, ...users]);
                }
                setHasMore(users.length === limit);
            } catch (err) {
                setError('Failed to search users.');
            } finally {
                setLoading(false);
            }
        } else {
            setChannelResults([]);
            setUserResults([]);
            setHasMore(false);
            setLoading(false);
        }
    };

    // Initial load and search
    useEffect(() => {
        if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        debounceTimeout.current = setTimeout(() => {
            setOffset(0);
            setHasMore(true);
            if (searchTerm.trim() === '' && context === 'users') {
                setUserResults([]);
                setSubmittedTerm('');
                setLoading(false);
                return;
            }
            fetchResults(searchTerm, 0);
        }, 400);
        return () => {
            if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        };
    }, [searchTerm, context]);

    useEffect(() => {
        // Clear all search state when switching segments
        setChannelResults([]);
        setUserResults([]);
        setOffset(0);
        setHasMore(true);
        setError(null);
        setLoading(false);
        setSubmittedTerm('');
        setSearchTerm('');
    }, [context]);

    const handleClick = (channel: Channel) => {
        setActiveChannel(channel);
        setShowDirectory(false);
    };

    return (
        <div className="directory-search-container str-chat rocktheme-community">
            <div className="directory-search-title">Directory</div>

            <div className="directory-search-segments">
                {['channels', 'users'].map((type) => (
                    <button
                        key={type}
                        className={
                            'directory-search-segment' +
                            (context === type ? ' directory-search-segment--active' : '')
                        }
                        onClick={() => setContext(type as DirectoryContextType)}
                        type="button"
                    >
                        {type.charAt(0).toUpperCase() + type.slice(1)}
                    </button>
                ))}
            </div>

            <div className="directory-search-header">
                <label className="directory-search-input-label">
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={e => setSearchTerm(e.target.value)}
                        placeholder={context === 'channels' ? 'Search channels...' : 'Search users...'}
                    />
                </label>
            </div>
            {error && <div className="error">{error}</div>}
            {channelResults.length > 0 && context === 'channels' && (
                <ChannelSearchResults
                    results={channelResults}
                    lastItemRef={lastItemRef}
                    handleClick={handleClick}
                />
            )}
            {context === 'users' && userResults.length > 0 && (
                <UserSearchResults results={userResults} />
            )}
            {context === 'channels' && !loading && !error && submittedTerm && channelResults.length === 0 && (
                <div className="directory-search-no-results">
                    No channels found.
                </div>
            )}
            {context === 'users' && !loading && !error && submittedTerm && userResults.length === 0 && (
                <div className="directory-search-no-results">
                    No users found.
                </div>
            )}
        </div>
    );
};