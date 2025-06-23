import React, { useMemo, useState } from "react";
import {
    MessageContextValue, useChatContext, useTranslationContext, Avatar as DefaultAvatar, Attachment as DefaultAttachment, EditMessageForm as DefaultEditMessageForm,
    MessageOptions as DefaultMessageOptions, MessageDeleted as DefaultMessageDeleted, MessageRepliesCountButton as DefaultMessageRepliesCountButton, MessageStatus as DefaultMessageStatus,
    ReactionsList as DefaultReactionList, StreamedMessageText as DefaultStreamedMessageText, MML,
    messageHasAttachments,
    messageHasReactions,
    isMessageBounced,
    isMessageEdited,
    Modal,
    MessageInput,
    Poll,
    MessageText,
    MessageErrorIcon,
    areMessageUIPropsEqual,
    MessageUIComponentProps,
    useMessageContext,
    useComponentContext,
    EditMessageModal,
    // MessageTimestamp,
    MessageTimestamp as DefaultMessageTimestamp,


} from 'stream-chat-react';
import clsx from 'clsx';

import { isMessageBlocked } from './utils';
import { MessageBlocked as DefaultMessageBlocked } from './MessageBlocked';
import { MessageBouncePrompt as DefaultMessageBouncePrompt } from './MessageBouncePrompt';
import { MessageEditedTimestamp } from './MessageEditedTimestamp';
import { MessageBounceModal } from './MessageBounceModal';
// import { MessageTimestamp as DefaultMessageTimestamp } from './MessageTimestamp';
import RockBadge from '../Badge/RockBadge';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../ChatViewStyle';
import { RockMessageThreadReplyContent } from './RockMessageThreadReplyContent';
import { RockMessageTimestamp } from "./RockMessageTimestamp";

type RockMessageProps = MessageContextValue;

// Rock Message Custom Component
const RockMessageWithContext = (props: RockMessageProps) => {

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

    const { client } = useChatContext('RockMessage');
    const { t } = useTranslationContext('RockMessage');
    const [isBounceDialogOpen, setIsBounceDialogOpen] = useState(false);
    const [isEditedTimestampOpen, setEditedTimestampOpen] = useState(false);

    const {
        Attachment = DefaultAttachment,
        Avatar = DefaultAvatar,
        EditMessageInput = DefaultEditMessageForm,
        MessageOptions = DefaultMessageOptions,
        // TODO: remove this "passthrough" in the next
        // major release and use the new default instead
        MessageActions = MessageOptions,
        // MessageBlocked = DefaultMessageBlocked,
        MessageDeleted = DefaultMessageDeleted,
        MessageBouncePrompt = DefaultMessageBouncePrompt,
        MessageRepliesCountButton = DefaultMessageRepliesCountButton,
        MessageStatus = DefaultMessageStatus,
        MessageTimestamp = DefaultMessageTimestamp,
        ReactionsList = DefaultReactionList,
        StreamedMessageText = DefaultStreamedMessageText,
        PinIndicator,
    } = useComponentContext('MessageSimple');

    const hasAttachment = messageHasAttachments(message);
    const hasReactions = messageHasReactions(message);
    const isAIGenerated = useMemo(
        () => isMessageAIGenerated?.(message),
        [isMessageAIGenerated, message],
    );

    if (message.deleted_at || message.type === 'deleted') {
        return <MessageDeleted message={message} />;
    }

    if (isMessageBlocked(message)) {
        return <DefaultMessageBlocked />;
    }

    const showMetadata = !groupedByUser || endOfGroup;
    const showReplyCountButton = !threadList && !!message.reply_count;
    const allowRetry = message.status === 'failed';
    const isBounced = isMessageBounced(message);
    const isEdited = isMessageEdited(message) && !isAIGenerated;

    let handleClick: (() => void) | undefined = undefined;

    if (allowRetry) {
        handleClick = () => handleRetry(message);
    } else if (isBounced) {
        handleClick = () => setIsBounceDialogOpen(true);
    } else if (isEdited) {
        handleClick = () => setEditedTimestampOpen((prev) => !prev);
    }


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

    const communityRootClassName = clsx(
        'rock-message',
        `rock-message--${message.type}`,
        `rock-message--${message.status}`,
        isMyMessage()
            ? 'rock-chat-message--me'
            : 'rock-chat-message--other',
        message.text ? 'rock-chat-message--has-text' : 'has-no-text',
        {
            'rock-chat-message--has-attachment': hasAttachment,
            'rock-chat-message--highlighted': highlighted,
            'rock-chat-message-pinned pinned-message': message.pinned,
            'rock-chat-message--with-reactions': hasReactions,
            'rock-chat-message-send-can-be-retried':
                message?.status === 'failed' && message?.error?.status !== 403,
            'rock-chat-message-send-failed':
                message?.status === 'failed' && message?.error?.status !== 403,
            'rock-chat-message-with-thread-link': showReplyCountButton,
        },
    );

    const poll = message.poll_id && client.polls.fromState(message.poll_id);

    const getRockBadges = () => {
        var streamBadges = message?.user?.rock_badges

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

    if (!message.user) {
        return <></>
    };


    interface MessageAction {
        key: string;
        icon: string;
        title: string;
        onClick: (e: React.BaseSyntheticEvent) => void;
    }

    const getSupportedMessageActions = (): MessageAction[] => {

        const actions: MessageAction[] = [
            {
                key: 'react',
                icon: 'fas fa-smile',
                title: 'Add Reaction',
                onClick: () => {
                    console.log('React clicked');
                },
            },
            {
                key: 'quote',
                icon: 'fas fa-quote-right',
                title: 'Quote',
                onClick: () => {
                    console.log('React clicked');
                },
            },
            {
                key: 'reply',
                icon: 'fas fa-reply',
                title: 'Reply in Thread',
                onClick: (e) => {
                    console.log('Reply clicked');
                    handleOpenThread(e);
                },
            },
            {
                key: 'more',
                icon: 'fas fa-ellipsis-v',
                title: 'More Options',
                onClick: () => {
                    console.log('More clicked');
                },
            },
        ];

        return actions;
    };

    const messageActions = getSupportedMessageActions();


    const conversationalComponent = () => {
        return (
            <>
                {/* {editing && (
                        <Modal
                            className='str-chat__edit-message-modal'
                            onClose={clearEditingState}
                            open={editing}
                        >
                            <MessageInput
                                clearEditingState={clearEditingState}
                                grow
                                hideSendButton
                                Input={EditMessageInput}
                                message={message}
                                {...additionalMessageInputProps}
                            />
                        </Modal>
                    )}
    
                    {isBounceDialogOpen && (
                        <MessageBounceModal
                            MessageBouncePrompt={MessageBouncePrompt}
                            onClose={() => setIsBounceDialogOpen(false)}
                            open={isBounceDialogOpen}
                        />
                    )}
    
                    <div className='rock-message-container'>
                        <div className={rootClassName} key={message.id}>
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
    
                            <div
                                className={clsx('str-chat__message-inner', {
                                    'str-chat__simple-message--error-failed': allowRetry || isBounced,
                                })}
                                data-testid='message-inner'
                                onClick={handleClick}
                                onKeyUp={handleClick}>
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
                                                {t<string>('Edited')}
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
                    </div> */}
            </>
        )
    }

    const communityComponent = () => {
        return (
            <>
                <div className="rock-message-wrapper">
                    {editing && (
                        <EditMessageModal additionalMessageInputProps={additionalMessageInputProps} />
                    )}

                    {isBounceDialogOpen && (
                        <MessageBounceModal
                            MessageBouncePrompt={MessageBouncePrompt}
                            onClose={() => setIsBounceDialogOpen(false)}
                            open={isBounceDialogOpen}
                        />
                    )}

                    <div className="rock-message-container">
                        <div className='rock-message-container'>
                            {message.user && (
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

                            <div className="rock-message-layout">
                                <div className="rock-message-title-layout">
                                    <span className='rock-message-author'>
                                        {message.user!.name || message.user!.id}
                                    </span>
                                    <div className='rock-message-badges'>
                                        {getRockBadges() as React.ReactNode}
                                    </div>

                                    <div className="rock-message-time">
                                        {/* Show a simple 1:42PM formatted time */}
                                        <RockMessageTimestamp
                                            customClass='rock-message-simple-timestamp'
                                            message={message}
                                        />
                                    </div>
                                </div>

                                <div className="rock-message-content">
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
                                            align={'left'}
                                            source={message.mml}
                                        />
                                    )}

                                    {showReplyCountButton && (
                                        <RockMessageThreadReplyContent
                                            onClick={handleOpenThread}
                                            reply_count={message.reply_count}
                                        />
                                    )}

                                    <div className="rock-chat-message-send-failed-icon">
                                        <i className='fa fa-exclamation-circle' />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="rock-message-actions">
                            {messageActions.map((action) => (
                                <button
                                    key={action.key}
                                    className="rock-message-action"
                                    title={action.title}
                                    onClick={(e) => { action.onClick(e); }}>
                                    <i className={action.icon} />
                                </button>
                            ))}
                        </div>
                    </div>
                </div >
            </>
        )
    }

    const { chatViewStyle } = useChatConfig();

    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? (communityComponent()) : (conversationalComponent())}
        </>)
}

const MemoizedRockMessage = React.memo(
    RockMessageWithContext,
    areMessageUIPropsEqual,
) as typeof RockMessageWithContext;

/**
 * The default UI component that renders a message and receives functionality and logic from the MessageContext.
 */
export const RockMessage = (props: MessageUIComponentProps) => {
    const messageContext = useMessageContext('MessageSimple');

    return <MemoizedRockMessage {...messageContext} {...props} />;
};

// import React, { useMemo, useState } from 'react';
// import clsx from 'clsx';
// import {
//     useChatContext,
//     useTranslationContext,
//     Avatar as DefaultAvatar,
//     Attachment as DefaultAttachment,
//     EditMessageForm as DefaultEditMessageForm,
//     MessageOptions as DefaultMessageOptions,
//     MessageDeleted as DefaultMessageDeleted,
//     MessageRepliesCountButton as DefaultMessageRepliesCountButton,
//     MessageStatus as DefaultMessageStatus,
//     ReactionsList as DefaultReactionList,
//     StreamedMessageText as DefaultStreamedMessageText,
//     MML,
//     messageHasAttachments,
//     messageHasReactions,
//     isMessageBounced,
//     isMessageEdited,
//     Modal,
//     MessageInput,
//     Poll,
//     MessageText,
//     MessageErrorIcon,
//     areMessageUIPropsEqual,
//     MessageUIComponentProps,
//     useMessageContext,
//     useComponentContext,
// } from 'stream-chat-react';
// import { isMessageBlocked } from './utils';
// import { MessageBlocked as DefaultMessageBlocked } from './MessageBlocked';
// import { MessageBouncePrompt as DefaultMessageBouncePrompt } from './MessageBouncePrompt';
// import { MessageEditedTimestamp } from './MessageEditedTimestamp';
// import { MessageBounceModal } from './MessageBounceModal';
// import { MessageTimestamp as DefaultMessageTimestamp } from './MessageTimestamp';
// import RockBadge from '../Badge/RockBadge';
// import { useChatConfig } from '../Chat/ChatConfigContext';
// import { ChatViewStyle } from '../ChatViewStyle';
// import { RockMessageThreadReplyContent } from './RockMessageThreadReplyContent';

// import type { MessageContextValue } from 'stream-chat-react';


// const MessageSimpleWithContext = (props: MessageContextValue) => {
//     (
//         props: MessageContextValue,
//     ) => {
//         const {
//             additionalMessageInputProps,
//             clearEditingState,
//             editing,
//             endOfGroup,
//             firstOfGroup,
//             groupedByUser,
//             handleAction,
//             handleOpenThread,
//             handleRetry,
//             highlighted,
//             isMessageAIGenerated,
//             isMyMessage,
//             message,
//             onUserClick,
//             onUserHover,
//             renderText,
//             threadList,
//         } = props;
//         const { client } = useChatContext('MessageSimple');
//         const { t } = useTranslationContext('MessageSimple');
//         const [isBounceDialogOpen, setIsBounceDialogOpen] = useState(false);
//         const [isEditedTimestampOpen, setEditedTimestampOpen] = useState(false);

//         const {
//             Attachment = DefaultAttachment,
//             Avatar = DefaultAvatar,
//             EditMessageInput = DefaultEditMessageForm,
//             MessageOptions = DefaultMessageOptions,
//             // TODO: remove this "passthrough" in the next
//             // major release and use the new default instead
//             MessageActions = MessageOptions,
//             // MessageBlocked = DefaultMessageBlocked,
//             MessageDeleted = DefaultMessageDeleted,
//             MessageBouncePrompt = DefaultMessageBouncePrompt,
//             MessageRepliesCountButton = DefaultMessageRepliesCountButton,
//             MessageStatus = DefaultMessageStatus,
//             MessageTimestamp = DefaultMessageTimestamp,
//             ReactionsList = DefaultReactionList,
//             StreamedMessageText = DefaultStreamedMessageText,
//             PinIndicator,
//         } = useComponentContext('MessageSimple');

//         const hasAttachment = messageHasAttachments(message);
//         const hasReactions = messageHasReactions(message);
//         const isAIGenerated = useMemo(
//             () => isMessageAIGenerated?.(message),
//             [isMessageAIGenerated, message],
//         );

//         // if (message.customType === "message.date") {
//         //     return null;
//         // }

//         if (message.deleted_at || message.type === 'deleted') {
//             return <MessageDeleted message={message} />;
//         }

//         if (isMessageBlocked(message)) {
//             return <DefaultMessageBlocked />;
//         }

//         const showMetadata = !groupedByUser || endOfGroup;
//         const showReplyCountButton = !threadList && !!message.reply_count;
//         const allowRetry = message.status === 'failed';
//         const isBounced = isMessageBounced(message);
//         const isEdited = isMessageEdited(message) && !isAIGenerated;

//         let handleClick: (() => void) | undefined = undefined;

//         if (allowRetry) {
//             handleClick = () => handleRetry(message);
//         } else if (isBounced) {
//             handleClick = () => setIsBounceDialogOpen(true);
//         } else if (isEdited) {
//             handleClick = () => setEditedTimestampOpen((prev) => !prev);
//         }

//         const rootClassName = clsx(
//             'str-chat__message str-chat__message-simple',
//             `str-chat__message--${message.type}`,
//             `str-chat__message--${message.status}`,
//             'rock-chat__message rock-chat__message-simple',
//             isMyMessage()
//                 ? 'str-chat__message--me str-chat__message-simple--me'
//                 : 'str-chat__message--other',
//             message.text ? 'str-chat__message--has-text' : 'has-no-text',
//             {
//                 'str-chat__message--has-attachment': hasAttachment,
//                 'str-chat__message--highlighted': highlighted,
//                 'str-chat__message--pinned pinned-message': message.pinned,
//                 'str-chat__message--with-reactions': hasReactions,
//                 'str-chat__message-send-can-be-retried':
//                     message?.status === 'failed',
//                 'str-chat__message-with-thread-link': showReplyCountButton,
//                 'str-chat__virtual-message__wrapper--end': endOfGroup,
//                 'str-chat__virtual-message__wrapper--first': firstOfGroup,
//                 'str-chat__virtual-message__wrapper--group': groupedByUser,
//             },
//         );

//         const communityRootClassName = clsx(
//             'rock-message',
//             `rock-message--${message.type}`,
//             `rock-message--${message.status}`,
//             isMyMessage()
//                 ? 'rock-chat-message--me'
//                 : 'rock-chat-message--other',
//             message.text ? 'rock-chat-message--has-text' : 'has-no-text',
//             {
//                 'rock-chat-message--has-attachment': hasAttachment,
//                 'rock-chat-message--highlighted': highlighted,
//                 'rock-chat-message-pinned pinned-message': message.pinned,
//                 'rock-chat-message--with-reactions': hasReactions,
//                 'rock-chat-message-send-can-be-retried':
//                     message?.status === 'failed',
//                 'rock-chat-message-send-failed':
//                     message?.status === 'failed',
//                 'rock-chat-message-with-thread-link': showReplyCountButton,
//             },
//         );

//         const poll = message.poll_id && client.polls.fromState(message.poll_id);

//         const getRockBadges = () => {
//             var streamBadges = message?.user?.rock_badges

//             // If the streamBadges object is an array, map through it and create a new array of RockBadge components
//             if (!Array.isArray(streamBadges)) {
//                 return <div></div>;
//             }

//             var badgeComponents = streamBadges.map((badge: any) => {
//                 return (
//                     <RockBadge
//                         badgeText={badge.Name}
//                         foregroundColor={badge.ForegroundColor}
//                         backgroundColor={badge.BackgroundColor}
//                     />
//                 )
//             })

//             return badgeComponents
//         }

//         if (!message.user) {
//             return <></>
//         };

//         interface MessageAction {
//             key: string;
//             icon: string;
//             title: string;
//             onClick: (e: React.BaseSyntheticEvent) => void;
//         }

//         const getSupportedMessageActions = (): MessageAction[] => {

//             const actions: MessageAction[] = [
//                 {
//                     key: 'react',
//                     icon: 'fas fa-smile',
//                     title: 'Add Reaction',
//                     onClick: () => {
//                         console.log('React clicked');
//                     },
//                 },
//                 {
//                     key: 'quote',
//                     icon: 'fas fa-quote-right',
//                     title: 'Quote',
//                     onClick: () => {
//                         console.log('React clicked');
//                     },
//                 },
//                 {
//                     key: 'reply',
//                     icon: 'fas fa-reply',
//                     title: 'Reply in Thread',
//                     onClick: (e) => {
//                         console.log('Reply clicked');
//                         handleOpenThread(e);
//                     },
//                 },
//                 {
//                     key: 'more',
//                     icon: 'fas fa-ellipsis-v',
//                     title: 'More Options',
//                     onClick: () => {
//                         console.log('More clicked');
//                     },
//                 },
//             ];

//             return actions;
//         };

//         const messageActions = getSupportedMessageActions();

//         const conversationalComponent = () => {
//             return (
//                 <>
//                     {/* {editing && (
//                     <Modal
//                         className='str-chat__edit-message-modal'
//                         onClose={clearEditingState}
//                         open={editing}
//                     >
//                         <MessageInput
//                             clearEditingState={clearEditingState}
//                             grow
//                             hideSendButton
//                             Input={EditMessageInput}
//                             message={message}
//                             {...additionalMessageInputProps}
//                         />
//                     </Modal>
//                 )}

//                 {isBounceDialogOpen && (
//                     <MessageBounceModal
//                         MessageBouncePrompt={MessageBouncePrompt}
//                         onClose={() => setIsBounceDialogOpen(false)}
//                         open={isBounceDialogOpen}
//                     />
//                 )}

//                 <div className='rock-message-container'>
//                     <div className={rootClassName} key={message.id}>
//                         {message.user && !isMyMessage() && (
//                             <div className='rock-avatar-container'>
//                                 <Avatar
//                                     image={message.user.image}
//                                     name={message.user.name || message.user.id}
//                                     onClick={onUserClick}
//                                     onMouseOver={onUserHover}
//                                     user={message.user}
//                                 />
//                             </div>
//                         )}

//                         {message.user && !isMyMessage() && (
//                             <div className='rock-message-header'>
//                                 <span className='str-chat__message-simple-name rock-message-author'>
//                                     {message.user.name || message.user.id}
//                                 </span>
//                                 <div className='rock-message-badges'>
//                                     {getRockBadges() as React.ReactNode}
//                                 </div>
//                             </div>
//                         )}

//                         <div
//                             className={clsx('str-chat__message-inner', {
//                                 'str-chat__simple-message--error-failed': allowRetry || isBounced,
//                             })}
//                             data-testid='message-inner'
//                             onClick={handleClick}
//                             onKeyUp={handleClick}>
//                             <MessageActions />

//                             <div className='str-chat__message-bubble'>
//                                 {poll && <Poll poll={poll} />}
//                                 {message.attachments?.length && !message.quoted_message ? (
//                                     <Attachment
//                                         actionHandler={handleAction}
//                                         attachments={message.attachments}
//                                     />
//                                 ) : null}
//                                 {isAIGenerated ? (
//                                     <StreamedMessageText message={message} renderText={renderText} />
//                                 ) : (
//                                     <MessageText message={message} renderText={renderText} />
//                                 )}
//                                 {message.mml && (
//                                     <MML
//                                         actionHandler={handleAction}
//                                         align={isMyMessage() ? 'right' : 'left'}
//                                         source={message.mml}
//                                     />
//                                 )}
//                                 <MessageErrorIcon />
//                             </div>
//                         </div>

//                         <div className='rock-message-footer'>
//                             <div className='str-chat__message-reactions-host'>
//                                 {hasReactions && <ReactionsList reverse />}
//                             </div>

//                             {showReplyCountButton && (
//                                 <MessageRepliesCountButton
//                                     onClick={handleOpenThread}
//                                     reply_count={message.reply_count}
//                                 />
//                             )}

//                             {showMetadata && (
//                                 <div className='str-chat__message-metadata'>
//                                     <MessageStatus />
//                                     <MessageTimestamp customClass='str-chat__message-simple-timestamp' />
//                                     {isEdited && (
//                                         <span className='str-chat__mesage-simple-edited'>
//                                             {t<string>('Edited')}
//                                         </span>
//                                     )}
//                                     {isEdited && (
//                                         <MessageEditedTimestamp
//                                             calendar
//                                             open={isEditedTimestampOpen}
//                                         />
//                                     )}
//                                 </div>
//                             )}
//                         </div>
//                     </div>
//                 </div> */}
//                 </>
//             )
//         }

//         const communityComponent = () => {
//             return (
//                 <>
//                     <div className="rock-message-wrapper">
//                         {editing && (
//                             <Modal
//                                 className='str-chat__edit-message-modal'
//                                 onClose={clearEditingState}
//                                 open={editing}
//                             >
//                                 <MessageInput
//                                     clearEditingState={clearEditingState}
//                                     hideSendButton
//                                     Input={EditMessageInput}
//                                     message={message}
//                                     {...additionalMessageInputProps}
//                                 />
//                             </Modal>
//                         )}

//                         {isBounceDialogOpen && (
//                             <MessageBounceModal
//                                 MessageBouncePrompt={MessageBouncePrompt}
//                                 onClose={() => setIsBounceDialogOpen(false)}
//                                 open={isBounceDialogOpen}
//                             />
//                         )}

//                         <div className="rock-message-container">
//                             <div className='rock-message-container'>
//                                 {message.user && (
//                                     <div className='rock-avatar-container'>
//                                         <Avatar
//                                             image={message.user.image}
//                                             name={message.user.name || message.user.id}
//                                             onClick={onUserClick}
//                                             onMouseOver={onUserHover}
//                                             user={message.user}
//                                         />
//                                     </div>
//                                 )}

//                                 <div className="rock-message-layout">
//                                     <div className="rock-message-title-layout">
//                                         <span className='rock-message-author'>
//                                             {message.user!.name || message.user!.id}
//                                         </span>
//                                         <div className='rock-message-badges'>
//                                             {getRockBadges() as React.ReactNode}
//                                         </div>
//                                     </div>

//                                     <div className="rock-message-content">
//                                         {poll && <Poll poll={poll} />}
//                                         {message.attachments?.length && !message.quoted_message ? (
//                                             <Attachment
//                                                 actionHandler={handleAction}
//                                                 attachments={message.attachments}
//                                             />
//                                         ) : null}
//                                         {isAIGenerated ? (
//                                             <StreamedMessageText message={message} renderText={renderText} />
//                                         ) : (
//                                             <MessageText message={message} renderText={renderText} />
//                                         )}
//                                         {message.mml && (
//                                             <MML
//                                                 actionHandler={handleAction}
//                                                 align={'left'}
//                                                 source={message.mml}
//                                             />
//                                         )}

//                                         {showReplyCountButton && (
//                                             <RockMessageThreadReplyContent
//                                                 onClick={handleOpenThread}
//                                                 reply_count={message.reply_count}
//                                             />
//                                         )}

//                                         <div className="rock-chat-message-send-failed-icon">
//                                             <i className='fa fa-exclamation-circle' />
//                                         </div>
//                                     </div>
//                                 </div>
//                             </div>

//                             <div className="rock-message-actions">
//                                 {messageActions.map((action) => (
//                                     <button
//                                         key={action.key}
//                                         className="rock-message-action"
//                                         title={action.title}
//                                         onClick={(e) => { action.onClick(e); }}>
//                                         <i className={action.icon} />
//                                     </button>
//                                 ))}
//                             </div>
//                         </div>
//                     </div >
//                 </>
//             )
//         }

//         const { chatViewStyle } = useChatConfig();

//         return (
//             <>
//                 {chatViewStyle == ChatViewStyle.Community ? communityComponent() : conversationalComponent()}
//             </>
//         );

//     };
// };

// const MemoizedMessageSimple = React.memo(
//     RockMMessageSimple,
//     areMessageUIPropsEqual,
// );

// export const RockMessageSimple = (props: MessageContextValue) => {
//     const messageContext = useMessageContext('MessageSimple');
//     return <MemoizedMessageSimple {...messageContext} {...props} />;
// };
