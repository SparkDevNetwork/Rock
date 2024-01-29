/* eslint-disable @typescript-eslint/no-explicit-any */
import "@Obsidian/FieldTypes/index";
import { HttpBodyData, HttpResult, } from "@Obsidian/Types/Utility/http";
import { InvokeBlockActionFunc } from "@Obsidian/Types/Utility/block";
import { DefineComponent, ref } from "vue";
import { ComponentMountingOptions, VueWrapper, mount } from "@vue/test-utils";
import type { ComponentExposed, ComponentProps } from "vue-component-type-helpers";

type ComponentData<T> = T extends {
    data?(...args: any): infer D;
    // eslint-disable-next-line @typescript-eslint/ban-types
} ? D : {};

/**
 * Creates a mock-instance of InvokeBlockActionFunc that intercepts block actions
 * and instead calls the specified function to handle the request.
 *
 * @param actions The action names and the functions to call when they are triggered.
 *
 * @returns An instance of InvokeBlockActionFunc.
 */
export function mockBlockActions(actions: Record<string, (data: any) => HttpResult<any>> = {}): InvokeBlockActionFunc {
    return <T>(actionName: string, data: HttpBodyData = undefined): Promise<HttpResult<T>> => {
        if (actionName in actions) {
            return Promise.resolve(actions[actionName](data));
        }

        throw new Error("Mocked block action is not implemented.");
    };
}

/**
 * Mounts a block by performing any initialization required to make
 * it think it really exists on a page.
 *
 * @param originalComponent The component to be mounted.
 * @param configurationValues The configuration values for the block.
 * @param blockActions The block action interceptor.
 * @param options The custom mounting options to use.
 *
 * @returns An wrapper around the component instance.
 */
export function mountBlock<T, C = T extends ((...args: any) => any) | (new (...args: any) => any) ? T : T extends {
    props?: infer Props;
} ? DefineComponent<Props extends Readonly<(infer PropNames)[]> | (infer PropNames)[] ? {
    [key in PropNames extends string ? PropNames : string]?: any;
} : Props> : DefineComponent>(originalComponent: T, configurationValues: Record<string, unknown> | null, blockActions?: InvokeBlockActionFunc, options?: ComponentMountingOptions<C>): VueWrapper<ComponentExposed<C> & ComponentProps<C> & ComponentData<C>> {
    if (!options) {
        options = {};
    }

    if (!options.global) {
        options.global = {};
    }

    if (!options.global.provide) {
        options.global.provide = {};
    }

    // Attach the component to a real node so that form submit works.
    if (!options.attachTo) {
        document.body.innerHTML = `<div id="root"></div>`;

        options.attachTo = "#root";
    }

    options.global.provide["configurationValues"] = ref(configurationValues);
    options.global.provide["invokeBlockAction"] = blockActions ?? mockBlockActions();

    return mount(originalComponent, options);
}
