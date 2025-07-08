import React, { useMemo, useState } from "react";
import {
    MessageContextValue,
    useChatContext,
    Avatar as DefaultAvatar,
    Attachment as DefaultAttachment,
    MessageDeleted as DefaultMessageDeleted,
    ReactionsList as DefaultReactionList,
    StreamedMessageText as DefaultStreamedMessageText,
    MML,
    messageHasReactions,
    isMessageBounced,
    isMessageEdited,
    Poll,
    MessageText,
    useComponentContext,
    EditMessageModal,
    useDialog,
    useDialogIsOpen,
    DialogAnchor,
    ReactionSelector,
    useMessageComposer,
} from 'stream-chat-react';
import { isMessageBlocked } from './utils';
import { MessageBlocked as DefaultMessageBlocked } from './MessageBlocked';
import { MessageBouncePrompt as DefaultMessageBouncePrompt } from './MessageBouncePrompt';
import { MessageBounceModal } from './MessageBounceModal';
import RockBadge from '../Badge/RockBadge';
import { RockMessageThreadReplyContent } from './RockMessageThreadReplyContent';
import { RockMessageTimestamp } from "./RockMessageTimestamp";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import { ParentMessageThreadPreview } from "./ParentMessageThreadPreview";
import { useChannelMemberListContext } from "../ChannelMemberList/ChannelMemberListContext";

/**
 * CommunityMessage
 *
 * Renders a message in the community chat style. Handles all message actions, reactions, quoting,
 * reply-in-channel, and user profile popups. Uses Stream Chat context and custom hooks for state and configuration.
 *
 * Props:
 *   - All message context and UI props from Stream Chat's MessageContextValue.
 *
 * Features:
 *   - Handles deleted/blocked messages, AI-generated messages, attachments, polls, and MML.
 *   - Supports quoting, replying, reactions, and more options.
 *   - Shows user badges, timestamps, and profile popups.
 *   - Handles bounce dialog and edit modal.
 *
 * @param props - MessageContextValue (all message and UI context from Stream Chat)
 * @returns {JSX.Element}
 */
export const CommunityMessage = (props: MessageContextValue) => {
    // Destructure all needed props from the message context
    const {
        additionalMessageInputProps,
        editing,
        handleAction,
        handleOpenThread,
        handleRetry,
        isMessageAIGenerated,
        message,
        onUserHover,
        renderText,
        threadList,
    } = props;

    // Chat client and context
    const { client } = useChatContext('RockMessage');
    // State for bounce dialog and edit timestamp
    const [isBounceDialogOpen, setIsBounceDialogOpen] = useState(false);
    const [, setEditedTimestampOpen] = useState(false);

    // Component overrides from context
    const {
        Attachment = DefaultAttachment,
        Avatar = DefaultAvatar,
        MessageDeleted = DefaultMessageDeleted,
        MessageBouncePrompt = DefaultMessageBouncePrompt,
        ReactionsList = DefaultReactionList,
        StreamedMessageText = DefaultStreamedMessageText,
    } = useComponentContext('MessageSimple');

    // Derived message state
    const hasReactions = messageHasReactions(message);
    const isAIGenerated = useMemo(
        () => isMessageAIGenerated?.(message),
        [isMessageAIGenerated, message],
    );

    // Handle deleted or blocked messages
    if (message.deleted_at || message.type === 'deleted') {
        return <MessageDeleted message={message} />;
    }
    if (isMessageBlocked(message)) {
        return <DefaultMessageBlocked />;
    }

    // Reply, retry, and bounce logic
    const showReplyCountButton = !threadList && !!message.reply_count;
    const allowRetry = message.status === 'failed';
    const isBounced = isMessageBounced(message);
    const isEdited = isMessageEdited(message) && !isAIGenerated;

    // Click handler for retry, bounce, or edit
    let handleClick: (() => void) | undefined = undefined;
    if (allowRetry) {
        handleClick = () => handleRetry(message);
    } else if (isBounced) {
        handleClick = () => setIsBounceDialogOpen(true);
    } else if (isEdited) {
        handleClick = () => setEditedTimestampOpen((prev) => !prev);
    }

    // Custom thread open handler (opens right pane)
    const { setActivePane } = useChannelRightPane();
    const customHandleOpenThread = (e: React.BaseSyntheticEvent) => {
        handleOpenThread(e);
        setActivePane('threads');
    };

    // Poll support
    const poll = message.poll_id && client.polls.fromState(message.poll_id);

    /**
     * getRockBadges
     *
     * Renders all RockBadge components for the user.
     * @returns {JSX.Element[]}
     */
    const getRockBadges = () => {
        var streamBadges = message?.user?.rock_badges;
        if (!Array.isArray(streamBadges)) return <div></div>;
        return streamBadges.map((badge: any) => (
            <RockBadge
                key={badge.Name}
                badgeText={badge.Name}
                foregroundColor={badge.ForegroundColor}
                backgroundColor={badge.BackgroundColor}
            />
        ));
    };

    // Guard: no user
    if (!message.user) {
        return <></>;
    }

    // Reaction dialog state
    const reactionDialogId = `reaction-selector--${message.id}`;
    const reactionDialog = useDialog({ id: reactionDialogId });
    const reactionDialogIsOpen = useDialogIsOpen(reactionDialogId);
    const [reactionButtonRef, setReactionButtonRef] = useState<HTMLButtonElement | null>(null);

    // Message actions for the action bar
    interface MessageAction {
        key: string;
        icon: string;
        title: string;
        onClick: (e: React.BaseSyntheticEvent) => void;
    }
    const composer = useMessageComposer();
    const handleQuote = () => {
        composer.setQuotedMessage(message);
        const elements = message.parent_id
            ? document.querySelectorAll('.str-chat__thread .str-chat__textarea__textarea')
            : document.getElementsByClassName('str-chat__textarea__textarea');
        const textarea = elements.item(0);
        if (textarea instanceof HTMLTextAreaElement) {
            textarea.focus();
        }
    };
    const getSupportedMessageActions = (): MessageAction[] => [
        {
            key: 'react',
            icon: 'fas fa-smile',
            title: 'Add Reaction',
            onClick: () => reactionDialog.toggle(),
        },
        {
            key: 'quote',
            icon: 'fas fa-quote-right',
            title: 'Quote',
            onClick: () => handleQuote(),
        },
        {
            key: 'reply',
            icon: 'fas fa-reply',
            title: 'Reply in Thread',
            onClick: (e) => customHandleOpenThread(e),
        },
        {
            key: 'more',
            icon: 'fas fa-ellipsis-v',
            title: 'More Options',
            onClick: () => { /* Add more options logic here */ },
        },
    ];
    const messageActions = getSupportedMessageActions();

    // Show reply-in-channel preview if this is a reply
    const showIsReplyInChannel = !threadList && message.show_in_channel && message.parent_id;

    // User profile popup logic
    const { setSelectedUser } = useChannelMemberListContext();
    const showUserProfile = () => {
        const user = message.user;
        if (user) {
            setSelectedUser(user);
            setActivePane('members');
        } else {
            console.warn(`User not found for message ID: ${message.id}`);
        }
    };

    // Main render
    return (
        <>
            <div
                className="rock-message-wrapper"
                data-dialog-open={reactionDialogIsOpen || undefined}
                onClick={handleClick}
            >
                {/* Edit message modal */}
                {editing && (
                    <EditMessageModal additionalMessageInputProps={additionalMessageInputProps} />
                )}
                {/* Bounce dialog modal */}
                {isBounceDialogOpen && (
                    <MessageBounceModal
                        MessageBouncePrompt={MessageBouncePrompt}
                        onClose={() => setIsBounceDialogOpen(false)}
                        open={isBounceDialogOpen}
                    />
                )}
                <div className="rock-message-container-layout">
                    <div className="rock-message-container">
                        {/* User avatar */}
                        {message.user && (
                            <div className="rock-avatar-container">
                                <Avatar
                                    image={message.user.image}
                                    name={message.user.name || message.user.id}
                                    onClick={showUserProfile}
                                    onMouseOver={onUserHover}
                                    user={message.user}
                                />
                            </div>
                        )}
                        <div className="rock-message-layout">
                            <div className="rock-message-title-layout">
                                {/* User name and badges */}
                                <span className='rock-message-author'>
                                    {message.user!.name || message.user!.id}
                                </span>
                                <div className='rock-message-badges'>
                                    {getRockBadges() as React.ReactNode}
                                </div>
                                {/* Timestamp */}
                                <div className="rock-message-time">
                                    <RockMessageTimestamp
                                        customClass='rock-message-simple-timestamp'
                                        message={message}
                                    />
                                </div>
                            </div>
                            <div className="rock-message-content">
                                {/* Poll, reply preview, attachments, text, MML, reactions, reply count, failed icon */}
                                {poll && <Poll poll={poll} />}
                                {showIsReplyInChannel && (
                                    <div>
                                        <ParentMessageThreadPreview />
                                    </div>
                                )}
                                {message.attachments?.length && !message.quoted_message ? (
                                    <Attachment
                                        actionHandler={handleAction}
                                        attachments={message.attachments}
                                    />
                                ) : null}
                                {isAIGenerated ? (
                                    <StreamedMessageText message={message} renderText={renderText} />
                                ) : (
                                    <MessageText message={message} renderText={renderText} />
                                )}
                                {message.mml && (
                                    <MML
                                        actionHandler={handleAction}
                                        align={'left'}
                                        source={message.mml}
                                    />
                                )}
                                <div className='str-chat__message-reactions-host'>
                                    {hasReactions && <ReactionsList reverse />}
                                </div>
                                {showReplyCountButton && (
                                    <RockMessageThreadReplyContent
                                        onClick={customHandleOpenThread}
                                        reply_count={message.reply_count}
                                    />
                                )}
                                <div className="rock-chat-message-send-failed-icon">
                                    <i className='fa fa-exclamation-circle' />
                                </div>
                            </div>
                        </div>
                    </div>
                    {/* Only show message actions if this is not a reply from a thread. */}
                    {!showIsReplyInChannel && (
                        <div className="rock-message-actions">
                            {messageActions.map((action) => (
                                <button
                                    key={action.key}
                                    ref={action.key === 'react' ? setReactionButtonRef : undefined}
                                    className="rock-message-action"
                                    title={action.title}
                                    onClick={(e) => action.onClick(e)}
                                    aria-expanded='true'
                                    aria-haspopup='true'
                                    aria-label="aria/Open Message Action Menu">
                                    <i className={action.icon} />
                                </button>
                            ))}
                            {reactionDialogIsOpen && (
                                <DialogAnchor
                                    id={reactionDialogId}
                                    placement={"bottom-start"}
                                    referenceElement={reactionButtonRef}
                                    trapFocus={false}>
                                    <ReactionSelector />
                                </DialogAnchor>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};
