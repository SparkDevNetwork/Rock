import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { MessageSearch } from '../../MessageSearch/MessageSearch';
import { MentionsList } from '../../MentionsList/MentionsList';
import { ChannelMemberList } from '../../ChannelMemberList/ChannelMemberList';
import { useChannelMemberListContext } from '../../ChannelMemberList/ChannelMemberListContext';
import { PersonDetail } from '../../PersonDetail/PersonDetail';


export const MembersPaneContent: React.FC = () => {
    const { client, channel } = useChatContext();
    const { selectedUser } = useChannelMemberListContext();

    const isMemberSelected = !!selectedUser;
    const channelPaneHeaderTitle = isMemberSelected ? 'Member Details' : 'Members';
    const channelPaneHeaderIcon = isMemberSelected ? 'fas fa-user' : 'fas fa-users';

    return (
        <div className="channel-members-content">
            <ChannelPaneHeader title={channelPaneHeaderTitle} icon={channelPaneHeaderIcon} />

            {isMemberSelected && selectedUser && (
                // <ChannelMember member={selectedMember} />
                <PersonDetail user={selectedUser} />
            )}

            {!isMemberSelected && (
                <ChannelMemberList />
            )}
        </div>
    )
}

