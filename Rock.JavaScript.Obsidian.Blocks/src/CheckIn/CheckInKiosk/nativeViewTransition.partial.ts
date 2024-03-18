import { Comment, ComponentOptions, SetupContext, VNode, getCurrentInstance, getTransitionRawChildren } from "vue";

/**
 * Checks if the two nodes are considered the same node.
 *
 * @param n1 The first node.
 * @param n2 The second node.
 *
 * @returns true if both nodes are the same.
 */
function isSameVNode(n1: VNode, n2: VNode): boolean {
    return n1.type === n2.type && n1.key === n2.key;
}

/**
 * Gets the first valid child from the list of nodes. If no valid children
 * exist then the first child is returned.
 *
 * @param nodes The nodes to iterate over.
 *
 * @returns The first node that is not a comment, or the first node if they were all comments.
 */
function getFirstValidChild(nodes?: VNode[]): VNode | undefined {
    if (!nodes) {
        return undefined;
    }

    for (const node of nodes) {
        if (node.type !== Comment) {
            return node;
        }
    }

    return nodes.length > 0 ? nodes[0] : undefined;
}

/**
 * Handles native view transition API transitions.
 */
const NativeViewTransition: ComponentOptions = {
    name: "NativeViewTransition",

    props: {},

    setup(props: Record<string, unknown>, { slots }: SetupContext) {
        const instance = getCurrentInstance();
        let isAnimating = false;

        return () => {
            const children = slots.default && getTransitionRawChildren(slots.default(), true);

            const child = getFirstValidChild(children);

            if (child) {
                // If we don't currently have a child or the two vnodes are the
                // same type then just display the child. For example, on initial
                // render.
                if (!instance?.subTree || isSameVNode(instance.subTree, child)) {
                    console.log("subtree", instance?.subTree);
                    return child;
                }
            }
            else {
                // If we don't have a new child and the old child didn't
                // exist or was a comment, then we don't need to animate.
                if (instance?.subTree && instance.subTree.type === Comment) {
                    return child;
                }
            }

            // If we don't support transitions then just return the child.
            // @ts-expect-error startViewTransition is not standard yet.
            if (!document.startViewTransition) {
                return child;
            }

            // If we are currently in an animation, just set the child.
            if (isAnimating) {
                return child;
            }

            // Start a new animation.
            isAnimating = true;

            // @ts-expect-error startViewTransition is not standard yet.
            document.startViewTransition(async () => {
                instance?.update();
                isAnimating = false;
            });

            return instance?.subTree;
        };
    }
};

export default NativeViewTransition;
