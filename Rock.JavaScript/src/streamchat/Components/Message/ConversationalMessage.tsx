import React, { useMemo, useState } from "react";
import {
    MessageContextValue,
    useChatContext,
    useTranslationContext,
    Avatar as DefaultAvatar,
    Attachment as DefaultAttachment,
    EditMessageForm as DefaultEditMessageForm,
    MessageOptions as DefaultMessageOptions,
    MessageDeleted as DefaultMessageDeleted,
    MessageRepliesCountButton as DefaultMessageRepliesCountButton,
    MessageStatus as DefaultMessageStatus,
    ReactionsList as DefaultReactionList,
    StreamedMessageText as DefaultStreamedMessageText,
    MML,
    messageHasAttachments,
    messageHasReactions,
    isMessageBounced,
    isMessageEdited,
    Modal,
    MessageInput,
    Poll,
    MessageText,
    MessageErrorIcon,
    useComponentContext,
    MessageTimestamp as DefaultMessageTimestamp,
} from 'stream-chat-react';
import clsx from 'clsx';
import { isMessageBlocked } from './utils';
import { MessageBlocked as DefaultMessageBlocked } from './MessageBlocked';
import { MessageBouncePrompt as DefaultMessageBouncePrompt } from './MessageBouncePrompt';
import { MessageEditedTimestamp } from './MessageEditedTimestamp';
import { MessageBounceModal } from './MessageBounceModal';
import RockBadge from '../Badge/RockBadge';

/**
 * ConversationalMessage
 *
 * Renders a message in the conversational chat style. Handles all message actions, reactions, editing,
 * retry, bounce dialog, attachments, polls, and metadata. Uses Stream Chat context and custom hooks for state and configuration.
 *
 * Props:
 *   - All message context and UI props from Stream Chat's MessageContextValue.
 *
 * Features:
 *   - Handles deleted/blocked messages, AI-generated messages, attachments, polls, and MML.
 *   - Supports editing, retry, bounce dialog, and message metadata.
 *   - Shows user badges, timestamps, and profile popups.
 *
 * @param props - MessageContextValue (all message and UI context from Stream Chat)
 * @returns {JSX.Element}
 */
export const ConversationalMessage = (props: MessageContextValue) => {
    // Destructure all needed props from the message context
    const {
        additionalMessageInputProps,
        clearEditingState,
        editing,
        endOfGroup,
        firstOfGroup,
        groupedByUser,
        handleAction,
        handleOpenThread,
        handleRetry,
        highlighted,
        isMessageAIGenerated,
        isMyMessage,
        message,
        onUserClick,
        onUserHover,
        renderText,
        threadList,
    } = props;

    // Guard: no user
    if (!message.user) {
        return <></>;
    }

    // Chat client and translation context
    const { client } = useChatContext('RockMessage');
    const { t } = useTranslationContext('RockMessage');
    // State for bounce dialog and edit timestamp
    const [isBounceDialogOpen, setIsBounceDialogOpen] = useState(false);
    const [isEditedTimestampOpen, setEditedTimestampOpen] = useState(false);

    // Component overrides from context
    const {
        Attachment = DefaultAttachment,
        Avatar = DefaultAvatar,
        EditMessageInput = DefaultEditMessageForm,
        MessageOptions = DefaultMessageOptions,
        MessageActions = MessageOptions, // TODO: remove passthrough in next major release
        MessageDeleted = DefaultMessageDeleted,
        MessageBouncePrompt = DefaultMessageBouncePrompt,
        MessageRepliesCountButton = DefaultMessageRepliesCountButton,
        MessageStatus = DefaultMessageStatus,
        MessageTimestamp = DefaultMessageTimestamp,
        ReactionsList = DefaultReactionList,
        StreamedMessageText = DefaultStreamedMessageText,
    } = useComponentContext('MessageSimple');

    // Derived message state
    const hasAttachment = messageHasAttachments(message);
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

    // Metadata and reply logic
    const showMetadata = !groupedByUser || endOfGroup;
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

    // CSS class for the message root
    const rootClassName = clsx(
        'str-chat__message str-chat__message-simple',
        `str-chat__message--${message.type}`,
        `str-chat__message--${message.status}`,
        'rock-chat__message rock-chat__message-simple',
        isMyMessage()
            ? 'str-chat__message--me str-chat__message-simple--me'
            : 'str-chat__message--other',
        message.text ? 'str-chat__message--has-text' : 'has-no-text',
        {
            'str-chat__message--has-attachment': hasAttachment,
            'str-chat__message--highlighted': highlighted,
            'str-chat__message--pinned pinned-message': message.pinned,
            'str-chat__message--with-reactions': hasReactions,
            'str-chat__message-send-can-be-retried':
                message?.status === 'failed' && message?.error?.status !== 403,
            'str-chat__message-with-thread-link': showReplyCountButton,
            'str-chat__virtual-message__wrapper--end': endOfGroup,
            'str-chat__virtual-message__wrapper--first': firstOfGroup,
            'str-chat__virtual-message__wrapper--group': groupedByUser,
        },
    );

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

    // Main render
    return (
        <>
            {/* Edit message modal */}
            {editing && (
                <Modal
                    className='str-chat__edit-message-modal'
                    onClose={clearEditingState}
                    open={editing}>
                    <MessageInput
                        clearEditingState={clearEditingState}
                        hideSendButton
                        Input={EditMessageInput}
                        {...additionalMessageInputProps}
                    />
                </Modal>
            )}
            {/* Bounce dialog modal */}
            {isBounceDialogOpen && (
                <MessageBounceModal
                    MessageBouncePrompt={MessageBouncePrompt}
                    onClose={() => setIsBounceDialogOpen(false)}
                    open={isBounceDialogOpen}
                />
            )}
            <div className='rock-message-container'>
                <div className={rootClassName} key={message.id}>
                    {/* User avatar and header (if not my message) */}
                    {message.user && !isMyMessage() && (
                        <div className='rock-avatar-container'>
                            <Avatar
                                image={message.user.image}
                                name={message.user.name || message.user.id}
                                onClick={onUserClick}
                                onMouseOver={onUserHover}
                                user={message.user}
                            />
                        </div>
                    )}
                    {message.user && !isMyMessage() && (
                        <div className='rock-message-header'>
                            <span className='str-chat__message-simple-name rock-message-author'>
                                {message.user.name || message.user.id}
                            </span>
                            <div className='rock-message-badges'>
                                {getRockBadges() as React.ReactNode}
                            </div>
                        </div>
                    )}
                    {/* Message bubble and content */}
                    <div
                        className={clsx('str-chat__message-inner', {
                            'str-chat__simple-message--error-failed': allowRetry || isBounced,
                        })}
                        data-testid='message-inner'
                        onClick={handleClick}
                        onKeyUp={handleClick}
                    >
                        <MessageActions />
                        <div className='str-chat__message-bubble'>
                            {poll && <Poll poll={poll} />}
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
                                    align={isMyMessage() ? 'right' : 'left'}
                                    source={message.mml}
                                />
                            )}
                            <MessageErrorIcon />
                        </div>
                    </div>
                    {/* Footer: reactions, reply count, metadata */}
                    <div className='rock-message-footer'>
                        <div className='str-chat__message-reactions-host'>
                            {hasReactions && <ReactionsList reverse />}
                        </div>
                        {showReplyCountButton && (
                            <MessageRepliesCountButton
                                onClick={handleOpenThread}
                                reply_count={message.reply_count}
                            />
                        )}
                        {showMetadata && (
                            <div className='str-chat__message-metadata'>
                                <MessageStatus />
                                <MessageTimestamp customClass='str-chat__message-simple-timestamp' />
                                {isEdited && (
                                    <span className='str-chat__mesage-simple-edited'>
                                        {t('Edited') as string}
                                    </span>
                                )}
                                {isEdited && (
                                    <MessageEditedTimestamp
                                        calendar
                                        open={isEditedTimestampOpen}
                                    />
                                )}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </>
    );
};