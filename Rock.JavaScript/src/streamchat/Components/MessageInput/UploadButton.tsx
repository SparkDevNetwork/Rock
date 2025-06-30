import clsx from 'clsx';
import { nanoid } from 'nanoid';
import type { ChangeEvent, ComponentProps } from 'react';
import React, { forwardRef, useCallback, useMemo } from 'react';
import { MessageComposerConfig } from 'stream-chat';
import { useTranslationContext, useMessageInputContext, useMessageComposer, useAttachmentManagerState, useStateStore } from 'stream-chat-react';
import { PartialSelected } from 'stream-chat-react/dist/types/types';


const attachmentManagerConfigStateSelector = (state: MessageComposerConfig) => ({
    acceptedFiles: state.attachments.acceptedFiles,
    maxNumberOfFilesPerMessage: state.attachments.maxNumberOfFilesPerMessage,
});

/**
 * @deprecated Use FileInputProps instead.
 */
export type UploadButtonProps = {
    onFileChange: (files: Array<File>) => void;
    resetOnChange?: boolean;
} & Omit<ComponentProps<'input'>, 'type' | 'onChange'>;

/**
 * @deprecated Use FileInput instead
 */

export const UploadButton = forwardRef(function UploadButton(
    { onFileChange, resetOnChange = true, ...rest }: UploadButtonProps,
    ref: React.ForwardedRef<HTMLInputElement>,
) {
    const handleInputChange = useHandleFileChangeWrapper(resetOnChange, onFileChange);

    return <input onChange={handleInputChange} ref={ref} type='file' {...rest} />;
});

export type FileInputProps = UploadButtonProps;

export const FileInput = UploadButton;

export const UploadFileInput = forwardRef(function UploadFileInput(
    {
        className,
        onFileChange: onFileChangeCustom,
        ...props
    }: PartialSelected<FileInputProps, 'onFileChange'>,
    ref: React.ForwardedRef<HTMLInputElement>,
) {
    const { t } = useTranslationContext('UploadFileInput');
    const { cooldownRemaining } = useMessageInputContext();
    const messageComposer = useMessageComposer();
    const { attachmentManager } = messageComposer;
    const { isUploadEnabled } = useAttachmentManagerState();
    const { acceptedFiles, maxNumberOfFilesPerMessage } = useStateStore(
        messageComposer.configState,
        attachmentManagerConfigStateSelector,
    );
    const id = useMemo(() => nanoid(), []);

    const onFileChange = useCallback(
        (files: Array<File>) => {
            attachmentManager.uploadFiles(files);
            onFileChangeCustom?.(files);
        },
        [onFileChangeCustom, attachmentManager],
    );

    return (
        <FileInput
            accept={acceptedFiles?.join(',')}
            aria-label={t('aria/File upload')}
            data-testid='file-input'
            disabled={!isUploadEnabled || !!cooldownRemaining}
            id={id}
            multiple={maxNumberOfFilesPerMessage > 1}
            {...props}
            className={clsx('str-chat__file-input', className)}
            onFileChange={onFileChange}
            ref={ref}
        />
    );
});

export const useHandleFileChangeWrapper = (
    resetOnChange = false,
    handler?: (files: Array<File>) => void,
) =>
    useCallback(
        ({ currentTarget }: ChangeEvent<HTMLInputElement>) => {
            const { files } = currentTarget;

            if (!files) return;

            try {
                handler?.(Array.from(files));
            } catch (error) {
                console.error(error);
            }

            if (resetOnChange) currentTarget.value = '';
        },
        [handler, resetOnChange],
    );