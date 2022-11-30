import { GenericServerFunctions, ServerFunctions } from "./types";
import { Engine } from "./engine";

function isPromise<T>(obj: PromiseLike<T> | T): obj is PromiseLike<T> {
    return !!obj && (typeof obj === "object" || typeof obj === "function") && typeof (obj as Record<string, unknown>).then === "function";
}

function createServerProxy<TServer extends ServerFunctions<TServer> = GenericServerFunctions>(engine: Engine, identifier: string, skipReconnectingCheck: boolean): TServer {
    return new Proxy<TServer>({} as TServer, {
        get(_, propertyName) {
            return async (...args: unknown[]): Promise<unknown> => {
                if (typeof propertyName !== "string") {
                    return;
                }

                if (!skipReconnectingCheck) {
                    await engine.ensureConnected();
                }

                try {
                    return await engine.invoke(identifier, propertyName, args);
                }
                catch (error) {
                    if (engine.isDisconnected || skipReconnectingCheck || !engine.isReconnecting) {
                        throw error;
                    }

                    await engine.ensureConnected();
                    await engine.invoke(identifier, propertyName, args);
                }
            };
        }
    });
}

export class Topic<TServer extends ServerFunctions<TServer> = GenericServerFunctions> {
    private engine: Engine;
    private identifier: string;
    private reconnectCallbacks: (((server: TServer) => void) | ((server: TServer) => PromiseLike<void>))[] = [];

    /**
     * Allows messages to be sent to the server. Any property access is treated
     * like a message function whose property name is the message name.
     */
    public server: TServer;

    /**
     * Creates a new topic proxy that will facilitate communication with the
     * Rock RealTime backend.
     *
     * @param identifier The unique identifier of the topic to connect to.
     * @param engine The engine that will handle communication to the backend.
     */
    public constructor(identifier: string, engine: Engine) {
        this.identifier = identifier;
        this.engine = engine;

        engine.onReconnect(async () => this.reconnected());

        this.server = createServerProxy<TServer>(engine, identifier, false);
    }

    /**
     * Gets the connection identifier for this topic. This will be the same for
     * all topics, but that should not be relied on staying that way in the future.
     */
    public get connectionId(): string | null {
        return this.engine.connectionId;
    }

    /**
     * Gets a value that indicates if the topic is currently reconnecting.
     */
    public get isReconnecting(): boolean {
        return this.engine.isReconnecting;
    }

    /**
     * Gets a value that indicates if the topic is disconnected and will no
     * longer try to connect to the server.
     */
    public get isDisconnected(): boolean {
        return this.engine.isDisconnected;
    }

    /**
     * Connects to the topic so that the backend knows of our presense.
     */
    public async connect(): Promise<void> {
        await this.engine.invokeCore("connectToTopic", this.identifier);
    }

    /**
     * Registers a handler to be called when a message with the given name
     * is received.
     *
     * @param messageName The message name that will trigger the handler.
     * @param handler The handler to be called when a message is received.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public on(messageName: string, handler: ((...args: any[]) => void)): void {
        this.engine.on(this.identifier, messageName, handler);
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and then reconnected. This means a new connection identifier is now
     * in use and any state information has been lost.The callback will be
     * called with a special server proxy that can be used while in a
     * reconnecting state. The normal server proxy will pause messages until
     * the reconnect is completed and all callbacks have finished.
     * 
     * @param callback The callback to be called.
     */
    public onReconnect(callback: ((server: TServer) => void) | ((server: TServer) => PromiseLike<void>)): void {
        this.reconnectCallbacks.push(callback);
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and will no longer try to reconnect.
     * 
     * @param callback The callback to be called.
     */
    public onDisconnect(callback: (() => void) | (() => PromiseLike<void>)): void {
        this.engine.onDisconnect(callback);
    }

    /**
     * Called once we have performed a full reconnection. Reconnect to the topic
     * on the server and then fire off any reconnect callbacks registered on
     * this topic.
     */
    private async reconnected(): Promise<void> {
        await this.connect();

        const serverProxy = createServerProxy<TServer>(this.engine, this.identifier, true);

        for (const callback of this.reconnectCallbacks) {
            try {
                const result = callback(serverProxy);

                if (isPromise(result)) {
                    await result;
                }
            }
            catch (error) {
                console.error(error);
            }
        }
    }
}
