// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import {
    ref,
    Ref,
    watch
} from "vue";
import {
    BackgroundSize,
    BackgroundFit,
    ButtonWidth,
    BorderStyle,
    CssStyleDeclarationKebabKey,
    ProviderAlreadyExistsError,
    ProviderNotCreatedError,
    ShorthandStyleValueProviderHooks,
    ShorthandValueProvider,
    StyleSheetMode,
    StyleValueProviderHooks,
    ValueConverter,
    ValueProvider,
    ValueProviderHooks,
    WeakPair,
    ButtonWidthValues
} from "./types.partial";
import {
    addRuleset,
    borderStyleConverter,
    createDomWatcher,
    createElements,
    DefaultBodyAlignment,
    DefaultBodyColor,
    DefaultBodyWidth,
    DefaultEmailBackgroundColor,
    findElements,
    GlobalStylesCssSelectors,
    numberToStringConverter,
    percentageConverter,
    pixelConverter,
    RockStylesCssClass,
    stringConverter,
    updateStyleElementTextContent
} from "./utils.partial";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { Enumerable } from "@Obsidian/Utility/linq";
import { isNotNullish, isNullish } from "@Obsidian/Utility/util";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export function inlineStyleProvider<T>(
    element: HTMLElement,
    property: CssStyleDeclarationKebabKey,
    converter: ValueConverter<T, string | null>,
    copyToElements?: HTMLElement[] | null | undefined,
    hooks?: StyleValueProviderHooks<T, string | null> | null | undefined
): ValueProvider<T> {
    const initialTargetValue = element.style.getPropertyValue(property);

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T>(
        converter.toSource(initialTargetValue) as T
    );
    hooks?.onSourceValueUpdated?.(value.value as T);

    const targetElements = [element, ...(copyToElements ?? [])];

    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        const targetValue = converter.toTarget(newValue as T);

        if (targetValue != null) {
            targetElements.forEach(element => {
                element.style.setProperty(property, targetValue);
                hooks?.onStyleUpdated?.(element.style, targetValue);
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.removeProperty(property);
                hooks?.onStyleUpdated?.(element.style, targetValue);
            });
        }

        hooks?.onTargetValueUpdated?.(targetValue);
    });

    return {
        get value() {
            return value.value as T;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose: () => {
            watcher();
        }
    };
}

export function shorthandInlineStyleProvider<T>(
    element: HTMLElement,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: Record<"top" | "right" | "bottom" | "left", CssStyleDeclarationKebabKey>,
    converter: ValueConverter<T, string | null>,
    copyToElements?: HTMLElement[] | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<T, string | null> | null | undefined
): ShorthandValueProvider<T> {
    let isDisposed: boolean = false;
    const targetElements = [element, ...(copyToElements ?? [])];
    const initialTargetValue = {
        shorthand: element.style.getPropertyValue(shorthandProperty),
        top: element.style.getPropertyValue(longhandProperties.top),
        bottom: element.style.getPropertyValue(longhandProperties.bottom),
        left: element.style.getPropertyValue(longhandProperties.left),
        right: element.style.getPropertyValue(longhandProperties.right)
    };

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const shorthandValue = ref<T>(
        converter.toSource(initialTargetValue.shorthand) as T
    );
    const topValue = ref<T>(
        converter.toSource(initialTargetValue.top) as T
    );
    const rightValue = ref<T>(
        converter.toSource(initialTargetValue.right) as T
    );
    const bottomValue = ref<T>(
        converter.toSource(initialTargetValue.bottom) as T
    );
    const leftValue = ref<T>(
        converter.toSource(initialTargetValue.left) as T
    );

    hooks?.onSourceValueUpdated?.({
        shorthand: shorthandValue.value as T,
        top: topValue.value as T,
        bottom: bottomValue.value as T,
        left: leftValue.value as T,
        right: rightValue.value as T,
    });

    const topRightBottomLeftWatcher = watch(
        [topValue, rightValue, bottomValue, leftValue],
        ([newTop, newRight, newBottom, newLeft]) => {
            const targetTop = converter.toTarget(newTop as T);
            const targetBottom = converter.toTarget(newBottom as T);
            const targetLeft = converter.toTarget(newLeft as T);
            const targetRight = converter.toTarget(newRight as T);

            if ([targetTop, targetBottom, targetLeft, targetRight].every((val) => val === null)) {
                targetElements.forEach(element => {
                    element.style.removeProperty(shorthandProperty);

                    hooks?.onStyleUpdated?.(element.style, {
                        shorthand: null,
                        top: null,
                        bottom: null,
                        left: null,
                        right: null,
                    });
                });

                hooks?.onTargetValueUpdated?.({
                    shorthand: null,
                    top: null,
                    bottom: null,
                    left: null,
                    right: null,
                });
            }
        });

    const topWatcher = watch(topValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            top: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.top);

                hooks?.onStyleUpdated?.(element.style, {
                    top: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.top, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    top: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            top: targetValue
        });
    });

    const bottomWatcher = watch(bottomValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            bottom: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.bottom);

                hooks?.onStyleUpdated?.(element.style, {
                    bottom: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.bottom, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    bottom: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            bottom: targetValue
        });
    });

    const leftWatcher = watch(leftValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            left: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.left);

                hooks?.onStyleUpdated?.(element.style, {
                    left: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.left, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    left: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            left: targetValue
        });
    });

    const rightWatcher = watch(rightValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            right: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(element.style, {
                    right: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.right, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    right: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            right: targetValue
        });
    });

    const shorthandWatcher = watch(shorthandValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            shorthand: newValue as T,
        });

        const targetValue = converter.toTarget(newValue as T);

        if (targetValue === null) {
            const sourceValue = converter.toSource(targetValue);
            (topValue as Ref<T>).value = sourceValue;
            (rightValue as Ref<T>).value = sourceValue;
            (bottomValue as Ref<T>).value = sourceValue;
            (leftValue as Ref<T>).value = sourceValue;

            targetElements.forEach(element => {
                // Remove all properties when the shorthand property is cleared.
                element.style.removeProperty(shorthandProperty);
                element.style.removeProperty(longhandProperties.top);
                element.style.removeProperty(longhandProperties.bottom);
                element.style.removeProperty(longhandProperties.left);
                element.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(element.style, {
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue
                });

                hooks?.onTargetValueUpdated?.({
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue,
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(shorthandProperty, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    shorthand: targetValue
                });
            });

            hooks?.onTargetValueUpdated?.({
                shorthand: targetValue
            });
        }
    });

    return {
        get shorthandValue() {
            return shorthandValue.value as T;
        },
        set shorthandValue(newValue: T) {
            (shorthandValue as Ref<T>).value = newValue;
        },
        get topValue() {
            return topValue.value as T;
        },
        set topValue(newValue: T) {
            (topValue as Ref<T>).value = newValue;
        },
        get rightValue() {
            return rightValue.value as T;
        },
        set rightValue(newValue: T) {
            (rightValue as Ref<T>).value = newValue;
        },
        get bottomValue() {
            return bottomValue.value as T;
        },
        set bottomValue(newValue: T) {
            (bottomValue as Ref<T>).value = newValue;
        },
        get leftValue() {
            return leftValue.value as T;
        },
        set leftValue(newValue: T) {
            (leftValue as Ref<T>).value = newValue;
        },
        get isDisposed(): boolean {
            return isDisposed;
        },
        dispose: () => {
            topRightBottomLeftWatcher();
            topWatcher();
            bottomWatcher();
            leftWatcher();
            rightWatcher();
            shorthandWatcher();
            isDisposed = true;
        },
    };
}

/**
 * Creates a ValueProvider for a stylesheet pixel value.
 *
 * @param element The element that owns the stylesheet.
 * @param styleCssClass The CSS class of the stylesheet.
 * @param rulesetCssSelector The CSS selectors of the ruleset.
 * @param property The CSS property to manage.
 * @returns
 */
export function styleSheetProvider<T>(
    element: Element,
    styleCssClass: string,
    rulesetCssSelector: string,
    property: CssStyleDeclarationKebabKey,
    converter: ValueConverter<T, string | null>,
    hooks?: StyleValueProviderHooks<T, string | null> | null | undefined
): ValueProvider<T> {
    const initialElements = findElements(element, styleCssClass, rulesetCssSelector);
    const initialTargetValue = initialElements?.ruleset?.style.getPropertyValue(property) ?? null;

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T>(
        converter.toSource(initialTargetValue) as T
    );

    hooks?.onSourceValueUpdated?.(value.value as T);

    // Update the stylesheet whenever the value changes.
    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(property);
            }
            else {
                ruleset.style.setProperty(property, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, targetValue);
            hooks?.onTargetValueUpdated?.(targetValue);

            updateStyleElementTextContent(elements);
        }
    });

    return {
        get value() {
            return value.value as T;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose: () => {
            watcher();
        }
    };
}

/**
 * Creates a ShorthandValueProvider for a stylesheet pixel value.
 *
 * @param element The element that owns the stylesheet.
 * @param styleCssClass The CSS class of the stylesheet.
 * @param rulesetCssSelector The CSS selectors of the ruleset.
 * @param shorthandProperty The shorthand property to manage.
 * @param longhandProperties The longhand properties that make up the shorthand property.
 * @param converter The converter to use for converting values to and from CSS properties.
 * @returns A ShorthandValueProvider for managing the shorthand and longhand properties.
 */
export function shorthandStyleSheetProvider<T extends number | string | boolean | null | undefined>(
    element: Element,
    styleCssClass: string,
    rulesetCssSelector: string,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: Record<"top" | "right" | "bottom" | "left", CssStyleDeclarationKebabKey>,
    converter: ValueConverter<T, string | null>,
    hooks?: ShorthandStyleValueProviderHooks<T, string | null> | null | undefined
): ShorthandValueProvider<T> {
    let isDisposed: boolean = false;
    const initialElements = findElements(element, styleCssClass, rulesetCssSelector);
    const initialTargetValues = {
        shorthand: initialElements?.ruleset?.style.getPropertyValue(shorthandProperty) ?? null,
        top: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.top) ?? null,
        bottom: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.bottom) ?? null,
        right: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.right) ?? null,
        left: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.left) ?? null
    };

    hooks?.onTargetValueUpdated?.(initialTargetValues);

    const shorthandValue = ref<T>(
        converter.toSource(initialTargetValues.shorthand)
    );
    const topValue = ref<T>(
        converter.toSource(initialTargetValues.top)
    );
    const rightValue = ref<T>(
        converter.toSource(initialTargetValues.right)
    );
    const bottomValue = ref<T>(
        converter.toSource(initialTargetValues.bottom)
    );
    const leftValue = ref<T>(
        converter.toSource(initialTargetValues.left)
    );

    hooks?.onSourceValueUpdated?.({
        shorthand: shorthandValue.value as T,
        top: topValue.value as T,
        bottom: bottomValue.value as T,
        left: leftValue.value as T,
        right: rightValue.value as T,
    });

    const topRightBottomLeftWatcher = watch(
        [topValue, rightValue, bottomValue, leftValue],
        ([newTop, newRight, newBottom, newLeft]) => {
            if ([newTop, newRight, newBottom, newLeft].every((val) => isNullish(val))) {
                const elements = findElements(element, styleCssClass, rulesetCssSelector);

                if (elements?.ruleset) {
                    const { ruleset } = elements;

                    ruleset.style.removeProperty(shorthandProperty);

                    hooks?.onStyleUpdated?.(ruleset.style, {
                        shorthand: null
                    });

                    hooks?.onTargetValueUpdated?.({
                        shorthand: null
                    });

                    updateStyleElementTextContent(elements);
                }
            }
        });

    const topWatcher = watch(topValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            top: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);

            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.top);
            }
            else {
                ruleset.style.setProperty(longhandProperties.top, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                top: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                top: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const bottomWatcher = watch(bottomValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            bottom: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.bottom);
            }
            else {
                ruleset.style.setProperty(longhandProperties.bottom, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                bottom: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                bottom: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const leftWatcher = watch(leftValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            left: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.left);
            }
            else {
                ruleset.style.setProperty(longhandProperties.left, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                left: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                left: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const rightWatcher = watch(rightValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            right: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.right);
            }
            else {
                ruleset.style.setProperty(longhandProperties.right, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                right: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                right: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const shorthandWatcher = watch(shorthandValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            shorthand: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                (topValue as Ref<T>).value = null as T;
                (rightValue as Ref<T>).value = null as T;
                (bottomValue as Ref<T>).value = null as T;
                (leftValue as Ref<T>).value = null as T;

                // Remove all properties when the shorthand property is cleared.
                ruleset.style.removeProperty(shorthandProperty);
                ruleset.style.removeProperty(longhandProperties.top);
                ruleset.style.removeProperty(longhandProperties.bottom);
                ruleset.style.removeProperty(longhandProperties.left);
                ruleset.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(ruleset.style, {
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue
                });
            }
            else {
                ruleset.style.setProperty(shorthandProperty, targetValue);

                hooks?.onStyleUpdated?.(ruleset.style, {
                    shorthand: targetValue
                });
            }

            hooks?.onTargetValueUpdated?.({
                shorthand: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    return {
        get shorthandValue() {
            return shorthandValue.value as T;
        },
        set shorthandValue(newValue: T) {
            (shorthandValue as Ref<T>).value = newValue;
        },
        get topValue() {
            return topValue.value as T;
        },
        set topValue(newValue: T) {
            (topValue as Ref<T>).value = newValue;
        },
        get rightValue() {
            return rightValue.value as T;
        },
        set rightValue(newValue: T) {
            (rightValue as Ref<T>).value = newValue;
        },
        get bottomValue() {
            return bottomValue.value as T;
        },
        set bottomValue(newValue: T) {
            (bottomValue as Ref<T>).value = newValue;
        },
        get leftValue() {
            return leftValue.value as T;
        },
        set leftValue(newValue: T) {
            (leftValue as Ref<T>).value = newValue;
        },
        get isDisposed(): boolean {
            return isDisposed;
        },
        dispose: () => {
            topRightBottomLeftWatcher();
            topWatcher();
            bottomWatcher();
            leftWatcher();
            rightWatcher();
            shorthandWatcher();
            isDisposed = true;
        },
    };
}

export function attributeProvider<T>(
    element: Element,
    attribute: string,
    converter: ValueConverter<T, string | null>,
    copyToElements?: Element[],
    hooks?: ValueProviderHooks<T, string | null>
): ValueProvider<T> {
    const targetElements = [element, ...(copyToElements ?? [])];

    const initialTargetValue = element.getAttribute(attribute);

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T | null | undefined>(
        converter.toSource(initialTargetValue)
    );

    hooks?.onSourceValueUpdated?.(value.value as T);

    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.removeAttribute(attribute);
            });
        }
        else {
            targetElements.forEach(element => {
                element.setAttribute(attribute, targetValue);
            });
        }

        hooks?.onTargetValueUpdated?.(targetValue);
    });

    return {
        get value(): T {
            return (value as Ref<T>).value;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose() {
            watcher();
        }
    };
}

export function createBorderStyleProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<BorderStyle | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<string | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-style",
            {
                top: "border-top-style",
                bottom: "border-bottom-style",
                right: "border-right-style",
                left: "border-left-style"
            },
            borderStyleConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-style",
            {
                top: "border-top-style",
                bottom: "border-bottom-style",
                right: "border-right-style",
                left: "border-left-style"
            },
            borderStyleConverter,
            hooks
        );
    }
}

export function createBorderWidthProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<number | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<number | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-width",
            {
                top: "border-top-width",
                bottom: "border-bottom-width",
                right: "border-right-width",
                left: "border-left-width"
            },
            pixelConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-width",
            {
                top: "border-top-width",
                bottom: "border-bottom-width",
                right: "border-right-width",
                left: "border-left-width"
            },
            pixelConverter,
            hooks
        );
    }
}

export function createBorderColorProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<string | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<string | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-color",
            {
                top: "border-top-color",
                bottom: "border-bottom-color",
                right: "border-right-color",
                left: "border-left-color"
            },
            borderStyleConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-color",
            {
                top: "border-top-color",
                bottom: "border-bottom-color",
                right: "border-right-color",
                left: "border-left-color"
            },
            borderStyleConverter,
            hooks
        );
    }
}

export function createBackgroundImageProvider(
    element: HTMLElement,
    copyToElements: HTMLElement[] | null | undefined
): ValueProvider<ListItemBag | null | undefined> {
    const targetElements = [element, ...(copyToElements ?? [])];
    const value = ref<ListItemBag | null | undefined>(getValue(element));

    function getValue(element: HTMLElement): ListItemBag | null | undefined {
        const backgroundImageGuid = toGuidOrNull(element.dataset["backgroundImageGuid"]);

        if (backgroundImageGuid) {
            return {
                value: backgroundImageGuid,
                text: element.dataset["backgroundImageFileName"]
            };
        }
        else {
            return null;
        }
    }

    const watcher = watch(value, (newValue) => {
        const fileGuid = toGuidOrNull(newValue?.value);

        if (fileGuid) {
            const fileName = newValue?.text;
            const backgroundImageValue = `url('/GetImage.ashx?guid=${fileGuid}')`;

            targetElements.forEach(element => {
                element.style.backgroundImage = backgroundImageValue;
            });

            // These hold data that cannot be easily retrieved from style properties.
            element.dataset["backgroundImageGuid"] = fileGuid ?? "";
            element.dataset["backgroundImageFileName"] = fileName ?? "";
        }
        else {
            targetElements.forEach(element => {
                element.style.backgroundImage = "";
            });

            delete element.dataset["backgroundImageGuid"];
            delete element.dataset["backgroundImageFileName"];
        }
    });

    const provider = {
        get value(): ListItemBag | null | undefined {
            return value.value;
        },
        set value(newValue: ListItemBag | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createBackgroundSizeProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined
): ValueProvider<BackgroundSize | null | undefined> {
    type CSSStyleDeclarationBackground = {
        backgroundColor: string;
        backgroundImage: string;
        backgroundPosition: string;
        backgroundSize: string;
        backgroundRepeat: string;
        backgroundAttachment: string;
        backgroundClip: string;
        backgroundOrigin: string;
    };

    const backgroundSizeFitWidth = "100% auto" as const;
    const backgroundSizeFitHeight = "auto 100%" as const;
    const backgroundSizeOriginal = "auto" as const;

    const targetElements = [element, ...(copyToElements ?? [])];

    // Chromium browsers have an issue with `background-size: <percentage> auto;`
    // when set with `element.style.backgroundSize = "100% auto;"` and `element.style.setProperty("background-size", "100% auto;").
    // Only the "100%" value is stored...
    // We will work around this by updating the entire `style` attribute instead of
    // the individual style properties.

    if (!getCurrentElementValue()) {
        // Default to "fit-width".
        setBackgroundSize(backgroundSizeFitWidth);
    }

    const value = ref<BackgroundSize | null | undefined>(getCurrentElementValue());

    function parseBackgroundShorthand(backgroundString: string): CSSStyleDeclarationBackground[] {
        // Define default values for each sub-property
        const defaultValues: CSSStyleDeclarationBackground = {
            backgroundColor: "",
            backgroundImage: "",
            backgroundPosition: "",
            backgroundSize: "",
            backgroundRepeat: "",
            backgroundAttachment: "",
            backgroundClip: "",
            backgroundOrigin: ""
        };

        // Split the layers by commas
        const layers = backgroundString.split(/\s*,\s*/);

        // Parse each layer
        const parsedLayers = layers.map(layer => parseLayer(layer.trim(), defaultValues));

        return parsedLayers;
    }

    function parseLayer(layerString: string, defaultValues: CSSStyleDeclarationBackground): CSSStyleDeclarationBackground {
        const result: CSSStyleDeclarationBackground = {
            ...defaultValues
        };

        let backgroundClipHandled: boolean = false;

        // Split the layer into tokens
        const tokens = layerString.split(/\s+/);
        let positionAndSizeHandled = false;

        for (let i = 0; i < tokens.length; i++) {
            const token = tokens[i];

            if (isColor(token)) {
                result.backgroundColor = token;
            }
            else if (isUrl(token) || isGradient(token) || token === "none") {
                result.backgroundImage = token;
            }
            else if (isRepeatValue(token)) {
                result.backgroundRepeat = token;
            }
            else if (isAttachmentValue(token)) {
                result.backgroundAttachment = token;
            }
            else if (isBoxValue(token)) {
                if (!backgroundClipHandled) {
                    result.backgroundClip = token;
                    // Default to same value
                    result.backgroundOrigin = token;
                    backgroundClipHandled = true;
                }
                else {
                    result.backgroundOrigin = token;
                }
            }
            else if (token.includes("/")) {
                const [position, size] = token.split("/");
                result.backgroundPosition = position.trim();
                result.backgroundSize = size.trim();
                positionAndSizeHandled = true;
            }
            else if (!positionAndSizeHandled) {
                result.backgroundPosition = token;
                if (i + 1 < tokens.length && tokens[i + 1] === "/") {
                    result.backgroundSize = tokens[i + 2];
                    // Skip position, "/", and size
                    i += 2;
                    positionAndSizeHandled = true;
                }
            }
        }

        return result;
    }

    // Helper functions
    function isColor(value: string): boolean {
        return /^(#(?:[0-9a-fA-F]{3}){1,2}|rgba?\(.*?\)|transparent|[a-z]+)$/.test(value);
    }

    function isUrl(value: string): boolean {
        return /^url\((.*?)\)$/.test(value);
    }

    function isGradient(value: string): boolean {
        return /^(linear-gradient|radial-gradient|conic-gradient)\(.*?\)$/.test(value);
    }

    function isRepeatValue(value: string): boolean {
        return /^(repeat|no-repeat|repeat-x|repeat-y|space|round)$/.test(value);
    }

    function isAttachmentValue(value: string): boolean {
        return /^(scroll|fixed|local)$/.test(value);
    }

    function isBoxValue(value: string): boolean {
        return /^(border-box|padding-box|content-box)$/.test(value);
    }

    function getCurrentElementValue(): BackgroundSize | null | undefined {
        const backgroundSize = getCurrentElementStyleBackgroundSize();
        if (backgroundSize === backgroundSizeFitWidth) {
            return "fit-width";
        }
        else if (backgroundSize === backgroundSizeFitHeight) {
            return "fit-height";
        }
        else if (backgroundSize === backgroundSizeOriginal) {
            return "original";
        }
        else {
            return null;
        }
    }

    function setBackgroundSize(backgroundSize: string): void {
        // Use the removeProperty to figure out how to remove the current value.
        targetElements.forEach(element => {
            element.style.removeProperty("background-size");

            const styleDeclarationsString = element.getAttribute("style");
            if (styleDeclarationsString) {
                const suffix = styleDeclarationsString.endsWith(";") ? " " : "; ";
                element.setAttribute("style", `${styleDeclarationsString}${suffix}background-size: ${backgroundSize};`);
            }
            else {
                element.setAttribute("style", `background-size: ${backgroundSize};`);
            }
        });
    }

    function getCurrentElementStyleBackgroundSize(): string {
        const styleDeclarationsString = element.getAttribute("style");

        if (styleDeclarationsString) {
            type PropertyAndValue = {
                property: string;
                value: string;
            };

            const properties = Enumerable
                .from(styleDeclarationsString.split(";"))
                .select((propertyString): PropertyAndValue | null => {
                    propertyString = propertyString.trim();
                    const firstColonIndex = propertyString.indexOf(":");

                    if (firstColonIndex !== -1 && (firstColonIndex + 1) < propertyString.length) {
                        return {
                            property: propertyString.substring(0, firstColonIndex),
                            value: propertyString.substring(firstColonIndex + 1).trimStart()
                        };
                    }
                    else {
                        return null;
                    }
                })
                .where(property => property?.property === "background" || property?.property === "background-size")
                .ofType(isNotNullish)
                .toList();

            const backgroundSize = properties.lastOrUndefined(property => property.property === "background-size");
            if (backgroundSize?.value) {
                return backgroundSize.value;
            }

            // Try to harvest the background size from the background shorthand property.
            const background = properties.lastOrUndefined(property => property.property === "background");
            if (background?.value) {
                // Parse the background.
                const backgroundStyles = parseBackgroundShorthand(background.value);
                return Enumerable.from(backgroundStyles)
                    .where(style => !!style.backgroundSize)
                    .lastOrDefault()?.backgroundSize ?? "";
            }
        }

        return "";
    }

    const watcher = watch(value, (newValue) => {
        if (newValue === "fit-width") {
            setBackgroundSize(backgroundSizeFitWidth);
        }
        else if (newValue === "fit-height") {
            setBackgroundSize(backgroundSizeFitHeight);
        }
        else if (newValue === "original") {
            setBackgroundSize(backgroundSizeOriginal);
        }
        else {
            targetElements.forEach(element => {
                element.style.removeProperty("background-size");
            });
        }
    });

    const provider = {
        get value(): BackgroundSize | null | undefined {
            return value.value;
        },
        set value(newValue: BackgroundSize | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createBackgroundFitProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined
): ValueProvider<BackgroundFit[] | null | undefined> {
    const targetElements = [element, ...(copyToElements ?? [])];

    const value = ref<BackgroundFit[] | null | undefined>(getValue(element));

    function getValue(element: HTMLElement): BackgroundFit[] | null | undefined {
        const backgroundRepeat = element.style.backgroundRepeat;
        const backgroundPosition = element.style.backgroundPosition;

        const properties: BackgroundFit[] = [];

        if (backgroundRepeat === "repeat") {
            properties.push("repeat");
        }

        if (backgroundPosition === "center center") {
            properties.push("center");
        }

        if (!properties.length) {
            const hasStyleProperty = !!backgroundRepeat || !!backgroundPosition;

            if (hasStyleProperty) {
                // Return an empty array if there is an unrecognized style property
                // so that the property control can display a clear button.
                return [];
            }
            else {
                return null;
            }
        }
        else {
            return properties;
        }
    }

    const watcher = watch(value, (newValue, oldValue) => {
        if (newValue) {
            if (newValue.includes("repeat")) {
                targetElements.forEach(element => {
                    element.style.backgroundRepeat = "repeat";
                });
            }
            else if (oldValue?.includes("repeat")) {
                // The old value was "repeat-repeat" and the user deselected it,
                // so change the style explicitly to "no-repeat".
                targetElements.forEach(element => {
                    element.style.backgroundRepeat = "no-repeat";
                });
            }

            if (newValue.includes("center")) {
                targetElements.forEach(element => {
                    element.style.backgroundPosition = "top center";
                });
            }
            else if (oldValue?.includes("center")) {
                // The old value was "center" and the user deselected it,
                // so change the style explicitly to "top left".
                targetElements.forEach(element => {
                    element.style.backgroundPosition = "top left";
                });
            }
        }
        else {
            // The value was set to null so remove all styles.
            // This can happen when a clear button is clicked.
            targetElements.forEach(element => {
                element.style.removeProperty("background-repeat");
                element.style.removeProperty("background-position");
            });
        }
    });

    const provider = {
        get value(): BackgroundFit[] | null | undefined {
            return value.value;
        },
        set value(newValue: BackgroundFit[] | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createDomWatcherProvider<T>(
    root: Document | Element,
    selector: string,
    valueProviderFactory: (element: Element) => ValueProvider<T>,
    fallback: T,
    options?: { includeSelf?: boolean | string; }
): ValueProvider<T> {
    const valueProviderKeys = new Set<Element>();
    let valueProviders = new WeakMap<Element, { provider: ValueProvider<T> }>();

    const value = ref<T>(getValueProviders().firstOrDefault()?.value ?? fallback);

    // Watch for element changes.
    const domWatcher = createDomWatcher(root, selector, options);
    domWatcher.onElementFound((el) => {
        const provider = addValueProvider(el);

        const isFirstValueProvider = valueProviderKeys.size === 1;

        if (isFirstValueProvider && !value.value) {
            // Get the value from the first provider.
            (value as Ref<T>).value = provider.value;
        }
        else {
            // Ensure the new value provider gets the current value.
            provider.value = (value as Ref<T>).value;
        }
    });
    domWatcher.onElementRemoved(removeValueProvider);

    function addValueProvider(element: Element): ValueProvider<T> {
        // Ensure the key exists.
        if (!valueProviderKeys.has(element)) {
            valueProviderKeys.add(element);
        }

        if (valueProviders.has(element)) {
            return valueProviders.get(element)!.provider;
        }
        else {
            const provider = valueProviderFactory(element);
            valueProviders.set(element, { provider });
            return provider;
        }
    }

    function removeValueProvider(element: Element): ValueProvider<T> | undefined {
        if (valueProviderKeys.has(element)) {
            valueProviderKeys.delete(element);
        }

        if (valueProviders.has(element)) {
            const provider = valueProviders.get(element)!.provider;
            valueProviders.delete(element);
            return provider;
        }
    }

    function getValueProviders(): Enumerable<ValueProvider<T>> {
        return Enumerable.from(valueProviderKeys)
            .where(key => valueProviders.has(key))
            .select(key => valueProviders.get(key)!.provider);
    }

    const watcher = watch(value, (newValue) => {
        // Set the new value in all providers.
        getValueProviders().forEach(provider => {
            provider.value = newValue as T;
        });
    });

    return {
        get value(): T {
            return value.value as T;
        },

        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },

        dispose() {
            watcher();

            // Copy the keys array since keys will be removed from the original array.
            [...valueProviderKeys].forEach(element => {
                const provider = removeValueProvider(element);

                if (provider) {
                    provider.dispose();
                }
            });

            // To be safe, clear the keys and provider objects too.
            valueProviderKeys.clear();
            valueProviders = new WeakMap<Element, { provider: ValueProvider<T> }>();

            domWatcher.dispose();
        },
    };
}

const globalBodyWidthProviderCache = new WeakPair<Document, ValueProvider<number | null | undefined>>();

export function createGlobalBodyWidthProvider(
    document: Document
): ValueProvider<number | null | undefined> {
    if (globalBodyWidthProviderCache.has(document)) {
        throw new ProviderAlreadyExistsError("GlobalBodyWidthProvider");
    }

    // When `max-width` is changed, it should also update `width` attributes on matching elements.
    // Watch for body inner table elements.
    const widthAttributeProvider = createDomWatcherProvider(
        document.body,
        `${GlobalStylesCssSelectors.bodyWidth}:not([data-body-width-component="true"])`,
        (el) => attributeProvider(el, "width", numberToStringConverter),
        undefined
    );

    // When `max-width` is changed and has a value, the `width` style should also be set to 100%.
    // There should already be an inline style `width: 100%` on the inner table but add this as a fallback.
    const widthStyleSheetProvider = styleSheetProvider(
        document.body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.bodyWidth,
        "width",
        stringConverter
    );

    // The returned provider is primarily based on this provider's value.
    // Hooks are used to update the side-effect provider values.
    const maxWidthStyleSheetProvider = styleSheetProvider(
        document.body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.bodyWidth,
        "max-width",
        pixelConverter, {
        onSourceValueUpdated(newWidth: number | null | undefined): void {
            widthAttributeProvider.value = newWidth;
            if (!isNullish(newWidth)) {
                widthStyleSheetProvider.value = "100%";
            }
        }
    });

    const provider = createDefaultValueProvider(
        {
            get value(): number | null | undefined {
                return maxWidthStyleSheetProvider.value;
            },

            set value(newValue: number | null | undefined) {
                maxWidthStyleSheetProvider.value = newValue;
            },

            dispose(): void {
                maxWidthStyleSheetProvider.dispose();
                widthStyleSheetProvider.dispose();
                widthAttributeProvider.dispose();
                globalBodyWidthProviderCache.delete(document);
            }
        },
        DefaultBodyWidth
    );

    // There should only ever be one global provider. Overwrite it here.
    globalBodyWidthProviderCache.set(document, provider);

    return provider;
}

export function getGlobalBodyWidthProvider(
    document: Document
): ValueProvider<number | null | undefined> {
    if (!globalBodyWidthProviderCache.has(document)) {
        throw new ProviderNotCreatedError("GlobalBodyWidthProvider");
    }

    return globalBodyWidthProviderCache.get(document)!;
}

const globalBodyAlignmentProviderCache = new WeakPair<Document, ValueProvider<string | null | undefined>>();

export function createGlobalBodyAlignmentProvider(
    document: Document
): ValueProvider<string | null | undefined> {
    if (globalBodyAlignmentProviderCache.has(document)) {
        throw new ProviderAlreadyExistsError("GlobalBodyAlignmentProvider");
    }

    const provider = createDefaultValueProvider(
        createDomWatcherProvider(
            document.body,
            GlobalStylesCssSelectors.bodyAlignment,
            (el) => attributeProvider(el, "align", stringConverter),
            undefined
        ),
        DefaultBodyAlignment
    );

    globalBodyAlignmentProviderCache.set(document, provider);

    return provider;
}

export function getGlobalBodyAlignmentProvider(
    document: Document
): ValueProvider<string | null | undefined> {
    if (!globalBodyAlignmentProviderCache.has(document)) {
        throw new ProviderNotCreatedError("GlobalBodyAlignmentProvider");
    }

    return globalBodyAlignmentProviderCache.get(document)!;
}

function createDefaultValueProvider<T>(
    valueProvider: ValueProvider<T>,
    defaultValue: T,
    applyDefaultCheck: ((value: T) => boolean) = ((value: T) => isNullish(value))
): ValueProvider<T> {
    if (defaultValue !== undefined && applyDefaultCheck(valueProvider.value)) {
        valueProvider.value = defaultValue;
    }

    return valueProvider;
}

const globalBodyBackgroundColorProviderCache = new WeakPair<Document, ValueProvider<string | null | undefined>>();

export function createGlobalBodyBackgroundColorProvider(
    document: Document
): ValueProvider<string | null | undefined> {
    if (globalBodyBackgroundColorProviderCache.has(document)) {
        throw new ProviderAlreadyExistsError("GlobalBodyBackgroundColorProvider");
    }

    const head = document.body;
    const body = document.body;

    const bgcolorAttributeProvider = createDomWatcherProvider(
        body,
        `${GlobalStylesCssSelectors.bodyColor}:not([data-body-color-component="true"])`,
        (el) => attributeProvider(el, "bgcolor", stringConverter),
        undefined
    );

    const backgroundColorStyleSheetProvider = styleSheetProvider(
        head,
        RockStylesCssClass,
        GlobalStylesCssSelectors.bodyColor,
        "background-color",
        stringConverter
    );

    const value = ref<string | null | undefined>(backgroundColorStyleSheetProvider.value);

    const watcher = watch(value, (newValue) => {
        backgroundColorStyleSheetProvider.value = newValue;
        bgcolorAttributeProvider.value = newValue;
    });

    const provider = createDefaultValueProvider(
        {
            get value(): string | null | undefined {
                return value.value;
            },
            set value(newValue: string | null | undefined) {
                value.value = newValue;
            },
            dispose() {
                watcher();
                backgroundColorStyleSheetProvider.dispose();
                bgcolorAttributeProvider.dispose();
            }
        },
        DefaultBodyColor
    );

    globalBodyBackgroundColorProviderCache.set(document, provider);

    return provider;
}

export function getGlobalBodyBackgroundColorProvider(
    document: Document
): ValueProvider<string | null | undefined> {

    if (!globalBodyBackgroundColorProviderCache.has(document)) {
        throw new ProviderNotCreatedError("GlobalBodyBackgroundColorProvider");
    }

    return globalBodyBackgroundColorProviderCache.get(document)!;
}

const globalBackgroundColorProviderCache = new WeakPair<Document, ValueProvider<string | null | undefined>>();

export function createGlobalBackgroundColorProvider(
    document: Document
): ValueProvider<string | null | undefined> {
    if (globalBackgroundColorProviderCache.has(document)) {
        throw new ProviderAlreadyExistsError("GlobalBackgroundColorProvider");
    }

    const head = document.body;
    const body = document.body;

    const bgcolorAttributeProvider = createDomWatcherProvider(
        body,
        GlobalStylesCssSelectors.backgroundColor,
        (el) => attributeProvider(el, "bgcolor", stringConverter),
        undefined
    );

    const backgroundColorStyleSheetProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.backgroundColor,
        "background-color",
        stringConverter
    );

    const value = ref<string | null | undefined>(backgroundColorStyleSheetProvider.value);

    const watcher = watch(value, (newValue) => {
        backgroundColorStyleSheetProvider.value = newValue;
        bgcolorAttributeProvider.value = newValue;
    });

    const provider = createDefaultValueProvider(
        {
            get value(): string | null | undefined {
                return value.value;
            },
            set value(newValue: string | null | undefined) {
                value.value = newValue;
            },
            dispose() {
                watcher();
                backgroundColorStyleSheetProvider.dispose();
                bgcolorAttributeProvider.dispose();
            }
        },
        DefaultEmailBackgroundColor
    );

    globalBackgroundColorProviderCache.set(document, provider);

    return provider;
}

export function getGlobalBackgroundColorProvider(
    document: Document
): ValueProvider<string | null | undefined> {
    if (!globalBackgroundColorProviderCache.has(document)) {
        throw new ProviderNotCreatedError("GlobalBackgroundColorProvider");
    }

    return globalBackgroundColorProviderCache.get(document)!;
}

const globalButtonWidthValuesProviderCache = new WeakPair<Document, ValueProvider<ButtonWidthValues | null | undefined>>();

export function createGlobalButtonWidthValuesProvider(
    document: Document
): ValueProvider<ButtonWidthValues | null | undefined> {
    if (globalButtonWidthValuesProviderCache.has(document)) {
        throw new ProviderAlreadyExistsError("GlobalButtonWidthValuesProvider");
    }

    const body = document.body;

    const buttonShellMaxWidthProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.buttonWidthValuesShell,
        "max-width",
        percentageConverter
    );

    const buttonShellWidthProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.buttonWidthValuesShell,
        "width",
        stringConverter
    );

    const buttonShellWidthAttributeProvider = createDomWatcherProvider(
        body,
        `.component-button .button-shell:not([data-component-button-width="true"]), .component-rsvp .rsvp-button-shell:not([data-component-button-width="true"])`,
        (el) => attributeProvider(
            el,
            "width",
            stringConverter),
        undefined
    );

    const buttonLinkDisplayProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        GlobalStylesCssSelectors.buttonWidthValuesButton,
        "display",
        stringConverter
    );

    const value = ref<ButtonWidthValues | null | undefined>({
        width: getCurrentWidth(),
        fixedWidth: getCurrentFixedPixelWidth()
    });

    function getCurrentWidth(): ButtonWidth | null | undefined {
        // Only check the style sheet providers since they aren't dependent
        // on the existence of a button component.
        const maxWidth = buttonShellMaxWidthProvider.value;
        const width = buttonShellWidthProvider.value;
        const display = buttonLinkDisplayProvider.value;

        if (display === "inline-block" && maxWidth === 100) {
            return "fitToText";
        }
        else if (maxWidth === 100) {
            return "full";
        }
        else if (width?.endsWith("px") && display === "block") {
            return "fixed";
        }
        else {
            return null;
        }
    }

    function getCurrentFixedPixelWidth(): number | null | undefined {
        const width = buttonShellWidthProvider.value;

        if (width?.endsWith("px")) {
            const fixedPixelWidth = parseInt(width);

            if (!Number.isNaN(fixedPixelWidth)) {
                return fixedPixelWidth;
            }
        }
    }

    const watcher = watch(value, (newValue) => {
        switch (newValue?.width) {
            case "fitToText":
                buttonShellMaxWidthProvider.value = 100;
                buttonShellWidthProvider.value = null;
                buttonShellWidthAttributeProvider.value = null;
                buttonLinkDisplayProvider.value = "inline-block";
                break;
            case "full":
                buttonShellMaxWidthProvider.value = 100;
                buttonShellWidthProvider.value = "100%";
                buttonShellWidthAttributeProvider.value = "100%";
                buttonLinkDisplayProvider.value = "block";
                break;
            case "fixed":
                buttonShellMaxWidthProvider.value = null;
                // Default to 100px for fixed width.
                buttonShellWidthProvider.value = `${newValue.fixedWidth ?? 100}px`;
                buttonShellWidthAttributeProvider.value = `${newValue.fixedWidth ?? 100}`;
                buttonLinkDisplayProvider.value = "block";
                break;
            default:
                buttonShellMaxWidthProvider.value = null;
                buttonShellWidthProvider.value = null;
                buttonShellWidthAttributeProvider.value = null;
                buttonLinkDisplayProvider.value = null;
        }
    });

    const provider = createDefaultValueProvider(
        {
            get value(): ButtonWidthValues | null | undefined {
                return value.value;
            },
            set value(newValue: ButtonWidthValues | null | undefined) {
                value.value = newValue;
            },
            dispose() {
                watcher();
                buttonShellMaxWidthProvider.dispose();
                buttonShellWidthProvider.dispose();
                buttonShellWidthAttributeProvider.dispose();
                buttonLinkDisplayProvider.dispose();
            }
        },
        {
            width: "fitToText",
            fixedWidth: undefined
        },
        value => isNullish(value?.fixedWidth)
    );

    globalButtonWidthValuesProviderCache.set(document, provider);

    return provider;
}

export function getGlobalButtonWidthValuesProvider(
    document: Document
): ValueProvider<ButtonWidthValues | null | undefined> {
    if (!globalButtonWidthValuesProviderCache.has(document)) {
        throw new ProviderNotCreatedError("GlobalButtonWidthValuesProvider");
    }

    return globalButtonWidthValuesProviderCache.get(document)!;
}

export function createButtonWidthValuesProvider(
    buttonShellElement: HTMLElement
): ValueProvider<ButtonWidthValues | null | undefined> {
    const buttonShellMaxWidthProvider = inlineStyleProvider(
        buttonShellElement,
        "max-width",
        percentageConverter
    );

    const buttonShellWidthProvider = inlineStyleProvider(
        buttonShellElement,
        "width",
        stringConverter
    );

    const buttonShellWidthAttributeProvider = attributeProvider(
        buttonShellElement,
        "width",
        stringConverter
    );

    const buttonLinkDisplayProvider = createDomWatcherProvider(
        buttonShellElement,
        ".button-link, .rsvp-accept-link, .rsvp-decline-link",
        (el) => inlineStyleProvider(
            el as HTMLElement,
            "display",
            stringConverter
        ),
        undefined
    );

    const value = ref<ButtonWidthValues>({
        width: getCurrentWidth(),
        fixedWidth: getCurrentFixedPixelWidth()
    });

    const watcher = watch(value, (newValue) => {
        let isOverriddenForComponent = true;
        switch (newValue?.width) {
            case "fitToText":
                buttonShellMaxWidthProvider.value = 100;
                buttonShellWidthProvider.value = null;
                buttonShellWidthAttributeProvider.value = null;
                buttonLinkDisplayProvider.value = "inline-block";
                break;
            case "full":
                buttonShellMaxWidthProvider.value = 100;
                buttonShellWidthProvider.value = "100%";
                buttonShellWidthAttributeProvider.value = "100%";
                buttonLinkDisplayProvider.value = "block";
                break;
            case "fixed":
                buttonShellMaxWidthProvider.value = null;
                // Default to 100px for fixed width.
                buttonShellWidthProvider.value = `${newValue.fixedWidth ?? 100}px`;
                buttonShellWidthAttributeProvider.value = `${newValue.fixedWidth ?? 100}`;
                buttonLinkDisplayProvider.value = "block";
                break;
            default:
                isOverriddenForComponent = false;
                buttonShellMaxWidthProvider.value = null;
                buttonShellWidthProvider.value = null;
                buttonShellWidthAttributeProvider.value = null;
                buttonLinkDisplayProvider.value = null;
                break;
        }

        if (isOverriddenForComponent) {
            // Doing this here instead of the width attribute sourceValueUpdated hook
            // because the attribute could be set to null but still be a component-specific override.
            // This override will prevent the global version of this property from setting component-overridden attribute values.
            buttonShellElement.setAttribute("data-component-button-width", "true");
        }
        else {
            buttonShellElement.removeAttribute("data-component-button-width");
        }
    });

    function getCurrentWidth(): ButtonWidth | null | undefined {
        const widthAttribute = buttonShellWidthAttributeProvider.value;

        if (widthAttribute === "100%") {
            return "full";
        }
        else if (widthAttribute) {
            return "fixed";
        }
        else if (buttonLinkDisplayProvider.value === "inline-block") {
            return "fitToText";
        }
        else {
            return null;
        }
    }

    function getCurrentFixedPixelWidth(): number | null | undefined {
        const widthAttribute = buttonShellWidthAttributeProvider.value;

        if (widthAttribute && !widthAttribute.endsWith("%")) {
            const fixedPixelWidth = parseInt(widthAttribute);

            if (!Number.isNaN(fixedPixelWidth)) {
                return fixedPixelWidth;
            }
        }
    }

    return {
        get value(): ButtonWidthValues {
            return value.value;
        },
        set value(newValue: ButtonWidthValues) {
            value.value = newValue;
        },
        dispose() {
            watcher();
            buttonShellMaxWidthProvider.dispose();
            buttonShellWidthProvider.dispose();
            buttonShellWidthAttributeProvider.dispose();
            buttonLinkDisplayProvider.dispose();
        }
    };
}