import { newGuid } from "@Obsidian/Utility/guid";
import { createCommentVNode, createTextVNode, createVNode, defineComponent, PropType, VNode, VNodeArrayChildren } from "vue";

export default defineComponent({
    props: {
        /**
         * The HTML content that will be rendered before the slot content.
         */
        pre: {
            type: String as PropType<string | null>,
            required: false
        },

        /**
         * The HTML content that will be rendered after the slot content.
         */
        post: {
            type: String as PropType<string | null>,
            required: false
        }
    },

    setup(props, ctx) {
        function getAttributes(node: Element): Record<string, string> {
            const attrs: Record<string, string> = {};
            for (const attr of node.attributes) {
                attrs[attr.name] = attr.value;
            } return attrs;
        }

        function htmlToNodes(nodes: ChildNode[]): VNode[] {
            return nodes.map(element => {
                if (element.nodeType === Node.TEXT_NODE) {
                    return createTextVNode(element.nodeValue ?? undefined);
                }
                else if (element.nodeType === Node.COMMENT_NODE) {
                    return createCommentVNode(element.nodeValue ?? undefined);
                }
                else if (element.nodeType === Node.ELEMENT_NODE && element instanceof Element) {
                    if (element.childNodes.length) {
                        return createVNode(element.tagName, getAttributes(element), htmlToNodes(Array.from(element.childNodes)));
                    }

                    return createVNode(element.tagName, getAttributes(element), element.innerHTML);
                }

                // This will be removed by the filter below.
                return undefined as unknown as VNode;
            })
                .filter(v => v !== undefined);
        }

        function replaceMagicVNode(nodes: VNodeArrayChildren, id: string, contentNodes: VNode[]): boolean {
            for (let i = 0; i < nodes.length; i++) {
                const node = nodes[i];

                if (!node) {
                    continue;
                }

                if (Array.isArray(node)) {
                    if (replaceMagicVNode(node, id, contentNodes)) {
                        return true;
                    }
                }
                else if (typeof node === "object") {
                    if (node.props?.id === id) {
                        nodes.splice(i, 1, ...contentNodes);
                        return true;
                    }

                    if (node.children && Array.isArray(node.children)) {
                        if (replaceMagicVNode(node.children, id, contentNodes)) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        return () => {
            if (!props.pre && !props.post) {
                return ctx.slots.default?.();
            }

            const element = document.createElement("div");
            const magic = `magic-${newGuid()}`;

            element.innerHTML = `${props.pre ?? ""}<div id="${magic}"></div>${props.post ?? ""}`;

            const vnodes = htmlToNodes(Array.from(element.childNodes));
            const content = ctx.slots.default?.() ?? [];

            if (replaceMagicVNode(vnodes, magic, content)) {
                return vnodes;
            }
            else {
                return content;
            }
        };
    }
});
