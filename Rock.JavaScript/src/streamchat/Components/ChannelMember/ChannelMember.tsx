import React from "react";
import { ChannelMemberResponse } from "stream-chat";
import { Avatar, useChatContext } from "stream-chat-react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import RockBadge from "../Badge/RockBadge";

interface ChannelMemberProps {
    member: ChannelMemberResponse;
}

export const ChannelMember: React.FC<ChannelMemberProps> = ({ member }) => {
    if (!member || !member.user) {
        return null;
    }

    interface ChannelMemberAction {
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

        console.log(channel);

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
        if (!member.user) {
            return false;
        }

        if (member.user.id === client.userID) {
            return false; // Don't allow DM with self
        }

        const openDmAllowed = member.user!.rock_open_direct_message_allowed ?? false;
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
        var streamBadges = member.user?.rock_badges;

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

    const channelMemberActions = (): ChannelMemberAction[] => {
        const actions: ChannelMemberAction[] = [];

        if (!member.user?.id) {
            return actions;
        }

        if (shouldShowDirectMessageAction()) {
            actions.push({
                key: 'send-message',
                label: 'Send Message',
                icon: 'fas fa-comment',
                onClick: () => {
                    initiateDirectMessage(member.user!.id);
                },
            });
        }

        return actions;
    };

    return (
        <div className="channel-member-pane-layout">
            <div className="channel-member-header">
                <Avatar user={member.user} image={member.user.image} className="channel-member-avatar" />

                <div className="channel-member-header-title-layout">
                    <h6 className="rock-channel-member-title">{member.user.name || member.user.id}</h6>

                    <div className="member-badges-layout">
                        {getRockBadges()}
                    </div>
                </div>
            </div>
            <div className="channel-member-actions-pane">
                {channelMemberActions().map(action => (
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
        </div>
    );
};