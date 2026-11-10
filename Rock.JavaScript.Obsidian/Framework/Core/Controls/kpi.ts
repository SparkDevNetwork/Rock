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

import { BadgeType } from "@Obsidian/Enums/Controls/badgeType";
import { BarSeries, LinearGaugeSeries, LineSeries, PieSeries } from "@Obsidian/Core/Controls/chart";
import { asFormattedString, toCurrencyOrNull } from "@Obsidian/Utility/numberUtils";
import { CurrencyInfoBag } from "@Obsidian/ViewModels/Rest/Utilities/currencyInfoBag";

// #region Types

// #region Card

/** The overall shape of a KPI card. */
export const KpiLayout = {
    /** Header, metric, chart, content and footer. */
    Default: "default",

    /** Header, metric and a small chart beside it. No content area. */
    Compact: "compact"
} as const;

export type KpiLayout = typeof KpiLayout[keyof typeof KpiLayout];

/**
 * The colored top border of a KPI card.
 *
 * Status accents are a judgment. Category accents are a grouping.
 * Only one kind should be used within a container.
 */
export const KpiAccent = {
    Success: "success",
    Info: "info",
    Warning: "warning",
    Danger: "danger",
    Category1: "category-1",
    Category2: "category-2",
    Category3: "category-3",
    Category4: "category-4",
    Category5: "category-5",
    Category6: "category-6",
    Category7: "category-7",
    Category8: "category-8"
} as const;

export type KpiAccent = typeof KpiAccent[keyof typeof KpiAccent];

/** One section of the "About this KPI" help popover. Sections render in the order given. */
export type KpiHelpSection = {
    /** An optional heading, such as "What it measures". Omit it for a plain paragraph. */
    label?: string | null;

    /** The section text. */
    text: string;
};

/** A badge that qualifies the headline value, such as "Needs Review". */
export type KpiLabel = {
    /** The badge text. Two to three words. */
    text: string;

    /** The badge style. */
    type: BadgeType;
};

/** The single footer action of a KPI card. */
export type KpiAction = {
    /** The action text, in title case. */
    text: string;

    /** When set the action navigates here. When not set the card emits `actionClick`. */
    url?: string | null;
};

// #endregion Card

// #region Values

/** How a numeric value is formatted. String values are always shown as given. */
export const KpiValueFormat = {
    /** Adds separators only. 20500.75 renders "20,500.75". */
    Number: "number",

    /** The value is seconds. 4925 renders "1h 22m 5s". */
    Duration: "duration",

    /** Uses the currency's own decimal places. */
    Currency: "currency",

    /** The value is already a percent. 38.4 renders "38.4%". */
    Percent: "percent"
} as const;

export type KpiValueFormat = typeof KpiValueFormat[keyof typeof KpiValueFormat];

/** The size of the headline value. */
export const KpiValueSize = {
    /** 28px bold. */
    Default: "default",

    /** Smaller text for longer strings or narrow cards. */
    Small: "small"
} as const;

export type KpiValueSize = typeof KpiValueSize[keyof typeof KpiValueSize];

/** A value shown on a KPI card. `null` or an empty string shows a dash. */
export type KpiValue = number | string | null;

// #endregion Values

// #region Trend

/** A trend that has no percent to show. */
export const KpiTrendDirection = {
    /** Went up, but the percent is unknown, such as when the prior period was 0. */
    Up: "up",

    /** Went down, but the percent is unknown. */
    Down: "down",

    /** No change. Always neutral. */
    Flat: "flat",

    /** Not enough history to calculate a trend yet. Always neutral. */
    Unknown: "unknown",

    /** No prior period exists at all. Shows "New". */
    New: "new"
} as const;

export type KpiTrendDirection = typeof KpiTrendDirection[keyof typeof KpiTrendDirection];

/**
 * The trend of the headline value.
 *
 * A number is a percent change, so 28.4 shows "28.4%" with an up arrow and -12 shows "12%" with a down arrow.
 */
export type KpiTrend = number | KpiTrendDirection;

/** The trend of a content item. Items have no "new" trend. */
export type KpiItemTrend = Exclude<KpiTrend, "new">;

/** How much of the headline trend is shown. */
export const KpiTrendDisplay = {
    /** The arrow and the percent. */
    Full: "full",

    /** The arrow only, with the detail in a tooltip. */
    Icon: "icon"
} as const;

export type KpiTrendDisplay = typeof KpiTrendDisplay[keyof typeof KpiTrendDisplay];

// #endregion Trend

// #region Chart

/** The chart types a KPI card can draw. */
export const KpiChartType = {
    Line: "line",
    Bar: "bar",
    Pie: "pie",
    LinearGauge: "linearGauge",
    Progress: "progress"
} as const;

export type KpiChartType = typeof KpiChartType[keyof typeof KpiChartType];

/** A line chart. Compact cards allow this type. */
export type KpiLineChart = {
    type: typeof KpiChartType.Line;
    labels: string[];
    series: LineSeries[];
};

/** A vertical bar chart. Compact cards allow this type. */
export type KpiBarChart = {
    type: typeof KpiChartType.Bar;
    labels: string[];
    series: BarSeries[];
};

/** A pie chart. The headline value must equal the pie's total. */
export type KpiPieChart = {
    type: typeof KpiChartType.Pie;
    labels: string[];
    series: PieSeries[];
};

/** A linear gauge. */
export type KpiLinearGaugeChart = {
    type: typeof KpiChartType.LinearGauge;
    series: LinearGaugeSeries[];
};

/** A single progress bar. */
export type KpiProgressChart = {
    type: typeof KpiChartType.Progress;

    /** The percent complete, from 0 to 100. */
    value: number;
};

/** The chart drawn by a KPI card. Labels and values appear only in tooltips. */
export type KpiChart = KpiLineChart | KpiBarChart | KpiPieChart | KpiLinearGaugeChart | KpiProgressChart;

// #endregion Chart

// #region Content

/** The kind of content shown below the chart. */
export const KpiContentDisplay = {
    /** Rows with a label, a value and a trend. */
    MicroGrid: "microGrid",

    /** Small cards in columns, each with a label, a value and a trend. */
    MicroCards: "microCards",

    /** The card's default slot. */
    Custom: "custom"
} as const;

export type KpiContentDisplay = typeof KpiContentDisplay[keyof typeof KpiContentDisplay];

/** One row of a micro-grid or one micro-card. */
export type KpiContentItem = {
    /** The item label. */
    label: string;

    /** The item value, formatted using the content's value format when it is a number. */
    value: KpiValue;

    /** The item trend. */
    valueTrend?: KpiItemTrend | null;

    /** An optional tooltip for the trend when only its icon is shown. */
    valueTrendTooltip?: string | null;

    /** When true the item is shown last, with muted colors and a faded trend. Used for "Unknown" or "Other". */
    isMuted?: boolean;

    /** Micro-grid only. Turns the label into a link. */
    url?: string | null;

    /** An optional tooltip shown when hovering the value. */
    help?: string | null;
};

/** The content shown below the chart. Not shown on compact cards. */
export type KpiContent = {
    /** An optional title. Use sparingly. */
    title?: string | null;

    /** The kind of content. */
    display: KpiContentDisplay;

    /** Micro-cards only. The number of columns. Defaults to 3. */
    columns?: 2 | 3 | 4;

    /** How numeric item values are formatted. Defaults to "number". */
    valueFormat?: KpiValueFormat;

    /** Micro-grid only. Prefixes each row with its position, in the order given. */
    isNumbered?: boolean;

    /** The items, in display order. Required for micro-grid and micro-cards. Ignored for custom. */
    items?: KpiContentItem[] | null;
};

// #endregion Content

// #region Trend Info

/** Whether a trend is good, bad or neither. This sets its color, separately from its arrow. */
export const KpiTrendSentiment = {
    Positive: "positive",
    Negative: "negative",
    Neutral: "neutral"
} as const;

export type KpiTrendSentiment = typeof KpiTrendSentiment[keyof typeof KpiTrendSentiment];

/** Options that change how a trend is described by `getKpiTrendInfo`. */
export type KpiTrendOptions = {
    /** When true a rise is negative and a drop is positive. Does not apply to "flat" or "unknown". Defaults to false. */
    isLowerBetter?: boolean;

    /** When "icon" the percent is hidden and only the arrow shows. "new" always shows its text. Defaults to "full". */
    display?: KpiTrendDisplay;

    /** A tooltip for the trend. When not set only "unknown" gets a default tooltip. */
    tooltip?: string | null;
};

/** Everything a control needs to draw a trend. */
export type KpiTrendInfo = {
    /** The direction of the trend. A numeric trend becomes "up", "down" or "flat". */
    direction: KpiTrendDirection;

    /** Whether the trend is good, bad or neither. */
    sentiment: KpiTrendSentiment;

    /** The icon to show. */
    iconCssClass: string;

    /** The text to show beside the icon, such as "28%" or "New". `null` when only the icon shows. */
    text: string | null;

    /** True when only the icon shows. */
    isIconOnly: boolean;

    /** True when the trend is drawn plain, with no badge fill. Only "unknown" is plain. */
    isPlain: boolean;

    /** The tooltip for the trend, or `null` when there is none. */
    tooltip: string | null;

    /** A text description of the trend for screen readers. */
    ariaLabel: string;
};

// #endregion Trend Info

// #endregion Types

// #region Utils

/** The text shown for a missing value. */
const emptyValueText = "-";

/** The default tooltip for an "unknown" trend. */
const unknownTrendTooltip = "Not enough data to calculate a trend yet.";

const secondsPerHour = 3600;
const secondsPerMinute = 60;

/**
 * Determines if a KPI value has data to show. `0` counts as data; `null`, `undefined` and an empty string do not.
 *
 * @param value The value to check.
 * @returns `true` if the value should be shown, `false` if a dash should be shown instead.
 */
export function hasKpiValue(value: KpiValue | undefined): boolean {
    return value !== null && value !== undefined && value !== "";
}

/**
 * Formats a number of seconds as every non-zero unit, such as "1h 22m 5s".
 *
 * @param totalSeconds The number of seconds.
 * @returns The formatted duration. Zero seconds returns "0s".
 */
function formatDuration(totalSeconds: number): string {
    let remaining = Math.round(Math.abs(totalSeconds));

    // The sign comes after rounding, so a value that rounds to zero, such as -0.4, shows "0s" instead of "-0s".
    const sign = totalSeconds < 0 && remaining > 0 ? "-" : "";

    const hours = Math.floor(remaining / secondsPerHour);
    remaining -= hours * secondsPerHour;

    const minutes = Math.floor(remaining / secondsPerMinute);
    const seconds = remaining - (minutes * secondsPerMinute);

    const parts: string[] = [];

    if (hours > 0) {
        parts.push(`${asFormattedString(hours)}h`);
    }

    if (minutes > 0) {
        parts.push(`${minutes}m`);
    }

    if (seconds > 0 || parts.length === 0) {
        parts.push(`${seconds}s`);
    }

    return sign + parts.join(" ");
}

/**
 * Formats a KPI value for display. Numbers are never rounded, except durations which round to whole seconds.
 *
 * @param value The value to format. Strings are returned as given.
 * @param format How a numeric value is formatted. Defaults to "number".
 * @param currencyInfo The currency to use when the format is "currency". Defaults to US dollars.
 * @returns The formatted value, or a dash when there is no value.
 */
export function formatKpiValue(value: KpiValue | undefined, format: KpiValueFormat = KpiValueFormat.Number, currencyInfo: CurrencyInfoBag | null = null): string {
    if (!hasKpiValue(value)) {
        return emptyValueText;
    }

    if (typeof value === "string") {
        return value;
    }

    // hasKpiValue() has already ruled out null and undefined.
    const numberValue = value as number;

    switch (format) {
        case KpiValueFormat.Duration:
            return formatDuration(numberValue);

        case KpiValueFormat.Currency:
            return toCurrencyOrNull(numberValue, currencyInfo) ?? emptyValueText;

        case KpiValueFormat.Percent:
            return `${asFormattedString(numberValue)}%`;

        default:
            return asFormattedString(numberValue);
    }
}

/**
 * Gets the direction of a trend. A numeric trend is a percent change, so its sign gives the direction.
 *
 * @param trend The trend.
 * @returns The trend direction.
 */
function getKpiTrendDirection(trend: KpiTrend): KpiTrendDirection {
    if (typeof trend !== "number") {
        return trend;
    }

    if (trend > 0) {
        return KpiTrendDirection.Up;
    }

    if (trend < 0) {
        return KpiTrendDirection.Down;
    }

    return KpiTrendDirection.Flat;
}

/**
 * Gets whether a trend is good, bad or neither. The arrow follows the sign; the sentiment
 * flips for lower-is-better metrics, except for "flat", "unknown" and "new" which are always neutral.
 *
 * @param direction The trend direction.
 * @param isLowerBetter `true` if a lower value is the better outcome.
 * @returns The trend sentiment.
 */
function getKpiTrendSentiment(direction: KpiTrendDirection, isLowerBetter: boolean): KpiTrendSentiment {
    if (direction === KpiTrendDirection.Flat || direction === KpiTrendDirection.Unknown || direction === KpiTrendDirection.New) {
        return KpiTrendSentiment.Neutral;
    }

    const isRise = direction === KpiTrendDirection.Up;

    return isRise !== isLowerBetter ? KpiTrendSentiment.Positive : KpiTrendSentiment.Negative;
}

/**
 * Gets the icon for a trend direction.
 *
 * @param direction The trend direction.
 * @returns The icon CSS class.
 */
function getKpiTrendIconCssClass(direction: KpiTrendDirection): string {
    switch (direction) {
        case KpiTrendDirection.Down:
            return "ti ti-trending-down";

        case KpiTrendDirection.Flat:
            return "ti ti-minus";

        case KpiTrendDirection.Unknown:
            return "ti ti-circle-dashed";

        case KpiTrendDirection.New:
            return "ti ti-flag";

        default:
            return "ti ti-trending-up";
    }
}

/**
 * Describes how a trend should be drawn: its direction, color, icon, text and tooltip.
 *
 * @param trend The trend, or `null` / `undefined` when no trend should be shown.
 * @param options Options that change how the trend is described.
 * @returns The trend details, or `null` when no trend should be shown.
 */
export function getKpiTrendInfo(trend: KpiTrend | null | undefined, options: KpiTrendOptions = {}): KpiTrendInfo | null {
    if (trend === null || trend === undefined) {
        return null;
    }

    // A string that isn't a known direction, such as "" from a bag, is treated as no trend.
    if (typeof trend === "string" && !(Object.values(KpiTrendDirection) as string[]).includes(trend)) {
        return null;
    }

    // So is a NaN percent.
    if (typeof trend === "number" && Number.isNaN(trend)) {
        return null;
    }

    const direction = getKpiTrendDirection(trend);
    const isNumericTrend = typeof trend === "number";
    const percentText = isNumericTrend ? `${asFormattedString(Math.abs(trend))}%` : null;

    // "new" always shows its text, "unknown" and bare directions never have text,
    // and a percent hides only when the caller asks for the icon alone.
    const isIconDisplay = options.display === KpiTrendDisplay.Icon;
    let text: string | null = null;

    if (direction === KpiTrendDirection.New) {
        text = "New";
    }
    else if (percentText && !isIconDisplay) {
        text = percentText;
    }

    let tooltip = options.tooltip || null;

    if (!tooltip && direction === KpiTrendDirection.Unknown) {
        tooltip = unknownTrendTooltip;
    }
    else if (!tooltip && isIconDisplay && percentText) {
        // The percent is hidden when only the icon shows, so the tooltip shows it instead.
        tooltip = getKpiTrendAriaLabel(direction, percentText);
    }

    return {
        direction,
        sentiment: getKpiTrendSentiment(direction, options.isLowerBetter === true),
        iconCssClass: getKpiTrendIconCssClass(direction),
        text,
        isIconOnly: text === null,
        isPlain: direction === KpiTrendDirection.Unknown,
        tooltip,
        ariaLabel: getKpiTrendAriaLabel(direction, percentText)
    };
}

/**
 * Gets a screen reader description of a trend, such as "Up 28%" or "No change".
 *
 * @param direction The trend direction.
 * @param percentText The formatted percent, or `null` when the trend has no percent.
 * @returns The description.
 */
function getKpiTrendAriaLabel(direction: KpiTrendDirection, percentText: string | null): string {
    switch (direction) {
        case KpiTrendDirection.Up:
            return percentText ? `Up ${percentText}` : "Up";

        case KpiTrendDirection.Down:
            return percentText ? `Down ${percentText}` : "Down";

        case KpiTrendDirection.Flat:
            return "No change";

        case KpiTrendDirection.Unknown:
            return unknownTrendTooltip;

        default:
            return "New";
    }
}

/**
 * Puts content items in display order: items in the order given, then muted items such as "Other" last.
 * Items are never sorted by value; the caller passes them highest first.
 *
 * @param items The content items.
 * @returns A new array in display order.
 */
export function getKpiDisplayItems(items: KpiContentItem[] | null | undefined): KpiContentItem[] {
    if (!items) {
        return [];
    }

    return [
        ...items.filter(item => !item.isMuted),
        ...items.filter(item => item.isMuted)
    ];
}

// #endregion Utils
