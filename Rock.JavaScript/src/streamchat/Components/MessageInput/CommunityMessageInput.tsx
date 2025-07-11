// ComunityMessageInput.tsx
import React from 'react';
import { AttachmentSelector, CooldownTimer, MessageInputProps, QuotedMessagePreview, QuotedMessagePreviewHeader, TextareaComposer, useMessageComposer, WithDragAndDropUpload } from 'stream-chat-react';
import {
    AttachmentPreviewList,
    useMessageInputContext,
} from "stream-chat-react";
import { SendButton } from './SendButton';
import { useModal } from '../Modal/ModalContext';
import { InsertLinkModal } from './InsertLinkModal';
import { SendToChannelCheckbox } from './SendToChannelCheckbox';
// import { AttachmentSelector } from './AttachmentSelector';

const SendButtonWithCooldown = () => {
    const { handleSubmit, cooldownRemaining, setCooldownRemaining, hideSendButton } =
        useMessageInputContext();

    console.log("SendButtonWithCooldown", {
        cooldownRemaining,
        hideSendButton,
    });

    if (hideSendButton) {
        return null;
    }

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
    content?: React.ReactNode;
}


export const CommunityMessageInput = () => {

    const { showModal, hideModal } = useModal();

    const getSupportedComposerActions = (): ComposerAction[] => {
        return [
            {
                key: 'bold',
                icon: 'fas fa-bold',
                title: 'Bold',
                onClick: wrapWithMarkdown('**'),
            },
            {
                key: 'italic',
                icon: 'fas fa-italic',
                title: 'Italic',
                onClick: wrapWithMarkdown('*'),
            },
            {
                key: 'strikethrough',
                icon: 'fas fa-strikethrough',
                title: 'Strikethrough',
                onClick: wrapWithMarkdown('~~'),
            },
            {
                key: 'code',
                icon: 'fas fa-code',
                title: 'Inline Code',
                onClick: wrapWithMarkdown('`'),
            },
            {
                key: 'multi-line-code',
                icon: 'fas fa-file-code',
                title: 'Multi-line Code',
                onClick: wrapWithMarkdown('```', 3, true),
            },
            {
                key: 'link',
                icon: 'fas fa-link',
                title: 'Link',
                onClick: handleLinkClick
            },
            {
                key: 'upload',
                icon: 'fas fa-upload',
                title: 'Upload File',
                onClick: () => { },
                content: (
                    // <AttachmentSelector />
                    <AttachmentSelector />
                )
            },
        ];
    };

    const { textareaRef } =
        useMessageInputContext();

    const { textComposer } = useMessageComposer();

    const wrapWithMarkdown = (
        wrapper: string,
        offset: number = wrapper.length,
        includeLineBreak: boolean = false,
    ) => (e: React.BaseSyntheticEvent) => {
        e.preventDefault();
        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const originalText = textarea.value;

        let newText = '';
        let newCursor = 0;

        if (start === end) {
            // No selection
            const inserted = includeLineBreak
                ? `${wrapper}\n\n${wrapper}`
                : `${wrapper}${wrapper}`;

            newText = originalText.slice(0, start) + inserted + originalText.slice(end);
            newCursor = includeLineBreak
                ? start + wrapper.length + 1 // inside the \n\n block
                : start + offset;
        } else {
            const selectedText = originalText.slice(start, end);
            const wrapped = includeLineBreak
                ? `${wrapper}\n${selectedText}\n${wrapper}`
                : `${wrapper}${selectedText}${wrapper}`;

            newText = originalText.slice(0, start) + wrapped + originalText.slice(end);
            newCursor = includeLineBreak
                ? start + wrapped.length // cursor after closing wrapper
                : end + wrapper.length * 2;
        }

        textComposer.handleChange({
            text: newText,
            selection: {
                start: newCursor,
                end: newCursor,
            },
        });

        requestAnimationFrame(() => {
            textarea.focus();
            textarea.setSelectionRange(newCursor, newCursor);
        });
    };

    const handleLinkClick = (e: React.BaseSyntheticEvent) => {
        e.preventDefault();

        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const originalText = textarea.value;
        const selected = originalText.slice(start, end);

        const handleInsert = (markdownLink: string) => {
            const newText = originalText.slice(0, start) + markdownLink + originalText.slice(end);
            const newCursor = start + markdownLink.length;

            textComposer.handleChange({
                text: newText,
                selection: {
                    start: newCursor,
                    end: newCursor,
                },
            });

            requestAnimationFrame(() => {
                textarea.focus();
                textarea.setSelectionRange(newCursor, newCursor);
            });

            hideModal();
        };

        showModal({
            title: 'Insert Link',
            content: (
                <InsertLinkModal
                    selectedText={selected}
                    onInsert={handleInsert}
                    onCancel={hideModal}
                />
            ),
        });
    };


    return (
        <div className="rock-community-message-input str-chat__message-input">
            <SendToChannelCheckbox />
            <WithDragAndDropUpload>
                <div className="rock-message-composer">
                    <div className="rock-message-composer-input">
                        <QuotedMessagePreviewHeader />
                        <QuotedMessagePreview />
                        <AttachmentPreviewList />
                        <TextareaComposer containerClassName="rock-message-input-container" maxRows={16} />
                    </div>

                    <div className="rock-message-composer-footer">
                        <div className="rock-message-composer-actions">
                            {getSupportedComposerActions().map((action) => {
                                if (action.content) {
                                    return (
                                        <div key={action.key} className={`rock-message-composer-action ${action.key}`}>
                                            {action.content}
                                        </div>
                                    );
                                }

                                return (
                                    <button
                                        key={action.key}
                                        className={`rock-message-composer-action ${action.key}`}
                                        title={action.title}
                                        onClick={action.onClick}>
                                        <i className={action.icon}></i>
                                    </button>)
                            })}
                        </div>

                        <div className="rock-message-composer-send">
                            <SendButtonWithCooldown />
                        </div>
                    </div>
                </div>
            </WithDragAndDropUpload>
        </div>
    );
}
