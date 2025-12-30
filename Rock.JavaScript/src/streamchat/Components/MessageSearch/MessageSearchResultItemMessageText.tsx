import React, { useMemo } from 'react';
import { MessageResponse } from 'stream-chat';
import {
    renderText,
    isOnlyEmojis,
    QuotedMessage,
    defaultAllowedTagNames,
    RenderTextPluginConfigurator
} from 'stream-chat-react';
import clsx from 'clsx';

import { findAndReplace, ReplaceFunction } from "hast-util-find-and-replace";
import { u } from "unist-builder";
import { Nodes } from 'hast-util-find-and-replace/lib';

interface MessageSearchResultItemMessageTextProps {
    message: MessageResponse;
    searchTerm?: string;
}



// We're going to go through and wrap message text that should be highlighted in 
// a @highlight and @endhighlight tag. Then, we have a custom markdown plugin that will
// convert those tags into <mark> tags for styling.
const highlightTextRegex = new RegExp(
    /@highlight(.*?)@endhighlight/g
);

// Define the custom markdown plugin to replace @highlight and @endhighlight with <mark> tags

// wraps every letter b in <xxx></xxx> tags

// export const highlightMarkdownPlugin = () => {
//     const replace: ReplaceFunction = (match) =>
//         u('element', { tagName: customTagName, properties: {} }, [
//             u('text', match),
//         ]);

//     // Dummy regex to find the letter T
//     const letterTRegex = /T/g;
//     return (node: Nodes) => findAndReplace(node, [letterTRegex, replace]);

//     const highlightTextRegex = /@highlight(.*?)@endhighlight/g;

//     return (node: Nodes) => findAndReplace(node, [highlightTextRegex, replace]);
// };

// const highlightText = (text: string, searchTerm?: string): string => {
//     if (!searchTerm || !searchTerm.trim()) return text;
//     const escaped = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
//     const regex = new RegExp(`(${escaped})`, 'gi');
//     return text.replace(regex, '@highlight$1@endhighlight');
// };

const customTagName = "xxx";
const replace: ReplaceFunction = (_match, text) =>
    u('element', { tagName: customTagName, properties: {} }, [
        u('text', String(text)),
    ]);

const letterBRegex: RegExp = /t/g;
// const customRehypePlugin = () => (tree: Nodes) => findAndReplace(tree, [letterBRegex, replace]);
const customRehypePlugin = () => (tree: any) =>
    findAndReplace(tree, [letterBRegex, replace]);

const getRehypePlugins: RenderTextPluginConfigurator = (plugins) => {
    return [customRehypePlugin(), ...plugins];
};

const UnMemoizeMessageSearchResultItemMessageTextComponent: React.FC<
    MessageSearchResultItemMessageTextProps
> = ({ message, searchTerm }) => {
    const hasAttachment = message.attachments && message.attachments.length > 0;;
    const textToRender = message.text || '';
    const messageText = useMemo(() => {
        // const highlightedText = highlightText(textToRender, searchTerm);
        return renderText(textToRender, message.mentioned_users, {
            allowedTagNames: [...defaultAllowedTagNames, 'xxx'],
            // getRehypePlugins,
        });
    }, [message.mentioned_users, textToRender, searchTerm]);

    if (!textToRender && !message.quoted_message) return null;

    return (
        <div className="str-chat__message-text" tabIndex={0}>
            <div
                className={clsx('str-chat__message-text-inner', {
                    'str-chat__message-text-inner--has-attachment': hasAttachment,
                    'str-chat__message-text-inner--is-emoji':
                        isOnlyEmojis(textToRender) && !message.quoted_message,
                })}
            >
                {message.quoted_message && <QuotedMessage />}
                <div>{messageText}</div>
            </div>
        </div>
    );
};

export const MessageSearchResultItemMessageText = React.memo(
    UnMemoizeMessageSearchResultItemMessageTextComponent,
);
