/**
 * A simple retry policy class for throttling RealTime engine reconnects.
 */
export class RetryPolicy {
    private attemptCount = 0;
    private initialBackoff = 1000;
    private maximumBackoff = 60000;
    private backoffMultiplier = 1.5;
    private backoffRandomizer = 0.2;

    /**
     * Returns a promise that waits for the proper delay according to the
     * rules defined by this policy.
     */
    waitForRetry(): Promise<void> {
        this.attemptCount++;

        var delay = this.getBackoffDelay();

        return new Promise<void>((resolve) => {
            setTimeout(() => resolve(), delay);
        });
    }

    /**
     * Gets the backoff delay for the current attempt.
     */
    private getBackoffDelay(): number {
        var delay = this.initialBackoff;

        for (let i = 1; i < this.attemptCount; i++) {
            delay *= this.backoffMultiplier;
        }

        var randomizer = Math.max(0, this.backoffRandomizer);

        if (randomizer > Number.EPSILON) {
            var rnd = Math.random() * (delay * randomizer);

            delay += rnd;
        }

        return Math.min(this.maximumBackoff, delay);
    }
}
