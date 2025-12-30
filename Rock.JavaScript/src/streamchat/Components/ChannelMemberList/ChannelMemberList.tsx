import React, { useEffect, useRef, useState, useCallback } from 'react';
import { useChatContext } from 'stream-chat-react';
import type { ChannelMemberResponse, MemberFilters } from 'stream-chat';
import { ChannelMemberListItem } from './ChannelMemberListItem';
import { useChannelMemberListContext } from './ChannelMemberListContext';

/**
 * ChannelMemberListProps
 *
 * Props for the ChannelMemberList component. Currently empty, but can be extended for future needs.
 */
interface ChannelMemberListProps {
    // Reserved for future props
}

/**
 * ChannelMemberList
 *
 * Displays a searchable, paginated list of channel members. Supports infinite scroll (load more on scroll),
 * debounced search, and selection of a member to view details. Only members with public profiles are shown.
 *
 * - Uses Stream Chat's queryMembers API for fetching members.
 * - Debounces search input for performance.
 * - Handles loading, error, and empty states.
 * - Uses IntersectionObserver for infinite scroll.
 * - Selecting a member sets the selected user in context.
 *
 * @param props - ChannelMemberListProps (currently unused)
 * @returns {JSX.Element} The rendered member list UI.
 */
export const ChannelMemberList: React.FC<ChannelMemberListProps> = ({
}) => {
    // State for member results, loading, error, pagination, and search
    const [results, setResults] = useState<ChannelMemberResponse[]>([]); // List of fetched members
    const [loading, setLoading] = useState(true); // Loading state
    const [error, setError] = useState<string | null>(null); // Error message
    const [offset, setOffset] = useState(0); // Pagination offset
    const [hasMore, setHasMore] = useState(true); // Whether more members can be loaded
    const limit = 10; // Number of members to fetch per page
    // Use the 'MentionsList' chat context (may be a custom context for this UI)
    const { channel } = useChatContext('MentionsList');
    // Ref for the IntersectionObserver instance
    const observer = useRef<IntersectionObserver | null>(null);
    // State and ref for search term and debounce timer
    const [searchTerm, setSearchTerm] = useState('');
    const debounceTimeout = useRef<NodeJS.Timeout | null>(null);
    // Context setter for selected user (for showing details elsewhere)
    const { setSelectedUser: setSelectedMember } = useChannelMemberListContext();

    /**
     * handleLoadMore
     *
     * Loads the next page of members when the user scrolls to the bottom of the list.
     * Increments the offset and fetches more members.
     * Uses useCallback to avoid unnecessary re-renders.
     */
    const handleLoadMore = useCallback(async () => {
        const newOffset = offset + limit;
        setOffset(newOffset);
        await fetchMembers(newOffset);
    }, [offset, limit]);

    /**
     * lastItemRef
     *
     * Ref callback for the last list item. Sets up an IntersectionObserver to trigger loading more members
     * when the last item becomes visible in the viewport.
     *
     * @param node - The last list item's DOM node
     */
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

    // If no channel is available, show a fallback message
    if (!channel) {
        return <div className="message-search-container">No active channel found.</div>;
    }

    /**
     * fetchMembers
     *
     * Fetches members from the channel using Stream's queryMembers API.
     * Applies search filters and pagination. Only includes members with public profiles.
     * Handles loading, error, and updates the results state.
     *
     * @param offsetValue - The offset for pagination (0 for first page)
     */
    const fetchMembers = async (offsetValue: number) => {
        setLoading(true);
        setError(null);

        let memberFilters: MemberFilters = {
            banned: { $eq: false }, // Exclude banned members
        };

        // If a search term is provided, use autocomplete filter
        if (searchTerm && searchTerm.trim() !== '') {
            memberFilters.name = { $autocomplete: searchTerm.trim() };
        }

        try {
            const response = await channel.queryMembers(memberFilters, { created_at: -1 }, {
                limit: limit,
                offset: offsetValue,
            });

            const members = response.members;

            // Only include members with public profiles
            let filteredMembers: ChannelMemberResponse[] = members.filter(member => {
                return member.user && member.user.rock_profile_public;
            });

            // If this is the first page, replace results; otherwise, append
            if (offsetValue === 0) {
                setResults(filteredMembers);
            } else {
                setResults(prev => [...prev, ...filteredMembers]);
            }

            // If fewer than limit returned, no more members to load
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
        }, 400); // Match message search debounce
        return () => {
            if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
        };
    }, [searchTerm]);

    /**
     * handleClick
     *
     * Handles clicking on a member in the list. Sets the selected user in context for detail view.
     *
     * @param member - The ChannelMemberResponse for the clicked member
     */
    const handleClick = (member: ChannelMemberResponse) => {
        if (!member.user) {
            console.warn('Member does not have a user associated with it.');
            return;
        }
        setSelectedMember(member.user);
    }

    return (
        <div className="message-search-container">
            {/* Search input for filtering members by name */}
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
            {/* Error message if fetching members failed */}
            {error && <div className="error">{error}</div>}
            {/* No results message if search yields nothing */}
            {!loading && !error && results.length === 0 && (
                <div className="message-search-no-results">
                    <div className="message-search-no-results-inner">
                        <i className="fas fa-search" aria-hidden="true" />
                        <span>No results found.</span>
                    </div>
                </div>
            )}
            {/* Render the list of members if available */}
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
