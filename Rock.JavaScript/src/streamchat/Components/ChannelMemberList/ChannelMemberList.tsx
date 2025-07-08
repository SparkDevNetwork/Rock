import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useChatContext } from 'stream-chat-react';
import type { ChannelFilters, ChannelMemberResponse, MemberFilters, MessageResponse } from 'stream-chat';
import { MessageSearchResultItem } from '../MessageSearch/MessageSearchResultItem';
import { ChannelMemberListItem } from './ChannelMemberListItem';
import { useChannelMemberListContext } from './ChannelMemberListContext';

interface ChannelMemberListProps {
}

// each result from client.search has this shape

export const ChannelMemberList: React.FC<ChannelMemberListProps> = ({
}) => {
    const [results, setResults] = useState<ChannelMemberResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [offset, setOffset] = useState(0);
    const [hasMore, setHasMore] = useState(true);
    const limit = 10;
    const { client, channel } = useChatContext('MentionsList');
    const observer = useRef<IntersectionObserver | null>(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [submittedTerm, setSubmittedTerm] = useState('');
    const debounceTimeout = useRef<NodeJS.Timeout | null>(null);
    const { setSelectedUser: setSelectedMember } = useChannelMemberListContext();
    const handleLoadMore = useCallback(async () => {
        const newOffset = offset + limit;
        setOffset(newOffset);
        await fetchMembers(newOffset);
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


    const fetchMembers = async (offsetValue: number) => {
        setLoading(true);
        setError(null);

        let memberFilters: MemberFilters = {
            banned: { $eq: false },
        };

        if (searchTerm && searchTerm.trim() !== '') {
            memberFilters.name = { $autocomplete: searchTerm.trim() };
        }

        try {
            const response = await channel.queryMembers(memberFilters, { created_at: -1 }, {
                limit: limit,
                offset: offsetValue,
            });

            const members = response.members;

            let filteredMembers: ChannelMemberResponse[] = members.filter(member => {
                // Filter out members that don't have their profile listed as public
                return member.user && member.user.rock_profile_public;
            });

            if (offsetValue === 0) {
                setResults(filteredMembers);
            } else {
                setResults(prev => [...prev, ...filteredMembers]);
            }

            setHasMore(members.length === limit);
        } catch (err) {
            setError('Failed to fetch members.');
        } finally {
            setLoading(false);
        }
    }

    // Debounce search input and fetch members with $autocomplete
    useEffect(() => {
        if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        debounceTimeout.current = setTimeout(() => {
            setOffset(0);
            fetchMembers(0);
        }, 400); // match message search debounce
        return () => {
            if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        };
    }, [searchTerm]);

    const handleClick = (member: ChannelMemberResponse) => {
        if (!member.user) {
            console.warn('Member does not have a user associated with it.');
            return;
        }

        setSelectedMember(member.user);
    }

    return (
        <div className="message-search-container">
            <div className="message-search-header">
                <label className="message-search-input-label">
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={e => setSearchTerm(e.target.value)}
                        placeholder="Search members..."
                    />
                </label>
            </div>
            {error && <div className="error">{error}</div>}
            {!loading && !error && results.length === 0 && (
                <div className="message-search-no-results">
                    <div className="message-search-no-results-inner">
                        <i className="fas fa-search" aria-hidden="true" />
                        <span>No results found.</span>
                    </div>
                </div>
            )}
            {results.length > 0 && !loading && (
                <ul className="message-search-results">
                    {results.map((member, idx) => {
                        const isLast = idx === results.length - 1;
                        return (
                            <ChannelMemberListItem
                                key={member.user_id}
                                member={member}
                                ref={isLast ? lastItemRef : undefined}
                                onClick={() => handleClick(member)}
                            />
                        );
                    })}
                </ul>
            )}
        </div>
    );
};
