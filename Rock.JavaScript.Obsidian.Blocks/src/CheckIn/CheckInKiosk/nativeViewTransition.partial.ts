import { ComponentInternalInstance, ComponentOptions, SetupContext, VNode, getCurrentInstance, getTransitionRawChildren } from "vue";

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
 * Determines if the two arrays represent the same child vnodes.
 *
 * @param n1 The first array of child vnodes.
 * @param n2 The second array of child vnodes.
 *
 * @returns true if the two arrays have the same children; otherwise false.
 */
function areChildrenSame(n1: VNode[] | undefined | null, n2: VNode[] | undefined | null): boolean {
    if (!n1) {
        return !n2;
    }

    if (!n2) {
        return !n1;
    }

    if (n1.length !== n2.length) {
        return false;
    }

    for (let i = 0; i < n1.length; i++) {
        if (!isSameVNode(n1[i], n2[i])) {
            return false;
        }
    }

    return true;
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

            if (areChildrenSame(children, instance?.subTree?.children as VNode[])) {
                return children;
            }

            // If we don't support transitions then just return the child.
            // @ts-expect-error startViewTransition is not standard yet.
            if (!document.startViewTransition) {
                return children;
            }

            if (animationState === 2) {
                // If we are currently in an animation, just set the child.
                return children;
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
