import { AspNetEngine } from "./aspNetEngine";
import { Engine } from "./engine";
import { RetryPolicy } from "./retryPolicy";
import { Topic } from "./topic";
import { GenericServerFunctions, ServerFunctions } from "./types";

let globalEnginePromise: Promise<Engine> | null = null;

async function getEngine(): Promise<Engine> {
    let enginePromise = globalEnginePromise;

    if (!enginePromise) {
        let retryPolicy: RetryPolicy | null = null;

        enginePromise = globalEnginePromise = new Promise<Engine>(async (resolve, reject) => {
            // Try up to 10 times to connect. After each attempt we will wait
            // a period of time before trying again.
            for (let attemptCount = 0; attemptCount < 10; attemptCount++) {
                if (retryPolicy) {
                    await retryPolicy.waitForRetry();
                }

                try {
                    const engine = new AspNetEngine();

                    engine.onDisconnected(() => {
                        globalEnginePromise = null;
                    });

                    await engine.ensureConnected();

                    return resolve(engine);
                }
                catch (error) {
                    if (!retryPolicy) {
                        retryPolicy = new RetryPolicy();
                    }
                }
            }

            reject(new Error("Failed to connect to RealTime engine and maximum retry count has been exceeded."));
        });
    }

    return await enginePromise;
}

async function getTopic<TServer extends ServerFunctions<TServer> = GenericServerFunctions>(identifier: string): Promise<Topic<TServer>> {
    const engine = await getEngine();

    // Ensure we are still connected since we might be in a reconnecting state.
    await engine.ensureConnected();

    const topic = new Topic<TServer>(identifier, engine);

    await topic.connect();

    return topic;
}

export { getTopic };
