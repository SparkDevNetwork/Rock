import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Engine } from "./engine";

/**
 * The engine that can connect to an NET.Core server.
 */
export class NetCoreEngine extends Engine {
    private connection: HubConnection | null = null;
    private isManuallyDisconnecting: boolean = false;

    /**
     * Creates a new engine that can connect to an ASP.Net WebForms server.
     */
    public constructor() {
        super();
    }

    /** @inheritdoc */
    public override get connectionId(): string | null {
        return this.connection?.connectionId || null;
    }

    /** @inheritdoc */
    protected async startConnection(): Promise<void> {
        const connection = new HubConnectionBuilder()
            .withUrl("/rock-rt")
            .withAutomaticReconnect()
            .build();

        connection.on("message", this.onMessageReceived.bind(this));
        connection.onreconnecting(() => {
            if (!this.isManuallyDisconnecting) {
                this.transportReconnecting();
            }
        });
        connection.onreconnected(() => {
            if (!this.isManuallyDisconnecting) {
                this.transportReconnected();
            }
        });
        connection.onclose(() => {
            this.connection = null;
            if (!this.isManuallyDisconnecting) {
                this.transportDisconnected();
            }
        });

        await connection.start();

        this.connection = connection;
    }

    /** @inheritdoc */
    protected async closeConnection(): Promise<void> {
        if (this.connection) {
            this.isManuallyDisconnecting = true;
            await this.connection.stop();
            this.connection = null;
        }
    }

    /**
     * Called when a message is received from the hub.
     *
     * @param topicIdentifier The identifier of the topic that the message is intended for.
     * @param messageName The name of the message that was received.
     * @param messageParams The parameters to the message.
     */
    private onMessageReceived(topicIdentifier: string, messageName: string, messageParams: unknown[]): void {
        this.emit(topicIdentifier, messageName, messageParams);
    }

    /** @inheritdoc */
    public override async invokeCore(messageName: string, ...args: unknown[]): Promise<unknown> {
        if (!this.connection) {
            throw new Error("Connection to the RealTime system was lost.");
        }

        return await this.connection.invoke(messageName, ...args);
    }
}
