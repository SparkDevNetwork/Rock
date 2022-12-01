import { hubConnection, Proxy, Connection } from "signalr-no-jquery";
import { Engine } from "./engine";

interface IExtendedConnection extends Connection {
    reconnected(callback: () => void): void;
}

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
            const connection = hubConnection("/rock-rt", { useDefaultPath: false }) as IExtendedConnection;
            const hub = connection.createHubProxy("realTime");

            hub.on("message", this.onMessage.bind(this));
            connection.reconnecting(() => this.onTransportReconnecting());
            connection.reconnected(() => this.onTransportReconnect());
            connection.disconnected(() => {
                this.hub = null;
                this.onTransportDisconnect();
            });

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

    /** @inheritdoc */
    protected closeConnection(): Promise<void> {
        return new Promise((resolve) => {

            if (this.hub) {
                this.hub.connection.stop();
                this.hub = null;
            }

            resolve();
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
        this.emit(topicIdentifier, messageName, messageParams);
    }

    /** @inheritdoc */
    public override async invokeCore(messageName: string, ...args: unknown[]): Promise<unknown> {
        if (!this.hub) {
            throw new Error("Connection to the RealTime system was lost.");
        }

        return await this.hub.invoke(messageName, ...args);
    }
}
