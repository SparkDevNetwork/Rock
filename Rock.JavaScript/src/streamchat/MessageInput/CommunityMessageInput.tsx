// ComunityMessageInput.tsx
import React, { PropsWithChildren } from 'react';
import { CooldownTimer, MessageInputProps, SendButton, TextareaComposer, WithDragAndDropUpload } from 'stream-chat-react';
import {
    AttachmentPreviewList,
    LinkPreviewList,
    QuotedMessagePreview,
    SimpleAttachmentSelector,
    useComponentContext,
    useMessageInputContext,
} from "stream-chat-react";

const SendButtonWithCooldown = () => {
    const { handleSubmit, cooldownRemaining, setCooldownRemaining } =
        useMessageInputContext();
    return cooldownRemaining ? (
        <CooldownTimer
            cooldownInterval={cooldownRemaining}
            setCooldownRemaining={setCooldownRemaining}
        />
    ) : (
        <SendButton sendMessage={handleSubmit} />
    );
};

interface ComposerAction {
    key: string;
    icon: string;
    title: string;
    onClick: (e: React.BaseSyntheticEvent) => void;
}
const getSupportedComposerActions = (): ComposerAction[] => {
    return [
        { key: 'bold', icon: 'fas fa-bold', title: 'Bold', onClick: () => { } },
        { key: 'italic', icon: 'fas fa-italic', title: 'Italic', onClick: () => { } },
        { key: 'strikethrough', icon: 'fas fa-strikethrough', title: 'Strikethrough', onClick: () => { } },
        { key: 'code', icon: 'fas fa-code', title: 'Code', onClick: () => { } },
        { key: 'quote', icon: 'fas fa-quote-left', title: 'Quote', onClick: () => { } },
        { key: 'link', icon: 'fas fa-link', title: 'Link', onClick: () => { } },
        { key: 'upload', icon: 'fas fa-upload', title: 'Upload File', onClick: () => { } },
    ];
};

export const CommunityMessageInput = () => (
    <WithDragAndDropUpload>
        <div className="rock-message-composer">
            <div className="rock-message-composer-input">
                <AttachmentPreviewList />
                <TextareaComposer containerClassName="rock-message-input-container" maxRows={16} />
            </div>

            <div className="rock-message-composer-actions">
                {getSupportedComposerActions().map((action) => (
                    <button
                        key={action.key}
                        className={`rock-message-composer-action ${action.key}`}
                        title={action.title}
                        onClick={action.onClick}>
                        <i className={action.icon}></i>
                    </button>
                ))}
            </div>
        </div>
    </WithDragAndDropUpload>
);
