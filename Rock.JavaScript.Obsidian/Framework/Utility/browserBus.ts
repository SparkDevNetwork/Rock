import { Guid } from "@Obsidian/Types";
import { BlockBeginEditData, BlockEndEditData, BrowserBusCallback, BrowserBusOptions, Message, QueryStringChangedData } from "@Obsidian/Types/Utility/browserBus";
import { areEqual } from "./guid";

/*
 * READ THIS BEFORE MAKING ANY CHANGES TO THE BUS.
 *
 * OVERVIEW
 *
 * The browser bus is a basic pubsub interface within a single page. If you
 * publish a message to one instance of the bus it will be available to any
 * other instance on the same page. This uses document.addEventListener()
 * and document.dispatchEvent() with a single custom event name of `rockMessage`.
 *
 * The browser bus will not communicate with other browsers on the same page or
 * even other tabs within the same browser.
 *
 * For full documentation, see the gitbook developer documentation.
 *
 * FRAMEWORK MESSAGES
 *
 * All "framework" messages should have a type defined in
 * @Obsidian/Types/Utility/browserBus that specify the data type expected. If
 * no data type is expected than `void` can be used as the type. Message data
 * should always be an object rather than a primitive. This allows us to add
 * additional values without it being a breaking change to existing code.
 *
 * Additionally, all framework messages should have their name defined in either
 * the PageMessages object or BlockMessages object. This is for uniformity so it
 * is easier for core code and plugins to subscribe to these messages and know
 * they got the right message name.
 *
 * SUBSCRIBE OVERLOADS
 *
 * When adding new framework messages, be sure to add overloads to the
 * subscribe, subscribeToBlock and subscribeToBlockType functions for that
 * message name and data type. This compiles away to nothing but provides a
 * much better TypeScript experience.
 */


/**
 * Framework messages that will be sent for pages.
 */
export const PageMessages = {
    /**
     * Sent when the query string is changed outside the context of a page load.
     */
    QueryStringChanged: "page.core.queryStringChanged"
} as const;

/**
 * Framework messages that will be sent for blocks.
 */
export const BlockMessages = {
    /**
     * Sent just before a block switches into edit mode.
     */
    BeginEdit: "block.core.beginEdit",

    /**
     * Sent just after a block switches out of edit mode.
     */
    EndEdit: "block.core.endEdit",
} as const;

/**
 * Gets an object that will provide access to the browser bus. This bus will
 * allow different code on the page to send and receive messages betwen each
 * other as well as plain JavaScript. This bus does not cross page boundaries.
 *
 * Meaning, if you publish a message in one tab it will not show up in another
 * tab in the same (or a different) browser. Neither will messages magically
 * persist across page loads.
 *
 * If you call this method you are responsible for calling the {@link BrowserBus.dispose}
 * function when you are done with the bus. If you do not then your component
 * will probably never be garbage collected and your subscribed event handlers
 * will continue to be called.
 *
 * @param options Custom options to construct the {@link BrowserBus} object with. This should normally not be needed.
 *
 * @returns The object that provides access to the browser bus.
 */
export function useBrowserBus(options?: BrowserBusOptions): BrowserBus {
    return new BrowserBus(options ?? {});
}

// #region Internal Types

/**
 * Internal message handler state that includes the filters used to decide
 * if the callback is valid for the message.
 */
type MessageHandler = {
    /** If not nullish messages must match this message name. */
    name?: string;

    /** If not nullish then messages must be from this block type. */
    blockType?: Guid;

    /** If not nullish them messages must be from this block instance. */
    block?: Guid;

    /** The callback that will be called. */
    callback: BrowserBusCallback;
};

// #endregion

// #region Internal Implementation

/** This is the JavaScript event name we use with dispatchEvent(). */
const customDomEventName = "rockMessage";

/**
 * The main browser bus implementation. This uses a shared method to publish
 * and subscribe to messages such that if you create two BrowserBus instances on
 * the same page they will still be able to talk to each other.
 *
 * However, they will not be able to talk to instances on other pages such as
 * in other browser tabs.
 */
export class BrowserBus {
    /** The registered handlers that will potentially be invoked. */
    private handlers: MessageHandler[] = [];

    /** The options we were created with. */
    private options: BrowserBusOptions;

    /** The event listener. Used so we can remove the listener later. */
    private eventListener: (e: Event) => void;

    /**
     * Creates a new instance of the bus and prepares it to receive messages.
     *
     * This should be considered an internal constructor and not used by plugins.
     *
     * @param options The options that describe how this instance should operate.
     */
    constructor(options: BrowserBusOptions) {
        this.options = { ...options };

        this.eventListener = e => this.onEvent(e);
        document.addEventListener(customDomEventName, this.eventListener);
    }

    // #region Private Functions

    /**
     * Called when an event is received from the document listener.
     *
     * @param event The low level JavaScript even that was received.
     */
    private onEvent(event: Event): void {
        if (!(event instanceof CustomEvent)) {
            return;
        }

        let message = event.detail as Message;

        // Discard the message if it is not valid.
        if (!message.name) {
            return;
        }

        // If we got a message without a timestamp, it probably came from
        // plain JavaScript, so set it to 0.
        if (typeof message.timestamp === "undefined") {
            message = { ...message, timestamp: 0 };
        }

        this.onMessage(message);
    }

    /**
     * Called when a browser bus message is received from the bus.
     *
     * @param message The message that was received.
     */
    private onMessage(message: Message): void {
        // Make a copy of the handlers in case our list of handlers if modified
        // inside a handler.
        const handlers = [...this.handlers];

        for (const handler of handlers) {
            try {
                // Perform all the filtering. We could do this all in one
                // line but this is easier to read and understand.
                if (handler.name && handler.name !== message.name) {
                    continue;
                }

                if (handler.blockType && !areEqual(handler.blockType, message.blockType)) {
                    continue;
                }

                if (handler.block && !areEqual(handler.block, message.block)) {
                    continue;
                }

                // All filters passed, execute the callback.
                handler.callback(message);
            }
            catch (e) {
                // Catch the error and display it so other handlers will still
                // be checked and called.
                console.error(e);
            }
        }
    }

    // #endregion

    // #region Public Functions

    /**
     * Frees up any resources used by this browser bus instance.
     */
    public dispose(): void {
        document.removeEventListener(customDomEventName, this.eventListener);
        this.handlers.splice(0, this.handlers.length);
    }

    /**
     * Publishes a named message without any data.
     *
     * @param messageName The name of the message to publish.
     */
    public publish(messageName: string): void;

    /**
     * Publishes a named message with some custom data.
     *
     * @param messageName The name of the message to publish.
     * @param data The custom data to include with the message.
     */
    public publish(messageName: string, data: unknown): void;

    /**
     * Publishes a named message with some custom data.
     *
     * @param messageName The name of the message to publish.
     * @param data The custom data to include with the message.
     */
    public publish(messageName: string, data?: unknown): void {
        this.publishMessage({
            name: messageName,
            timestamp: Date.now(),
            blockType: this.options.blockType,
            block: this.options.block,
            data
        });
    }

    /**
     * Publishes a message to the browser bus. No changes are made to the
     * message object.
     *
     * Do not use this message to publish a block message unless you have
     * manually filled in the {@link Message.blockType} and
     * {@link Message.block} properties.
     *
     * @param message The message to publish.
     */
    public publishMessage(message: Message): void {
        const event = new CustomEvent<Message>(customDomEventName, {
            detail: message
        });

        document.dispatchEvent(event);
    }

    // #endregion

    // #region subscribe()

    /**
     * Subscribes to the named message from any source.
     *
     * @param messageName The name of the message to subscribe to.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribe<TData = unknown>(messageName: string, callback: BrowserBusCallback<TData>): void;

    /**
     * Subscribes to the named message from any source.
     *
     * @param messageName The name of the message to subscribe to.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribe(messageName: "page.core.queryStringChanged", callback: BrowserBusCallback<QueryStringChangedData>): void;

    /**
     * Subscribes to the named message from any source.
     *
     * @param messageName The name of the message to subscribe to.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribe(messageName: "block.core.beginEdit", callback: BrowserBusCallback<BlockBeginEditData>): void;

    /**
     * Subscribes to the named message from any source.
     *
     * @param messageName The name of the message to subscribe to.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribe(messageName: "block.core.endEdit", callback: BrowserBusCallback<BlockEndEditData>): void;

    /**
     * Subscribes to any message that is sent.
     *
     * @param callback The callback to invoke when the message is received.
     */
    public subscribe(callback: BrowserBusCallback): void;

    /**
     * Subscribes to messages from any source.
     *
     * @param messageNameOrCallback The name of the message to subscribe to or the callback.
     * @param callback The callback to invoke when the message is received.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public subscribe(messageNameOrCallback: string | BrowserBusCallback<any>, callback?: BrowserBusCallback<any>): void {
        let name: string | undefined;

        if (typeof messageNameOrCallback === "string") {
            name = messageNameOrCallback;
        }
        else {
            name = undefined;
            callback = messageNameOrCallback;
        }

        if (!callback) {
            return;
        }

        this.handlers.push({
            name,
            callback
        });
    }

    // #endregion

    // #region subscribeToBlockType()

    /**
     * Subscribes to the named message from any block instance with a matching
     * block type identifier.
     *
     * @param messageName The name of the message to subscribe to.
     * @param blockType The identifier of the block type.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlockType<TData = unknown>(messageName: string, blockType: Guid, callback: BrowserBusCallback<TData>): void;

    /**
     * Subscribes to the named message from any block instance with a matching
     * block type identifier.
     *
     * @param messageName The name of the message to subscribe to.
     * @param blockType The identifier of the block type.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlockType(messageName: "block.core.beginEdit", blockType: Guid, callback: BrowserBusCallback<BlockBeginEditData>): void;

    /**
     * Subscribes to the named message from any block instance with a matching
     * block type identifier.
     *
     * @param messageName The name of the message to subscribe to.
     * @param blockType The identifier of the block type.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlockType(messageName: "block.core.endEdit", blockType: Guid, callback: BrowserBusCallback<BlockEndEditData>): void;

    /**
     * Subscribes to any message that is sent from any block instance with a
     * matching block type identifier.
     *
     * @param blockType The identifier of the block type.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlockType(blockType: Guid, callback: BrowserBusCallback): void;

    /**
     * Subscribes to messages from any block instance with a matching block
     * type identifier.
     *
     * @param messageNameOrBlockType The name of the message to subscribe to or the block type.
     * @param blockTypeOrCallback The block type or the callback function.
     * @param callback The callback to invoke when the message is received.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public subscribeToBlockType(messageNameOrBlockType: string | Guid, blockTypeOrCallback: Guid | BrowserBusCallback<any>, callback?: BrowserBusCallback<any>): void {
        let name: string | undefined;
        let blockType: Guid;

        if (typeof blockTypeOrCallback === "string") {
            name = messageNameOrBlockType;
            blockType = blockTypeOrCallback;
        }
        else {
            blockType = messageNameOrBlockType;
            callback = blockTypeOrCallback;
        }

        if (!blockType || !callback) {
            return;
        }

        this.handlers.push({
            name,
            blockType,
            callback
        });
    }

    // #endregion

    // #region subscribeToBlock()

    /**
     * Subscribes to the named message from a single block instance.
     *
     * @param messageName The name of the message to subscribe to.
     * @param block The identifier of the block.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlock<TData = unknown>(messageName: string, block: Guid, callback: BrowserBusCallback<TData>): void;

    /**
     * Subscribes to the named message from a single block instance.
     *
     * @param messageName The name of the message to subscribe to.
     * @param block The identifier of the block.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlock(messageName: "block.core.beginEdit", block: Guid, callback: BrowserBusCallback<BlockBeginEditData>): void;

    /**
     * Subscribes to the named message from a single block instance.
     *
     * @param messageName The name of the message to subscribe to.
     * @param block The identifier of the block.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlock(messageName: "block.core.endEdit", block: Guid, callback: BrowserBusCallback<BlockEndEditData>): void;

    /**
     * Subscribes to any message that is sent from a single block instance.
     *
     * @param block The identifier of the block.
     * @param callback The callback to invoke when the message is received.
     */
    public subscribeToBlock(block: Guid, callback: BrowserBusCallback): void;

    /**
     * Subscribes to messages from a single block instance.
     *
     * @param messageNameOrBlock The name of the message to subscribe to or the block.
     * @param blockOrCallback The block or the callback function.
     * @param callback The callback to invoke when the message is received.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public subscribeToBlock(messageNameOrBlock: string | Guid, blockOrCallback: Guid | BrowserBusCallback<any>, callback?: BrowserBusCallback<any>): void {
        let name: string | undefined;
        let block: Guid;

        if (typeof blockOrCallback === "string") {
            name = messageNameOrBlock;
            block = blockOrCallback;
        }
        else {
            block = messageNameOrBlock;
            callback = blockOrCallback;
        }

        if (!block || !callback) {
            return;
        }

        this.handlers.push({
            name,
            block,
            callback
        });
    }

    // #endregion
}

// #endregion
