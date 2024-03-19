import { Comment, ComponentInternalInstance, ComponentOptions, SetupContext, VNode, getCurrentInstance, getTransitionRawChildren } from "vue";

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

/** Current animation state, 0 = idle, 1 = pending, 2 = animating. */
let animationState: 0 | 1 | 2 = 0;

/** Components that need to be force updated once the animation starts. */
const pendingInstances: ComponentInternalInstance[] = [];

/**
 * Handles native view transition API transitions.
 */
const NativeViewTransition: ComponentOptions = {
    name: "NativeViewTransition",

    props: {},

    setup(_props: Record<string, unknown>, { slots }: SetupContext) {
        const instance = getCurrentInstance();

        return () => {
            const children = slots.default && getTransitionRawChildren(slots.default(), true);
            const child = getFirstValidChild(children);

            if (child) {
                // If we don't currently have a child or the two vnodes are the
                // same type then just display the child. For example, on initial
                // render.
                if (!instance?.subTree || isSameVNode(instance.subTree, child)) {
                    return child;
                }
            }
            else {
                // If we don't have a new child and the old child didn't
                // exist or was a comment, then we don't need to animate.
                if (!instance?.subTree || instance.subTree.type === Comment) {
                    return child;
                }
            }

            // If we don't support transitions then just return the child.
            // @ts-expect-error startViewTransition is not standard yet.
            if (!document.startViewTransition) {
                return child;
            }

            if (animationState === 2) {
                // If we are currently in an animation, just set the child.
                return child;
            }
            else if (animationState === 1) {
                // If we are waiting for the animation to start then make us
                // pending and return the current content.

                if (instance) {
                    pendingInstances.push(instance);
                }

                return instance?.subTree;
            }
            else {
                // Note that we are starting to animate.
                animationState = 1;

                // @ts-expect-error startViewTransition is not standard yet.
                document.startViewTransition(async () => {
                    // Note that we are ready to accept new content.
                    animationState = 2;

                    // Force new content to be placed in DOM.
                    instance?.update();
                    for (const inst of pendingInstances) {
                        inst.update();
                    }

                    pendingInstances.splice(0, pendingInstances.length);

                    // Note that we are no longer in an animation.
                    animationState = 0;
                });

                return instance?.subTree;
            }
        };
    }
};

export default NativeViewTransition;
