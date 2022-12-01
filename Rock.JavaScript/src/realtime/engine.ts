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

function isPromise<T>(obj: PromiseLike<T> | T): obj is PromiseLike<T> {
    return !!obj && (typeof obj === "object" || typeof obj === "function") && typeof (obj as Record<string, unknown>).then === "function";
}

const maxReconnectAttempts = 10;
const reconnectDelay: number[] = [500, 2_500, 5_000, 10_000, 30_000];

/**
 * General functionality for any RealTime engine.
 */
export abstract class Engine {
    private readonly emitter: Emitter<Record<EventType, unknown[]>>;
    private startPromise: PromiseCompletionSource | null = null;
    private reconnectPromise: PromiseCompletionSource | null = null;
    private reconnectAttemptCount: number = 0;
    private reconnectTimer: NodeJS.Timeout | null = null;
    private reconnectCallbacks: ((() => void) | (() => PromiseLike<void>))[] = [];
    private disconnectCallbacks: (() => void)[] = [];
    private isDisconnectedInternal: boolean = false;

    protected constructor() {
        this.emitter = mitt();
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

        if (this.startPromise) {
            await this.startPromise.promise;

            if (this.reconnectPromise) {
                await this.reconnectPromise.promise;
            }

            return;
        }

        this.startPromise = new PromiseCompletionSource();
        try {
            await this.startConnection();
            this.startPromise.resolve();
        }
        catch (error: unknown) {
            this.disconnected();
            this.startPromise.reject(error);

            throw error;
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
    protected onTransportReconnecting(): void {
        if (!this.reconnectPromise) {
            this.reconnectPromise = new PromiseCompletionSource();
        }
    }

    /**
     * Notification that the engine has completed a transport level reconnection.
     * A reconnect can be successful even if the transport has connected to an
     * entirely different server - which is an invalid state for us.
     */
    protected async onTransportReconnect(): Promise<void> {
        if (!this.reconnectPromise) {
            return;
        }

        // Check if the server still knows about our connection.
        if (await this.invokeCore("isConnectionValid") === true) {
            this.reconnectPromise.resolve();
            this.reconnectPromise = null;
            return;
        }

        this.scheduleReconnectAttempt();
    }

    /**
     * Notification that the engine has had a transport level disconnection.
     * No automatic reconnect attempts are being performed by the transport.
     */
    protected onTransportDisconnect(): void {
        this.scheduleReconnectAttempt();
    }

    /**
     * Schedule a reconnect attempt. This is not a transport level reconnect
     * so all connection state has been lost.
     */
    private scheduleReconnectAttempt(): void {
        if (this.isDisconnected || this.reconnectTimer) {
            return;
        }

        if (!this.reconnectPromise) {
            this.reconnectPromise = new PromiseCompletionSource();
        }

        this.reconnectAttemptCount += 1;
        this.reconnectTimer = setTimeout(() => this.attemptReconnect(), this.getReconnectDelay());
    }

    /**
     * Attempt a full reconnect. First we ensure we are fully disconnected at
     * the transport level and then start a new connection.
     */
    private async attemptReconnect(): Promise<void> {
        try {
            // Close the connection and then start it again. After we have
            // connected call our reconnected function to emit callbacks.
            await this.closeConnection();
            await this.startConnection();
            await this.reconnected();
        }
        catch (error) {
            this.reconnectAttemptCount += 1;

            if (this.reconnectAttemptCount <= maxReconnectAttempts) {
                this.reconnectTimer = setTimeout(() => this.attemptReconnect(), this.getReconnectDelay());
            }
            else {
                this.disconnected();

                throw error;
            }
        }
    }

    /**
     * Gets the time to wait before attempting to reconnect.
     *
     * @returns The number of milliseconds to wait.
     */
    private getReconnectDelay(): number {
        return this.reconnectAttemptCount < reconnectDelay.length
            ? reconnectDelay[this.reconnectAttemptCount]
            : reconnectDelay[reconnectDelay.length - 1];
    }

    /**
     * Called once we have performed a full reconnection and need to fire all
     * reconnect callbacks.
     */
    private async reconnected(): Promise<void> {
        for (const callback of this.reconnectCallbacks) {
            try {
                const result = callback();

                if (isPromise(result)) {
                    await result;
                }
            }
            catch (error) {
                console.error(error);
            }
        }

        // Clean up the existing reconnection attempt data.
        this.reconnectAttemptCount = 0;
        this.reconnectPromise?.resolve();
        this.reconnectPromise = null;
        this.reconnectTimer = null;
    }

    /**
     * Called once we have hit a full disconnect situation where we will
     * no longer try to connect or reconnect.
     */
    private disconnected(): void {
        this.isDisconnectedInternal = true;
        this.reconnectPromise?.reject("RealTime engine is disconnected.");
        this.reconnectPromise = null;
        this.reconnectTimer = null;

        for (const callback of this.disconnectCallbacks) {
            try {
                callback();
            }
            catch (error) {
                console.error(error);
            }
        }
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and then reconnected. This means a new connection identifier is now
     * in use and any state information has been lost.
     * 
     * @param callback The callback to be called.
     */
    public onReconnect(callback: (() => void) | (() => PromiseLike<void>)): void {
        this.reconnectCallbacks.push(callback);
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and will no longer try to reconnect.
     * 
     * @param callback The callback to be called.
     */
    public onDisconnect(callback: () => void): void {
        this.disconnectCallbacks.push(callback);
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
     * Emits the event for the topic and message name.
     * 
     * @param topicIdentifier The identifier of the topic that the message was received from.
     * @param messageName The name of the message that was received.
     * @param eventArgs The arguments that were sent with the message.
     */
    protected emit(topicIdentifier: string, messageName: string, eventArgs: unknown[]): void {
        this.emitter.emit(`${topicIdentifier}-${messageName}`, eventArgs);
    }
}
