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

import { ChartData, ChartDataset, ChartOptions, Point } from "@Obsidian/Libs/chart";
import { Enumerable, GroupedEnumerable } from "@Obsidian/Utility/linq";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { isNullish } from "@Obsidian/Utility/util";
import { ChartNumericDataPointBag } from "@Obsidian/ViewModels/Reporting/chartNumericDataPointBag";

export const enum NavigationUrlKey {
    MessagePerformancePage = "MessagePerformancePage",
    ParentPage = "ParentPage"
}

export type ChartDateByNumericDataPoint = ChartNumericDataPointBag & {
    rockDateTime: RockDateTime;
};

export class BarChartDataBuilder {
    private constructor(private dataPoints: ChartNumericDataPointBag[]) { }

    public static createFromDataPoints(dataPoints: ChartNumericDataPointBag[]): BarChartDataBuilder {
        return new BarChartDataBuilder(dataPoints);
    }

    public build(): ChartData<"bar"> {
        const dataPointsEnumerable = Enumerable.from(this.dataPoints);

        const labels = dataPointsEnumerable
            .select(d => d.label)
            .ofType((d): d is string => !isNullish(d))
            .distinct()
            .toArray();

        const groupedBySeries = dataPointsEnumerable
            .groupBy(d => d.seriesName)
            .ofType((group): group is GroupedEnumerable<string, ChartNumericDataPointBag> => {
                return !isNullish(group.key);
            })
            .orderBy(group => group.key); // Order by series name alphabetically.

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

    public static create(options: ChartOptions<"bar"> = {}): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder(options);
    }

    public build(): ChartOptions<"bar"> {
        return { ...this.options };
    }

    /**
     * Formats the value as a percentage (with a '%' sign) in the Y-axis ticks and tooltips.
     *
     * This assumes the value is already a whole number representing a percentage (e.g., 50 for 50%).
     */
    withValueFormattedAsPercentage(options: { maxDecimalPlaces: number; } = { maxDecimalPlaces: 2 }): BarChartOptionsBuilder {
        return new BarChartOptionsBuilder({
            ...this.options,
            scales: {
                ...this.options.scales,
                y: {
                    ...this.options.scales?.y,
                    ticks: {
                        ...this.options.scales?.y?.ticks,
                        callback(tickValue, _index, _ticks) {
                            return `${tickValue}%`;
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
                                label += `${truncateNumber(context.parsed.y, options.maxDecimalPlaces)}%`;
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
                            return `${truncateNumber(num, options.maxDecimalPlaces)}%`;
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
}

export class LineChartDataBuilder {
    private constructor(private dataPoints: ChartNumericDataPointBag[]) { }

    public static createFromData(dataPoints: ChartNumericDataPointBag[]): LineChartDataBuilder {
        return new LineChartDataBuilder(dataPoints);
    }

    public build(): ChartData<"line"> {
        const labels = Enumerable
            .from(this.dataPoints)
            .select(d => d.label)
            .ofType((d): d is string => !isNullish(d))
            .distinct()
            .toArray();

        const groupedBySeries = Enumerable
            .from(this.dataPoints)
            .groupBy(d => d.seriesName)
            .ofType((group): group is GroupedEnumerable<string, ChartNumericDataPointBag> => {
                return !isNullish(group.key);
            })
            .orderBy(group => group.key); // Order by series name alphabetically.

        return {
            labels,
            datasets: groupedBySeries
                .select<ChartDataset<"line", (number | Point | null)[]>>(group => {
                    const seriesDataColors = group
                        .select(d => d.color ?? undefined)
                        .toArray();

                    return {
                        label: group.key,
                        data: group.select(d => d.value).toArray(),
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

    /**
     * Formats the value as a percentage (with a '%' sign) in the Y-axis ticks and tooltips.
     *
     * This assumes the value is already a whole number representing a percentage (e.g., 50 for 50%).
     */
    withValueFormattedAsPercentage(options: { maxDecimalPlaces: number; } = { maxDecimalPlaces: 2 }): LineChartOptionsBuilder {
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
                                return `${truncateNumber(tickValue, options.maxDecimalPlaces)}%`;
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
                                label += `${truncateNumber(context.parsed.y, options.maxDecimalPlaces)}%`;
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

function truncateNumber(value: number, decimalPlaces: number): number {
    const factor = 10 ** decimalPlaces;
    return Math.trunc(value * factor) / factor;
}

export type ConversionGoalStatus = "Not Tracked" | "Pending" | "Missed" | "Achieved";

export type UnsubscribeInfo = {
    unsubscribesFromFlow?: number | null | undefined;
    unsubscribesFromAll?: number | null | undefined;
    unsubscribesFromOther?: number | null | undefined;
};