import React, { useLayoutEffect, useMemo, useState } from "react";
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
import { MessageBouncePrompt as DefaultMessageBouncePrompt } from './MessageBouncePrompt';
import { MessageBounceModal } from './MessageBounceModal';
import RockBadge from '../Badge/RockBadge';
import { RockMessageThreadReplyContent } from './RockMessageThreadReplyContent';
import { RockMessageTimestamp } from "./RockMessageTimestamp";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import { ParentMessageThreadPreview } from "./ParentMessageThreadPreview";
import { useChannelMemberListContext } from "../ChannelMemberList/ChannelMemberListContext";
import type { Placement } from '@popperjs/core';
import { useModal } from "../Modal/ModalContext";

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
        handleEdit,
        handleDelete,
        handleRetry,
        isMessageAIGenerated,
        message,
        onUserHover,
        renderText,
        threadList,
        isMyMessage
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

    // Reaction dialog state
    const reactionDialogId = `reaction-selector--${message.id}`;
    const reactionDialog = useDialog({ id: reactionDialogId });
    const reactionDialogIsOpen = useDialogIsOpen(reactionDialogId);
    const [reactionButtonRef, setReactionButtonRef] = useState<HTMLButtonElement | null>(null);

    // More actions dialog state
    const moreActionsDialogId = `message-more-actions--${message.id}`;
    const moreActionsDialog = useDialog({ id: moreActionsDialogId });
    const moreActionsDialogIsOpen = useDialogIsOpen(moreActionsDialogId);
    const [moreActionsButtonRef, setMoreActionsButtonRef] = useState<HTMLButtonElement | null>(null);

    // Verify delete modal state
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const { showModal, hideModal } = useModal();

    // Message actions for the action bar
    interface MessageAction {
        key: string;
        icon: string;
        title: string;
        onClick: (e: React.BaseSyntheticEvent) => void;
        destructive?: boolean;
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
    const getSupportedMessageActions = (): MessageAction[] => {
        const actions: MessageAction[] = [
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
        ];
        // Only show the thread reply action if not currently in a thread
        if (!threadList) {
            actions.push({
                key: 'reply',
                icon: 'fas fa-reply',
                title: 'Reply in Thread',
                onClick: (e) => customHandleOpenThread(e),
            });
        }

        if (isMyMessage()) {
            actions.push({
                key: 'more',
                icon: 'fas fa-ellipsis-v',
                title: 'More Options',
                onClick: () => moreActionsDialog.toggle(),
            });
        }

        return actions;
    };
    const messageActions = getSupportedMessageActions();

    const getMoreActionsMessageActions = (): MessageAction[] => {
        var actions: MessageAction[] = [];

        // Add the edit action if the user composed the message
        if (isMyMessage()) {
            actions.push({
                key: 'edit',
                icon: 'fas fa-pencil-alt',
                title: 'Edit Message',
                onClick: (e) => {
                    handleEdit(e);
                }
            });

            actions.push({
                key: 'delete',
                icon: 'fas fa-trash',
                title: 'Delete Message',
                onClick: (e) => {
                    showModal({
                        title: 'Delete Message',
                        content: (
                            <div className="delete-message-modal-content">
                                <p className="delete-message-modal-text">Are you sure you want to delete this message?</p>

                                <div className="delete-message-modal-actions">
                                    <button
                                        className="btn btn-secondary"
                                        onClick={() => hideModal()}>
                                        Cancel
                                    </button>

                                    <button
                                        className="btn btn-danger"
                                        onClick={async () => {
                                            try {
                                                await handleDelete(e);
                                                hideModal();
                                            } catch (error) {
                                                console.error("Failed to delete message", error);
                                            }
                                        }}>
                                        Delete
                                    </button>
                                </div>
                            </div>
                        )
                    })
                },
                destructive: true,
            });
        }

        return actions;
    }
    const moreActionsMessageActions: MessageAction[] = getMoreActionsMessageActions();

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

    function determineVerticalPlacement(el: HTMLElement): Placement {
        const rect = el.getBoundingClientRect();
        const spaceAbove = rect.top;
        const spaceBelow = window.innerHeight - rect.bottom;
        // If there’s more room below, place “bottom”, otherwise “top”
        return spaceBelow >= spaceAbove ? 'bottom-start' : 'top-start';
    }

    // In your component:
    const [placement, setPlacement] = useState<Placement>('bottom');
    useLayoutEffect(() => {
        if (moreActionsButtonRef) {
            setPlacement(determineVerticalPlacement(moreActionsButtonRef));
        }
    }, [moreActionsButtonRef, moreActionsDialogIsOpen]);

    // Poll support
    const poll = message.poll_id && client.polls.fromState(message.poll_id);

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

    // Main render
    return (
        <>
            <div
                className="rock-message-wrapper"
                data-dialog-open={reactionDialogIsOpen || moreActionsDialogIsOpen || undefined}
                onClick={handleClick}>
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
                                    ref={
                                        action.key === 'react'
                                            ? setReactionButtonRef
                                            : action.key === 'more'
                                                ? setMoreActionsButtonRef
                                                : undefined
                                    }
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
                                    placement={placement}
                                    referenceElement={reactionButtonRef}
                                    trapFocus={false}>
                                    <ReactionSelector />
                                </DialogAnchor>
                            )}

                            {moreActionsDialogIsOpen && (
                                <DialogAnchor
                                    id={moreActionsDialogId}
                                    placement={placement}

                                    referenceElement={moreActionsButtonRef}
                                    trapFocus={false}>
                                    <div className="rock-message-more-actions">
                                        {moreActionsMessageActions.map((action) => (
                                            <button
                                                key={action.key}
                                                className={`rock-message-more-action ${action.destructive ? 'rock-message-more-action--destructive' : ''}`}
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    action.onClick(e);
                                                }}
                                            >
                                                <i className={action.icon} />
                                                {action.title}
                                            </button>
                                        ))}
                                    </div>
                                </DialogAnchor>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};
