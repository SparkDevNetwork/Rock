import React, { useRef, useState } from 'react';
import clsx from 'clsx';
import {
    ChannelPreviewUIComponentProps,
    DefaultStreamChatGenerics,
    DialogManagerProvider,
    useChatContext,
    useComponentContext,
    useDialogIsOpen,
} from 'stream-chat-react';
import { Avatar as DefaultAvatar } from 'stream-chat-react';
import { RockChannelPreviewActionButtons } from './RockChannelActionButtons';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
const ChannelPreviewContent = <
    SCG extends DefaultStreamChatGenerics = DefaultStreamChatGenerics,
>(
    props: ChannelPreviewUIComponentProps<SCG>
) => {
    const {
        active,
        Avatar = DefaultAvatar,
        channel,
        className: customClassName = '',
        displayImage,
        displayTitle,
        groupChannelDisplayInfo,
        latestMessagePreview,
        onSelect: customOnSelectChannel,
        setActiveChannel,
        unread,
        watchers,
    } = props;

    const { ChannelPreviewActionButtons = RockChannelPreviewActionButtons } =
        useComponentContext<SCG>();

    const chatConfig = useChatConfig();
    const channelPreviewButton = useRef<HTMLButtonElement | null>(null);
    const isMuted = channel.muteStatus().muted;

    const { client } = useChatContext();
    const currentUserId = client.userID;

    // compute title using DefaultChatChannelNamer if displayTitle is empty
    const title =
        displayTitle ?? DefaultChatChannelNamer(channel, chatConfig.directMessageChannelTypeKey!, currentUserId);

    // use title for avatar fallback
    const avatarName = title || channel.state.messages?.at(-1)?.user?.id;

    const [isHovered, setIsHovered] = useState(false);
    const dialogId = `channel-actions-${channel.id}`;
    const isDialogOpen = useDialogIsOpen(dialogId);
    const showActions = isHovered || isDialogOpen;

    const onSelectChannel = (e: React.MouseEvent<HTMLButtonElement>) => {
        if (customOnSelectChannel) {
            customOnSelectChannel(e);
        } else if (setActiveChannel && channel) {
            setActiveChannel(channel, watchers);
        }
        channelPreviewButton.current?.blur();
    };

    return (
        <div
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
            className="str-chat__channel-preview-container">
            <div className="rock-channel-preview-container">
                <button
                    aria-label={`Select Channel: ${displayTitle || ''}`}
                    aria-selected={active}
                    data-testid="channel-preview-button"
                    onClick={onSelectChannel}
                    ref={channelPreviewButton}
                    className={clsx(
                        'str-chat__channel-preview-messenger str-chat__channel-preview',
                        active && 'str-chat__channel-preview-messenger--active',
                        unread && unread >= 1 && 'str-chat__channel-preview-messenger--unread',
                        isMuted && 'str-chat__channel-preview--muted',
                        customClassName,
                    )}
                    role='option'>
                    <div className='str-chat__channel-preview-messenger--left'>
                        <Avatar
                            className="str-chat__avatar--channel-preview"
                            groupChannelDisplayInfo={groupChannelDisplayInfo}
                            image={displayImage}
                            name={avatarName}
                        />
                    </div>

                    <div className="str-chat__channel-preview-end-row">
                        <div className="str-chat__channel-preview-text">
                            <div className="str-chat__channel-preview-end-first-row">
                                <div className="str-chat__channel-preview-messenger--name">
                                    <span>
                                        {title}
                                        {isMuted && <span title="Muted"> 🔇</span>}
                                    </span>
                                </div>
                                {!!unread && (
                                    <div className="str-chat__channel-preview-unread-badge">
                                        {unread}
                                    </div>
                                )}
                            </div>
                            <div className="str-chat__channel-preview-messenger--last-message">
                                {isMuted ? (
                                    <span title="Muted">Channel is muted</span>
                                ) : (
                                    <span>{latestMessagePreview}</span>
                                )}
                            </div>
                        </div>

                        {showActions && (
                            <div className="channel-preview-action-buttons-inline">
                                <ChannelPreviewActionButtons channel={channel} />
                            </div>
                        )}
                    </div>
                </button>
            </div>
        </div>
    );
};

const UnMemoizedChannelPreviewMessenger = <
    SCG extends DefaultStreamChatGenerics = DefaultStreamChatGenerics,
>(
    props: ChannelPreviewUIComponentProps<SCG>
) => (
    <DialogManagerProvider>
        <ChannelPreviewContent {...props} />
    </DialogManagerProvider>
);

export const RockChannelPreview = React.memo(
    UnMemoizedChannelPreviewMessenger,
) as typeof UnMemoizedChannelPreviewMessenger;
