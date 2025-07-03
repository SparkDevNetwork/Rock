import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { MessageSearch } from '../../MessageSearch/MessageSearch';
import { MentionsList } from '../../MentionsList/MentionsList';
import { ChannelMemberList } from '../../ChannelMemberList/ChannelMemberList';
import { useChannelMemberListContext } from '../../ChannelMemberList/ChannelMemberListContext';
import { ChannelMember } from '../../ChannelMember/ChannelMember';


export const MembersPaneContent: React.FC = () => {
    const { client, channel } = useChatContext();
    const { selectedMember } = useChannelMemberListContext();

    const isMemberSelected = !!selectedMember;
    const channelPaneHeaderTitle = isMemberSelected ? 'Member Details' : 'Members';
    const channelPaneHeaderIcon = isMemberSelected ? 'fas fa-user' : 'fas fa-users';

    return (
        <div className="channel-members-content">
            <ChannelPaneHeader title={channelPaneHeaderTitle} icon={channelPaneHeaderIcon} />

            {isMemberSelected && selectedMember && (
                <ChannelMember member={selectedMember} />
            )}

            {!isMemberSelected && (
                <ChannelMemberList />
            )}
        </div>
    )
}

