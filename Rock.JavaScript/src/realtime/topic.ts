import { GenericServerFunctions, ServerFunctions } from "./types";
import { Engine } from "./engine";

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

        this.server = new Proxy<TServer>({} as TServer, {
            get(_, propertyName) {
                return async (...args: unknown[]): Promise<unknown> => {
                    if (typeof propertyName !== "string") {
                        return;
                    }

                    return await engine.invoke(identifier, propertyName, args);
                };
            }
        });
    }

    /**
     * Gets the connection identifier for this topic. This will be the same for
     * all topics, but that should not be relied on staying that way in the future.
     */
    public get connectionId(): string | null {
        return this.engine.connectionId;
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
}
