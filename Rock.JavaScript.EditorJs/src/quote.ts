import EditorQuote, { QuoteData } from "@editorjs/quote"

/**
 * Custom implementation of the Quote block to deny line breaks.
 */
export class Quote extends EditorQuote {
    /**
     * Do not allow to press Enter inside the Quote. They are meant to be single
     * paragraphs. Default implementation had weird rendering bugs if this was
     * enabled.
     *
     * @returns true if line breaks should be allowed, false otherwise.
     */
    static get enableLineBreaks() {
        return false;
    }

    /**
     * Checks if the block's save data should be included in the structured
     * content stream.
     *
     * @param savedData The data that was returned by the save method.
     * @returns true if the block's contents should be included in the save data.
     */
    public validate(savedData: QuoteData) {
        return savedData.text !== "";
    }
}
