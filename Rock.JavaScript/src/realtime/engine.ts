import mitt, { Emitter, EventType } from "mitt";

/**
 * Simple deferred promise that can be completed at a later time.
 */
class PromiseCompletionSource<T = void> {
    private resolvePromise!: ((value: T) => void);
    private rejectPromise!: ((reason?: unknown) => void);
    private internalPromise: Promise<T>;

    constructor() {
        this.internalPromise = new Promise<T>((resolve, reject) => {
            this.resolvePromise = resolve;
            this.rejectPromise = reject;
        });
    }

    public resolve(value: T): void {
        this.resolvePromise(value);
    }

    public reject(reason?: unknown): void {
        this.rejectPromise(reason);
    }

    public get promise(): Promise<T> {
        return this.internalPromise;
    }
}

const StateEvent = {
    Reconnecting: "reconnecting",
    Reconnected: "reconnected",
    Disconnected: "disconnected"
};

/**
 * General functionality for any RealTime engine.
 */
export abstract class Engine {
    private readonly emitter: Emitter<Record<EventType, unknown[]>>;
    private readonly topicEmitter: Emitter<Record<EventType, { messageName: string, args: unknown[] }>>;
    private readonly stateEmitter: Emitter<Record<EventType, unknown[]>>;
    private startPromise: Promise<void> | null = null;
    private reconnectPromise: PromiseCompletionSource | null = null;
    private isDisconnectedInternal: boolean = false;

    protected constructor() {
        this.emitter = mitt();
        this.stateEmitter = mitt();
        this.topicEmitter = mitt();
    }

    /**
     * Get the identifier that uniquely identifiers our connection to the server.
     * Returns null is not yet connected.
     */
    public abstract get connectionId(): string | null;

    /**
     * Start the connection to the RealTime backend. This may be called more
     * than once to perform a reconnect, but closeConnection should always
     * be called before this is.
     */
    protected abstract startConnection(): Promise<void>;

    /**
     * Close the transport connection to the RealTime backend.
     */
    protected abstract closeConnection(): Promise<void>;

    /**
     * Invokes the message and sends it up to the Rock server.
     *
     * @param topicIdentifier The identifier of the topic to send the message to.
     * @param messageName The name of the message to be sent.
     * @param messageParams The parameters to pass to the message.
     *
     * @returns The value returned by the message call.
     */
    public async invoke(topicIdentifier: string, messageName: string, messageParams: unknown[]): Promise<unknown> {
        return await this.invokeCore("postMessage", topicIdentifier, messageName, messageParams);
    }

    /**
     * Invokes the core engine message and sends it up to the Rock server.
     *
     * @param messageName The name of the message to be sent.
     * @param messageParams The parameters to pass to the message.
     *
     * @returns The value returned by the message call.
     */
    public abstract invokeCore(messageName: string, ...args: unknown[]): Promise<unknown>;

    /**
     * Makes sure the engine is connected to the RealTime system and waits
     * until it is connected. Throws an exception if could not be connected.
     */
    public async ensureConnected(): Promise<void> {
        if (this.isDisconnected) {
            throw new Error("RealTime engine is disconnected.");
        }

        if (!this.startPromise) {
            this.startPromise = new Promise<void>(async (resolve, reject) => {
                try {
                    await this.startConnection();
                    resolve();
                }
                catch (error) {
                    this.disconnected();
                    reject(error);
                }
            });
        }

        await this.startPromise;

        if (this.reconnectPromise) {
            await this.reconnectPromise.promise;
        }
    }

    /**
     * Gets a value that indicates if the engine is currently reconnecting.
     */
    public get isReconnecting(): boolean {
        return this.reconnectPromise !== null;
    }

    /**
     * Gets a value that indicates if the engine is disconnected. This will
     * return true if no more attempts to connect will be made.
     */
    public get isDisconnected(): boolean {
        return this.isDisconnectedInternal;
    }

    /**
     * Notification that the engine is attempting to perform transport level
     * reconnection.
     */
    protected transportReconnecting(): void {
        if (!this.reconnectPromise) {
            this.reconnectPromise = new PromiseCompletionSource();

            this.stateEmitter.emit(StateEvent.Reconnecting, []);
        }
    }

    /**
     * Notification that the engine has completed a transport level reconnection.
     * A reconnect can be successful even if the transport has connected to an
     * entirely different server - which is an invalid state for us.
     */
    protected async transportReconnected(): Promise<void> {
        if (!this.reconnectPromise) {
            return;
        }

        // Check if the server still knows about our connection.
        if (await this.invokeCore("isConnectionValid") === true) {
            this.reconnectPromise.resolve();
            this.reconnectPromise = null;

            this.stateEmitter.emit(StateEvent.Reconnected, []);

            return;
        }

        this.disconnected();
        this.closeConnection();
    }

    /**
     * Notification that the engine has had a transport level disconnection.
     * No automatic reconnect attempts are being performed by the transport.
     */
    protected transportDisconnected(): void {
        this.disconnected();
    }

    /**
     * Called once we have hit a full disconnect situation where we will
     * no longer try to connect or reconnect.
     */
    private disconnected(): void {
        this.isDisconnectedInternal = true;
        this.reconnectPromise?.reject("RealTime engine is disconnected.");
        this.reconnectPromise = null;

        this.stateEmitter.emit(StateEvent.Disconnected, []);
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and is attempting to reconnect automatically. Messages can not be
     * sent at this time.
     * 
     * @param callback The callback to be called.
     */
    public onReconnecting(callback: (() => void)): void {
        this.stateEmitter.on(StateEvent.Reconnecting, callback);
    }

    /**
     * Registers a callback to be called when the connection has been
     * reconnected and is ready to send messages again.
     * 
     * @param callback The callback to be called.
     */
    public onReconnected(callback: (() => void)): void {
        this.stateEmitter.on(StateEvent.Reconnected, callback);
    }

    /**
     * Registers a callback to be called when the connection has been lost.
     * 
     * @param callback The callback to be called.
     */
    public onDisconnected(callback: () => void): void {
        this.stateEmitter.on(StateEvent.Disconnected, callback);
    }

    /**
     * Adds a listener for an incoming message to the specified topic.
     *
     * @param topicIdentifier The identifier of the topic that will receive the message.
     * @param messageName The name of the message to be received.
     * @param handler The function to call when a message is received.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- It's going to be up to caller to ensure they use the right handler arguments.
    public on(topicIdentifier: string, messageName: string, handler: (...args: any[]) => void): void {
        this.emitter.on(`${topicIdentifier}-${messageName}`, (eventArgs: unknown[]) => handler(...eventArgs));
    }

    /**
     * Adds a listener for all incoming messages to the specified topic.
     *
     * @param topicIdentifier The identifier of the topic that will receive the message.
     * @param handler The function to call when a message is received.
     */
    public onMessage(topicIdentifier: string, handler: (messageName: string, args: unknown[]) => void): void {
        this.topicEmitter.on(topicIdentifier, ev => {
            handler(ev.messageName, ev.args);
        });
    }

    /**
     * Emits the event for the topic and message name.
     * 
     * @param topicIdentifier The identifier of the topic that the message was received from.
     * @param messageName The name of the message that was received.
     * @param eventArgs The arguments that were sent with the message.
     */
    protected emit(topicIdentifier: string, messageName: string, eventArgs: unknown[]): void {
        this.emitter.emit(`${topicIdentifier}-${messageName}`, eventArgs);
        this.topicEmitter.emit(topicIdentifier, { messageName: messageName, args: eventArgs });
    }
}
