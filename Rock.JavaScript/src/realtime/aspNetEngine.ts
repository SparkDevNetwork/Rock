import { hubConnection, Proxy } from "signalr-no-jquery";
import { Engine } from "./engine";

/**
 * The engine that can connect to an ASP.Net WebForms server.
 */
export class AspNetEngine extends Engine {
    private hub: Proxy | null = null;

    /**
     * Creates a new engine that can connect to an ASP.Net WebForms server.
     */
    public constructor() {
        super();
    }

    /** @inheritdoc */
    public override get connectionId(): string | null {
        return this.hub?.connection.id || null;
    }

    /** @inheritdoc */
    protected startConnection(): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            const connection = hubConnection("/rock-rt", { useDefaultPath: false });
            const hub = connection.createHubProxy("realTime");

            hub.on("message", this.onMessage.bind(this));

            connection.disconnected(() => this.hub = null);

            connection.start()
                .done(() => {
                    this.hub = hub;
                    resolve();
                })
                .fail(() => {
                    reject(new Error("Failed to connect to RealTime hub."));
                });
        });
    }

    /**
     * Called when a message is received from the hub.
     *
     * @param topicIdentifier The identifier of the topic that the message is intended for.
     * @param messageName The name of the message that was received.
     * @param messageParams The parameters to the message.
     */
    private onMessage(topicIdentifier: string, messageName: string, messageParams: unknown[]): void {
        this.emitter.emit(`${topicIdentifier}-${messageName}`, messageParams);
    }

    /** @inheritdoc */
    public override async invokeCore(messageName: string, ...args: unknown[]): Promise<unknown> {
        if (!this.hub) {
            throw new Error("Connection to the RealTime system was lost.");
        }

        return await this.hub.invoke(messageName, ...args);
    }
}
