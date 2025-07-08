// RockChannelPreview.tsx
//
// This file defines the RockChannelPreview component, a custom channel preview UI for Stream Chat channels.
// It supports both conversational and community chat styles, and provides a detailed, accessible, and interactive
// preview of a chat channel, including avatar, title, unread badge, mute status, and contextual action buttons.
//
// Author: [Your Name]
// Date: [Today's Date]

import React, { useRef, useState } from 'react';
import clsx from 'clsx';
import {
    ChannelPreviewUIComponentProps, // Props for channel preview UI components from stream-chat-react
    DialogManagerProvider, // Provider for dialog management context
    useChatContext, // Hook to access chat client and user context
    useComponentContext, // Hook to access custom component overrides
    useDialogIsOpen, // Hook to check if a dialog is open by ID
} from 'stream-chat-react';
import { Avatar as DefaultAvatar } from 'stream-chat-react';
import { RockChannelPreviewActionButtons } from './RockChannelActionButtons';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../../ChatViewStyle';
import { useDirectoryContext } from '../Directory/DirectoryContext';

/**
 * ChannelPreviewContent
 *
 * Renders the main content for a channel preview, supporting both conversational and community chat styles.
 * Handles hover state, dialog state, and channel selection logic. Displays avatar, title, unread badge, mute status,
 * and contextual action buttons (mute, leave, hide, etc.).
 *
 * @param props - ChannelPreviewUIComponentProps from stream-chat-react
 * @returns JSX.Element
 */
const ChannelPreviewContent = (
    props: ChannelPreviewUIComponentProps
) => {
    // Destructure props with defaults and documentation
    const {
        active, // Whether this channel is currently active/selected
        Avatar = DefaultAvatar, // Avatar component to use (can be overridden)
        channel, // The Stream Chat channel object
        className: customClassName = '', // Additional CSS class for styling
        displayImage, // Optional image URL for the channel avatar
        displayTitle, // Optional display title for the channel
        groupChannelDisplayInfo, // Info for group channel avatars
        latestMessagePreview, // Text preview of the latest message
        onSelect: customOnSelectChannel, // Optional custom channel select handler
        setActiveChannel, // Function to set the active channel
        unread, // Number of unread messages
        watchers, // Optional watcher info
    } = props;

    // Allow for custom action buttons via component context, defaulting to RockChannelPreviewActionButtons
    const { ChannelPreviewActionButtons = RockChannelPreviewActionButtons } =
        useComponentContext();

    // Access chat configuration (e.g., style, DM channel type key)
    const chatConfig = useChatConfig();
    // Ref for the channel preview button (for focus management)
    const channelPreviewButton = useRef<HTMLButtonElement | null>(null);
    // Determine if the channel is muted for the current user
    const isMuted = channel.muteStatus().muted;

    // Access chat client and current user ID
    const { client } = useChatContext();
    const currentUserId = client.userID;

    // Compute the display title using DefaultChatChannelNamer if not provided
    const title =
        displayTitle ?? DefaultChatChannelNamer(
            channel,
            chatConfig.directMessageChannelTypeKey!,
            currentUserId,
            chatConfig.chatViewStyle
        );

    // Use the title as a fallback for the avatar name
    const avatarName = title || channel.state.messages?.at(-1)?.user?.id;

    // Hover state for showing action buttons
    const [isHovered, setIsHovered] = useState(false);
    // Dialog ID for contextual action menu
    const dialogId = `channel-actions-${channel.id}`;
    // Whether the dialog is currently open
    const isDialogOpen = useDialogIsOpen(dialogId);
    // Show action buttons if hovered or dialog is open
    const showActions = isHovered || isDialogOpen;

    // Directory context for toggling directory view
    const { setShowDirectory } = useDirectoryContext();

    /**
     * onSelectChannel
     * Handles channel selection logic, including hiding the directory and calling custom or default handlers.
     *
     * @param e - Mouse event from the button click
     */
    const onSelectChannel = (e: React.MouseEvent<HTMLButtonElement>) => {
        setShowDirectory(false); // Hide the directory when a channel is selected
        if (customOnSelectChannel) {
            // Use custom handler if provided
            customOnSelectChannel(e);
        } else if (setActiveChannel && channel) {
            // Otherwise, set the active channel using provided function
            setActiveChannel(channel, watchers);
        }
        // Remove focus from the button for accessibility
        channelPreviewButton.current?.blur();
    };

    // Determine which component style to use (community or conversational)
    const useCommunityComponent = chatConfig.chatViewStyle == ChatViewStyle.Community;

    /**
     * conversationalComponent
     *
     * Renders the conversational style channel preview, including avatar, title, unread badge, mute status,
     * latest message preview, and inline action buttons (shown on hover or dialog open).
     *
     * @returns JSX.Element
     */
    const conversationalComponent = () => {
        return (
            <div
                onMouseEnter={() => setIsHovered(true)} // Show actions on hover
                onMouseLeave={() => setIsHovered(false)} // Hide actions when not hovered
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
                        {/* Left section: Avatar */}
                        <div className='str-chat__channel-preview-messenger--left'>
                            <Avatar
                                className="str-chat__avatar--channel-preview"
                                groupChannelDisplayInfo={groupChannelDisplayInfo}
                                image={displayImage}
                                name={avatarName}
                            />
                        </div>

                        {/* Right section: Title, unread badge, last message, and actions */}
                        <div className="str-chat__channel-preview-end-row">
                            <div className="str-chat__channel-preview-text">
                                <div className="str-chat__channel-preview-end-first-row">
                                    <div className="str-chat__channel-preview-messenger--name">
                                        <span>
                                            {title}
                                            {/* Show mute icon if channel is muted */}
                                            {isMuted && <span title="Muted"> 🔇</span>}
                                        </span>
                                    </div>
                                    {/* Unread badge if there are unread messages */}
                                    {!!unread && (
                                        <div className="str-chat__channel-preview-unread-badge">
                                            {unread}
                                        </div>
                                    )}
                                </div>
                                <div className="str-chat__channel-preview-messenger--last-message">
                                    {/* Show muted message if muted, otherwise show latest message preview */}
                                    {isMuted ? (
                                        <span title="Muted">Channel is muted</span>
                                    ) : (
                                        <span>{latestMessagePreview}</span>
                                    )}
                                </div>
                            </div>

                            {/* Inline action buttons (mute, leave, hide, etc.), shown on hover or dialog open */}
                            <div className="channel-preview-actions-wrapper">
                                {showActions && (
                                    <div className="channel-preview-action-buttons-inline">
                                        <ChannelPreviewActionButtons channel={channel} />
                                    </div>
                                )}
                            </div>
                        </div>
                    </button>
                </div>
            </div>
        )
    }

    /**
     * communityComponent
     *
     * Renders the community style channel preview, a simplified version with avatar and title only.
     *
     * @returns JSX.Element
     */
    const communityComponent = () => {
        return (
            <div
                onMouseEnter={() => setIsHovered(true)}
                onMouseLeave={() => setIsHovered(false)}
                className="rock-channel-preview-container">
                <button
                    aria-label={`Select Channel: ${displayTitle || ''}`}
                    aria-selected={active}
                    data-testid="channel-preview-button"
                    onClick={onSelectChannel}
                    ref={channelPreviewButton}
                    className={clsx(
                        'rock-channel-preview',
                        active && 'rock-channel-preview-container--active',
                        unread && unread >= 1 && 'rock-channel-preview-container--unread',
                        isMuted && 'rock-channel-preview-container--muted',
                        customClassName,
                    )}
                    role='option'>
                    <div className="rock-channel-preview-content">
                        <Avatar
                            className="rock-channel-preview-avatar str-chat__avatar--channel-preview "
                            groupChannelDisplayInfo={groupChannelDisplayInfo}
                            image={displayImage}
                            name={avatarName}
                        />
                        {/* Title and mute icon */}
                        <div className="rock-channel-preview-title str-chat__channel-preview-messenger--name">
                            <span>
                                {title}
                                {isMuted && <span title="Muted"> 🔇</span>}
                            </span>
                        </div>
                    </div>
                </button>
            </div>
        )
    }

    // Render the appropriate component style based on chat configuration
    return useCommunityComponent ? (
        communityComponent()
    ) : (
        conversationalComponent()
    );

};

/**
 * UnMemoizedChannelPreviewMessenger
 *
 * Wraps ChannelPreviewContent in a DialogManagerProvider for dialog context support.
 *
 * @param props - ChannelPreviewUIComponentProps
 * @returns JSX.Element
 */
const UnMemoizedChannelPreviewMessenger = (
    props: ChannelPreviewUIComponentProps
) => (
    <DialogManagerProvider>
        <ChannelPreviewContent {...props} />
    </DialogManagerProvider>
);

/**
 * RockChannelPreview
 *
 * Memoized channel preview component for performance. Use this as the main export.
 *
 * @param props - ChannelPreviewUIComponentProps
 * @returns JSX.Element
 */
export const RockChannelPreview = React.memo(
    UnMemoizedChannelPreviewMessenger,
) as typeof UnMemoizedChannelPreviewMessenger;
