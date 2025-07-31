import React from "react";
import { UserResponse } from "stream-chat";
import { Avatar, useChatContext } from "stream-chat-react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import RockBadge from "../Badge/RockBadge";

interface PersonDetailProps {
    user: UserResponse;
}

export const PersonDetail: React.FC<PersonDetailProps> = ({ user }) => {
    if (!user) {
        return null;
    }

    interface PersonDetailAction {
        key: string;
        label: string;
        icon: string;
        onClick: () => void;
        dangerous?: boolean;
    }

    const { directMessageChannelTypeKey } = useChatConfig();
    const { client, setActiveChannel, channel } = useChatContext();
    const { closePane } = useChannelRightPane();

    const initiateDirectMessage = async (memberId: string) => {

        // Create a direct message channel with the member
        const currentUser = client.userID!;
        const channel = client.channel(directMessageChannelTypeKey, {
            members: [currentUser, memberId]
        });

        try {
            await channel.watch();

            // Set the active channel to the newly created DM channel
            if (channel.initialized) {
                setActiveChannel(channel);
                closePane();

            }
        }
        catch (error) {
            console.error("Failed to create direct message channel:", error);
        }
    }

    const shouldShowDirectMessageAction = (): boolean => {
        if (!user) {
            return false;
        }

        if (user.id === client.userID) {
            return false; // Don't allow DM with self
        }

        const openDmAllowed = user.rock_open_direct_message_allowed ?? false;
        if (!openDmAllowed) {
            return false; // Don't allow DM if user has not enabled it
        }

        // If the current channel is a direct message channel, we don't need to show the action
        if (channel && channel.type == directMessageChannelTypeKey) {
            return false;
        }

        return true;
    }

    const getRockBadges = () => {
        var streamBadges = user?.rock_badges;

        // If the streamBadges object is an array, map through it and create a new array of RockBadge components
        if (!Array.isArray(streamBadges)) {
            return <div></div>;
        }

        let rockBadgeComponents = streamBadges.map((badge: any) => {
            return (
                <RockBadge
                    badgeText={badge.Name}
                    foregroundColor={badge.ForegroundColor}
                    backgroundColor={badge.BackgroundColor}
                />
            )
        })

        return rockBadgeComponents
    }

    // Use a const for actions instead of a function
    const channelMemberActions: PersonDetailAction[] = [];
    if (user?.id && shouldShowDirectMessageAction()) {
        channelMemberActions.push({
            key: 'send-message',
            label: 'Send Message',
            icon: 'fas fa-comment',
            onClick: () => {
                initiateDirectMessage(user!.id);
            },
        });
    }

    return (
        <div className="channel-member-pane-layout">
            <div className="channel-member-header">
                <Avatar user={user} image={user.image} className="channel-member-avatar" />

                <div className="channel-member-header-title-layout">
                    <h6 className="rock-channel-member-title">{user.name || user.id}</h6>

                    <div className="member-badges-layout">
                        {getRockBadges()}
                    </div>
                </div>
            </div>
            {/* Only render actions pane if there are actions */}
            {channelMemberActions.length > 0 && (
                <div className="channel-member-actions-pane">
                    {channelMemberActions.map(action => (
                        <button
                            key={action.key}
                            className={`channel-member-action-btn channel-member-action-btn--${action.key}` + (action.dangerous ? ' channel-member-action-btn--danger' : '')}
                            onClick={action.onClick}
                            type="button">
                            <i className={action.icon} style={{ marginRight: 8 }} />
                            {action.label}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
};