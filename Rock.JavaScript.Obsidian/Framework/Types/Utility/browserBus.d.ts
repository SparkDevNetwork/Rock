import { Guid } from "..";

/**
 * A message that is sent or received on the browser bus.
 */
export type Message<TData = unknown> = {
    /** The name of the message. */
    name: string;

    /** The block type that published this message. */
    blockType?: Guid;

    /** The block that published this message. */
    block?: Guid;

    /**
     * The timestamp when this message was published. This is a JavaScript
     * time number from {@link Date.now()}.
     */
    timestamp: number;

    /** The data that was published with the message. This varies by message. */
    data: TData;
};

/**
 * The options that can be passed to the BrowserBus constructor.
 */
type BrowserBusOptions = {
    /** The block type to use when publishing messages. */
    blockType?: Guid;

    /** The block instance to use when publishing messages. */
    block?: Guid;
};

/**
 * The function signature of a browser bus callback.
 */
export type BrowserBusCallback<TData = unknown> = (message: Message<TData>) => void;

// #region Block Framework Messages

/**
 * The data that is available in a block BeginEdit message.
 */
export type BlockBeginEditData = void;

/**
 * The data that is available in a block EndEdit message.
 */
export type BlockEndEditData = void;

// #endregion

// #region Page Framework Messages

/**
 * The data that is available in a page queryStringChanged message.
 */
export type QueryStringChangedData = URLSearchParams;

// #endregion
