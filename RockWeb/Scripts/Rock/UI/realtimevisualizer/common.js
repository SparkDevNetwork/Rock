const Fireworks = (await import("https://cdn.jsdelivr.net/npm/fireworks-js/+esm")).Fireworks;
const confettiFactory = (await import("https://cdn.jsdelivr.net/npm/canvas-confetti/+esm")).create;

class Helper {
    fireworks = undefined;
    fireworksCounter = 0;
    confetti = undefined;

    /**
     * Creates a new instance of the helper utilities for the realtime visualizer.
     * 
     * @param {HTMLElement} container
     */
    constructor(container) {
        // Intialize the fireworks.
        this.fireworks = new Fireworks(container);

        // Initialize the confetti.
        const confettiCanvas = document.createElement("canvas");
        confettiCanvas.setAttribute("width", container.clientWidth.toString());
        confettiCanvas.setAttribute("height", container.clientHeight.toString());
        container.append(confettiCanvas);
        this.confetti = confettiFactory(confettiCanvas);
    }

    /**
     * Sets the height of the item. This calculates the height required
     * to display the entire item, including any margins on the first
     * and last child. This allows items to gently slide down to make
     * room for the new item.
     *
     * @param {HTMLElement} item The element whose height is to be set.
     */
    setItemHeight(item) {
        let topMargin = 0;
        let bottomMargin = 0;
        // get computed style for item
        let itemStyle = window.getComputedStyle(item);
        let itemYPadding = Number(itemStyle.paddingTop.replace("px", "") || "0") + Number(itemStyle.paddingBottom.replace("px", "") || "0");

        if (item.children.length > 0) {
            let computedStyle = window.getComputedStyle(item.children[0]);
            topMargin = Number(computedStyle.marginTop.replace("px", "") || "0");

            computedStyle = window.getComputedStyle(item.children[item.children.length - 1]);
            bottomMargin = Number(computedStyle.marginBottom.replace("px", "") || "0");
        }

        item.style.height = `${item.scrollHeight + topMargin + bottomMargin + itemYPadding}px`;
    }

    /**
     * Plays the audio file found on the item or uses the default URL. The item is
     * searched for a data-audio-url on itself or any child element.
     * 
     * @param {HTMLElement} item The element that will be searched for a data-audio-url attribute.
     * @param {string} defaultAudioUrl The default URL to use if one was not defined on the item.
     */
    playAudio(item, defaultAudioUrl) {
        try {
            let audioUrl = defaultAudioUrl || "";
            const urlElement = item.querySelector("[data-audio-url]");

            if (urlElement) {
                audioUrl = urlElement.dataset.audioUrl;
            }

            if (audioUrl) {
                new Audio(audioUrl).play();
            }
        }
        catch (error) {
            console.error(error);
        }
    }

    /**
     * Starts showing fireworks if they have not been started. If they have been
     * started then it increments a counter so the fireworks will only stop when
     * the counter has reached zero.
     */
    startFireworks() {
        if (this.fireworks && !this.fireworks.isRunning) {
            this.fireworks.start();
        }

        this.fireworksCounter++;
    }

    /**
     * Stops the fireworks. This actually decrements the tracked request count and
     * when it reaches zero they will be stopped.
     */
    stopFireworks() {
        if (this.fireworksCounter > 0) {
            this.fireworksCounter--;
        }

        if (!this.fireworks || this.fireworksCounter > 0) {
            return;
        }

        this.fireworks.waitStop().then(() => {
            // If we got a request to start the fireworks before they completely
            // cleared, then start them up again.
            if (this.fireworksCounter > 0) {
                this.fireworks.start();
            }
        });
    }

    /**
     * Shows a single burst of confetti.
     */
    showConfetti() {
        if (!this.confetti) {
            return;
        }

        this.confetti({
            particleCount: 100,
            startVelocity: 75,
            decay: 0.92,
            spread: 90,
            angle: 35,
            origin: { x: 0, y: 0.75 }
        });

        this.confetti({
            particleCount: 100,
            startVelocity: 75,
            decay: 0.92,
            spread: 90,
            angle: 125,
            origin: { x: 1, y: 0.75 }
        });
    }
}

export {
    Helper
};
