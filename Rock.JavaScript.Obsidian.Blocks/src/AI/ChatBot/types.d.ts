export type ChatBotConfigurationBag = {
    error?: string | null;

    sessionId?: number | null;

    sessions?: ChatSessionBag[] | null;

    messages?: ChatMessageBag[] | null;

    anchors?: ChatAnchorBag[] | null;

    isDebugAllowed: boolean;

    isDockedMode: boolean;
};

export type ChatSessionBag = {
    id: number;

    name?: string | null;

    lastMessageDateTime?: string | null;
};

export type SendMessageRequestBag = {
    message?: string | null;

    sessionId: number;

    isDebugEnabled: boolean;
};

export type SendMessageResponseBag = {
    message?: ChatMessageBag | null;

    tool?: string | null;

    logs?: ChatLogBag[] | null;
};

export type ChatMessageBag = {
    duration: number;

    role: AuthorRole;

    message?: string | null;

    tokenCount: number;

    consumedTokenCount: number;
};

export type ExtendedChatMessageBag = ChatMessageBag & {
    logs?: ChatLogBag[] | null;

    tool?: string | null;
};

export type ChatLogBag = {
    category?: string | null;

    logLevel: number;

    logLevelName?: string | null;

    message?: string | null;

    timestamp: number;
};

export type ChatAnchorBag = {
    id: number;

    entityTypeId: number;

    entityTypeName?: string | null;

    name?: string | null;
};

export type ToolCall = {
    name: string;

    args: string;

    result: string;

    error: string;

    duration: string;
};

export type SessionState = {
    content: string;
    width: string;
    top: string;
    scrollTop: number;
};
