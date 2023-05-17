import { GenericServerFunctions, ServerFunctions } from "./types";
import { Engine } from "./engine";

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
                    return await engine.invoke(identifier, propertyName, args);
                }
            };
        }
    });
}

export class Topic<TServer extends ServerFunctions<TServer> = GenericServerFunctions> {
    private engine: Engine;
    private identifier: string;

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
     * Registers a handler to be called when any message is received.
     *
     * @param handler The handler to be called when a message is received.
     */
    public onMessage(handler: ((messageName: string, args: unknown[]) => void)): void {
        this.engine.onMessage(this.identifier, handler);
    }

    /**
     * Registers a callback to be called when the connection has been
     * temporarily lost. An automatic reconnection is in progress. The topic
     * is now in a state where it can not send any messages.
     * 
     * @param callback The callback to be called.
     */
    public onReconnecting(callback: (() => void)): void {
        this.engine.onReconnecting(callback);
    }

    /**
     * Registers a callback to be called when the connection has been
     * reconnected. The topic can now send messages again.
     * 
     * @param callback The callback to be called.
     */
    public onReconnected(callback: (() => void)): void {
        this.engine.onReconnected(callback);
    }

    /**
     * Registers a callback to be called when the connection has been lost
     * and will no longer try to reconnect.
     * 
     * @param callback The callback to be called.
     */
    public onDisconnected(callback: (() => void)): void {
        this.engine.onDisconnected(callback);
    }
}
