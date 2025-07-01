import { BarElement, BarProps, Chart, ChartData, ChartDataset, ChartMeta, ChartOptions, ChartType, Plugin, Point } from "@Obsidian/Libs/chart";
import { ChartNumericDataPointBag } from "@Obsidian/ViewModels/Reporting/chartNumericDataPointBag";
import { Enumerable, GroupedEnumerable } from "./linq";
import { isNullish } from "./util";
import { RockDateTime } from "./rockDateTime";

function getCssValue(variableName: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(variableName).trim();
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
    isBackgroundDark?: (barColor: string) => boolean;
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
                if (options?.isBackgroundDark?.(barColor)) {
                    contrastColors[barColor] = "white";
                }
                else {
                    contrastColors[barColor] = "black";
                }
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

export class BarChartDataBuilder {
    private seriesNameOrderBy: ((seriesNameA: string, seriesNameB: string) => number) | undefined;

    private constructor(private dataPoints: ChartNumericDataPointBag[]) { }

    public static createFromDataPoints(dataPoints: ChartNumericDataPointBag[]): BarChartDataBuilder {
        return new BarChartDataBuilder(dataPoints);
    }

    /**
     * Sets the order of the series in the chart to match the supplied array. Any series not in the array will be placed at the end.
     *
     * This is useful when you want to control the order of the series in the chart
     * instead of relying on the order of the source array elements.
     *
     * For bar charts, this affects the order of the bars in the chart.
     */
    public withSeriesOrder(orderBy: string[]): BarChartDataBuilder;

    /**
     * Sets the order of the series in the chart based on a comparator function.
     *
     * This is useful when you want to control the order of the series in the chart
     * instead of relying on the order of the source array elements.
     *
     * For bar charts, this affects the order of the bars in the chart.
     */
    public withSeriesOrder(orderBy: ((seriesNameA: string, seriesNameB: string) => number)): BarChartDataBuilder;

    public withSeriesOrder(orderBy: string[] | ((seriesNameA: string, seriesNameB: string) => number)): BarChartDataBuilder {
        if (Array.isArray(orderBy)) {
            const orderMap = new Map<string, number>(
                orderBy.map((value, index) => [value, index])
            );

            this.seriesNameOrderBy = (a: string, b: string): number => {
                const indexA = orderMap.has(a) ? orderMap.get(a)! : Number.MAX_SAFE_INTEGER;
                const indexB = orderMap.has(b) ? orderMap.get(b)! : Number.MAX_SAFE_INTEGER;

                if (indexA < indexB) {
                    return -1;
                }

                if (indexA > indexB) {
                    return 1;
                }

                return 0;
            };
        }
        else {
            this.seriesNameOrderBy = orderBy;
        }
        return this;
    }

    public build(): ChartData<"bar"> {
        const dataPointsEnumerable = Enumerable.from(this.dataPoints);

        const labels = dataPointsEnumerable
            .select(d => d.label)
            .ofType((d): d is string => !isNullish(d))
            .distinct()
            .toArray();

        let groupedBySeries = dataPointsEnumerable
            .where(d => !isNullish(d.seriesName))
            .groupBy(d => d.seriesName!)
            .ofType((group): group is GroupedEnumerable<string, ChartNumericDataPointBag> => {
                return !isNullish(group.key);
            });

        if (this.seriesNameOrderBy) {
            groupedBySeries = groupedBySeries
                .orderBy(group => group.key, this.seriesNameOrderBy);
        }

        return {
            labels,
            datasets: groupedBySeries
                .select<ChartDataset<"bar", (number | [number, number] | null)[]>>(group => {
                    const seriesDataColors = group
                        .select(d => d.color ?? undefined)
                        .toArray();

                    return {
                        label: group.key,
                        data: group.select(d => d.value).toArray(),
                        backgroundColor: seriesDataColors,
                        borderColor: seriesDataColors,
                        grouped: true
                    };
                })
                .toArray()
        };
    }
}

export class BarChartOptionsBuilder {
    private constructor(private options: ChartOptions<"bar">) { }

    public static create(): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({});
    }

    public build(): ChartOptions<"bar"> {
        return { ...this.options };
    }

    withXAxisAsDate(
        options: {
            timeUnit: "millisecond" | "second" | "minute" | "hour" | "day" | "week" | "month" | "quarter" | "year";
        } = {
                timeUnit: "day"
            }
    ): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            scales: {
                ...this.options.scales,
                x: {
                    type: "time",
                    time: {
                        unit: options.timeUnit
                    }
                }
            }
        });
    }

    /**
     * Formats the value as a percentage (with a '%' sign) in the Y-axis ticks and tooltips.
     *
     * This assumes the value is a fractional number representing a percentage (e.g., 0.5 for 50%).
     */
    withYAxisAsPercent(
        options: {
            maxDecimalPlaces: number | undefined;
        } = {
                maxDecimalPlaces: undefined
            }
    ): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            scales: {
                ...this.options.scales,
                y: {
                    ...this.options.scales?.y,
                    ticks: {
                        ...this.options.scales?.y?.ticks,
                        callback(tickValue, _index, _ticks) {
                            if (typeof tickValue !== "number") {
                                tickValue = Number(tickValue);
                            }

                            if (isNaN(tickValue)) {
                                // If it's not a number, return the original value.
                                return `${tickValue}`;
                            }

                            return tickValue.toLocaleString(undefined, { style: "percent", maximumFractionDigits: options.maxDecimalPlaces });
                        },
                    }
                }
            },
            plugins: {
                ...this.options.plugins,
                tooltip: {
                    ...this.options.plugins?.tooltip,
                    callbacks: {
                        ...this.options.plugins?.tooltip?.callbacks,
                        label: function (context) {
                            let label = context.dataset.label || "";

                            if (label) {
                                label += ": ";
                            }

                            if (context.parsed.y !== null) {
                                label += context.parsed.y.toLocaleString(undefined, { style: "percent", maximumFractionDigits: options.maxDecimalPlaces });
                            }

                            return label;
                        }
                    }
                },

                barValueLabels: {
                    ...this.options.plugins?.barValueLabels,
                    formatter: (value: string) => {
                        const num = Number(value);

                        if (isNaN(num)) {
                            // If it's not a number, return the original value.
                            return value;
                        }
                        else {
                            return num.toLocaleString(undefined, { style: "percent", maximumFractionDigits: options.maxDecimalPlaces });
                        }
                    }
                }
            }
        });
    }

    withoutLegend(): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            plugins: {
                ...this.options.plugins,
                legend: {
                    // Disable the legend for the bar chart and don't copy previous settings.
                    display: false
                }
            }
        });
    }

    withResponsiveSizing(): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            responsive: true,
            maintainAspectRatio: false
        });
    }

    withoutTooltips(): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            plugins: {
                ...this.options.plugins,
                tooltip: {
                    // Disable the tooltip for the bar chart and don't copy previous settings.
                    enabled: false
                }
            }
        });
    }

    withBarValueLabels(options?: IBarValueLabelsPluginOptions): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            plugins: {
                ...this.options.plugins,
                barValueLabels: {
                    ...this.options.plugins?.barValueLabels,
                    ...options
                }
            }
        });
    }
}

export class LineChartDataBuilder {
    private seriesNameOrderBy: ((seriesNameA: string, seriesNameB: string) => number) | undefined;

    private constructor(private dataPoints: ChartNumericDataPointBag[]) { }

    public static createFromData(dataPoints: ChartNumericDataPointBag[]): LineChartDataBuilder {
        return new LineChartDataBuilder(dataPoints);
    }

    /**
     * Sets the order of the series in the chart to match the supplied array. Any series not in the array will be placed at the end.
     *
     * This is useful when you want to control the order of the series in the chart
     * instead of relying on the order of the source array elements.
     *
     * For line charts, this affects which line is on top of others.
     */
    public withSeriesOrder(orderBy: string[]): LineChartDataBuilder;

    /**
     * Sets the order of the series in the chart based on a comparator function.
     *
     * This is useful when you want to control the order of the series in the chart
     * instead of relying on the order of the source array elements.
     *
     * For line charts, this affects which line is on top of others.
     */
    public withSeriesOrder(orderBy: ((seriesNameA: string, seriesNameB: string) => number)): LineChartDataBuilder;

    public withSeriesOrder(orderBy: string[] | ((seriesNameA: string, seriesNameB: string) => number)): LineChartDataBuilder {
        if (Array.isArray(orderBy)) {
            const orderMap = new Map<string, number>(
                orderBy.map((value, index) => [value, index])
            );

            this.seriesNameOrderBy = (a: string, b: string): number => {
                const indexA = orderMap.has(a) ? orderMap.get(a)! : Number.MAX_SAFE_INTEGER;
                const indexB = orderMap.has(b) ? orderMap.get(b)! : Number.MAX_SAFE_INTEGER;

                if (indexA < indexB) {
                    return -1;
                }

                if (indexA > indexB) {
                    return 1;
                }

                return 0;
            };
        }
        else {
            this.seriesNameOrderBy = orderBy;
        }
        return this;
    }

    public build(): ChartData<"line"> {
        const dataPointsEnumerable = Enumerable.from(this.dataPoints);

        const labels = dataPointsEnumerable
            .select(d => d.label)
            .ofType((d): d is string => !isNullish(d))
            .distinct()
            .toArray();

        let groupedBySeries = dataPointsEnumerable
            .groupBy(d => d.seriesName)
            .ofType((group): group is GroupedEnumerable<string, ChartNumericDataPointBag> => {
                return !isNullish(group.key);
            });

        if (this.seriesNameOrderBy) {
            groupedBySeries = groupedBySeries
                .orderBy(group => group.key, this.seriesNameOrderBy);
        }

        return {
            labels,
            datasets: groupedBySeries
                .select<ChartDataset<"line", (number | Point | null)[]>>(group => {
                    // Convert to an array once so the grouped enumerable isn't iterated multiple times.
                    const groupArray = group.toArray();
                    const seriesDataColors = Enumerable.from(groupArray).select(d => d.color ?? undefined).toArray();
                    const dataDictionary = Enumerable.from(groupArray).toDictionary(d => d.label, d => d.value);

                    // Data points are ordered by the labels,
                    // and if a label is missing in the data,
                    // it will use the last value.
                    let lastValue = 0;
                    const data = Enumerable.from(labels)
                        .select(label => {
                            if (dataDictionary.has(label)) {
                                lastValue = dataDictionary.get(label)!;
                            }

                            return lastValue;
                        })
                        .toArray();

                    return {
                        label: group.key,
                        data,
                        backgroundColor: seriesDataColors,
                        borderColor: seriesDataColors
                    };
                })
                .toArray()
        };
    }
}

export class LineChartOptionsBuilder {
    private constructor(private options: ChartOptions<"line">) { }

    public static create(): LineChartOptionsBuilder {
        return new LineChartOptionsBuilder({});
    }

    public static createFrom(options: ChartOptions<"line"> = {}): LineChartOptionsBuilder {
        return new LineChartOptionsBuilder(options);
    }

    public build(): ChartOptions<"line"> {
        return { ...this.options };
    }

    withXAxisAsDate(
        options: {
            timeUnit: "millisecond" | "second" | "minute" | "hour" | "day" | "week" | "month" | "quarter" | "year";
        } = {
                timeUnit: "day"
            }
    ): LineChartOptionsBuilder {
        return new LineChartOptionsBuilder({
            ...this.options,
            scales: {
                ...this.options.scales,
                x: {
                    type: "time",
                    time: {
                        unit: options.timeUnit
                    }
                }
            }
        });
    }

    /**
     * Formats the value as a percentage (with a '%' sign) in the Y-axis ticks and tooltips.
     *
     * This assumes the value is a fractional number representing a percentage (e.g., 0.5 for 50%).
     */
    withYAxisAsPercent(
        options: {
            maxDecimalPlaces: number | undefined;
        } = {
                maxDecimalPlaces: undefined
            }
    ): LineChartOptionsBuilder {
        return new LineChartOptionsBuilder({
            ...this.options,
            scales: {
                ...this.options.scales,
                y: {
                    ...this.options.scales?.y,
                    ticks: {
                        ...this.options.scales?.y?.ticks,
                        callback(tickValue, _index, _ticks) {
                            if (typeof tickValue !== "number") {
                                tickValue = Number(tickValue);
                            }

                            if (isNaN(tickValue)) {
                                // If it's not a number, return the original value.
                                return `${tickValue}`;
                            }
                            else {
                                return tickValue.toLocaleString(undefined, { style: "percent", maximumFractionDigits: options.maxDecimalPlaces });
                            }
                        },
                    }
                }
            },
            plugins: {
                ...this.options.plugins,
                tooltip: {
                    ...this.options.plugins?.tooltip,
                    callbacks: {
                        ...this.options.plugins?.tooltip?.callbacks,
                        label: function (context) {
                            let label = context.dataset.label || "";

                            if (label) {
                                label += ": ";
                            }

                            if (context.parsed.y !== null) {
                                label += context.parsed.y.toLocaleString(undefined, { style: "percent", maximumFractionDigits: options.maxDecimalPlaces });
                            }

                            return label;
                        }
                    }
                }
            }
        });
    }

    withResponsiveSizing(): LineChartOptionsBuilder {
        return new LineChartOptionsBuilder({
            ...this.options,
            responsive: true,
            maintainAspectRatio: false
        });
    }
}

/**
 * Builds a cumulative chart series from an enumerable of RockDateTime objects.
 *
 * @param dateEnumerable An enumerable of RockDateTime objects or null/undefined values.
 * @param options An object containing options for the series.
 * @returns An array of ChartNumericDateTimeDataPoint objects representing the cumulative series.
 */
export function buildCumulativeRates(
    dateEnumerable: Enumerable<RockDateTime>,
    options: {
        /**
         * The name of the series to use in the chart.
         */
        seriesName: string;
        /**
         * The color to use for the series in the chart.
         */
        color: string;
        /**
         * The total count of recipients or messages to use for calculating the percentage.
         */
        totalCount: number;
        /**
         * Transforms the date after the ISO date string is parsed.
         * This is useful for grouping dates into weeks or months.
         */
        dateTransformer?: ((d: RockDateTime) => RockDateTime) | null | undefined;
        /**
         * Formats the date for display in the chart.
         * Defaults to outputting an ISO string if not provided.
         */
        dateFormatter?: ((d: RockDateTime) => string) | null | undefined;
    }
): Enumerable<ChartNumericDataPointBag & { rockDateTime: RockDateTime; }> {
    // Ensure the date transformer is defined, defaulting to the identity function if not provided.
    const dateTransformer = options.dateTransformer ?? (d => d);

    // Ensure the date formatter is defined, defaulting to outputting an ISO string if not provided.
    const dateFormatter = options.dateFormatter ?? (d => d.toISOString());

    return dateEnumerable
        .select(dateTransformer)
        .groupBy(date => date.toMilliseconds())
        .orderBy(group => group.key)
        .scan(
            { count: 0, dataPoint: undefined as unknown as ChartNumericDataPointBag & { rockDateTime: RockDateTime; } },
            (state, group) => {
                const date = group.first();
                const groupCount = group.count();
                const count = state.count + groupCount;

                const dataPoint: ChartNumericDataPointBag & { rockDateTime: RockDateTime; } = {
                    seriesName: options.seriesName,
                    color: options.color,
                    label: dateFormatter(date),
                    value: count / options.totalCount,
                    rockDateTime: date
                };

                return {
                    count,
                    dataPoint
                };
            },
        )
        .select(state => state.dataPoint);
}
