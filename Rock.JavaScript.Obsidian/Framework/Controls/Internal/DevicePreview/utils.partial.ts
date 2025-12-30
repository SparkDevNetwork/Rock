import { Ref, watch } from "vue";

export function useAutoHeightIframe(iframeRef: Ref<HTMLIFrameElement | null | undefined>, stateRef: Ref<{ isLoaded: boolean; }>): void {
    let observer: MutationObserver | null = null;

    function getCurrentHeight(iframe: HTMLIFrameElement): number {
        const doc = iframe.contentDocument ?? iframe.contentWindow?.document;

        return Math.max(
            doc?.body?.scrollHeight ?? 0,
            doc?.documentElement?.scrollHeight ?? 0
        );
    }

    async function getStableHeight(iframe: HTMLIFrameElement): Promise<number> {
        return new Promise((resolve) => {
            const start = Date.now();

            // Maximum number of attempts to wait for a stable height before giving up and resolving anyway.
            const maxTotalTries = 20;

            // Number of consecutive height checks required before treating the iframe content as stable.
            const minStableTries = 8;

            let totalTries = 0;
            let stableTries = 0;
            let lastHeight = -1;

            const check = (): void => {
                const currentHeight = getCurrentHeight(iframe);

                if (currentHeight !== 0) {
                    if (currentHeight !== lastHeight) {
                        lastHeight = currentHeight;
                        stableTries = 0;
                    }
                    else {
                        stableTries++;
                    }
                }

                totalTries++;

                if (stableTries >= minStableTries || totalTries >= maxTotalTries) {
                    console.log(`Iframe height stabilized after ${totalTries - stableTries} tries in ${Date.now() - start}ms.`);
                    resolve(currentHeight);
                }
                else {
                    requestAnimationFrame(check);
                }
            };

            requestAnimationFrame(check);
        });
    }

    // Clear the height of the iframe when the src or srcdoc attributes change.
    // This is important because the iframe may be reloaded with a new src or srcdoc value,
    // and we want to reset the height to allow the new content to be measured correctly.
    watch(iframeRef, (newIframe, _, onCleanup) => {
        if (observer) {
            observer.disconnect();
            observer = null;
        }

        if (newIframe) {
            // Observe attribute changes to detect early 'src' or 'srcdoc' updates
            observer = new MutationObserver(() => {
                // Reset height.
                newIframe.style.height = "";
            });

            observer.observe(newIframe, {
                attributes: true,
                attributeFilter: ["src", "srcdoc"]
            });

            onCleanup(() => {
                observer?.disconnect();
                observer = null;
            });
        }
    }, { immediate: true });

    watch([iframeRef, stateRef], async ([iframe, state], [_oldIframe, oldState]) => {
        if (iframe) {
            if (state !== oldState && state.isLoaded) {
                const height = await getStableHeight(iframe);
                const heightPx = `${height ? `${height}px` : ""}`;
                iframe.style.height = heightPx;
            }
            else {
                // Reset height when iframe is not loaded or is null.
                iframe.style.height = "";
            }
        }
    }, {
        immediate: true
    });
}