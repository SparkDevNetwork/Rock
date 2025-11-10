import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { ChannelMemberList } from '../../ChannelMemberList/ChannelMemberList';
import { useChannelMemberListContext } from '../../ChannelMemberList/ChannelMemberListContext';
import { PersonDetail } from '../../PersonDetail/PersonDetail';

/**
 * MembersPaneContent
 *
 * Displays the members pane for a chat channel, allowing users to view the list of channel members
 * or, if a member is selected, view detailed information about that member.
 *
 * - If a member is selected (via context), shows the PersonDetail component for that user.
 * - If no member is selected, shows the ChannelMemberList component.
 * - The header title and icon change depending on the current view.
 *
 * @returns {JSX.Element} The rendered members pane content.
 */
export const MembersPaneContent: React.FC = () => {
    // Access chat client and channel context (not directly used here, but available for future extension)
    const { client, channel } = useChatContext();
    // Access the selected user from the ChannelMemberList context
    const { selectedUser } = useChannelMemberListContext();

    // Determine if a member is currently selected
    const isMemberSelected = !!selectedUser;
    // Set the header title and icon based on selection state
    const channelPaneHeaderTitle = isMemberSelected ? 'Member Details' : 'Members';
    const channelPaneHeaderIcon = isMemberSelected ? 'fas fa-user' : 'fas fa-users';

    return (
        <div className="channel-members-content">
            {/* Header with dynamic title and icon */}
            <ChannelPaneHeader title={channelPaneHeaderTitle} icon={channelPaneHeaderIcon} />

            {/* If a member is selected, show their details; otherwise, show the member list */}
            {isMemberSelected && selectedUser && (
                // PersonDetail displays detailed info for the selected user
                <PersonDetail user={selectedUser} />
            )}

            {!isMemberSelected && (
                // ChannelMemberList displays the list of all channel members
                <ChannelMemberList />
            )}
        </div>
    )
}

