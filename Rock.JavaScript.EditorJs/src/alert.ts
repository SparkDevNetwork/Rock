import { BlockToolConstructorOptions } from "@editorjs/editorjs";
import EditorAlert from "editorjs-alert";

export class Alert extends EditorAlert {
    constructor(config: BlockToolConstructorOptions<any, any>) {
        super(config);
    }

    get CSS() {
        return {
            wrapper: 'alert',
            wrapperForType: (type: string) => `alert-${type}`,
            wrapperForAlignType: (alignType: string) => `text-${alignType}`,
            message: 'cdx-alert__message',
        };
    }
}
