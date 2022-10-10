import mitt, { Emitter, EventType } from "mitt";

/**
 * General functionality for any RealTime engine.
 */
export abstract class Engine {
    protected readonly emitter: Emitter<Record<EventType, unknown[]>>;
    private isStarting: boolean = false;
    private startResolve!: (() => void);
    private startReject!: ((reason: unknown) => void);
    private startPromise: Promise<void>;

    protected constructor() {
        this.startPromise = new Promise<void>((resolve, reject) => {
            this.startResolve = resolve;
            this.startReject = reject;
        });

        this.emitter = mitt();
    }

    /**
     * Get the identifier that uniquely identifiers our connection to the server.
     * Returns null is not yet connected.
     */
    public abstract get connectionId(): string | null;

    /**
     * Start the connection to the RealTime backend. This will only ever be
     * called once.
     */
    protected abstract startConnection(): Promise<void>;

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
        if (this.isStarting) {
            return await this.startPromise;
        }

        this.isStarting = true;
        try {
            await this.startConnection();
            this.startResolve();
        }
        catch (error: unknown) {
            this.startReject(error);
        }
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
}
