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

import { inject, InjectionKey, provide } from "vue";
import { RockColor } from "@Obsidian/Core/Utilities/rockColor";
import { Chart, BarElement, ChartMeta, ChartType, Plugin, BarProps } from "@Obsidian/Libs/chart";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { isHTMLElement } from "@Obsidian/Utility/dom";
import { tooltip } from "@Obsidian/Utility/tooltip";

export function isEnumValue<T extends Record<string, number | string>>(enumObject: T, value: unknown): value is T[keyof T] {
    return Object.values(enumObject).includes(value as T[keyof T]);
}

export const BarDatasetLabelsPlugin: Plugin<"bar"> = {
    id: "barDatasetLabels",
    afterDatasetDraw(chart: Chart<"bar">, { index: datasetIndex, meta: _meta }: { index: number; meta: ChartMeta<"bar"> }) {
        const { ctx, data } = chart;

        ctx.save();
        chart.getDatasetMeta(datasetIndex).data.forEach((datapoint, _datapointIndex) => {
            ctx.font = "normal 14px sans-serif";
            ctx.fillStyle = getCssValue("--color-interface-medium");
            ctx.textAlign = "center";
            const label = `${data.datasets[datasetIndex].label}`;
            ctx.fillText(label, datapoint.x, datapoint.y - 7);
        });
    }
};

const contrastColors: Record<string, string> = {};

// Define custom plugin options
interface IBarValueLabelsPluginOptions {
    formatter?: (value: string) => string;
}

// Extend the Chart.js type definitions
declare module "@Obsidian/Libs/chart" {
    // eslint-disable-next-line @typescript-eslint/naming-convention, @typescript-eslint/no-unused-vars
    interface PluginOptionsByType<TType extends ChartType> {
        barValueLabels?: IBarValueLabelsPluginOptions;
    }
}

export const BarValueLabelsPlugin: Plugin<"bar"> = {
    id: "barValueLabels",
    afterDatasetDraw(chart: Chart<"bar">, { index: datasetIndex, meta }: { index: number; meta: ChartMeta<"bar"> }, options?: IBarValueLabelsPluginOptions) {
        const { ctx, data } = chart;

        ctx.save();
        meta.data.forEach((dataElement, dataIndex) => {
            if (dataElement instanceof BarElement === false) {
                console.warn("BarValueLabelsPlugin can only be used with BarElement data elements.", dataElement);
                return;
            }

            const barProps: (keyof BarProps)[] = ["base"];
            const { base } = dataElement.getProps(barProps);

            const fontSize = 12;
            const minimumSpaceBetweenTextAndBorder = 4;
            const desiredSpaceBetweenTextAndBar = 8;
            ctx.font = `normal ${fontSize}px sans-serif`;
            ctx.textAlign = "center";

            // Because the value is printed on top of the bar,
            // we need to figure out whether white or black text is more readable.
            const barColor = data.datasets[datasetIndex].backgroundColor?.[dataIndex] || data.datasets[datasetIndex].borderColor?.[dataIndex] || "";

            if (typeof barColor !== "string") {
                console.warn("Bar color must be a string to determine contrast color to use the BarValueLabelsPlugin.", barColor);
                return;
            }

            if (!contrastColors[barColor]) {
                // Only calculate the contrast color once per bar color.
                const color = new RockColor(barColor);
                contrastColors[barColor] = color.luminosity >= .5 ? "black" : "white";
            }

            // Use the contrast color and fallback to the CSS contrast color if not defined.
            ctx.fillStyle = contrastColors[barColor] || getCssValue("--color-interface-contrast");

            const value = `${data.datasets[datasetIndex].data[dataIndex]}`;
            const formatted = options?.formatter ? options.formatter(value) : value;

            let textOffset = (fontSize / 2) + minimumSpaceBetweenTextAndBorder;
            if (base - dataElement.y > (fontSize / 2) + desiredSpaceBetweenTextAndBar + minimumSpaceBetweenTextAndBorder) {
                textOffset = (fontSize / 2) + desiredSpaceBetweenTextAndBar;
            }

            ctx.fillText(formatted, dataElement.x, dataElement.y + textOffset);
        });
    }
};

function getCssValue(variableName: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(variableName).trim();
}

type RockDateTimeParser = {
    parseRockDateTime(isoDateTime: string | null | undefined): RockDateTime | null;
};

const rockDateTimeParserInjectionKey: InjectionKey<RockDateTimeParser> = Symbol("rock-date-time-parser");

export function provideRockDateTimeParser(): RockDateTimeParser {
    const cachedDates = new Map<string, RockDateTime | null>();

    function parseRockDateTime(isoDateTime: string | null | undefined): RockDateTime | null {
        if (!isoDateTime) {
            return null;
        }

        if (cachedDates.has(isoDateTime)) {
            return cachedDates.get(isoDateTime) ?? null;
        }

        const dateTime = RockDateTime.parseISO(isoDateTime);
        cachedDates.set(isoDateTime, dateTime);

        return dateTime;
    }

    const parser: RockDateTimeParser = { parseRockDateTime };

    provide(rockDateTimeParserInjectionKey, parser);

    return parser;
}

export function useRockDateTimeParser(): RockDateTimeParser {
    const parser = inject<RockDateTimeParser>(rockDateTimeParserInjectionKey);

    if (!parser) {
        throw new Error("useRockDateTimeParser must be used after provideRockDateTimeParser has been called.");
    }

    return parser;
}

/**
 * Applies a tooltip to an element. The element should have an `data-original-title` attribute containing the tooltip text.
 *
 * Typical use, `:ref="applyTooltip"`
 */
export function applyTooltip(el: unknown): void {
    if (isHTMLElement(el)) {
        tooltip(el);
    }
}

export const RockDateTimeFormatter = {
    // #region Date-Only Formats

    /** 1/1/1970 */
    format_1_1_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "numeric",
            day: "numeric"
        }) ?? "";
    },

    /** 01/01/1970 */
    format_01_01_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "2-digit",
            day: "2-digit"
        }) ?? "";
    },

    /** Jan 1, 1970 */
    format_Jan_1_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "short",
            day: "numeric"
        }) ?? "";
    },

    /** Jan. 1, 1970 */
    format_JanDot_1_1970(input: RockDateTime | null | undefined): string {
        if (!input) {
            return "";
        }

        const base = input.toLocaleString({
            year: "numeric",
            month: "short",
            day: "numeric"
        });

        const monthOnly = input.toLocaleString({
            month: "short"
        });

        if (monthOnly.endsWith(".")) {
            // The locale already includes a dot after the month, so we can return it as is.
            return base;
        }
        else {
            return base.replace(monthOnly, `${monthOnly}.`);
        }
    },

    /** January 1, 1970 */
    format_January_1_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "long",
            day: "numeric"
        }) ?? "";
    },

    /** 01/01/70 */
    format_01_01_70(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "2-digit",
            month: "2-digit",
            day: "2-digit"
        }) ?? "";
    },

    /** Jan 1970 */
    format_Jan_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "short"
        }) ?? "";
    },

    /** Jan */
    format_Jan(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            month: "short"
        }) ?? "";
    },

    /** January 1970 */
    format_January_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "long"
        }) ?? "";
    },

    /** January 70 */
    format_January_70(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "2-digit",
            month: "long"
        }) ?? "";
    },

    /** Jan 1 */
    format_Jan_1(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            month: "short",
            day: "numeric"
        }) ?? "";
    },

    /** January 1 */
    format_January_1(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            month: "long",
            day: "numeric"
        }) ?? "";
    },

    /** 1970 */
    format_1970(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric"
        }) ?? "";
    },

    /** 1 */
    format_Day_1(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            day: "numeric"
        }) ?? "";
    },

    format_Thursday_JanDot_1_1970(input: RockDateTime | null | undefined): string {
        if (!input) {
            return "";
        }

        const formatted = input.toLocaleString({
            weekday: "long",
            year: "numeric",
            month: "short",
            day: "numeric"
        });

        const monthOnly = input.toLocaleString({ month: "short" });

        if (monthOnly.endsWith(".")) {
            // The locale already includes a dot after the month, so we can return it as is.
            return formatted;
        }
        else {
            return formatted.replace(monthOnly, `${monthOnly}.`);
        }
    },

    // #endregion Date-Only Formats

    // #region Time-Only Formats

    /** 1:00 AM */
    format_1_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            hour: "numeric",
            minute: "2-digit"
        }) ?? "";
    },

    /** 01:00 AM */
    format_01_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            hour: "2-digit",
            minute: "2-digit",
            hour12: true
        }) ?? "";
    },

    /** 1:00:00 AM */
    format_1_00_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            hour: "numeric",
            minute: "2-digit",
            second: "2-digit"
        }) ?? "";
    },

    /** 13:00 */
    format_13_00(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            hour: "2-digit",
            minute: "2-digit",
            hour12: false
        }) ?? "";
    },

    /** 13:00:00 */
    format_13_00_00(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: false
        }) ?? "";
    },

    // #endregion Time-Only Formats

    // #region Date Time Formats

    /** Jan 1, 1970, 1:00 AM */
    format_Jan_1_1970_1_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "short",
            day: "numeric",
            hour: "numeric",
            minute: "2-digit"
        }) ?? "";
    },

    /** Jan. 1, 1970 1:00 AM */
    format_JanDot_1_1970_1_00_AM(input: RockDateTime | null | undefined): string {
        if (!input) return "";

        const datePart = input.toLocaleString({
            year: "numeric",
            month: "short",
            day: "numeric"
        });

        const timePart = input.toLocaleString({
            hour: "numeric",
            minute: "2-digit"
        });

        const monthOnly = input.toLocaleString({ month: "short" });

        const dateWithDot = monthOnly.endsWith(".")
            ? datePart
            : datePart.replace(monthOnly, `${monthOnly}.`);

        return `${dateWithDot} ${timePart}`;
    },

    /** January 1, 1970 at 1:00 AM */
    format_January_1_1970_at_1_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "long",
            day: "numeric",
            hour: "numeric",
            minute: "2-digit"
        }) ?? "";
    },

    /** 1/1/1970, 1:00 AM */
    format_1_1_1970_1_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "numeric",
            day: "numeric",
            hour: "numeric",
            minute: "2-digit"
        }) ?? "";
    },

    /** 01/01/1970, 01:00 AM */
    format_01_01_1970_01_00_AM(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            hour12: true
        }) ?? "";
    },

    /** 01/01/1970, 13:00 */
    format_01_01_1970_13_00(input: RockDateTime | null | undefined): string {
        return input?.toLocaleString({
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            hour12: false
        }) ?? "";
    }

    // #endregion Date Time Formats
};