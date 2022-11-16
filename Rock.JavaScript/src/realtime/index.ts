import { AspNetEngine } from "./aspNetEngine";
import { Topic } from "./topic";
import { GenericServerFunctions, ServerFunctions } from "./types";

const engine = new AspNetEngine();

async function getTopic<TServer extends ServerFunctions<TServer> = GenericServerFunctions>(identifier: string): Promise<Topic<TServer>> {
    await engine.ensureConnected();

    const topic = new Topic<TServer>(identifier, engine);

    await topic.connect();

    return topic;
}

export { getTopic };
