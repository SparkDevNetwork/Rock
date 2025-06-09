declare module "editorjs-alert" {
    import { BlockToolConstructorOptions } from "@editorjs/editorjs";

    export default class Alert {
        constructor(editor: BlockToolConstructorOptions<any, any>);
    }
}
