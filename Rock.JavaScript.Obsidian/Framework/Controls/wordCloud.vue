<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <svg ref="svgElement" class="rock-word-cloud" :style="svgStyle"></svg>
</template>

<script setup lang="ts">
    import { cloud } from "@Obsidian/Libs/d3";
    import { PropType, computed, onMounted, ref, watch, onBeforeUnmount } from "vue";

    /** Custom type to hold all our on-screen element information. */
    type WordElement = {
        fontSize: number;

        x: number;

        y: number;

        rotation: number;

        previousFontSize: number;

        previousX: number;

        previousY: number;

        previousRotation: number;

        element: SVGTextElement;
    };

    const props = defineProps({
        /** A list of case-sensitive words to be used in drawing the cloud. */
        words: {
            type: Array as PropType<string[]>,
            default: []
        },

        /**
         * The width of the cloud SVG. This can be set to an empty string in
         * order to allow CSS to set the width. This can also be a percentage
         * value such as 100%.
         */
        width: {
            type: [Number, String] as PropType<number | string>,
            default: 500
        },

        /**
         * The height of the cloud SVG. This can be set to an empty string in
         * order to allow CSS to set the height. This can also be a percentage
         * value such as 100%.
         */
        height: {
            type: [Number, String] as PropType<number | string>,
            default: 500
        },

        /**
         * The number of angles that will be used between minimumAngle and
         * maximumAngle, inclusive. If you provide a value of 3 then you will
         * get 3 total angles: minimumAngle, maximumAngle and the half-way
         * point between the two.
         */
        angleCount: {
            type: Number as PropType<number>,
            default: 5
        },

        /**
         * The minimum angle to use when rotating the text, this is expressed
         * in degrees with 0 being normal left-to-right text.
         */
        minimumAngle: {
            type: Number as PropType<number>,
            default: -90
        },

        /**
         * The maximum angle to use when rotating the text, this is expressed
         * in degrees with 0 being normal left-to-right text.
         */
        maximumAngle: {
            type: Number as PropType<number>,
            default: 90
        },

        /** The font name to use when drawing the words. */
        fontName: {
            type: String as PropType<string>,
            default: "Impact"
        },

        /** The minimum font size to use when drawing the words. */
        minimumFontSize: {
            type: Number as PropType<number>,
            default: 10
        },

        /** The maximum font size to use when drawing the words. */
        maximumFontSize: {
            type: Number as PropType<number>,
            default: 96
        },

        /** The list of CSS colors to use when drawing the words. */
        colors: {
            type: Array as PropType<string[]>,
            default: ["#0193B9", "#F2C852", "#1DB82B", "#2B515D", "#ED3223"]
        },

        /** The amount of padding, in pixels, to put between words. */
        wordPadding: {
            type: Number as PropType<number>,
            default: 5
        },

        /**
         * By default the word cloud will preserve the existing size and
         * position of words and then animate them to their new position.
         * Setting this to true will disable that behavior and clear the
         * SVG before rendering the new words.
         */
        autoClear: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * The duration of the animation when placing and drawing the
         * words onto the SVG surface.
         */
        animationDuration: {
            type: Number as PropType<number>,
            default: 350
        }
    });

    const emit = defineEmits<{
        /** Called just before the word cloud begins an update of the surface. */
        (e: "beginDraw"): void,

        /** Called just after the word cloud has finished drawing the words. */
        (e: "endDraw"): void
    }>();

    // #region Values

    const svgElement = ref<HTMLCanvasElement | null>(null);
    const elements: Record<string, WordElement> = {};
    let animationStartTime: number | null = null;
    let animationFrame: number | null = null;
    let isUpdateRequested = false;
    let resizeObserver: ResizeObserver | null = null;

    // #endregion

    // #region Computed Values

    /**
     * The style values to be applied to the SVG element.
     */
    const svgStyle = computed((): Record<string, string> => {
        const styles: Record<string, string> = {};

        if (typeof props.width === "string") {
            styles.width = props.width;
        }
        else {
            styles.width = `${props.width}px`;
        }

        if (typeof props.height === "string") {
            styles.height = props.height;
        }
        else {
            styles.height = `${props.height}px`;
        }

        return styles;
    });

    /**
     * The angle count after applying sane constraints.
     */
    const constrainedAngleCount = computed((): number => {
        return Math.max(1, Math.min(180, Math.floor(props.angleCount)));
    });

    /**
     * The calculuated angle positions based on the minimum, maximum and
     * number of angles requested.
     */
    const angles = computed((): number[] => {
        const angleCount = Math.min(180, constrainedAngleCount.value);

        if (angleCount <= 1) {
            return [(props.minimumAngle + props.maximumAngle) / 2];
        }
        else if (angleCount === 2) {
            return [props.minimumAngle, props.maximumAngle];
        }
        else {
            const angleSize = (props.maximumAngle - props.minimumAngle) / (angleCount - 1);
            const angleList: number[] = [];

            for (let angle = props.minimumAngle; angle <= props.maximumAngle; angle += angleSize) {
                angleList.push(angle);
            }

            return angleList;
        }
    });

    // #endregion

    // #region Functions

    /**
     * Gets the width of the drawing surface.
     */
    function getWidth(): number {
        return svgElement.value?.clientWidth ?? 0;
    }

    /**
     * Gets the height of the drawing surface.
     */
    function getHeight(): number {
        return svgElement.value?.clientHeight ?? 0;
    }

    /**
     * Creates a scale function that will translate an input value to a scaled
     * output value. This uses a logarithmic calculation.
     *
     * @param outputMin The minimum value of the scaled output.
     * @param outputMax The maximum value of the scaled output.
     * @param inputMin The minimum value of the input.
     * @param inputMax The maximum value of the input.
     */
    function createLogScale(outputMin: number, outputMax: number, inputMin: number, inputMax: number): ((value: number) => number) {
        const logmin = Math.log(inputMin);
        const logmax = Math.log(inputMax);
        const outputRange = outputMax - outputMin;

        return (value: number): number => {
            if (value === inputMin && value === inputMax) {
                return outputMin;
            }

            return (((Math.log(value) - logmin) / (logmax - logmin)) * outputRange) + outputMin;
        };
    }

    /**
     * Creates a consumer in the form of a function that will take the next
     * item in the list of values each time it is called. When the end is
     * reached then it starts over from the beginning.
     *
     * @param values The values to be sequentially consumed.
     * @param seed The starting index to use when creating the consumer.
     */
    function createSequentialConsumer<T>(values: T[], seed: number): (() => T | undefined) {
        const items: T[] = [...values];
        let index = seed % items.length;

        return (): T | undefined => {
            if (items.length === 0) {
                return undefined;
            }

            if (index >= items.length) {
                index = 0;
            }

            return items[index++];
        };
    }

    /**
     * Maps a progress value to the final value to be used during an animation
     * frame.
     *
     * @param startValue The value at the start of the animation.
     * @param endValue The value at the end of the animation.
     * @param progress The progress of the animation, between 0 and 1 inclusive.
     */
    function mapAnimationValue(startValue: number, endValue: number, progress: number): number {
        // Bezier easing.
        let progressCurve = progress * progress * (3.0 - 2.0 * progress);

        return ((endValue - startValue) * progressCurve) + startValue;
    }

    /**
     * Renders the words after they have been placed on a virtual canvas.
     *
     * @param words The words and their position and size information.
     */
    function renderWords(words: cloud.Word[]): void {
        if (!svgElement.value) {
            return;
        }

        // Get or create the node that will represent the graphics surface.
        // It is translated so that 0,0 is the center of the SVG.
        let g = svgElement.value.firstChild as SVGGElement | null;

        if (!g) {
            g = document.createElementNS("http://www.w3.org/2000/svg", "g");
            svgElement.value.appendChild(g);
        }

        g.setAttribute("transform", `translate(${getWidth() / 2}, ${getHeight() / 2})`);

        // Remove all children if we are configured to automatically clear
        // on all updates.
        if (props.autoClear) {
            while (g.firstChild) {
                g.firstChild.remove();
            }
        }

        const colorConsumer = createSequentialConsumer(props.colors, Object.keys(elements).length);

        for (let index = 0; index < words.length; index += 1) {
            const word = words[index];
            if (!word.text) {
                continue;
            }

            let wordElement = elements[word.text];

            // If this is a brand new word then initialize the new element.
            if (!wordElement) {
                const textNode = document.createElementNS("http://www.w3.org/2000/svg", "text");

                // The element will start at center with a size of 0 and then
                // animate to the final position.
                textNode.setAttribute("text-anchor", "middle");
                textNode.style.fontFamily = word.font ?? "inherit";
                textNode.style.fill = colorConsumer() ?? "inherit";
                textNode.style.transform = "translate(0px, 0px) rotate(0deg)";
                textNode.textContent = word.text;
                g.appendChild(textNode);

                wordElement = {
                    element: textNode,
                    previousFontSize: 0,
                    previousRotation: 0,
                    previousX: 0,
                    previousY: 0,
                    fontSize: word.size ?? 0,
                    rotation: word.rotate ?? 0,
                    x: word.x ?? 0,
                    y: word.y ?? 0
                };

                elements[word.text] = wordElement;
            }
            else {
                // An existing word element was found, update it's new size and
                // position for animation.
                wordElement.fontSize = word.size ?? 0;
                wordElement.rotation = word.rotate ?? 0;
                wordElement.x = word.x ?? 0;
                wordElement.y = word.y ?? 0;
            }
        }

        // Find any old elements that need to be removed.
        const existingWords = words.filter(w => w.text).map(w => w.text);
        for (const wordKey of Object.keys(elements)) {
            if (!existingWords.includes(wordKey)) {
                elements[wordKey].fontSize = 0;
                elements[wordKey].x = 0;
                elements[wordKey].y = 0;
                elements[wordKey].rotation = 0;
            }
        }

        animationFrame = requestAnimationFrame(animateWordsFrame);
    }

    /**
     * Performs the calculations required for a single animation frame. The new
     * position and sizes are determined and then the elements updated to
     * reflect those new values.
     *
     * @param time The high-resolution time when this animation frame began.
     */
    function animateWordsFrame(time: number): void {
        // If this is the first frame then initialize as such.
        if (animationStartTime === null) {
            animationStartTime = time;
        }

        // Determine the progress in the animation sequence.
        const duration = Math.min(time - animationStartTime, props.animationDuration);
        const progress = duration / props.animationDuration;
        const isLastFrame = duration === props.animationDuration;

        for (const word of Object.keys(elements)) {
            const wordElement = elements[word];

            // Map the values for what they should be at this frame.
            const fontSize = mapAnimationValue(wordElement.previousFontSize, wordElement.fontSize, progress);
            const x = mapAnimationValue(wordElement.previousX, wordElement.x, progress);
            const y = mapAnimationValue(wordElement.previousY, wordElement.y, progress);
            const rotation = mapAnimationValue(wordElement.previousRotation, wordElement.rotation, progress);

            // Update the element to be positioned and sized correctly.
            wordElement.element.style.fontSize = `${fontSize}px`;
            wordElement.element.style.transform = `translate(${x}px, ${y}px) rotate(${rotation}deg)`;

            // If this is the last frame then remove any elements that have been
            // sized away, otherwise update the previous coordinates to the new
            // coordinates.
            if (isLastFrame) {
                if (wordElement.fontSize === 0) {
                    wordElement.element.remove();
                    delete (elements[word]);
                }
                else {
                    wordElement.previousFontSize = wordElement.fontSize;
                    wordElement.previousX = wordElement.x;
                    wordElement.previousY = wordElement.y;
                    wordElement.previousRotation = wordElement.rotation;
                }
            }
        }

        if (!isLastFrame) {
            animationFrame = requestAnimationFrame(animateWordsFrame);
        }
        else {
            animationStartTime = null;
            animationFrame = null;
            emit("endDraw");

            // If an update was requested while we were animating, start it now.
            if (isUpdateRequested) {
                processWords();
            }
        }
    }

    /**
     * Generates the word cloud based on all current configuration.
     */
    function processWords(): void {
        isUpdateRequested = false;

        if (!svgElement.value) {
            return;
        }

        emit("beginDraw");

        if (props.words.length === 0) {
            return renderWords([]);
        }

        const wordLookup: Record<string, number> = {};

        // Count how many times each word appears.
        for (const word of props.words) {
            wordLookup[word] = (wordLookup[word] ?? 0) + 1;
        }

        // Translate that into a cloud word object and then sort
        // it ascending.
        const words: cloud.Word[] = Object.entries(wordLookup)
            .map(e => ({ text: e[0], size: e[1] }))
            .sort((a, b) => a.size - b.size);

        // Get the min/max word counts and create the scale.
        const min = Math.min(...words.map(w => w.size ?? 0));
        const max = Math.max(...words.map(w => w.size ?? 0));
        const fontSizeScale = createLogScale(props.minimumFontSize, props.maximumFontSize, min, max);

        cloud()
            .size([getWidth(), getHeight()])
            .timeInterval(25)
            .spiral("archimedean")
            .words(words)
            .padding(5)
            .rotate(() => angles.value[Math.floor(Math.random() * angles.value.length)])
            .font(props.fontName)
            .fontSize(d => Math.floor(fontSizeScale(d.size ?? 0)))
            .on("end", renderWords)
            .start();
    }

    /**
     * Request an update to the SVG. If we are currently animating then just
     * queue up a new update request.
     */
    function requestUpdate(): void {
        if (animationFrame === null) {
            processWords();
        }
        else {
            isUpdateRequested = true;
        }
    }

    // #endregion

    onMounted(() => {
        if (!svgElement.value) {
            return;
        }

        // Watch for changes to the size of the SVG and re-render.
        if (window.ResizeObserver) {
            resizeObserver = new ResizeObserver(() => {
                requestUpdate();
            });

            resizeObserver.observe(svgElement.value);
        }
        else {
            // The resize observer always calls the callback when initialized,
            // so we only need to manually request the update if we don't
            // have resize observer available.
            requestUpdate();
        }
    });

    // #endregion

    onBeforeUnmount(() => {
        resizeObserver?.disconnect();
    });

    // Watch for changes to any of these values and re-render.
    const requestUpdateValues = [
        () => props.words,
        () => props.angleCount,
        () => props.minimumAngle,
        () => props.maximumAngle,
        () => props.fontName,
        () => props.minimumFontSize,
        () => props.maximumFontSize,
        () => props.colors,
        () => props.wordPadding
    ];

    watch(requestUpdateValues, () => {
        console.log("changed");
        requestUpdate();
    });
</script>
