import { Enumerable } from "@Obsidian/Utility/linq";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

// #region Types

export type Series = {
    label: string;
    data: (number | null)[];
    color?: string | undefined;
};

export type LineSeries = Series & {
    isUnfilled?: boolean | undefined;
    isLinear?: boolean | undefined;
    lineStyle?: LineStyle | undefined;
};

export const LineStyle = {
    Solid: "solid",
    Dashed: "dashed",
    Dotted: "dotted"
} as const;

export const LineStyleDescription: Record<string, string> = {
    "solid": "Solid",
    "dashed": "Dashed",
    "dotted": "Dotted"
};

export type LineStyle = typeof LineStyle[keyof typeof LineStyle];

export const LabelDateFormat = {
    Auto: "auto",
    Day: "day",
    Week: "week",
    Month: "month",
    Year: "year"
} as const;

export const LabelDateFormatDescription: Record<string, string> = {
    "auto": "Auto",

    "day": "Day",

    "week": "Week",

    "month": "Month",

    "year": "Year"
};

export type LabelDateFormat = typeof LabelDateFormat[keyof typeof LabelDateFormat];

export const LegendPosition = {
    Top: "top",
    Bottom: "bottom",
    Left: "left",
    Right: "right"
} as const;

export const LegendPositionDescription: Record<string, string> = {
    "top": "Top",
    "bottom": "Bottom",
    "left": "Left",
    "right": "Right"
};

export type LegendPosition = typeof LegendPosition[keyof typeof LegendPosition];

export const LegendAlign = {
    Start: "start",
    Center: "center",
    End: "end"
} as const;

export const LegendAlignDescription: Record<string, string> = {
    "start": "Start",
    "center": "Center",
    "end": "End"
};

export type LegendAlign = typeof LegendAlign[keyof typeof LegendAlign];

export type Legend = {
    hidden?: boolean | undefined;
    position?: LegendPosition | undefined;
    align?: LegendAlign | undefined;
};

export type TimeInput = string | number | RockDateTime;

export type XYPoint = {
    x: TimeInput;
    y: number | null;
};

export type AlignDataResult = {
    labels: string[];
    values: (number | null)[][];
};

// #endregion Types

// #region Utils

function asRockDateTime(t: TimeInput): RockDateTime {
    if (typeof t === "string") {
        const rockDateTime = RockDateTime.parseISO(t);
        if (!rockDateTime) {
            throw new Error(`Bad date: ${t}`);
        }
        return rockDateTime;
    }
    else if (typeof t === "number") {
        const rockDateTime = RockDateTime.fromMilliseconds(t);
        if (!rockDateTime) {
            throw new Error(`Bad date: ${t}`);
        }
        return rockDateTime;
    }
    else {
        return t;
    }
}

export class XYPointEnumerable extends Enumerable<XYPoint> {
    /**
     * Creates an XYPointEnumerable from a regular iterable (e.g., Array, Set).
     * @param iterableFactory - A factory function that produces an Iterable<XYPoint>.
     */
    constructor(iterableFactory: () => Iterable<XYPoint>) {
        super(iterableFactory);
    }

    /**
     * Creates an XYPointEnumerable from a regular iterable (e.g., Array, Set).
     * @param iterable - An iterable to create the XYPointEnumerable from.
     * @returns A new XYPointEnumerable instance.
     */
    static fromData(iterable: Iterable<XYPoint>): XYPointEnumerable;

    /**
     * Creates an XYPointEnumerable from a generator function.
     * @param generator - A function that produces an IterableIterator.
     * @returns A new XYPointEnumerable instance.
     */
    static fromData(generator: () => IterableIterator<XYPoint>): XYPointEnumerable;

    /**
     * Creates an XYPointEnumerable from a regular iterable (e.g., Array, Set) or a generator function.
     * @param source - Either an iterable or a generator function.
     * @returns A new XYPointEnumerable instance.
     */
    static fromData(source: Iterable<XYPoint> | (() => IterableIterator<XYPoint>)): XYPointEnumerable {
        if (typeof source === "function") {
            return new XYPointEnumerable(source); // Handle generator factory
        }
        else {
            return new XYPointEnumerable(() => source); // Handle regular iterable
        }
    }

    /**
     * Selects points as a decimal percentage [0-1] of the total.
     *
     * @param total The total value to calculate percentages against.
     * @returns A new XYPointEnumerable instance with points represented as percentages.
     *
     * @example
     * const data = XYPointEnumerable.fromData([
     *     { x: "2023-01-01T00:00:00", y: 3 },
     *     { x: "2023-01-02T00:00:00", y: 6 },
     *     { x: "2023-01-03T00:00:00", y: 10 },
     * ]);
     * const total = 10;
     * const percentages = data.selectAsPercent(total).toArray();
     * console.log(percentages);
     * // [
     * //    { x: "2023-01-01T00:00:00", y: 0.3 },
     * //    { x: "2023-01-02T00:00:00", y: 0.6 },
     * //    { x: "2023-01-03T00:00:00", y: 1.0 },
     * // ];
     */
    selectAsDecimalPercentage(total: number): XYPointEnumerable {
        if (total === 0) {
            return XYPointEnumerable.fromData(
                this.select(point => {
                    return {
                        x: point.x,
                        y: typeof point.y === "number" ? 0 : null
                    };
                })
            );
        }

        return XYPointEnumerable.fromData(
            this.select(point => {
                return {
                    x: point.x,
                    y: typeof point.y === "number" ? (point.y / total) : null
                };
            })
        );
    }

    /**
     * Squish data points by day, grouping them by the date part of the timestamp.
     * @example
     * const data = XYPointEnumerable.fromData([
     *     { x: "2023-01-01T09:00:00", y: 0 },
     *     { x: "2023-01-01T09:00:00", y: 1 },
     *     { x: "2023-01-01T10:00:00", y: 2 },
     *     { x: "2023-01-02T11:00:00", y: 3 },
     *     { x: "2023-01-03T12:00:00", y: 4 },
     * ]);
     * const squished = data.squishByDate().toArray();
     * console.log(squished);
     * // [
     * //    { x: "2023-01-01T00:00:00", y: 3 },
     * //    { x: "2023-01-02T00:00:00", y: 3 },
     * //    { x: "2023-01-03T00:00:00", y: 4 },
     * // ];
     */
    selectSquishedByDate(): XYPointEnumerable {
        return XYPointEnumerable.fromData(this
            .groupBy(point => {
                const rockDateTime = asRockDateTime(point.x);
                return rockDateTime.date.toISOString();
            })
            .select<XYPoint>(group => {
                const materializedGroup = group.toArray();

                const allNulls = Enumerable.from(materializedGroup).all(p => p.y === null);

                if (allNulls) {
                    return {
                        x: group.key,
                        y: null
                    };
                }

                return {
                    x: group.key,
                    y: Enumerable.from(materializedGroup).sum(point => point.y ?? 0)
                };
            }));
    }

    /**
     * Squish data points by date and time.
     * @example
     * const data = XYPointEnumerable.fromData([
     *     { x: "2023-01-01T09:00:00", y: 0 },
     *     { x: "2023-01-01T09:00:00", y: 1 },
     *     { x: "2023-01-01T10:00:00", y: 2 },
     *     { x: "2023-01-02T11:00:00", y: 3 },
     *     { x: "2023-01-03T12:00:00", y: 4 },
     * ]);
     * const squished = data.squishByDate().toArray();
     * console.log(squished);
     * // [
     * //    { x: "2023-01-01T09:00:00", y: 1 },
     * //    { x: "2023-01-01T10:00:00", y: 2 },
     * //    { x: "2023-01-02T11:00:00", y: 3 },
     * //    { x: "2023-01-03T12:00:00", y: 4 },
     * // ];
     */
    selectSquishedByDateTime(): XYPointEnumerable {
        return XYPointEnumerable.fromData(this
            .groupBy(point => {
                const rockDateTime = asRockDateTime(point.x);
                return rockDateTime.toISOString();
            })
            .select(group => {
                const materializedGroup = group.toArray();

                const allNulls = Enumerable.from(materializedGroup).all(p => p.y === null);

                if (allNulls) {
                    return {
                        x: group.key,
                        y: null
                    };
                }

                return {
                    x: group.key,
                    y: Enumerable.from(materializedGroup).sum(point => point.y ?? 0)
                };
            })
        );
    }

    /**
     * Select accumulated data points, where each point's Y value is the cumulative sum of all previous Y values.
     *
     * @example
     * const data = XYPointEnumerable.fromData([
     *     { x: "2023-01-01T00:00:00", y: 3 },
     *     { x: "2023-01-02T00:00:00", y: 3 },
     *     { x: "2023-01-03T00:00:00", y: 4 },
     * ]);
     * const accumulated = data.selectAccumulated().toArray();
     * console.log(accumulated);
     * // [
     * //    { x: "2023-01-01T00:00:00", y: 3 },
     * //    { x: "2023-01-02T00:00:00", y: 6 },
     * //    { x: "2023-01-03T00:00:00", y: 10 },
     * // ];
     */
    selectAccumulated(): XYPointEnumerable {
        return XYPointEnumerable.fromData(
            this.scan<XYPoint>(
                { x: 0, y: null },
                (prevPoint, point, _i) => {
                    if (typeof point.y === "number") {
                        return {
                            x: point.x,
                            y: (prevPoint.y ?? 0) + point.y
                        };
                    }
                    else {
                        return {
                            x: point.x,
                            y: prevPoint.y
                        };
                    }
                }
            )
        );
    }

    selectWithStartingPoint(startingPoint: XYPoint, predicate: (a: XYPoint, b: XYPoint) => boolean): XYPointEnumerable {
        const first = this.firstOrDefault();

        if (first && predicate(first, startingPoint)) {
            return this;
        }
        else {
            return this.prepend([startingPoint]);
        }
    }
}

/**
 * Aligns data points by date, returning shared labels and per-series values (with nulls for gaps).
 */
export function alignDataPointsByDate(
    dataPoints: XYPoint[][],
    { fillWith }: { fillWith: number | null | "previousValueOrNull" | "previousValueOrZero"; } = { fillWith: null }
): AlignDataResult {
    // Normalize timestamp -> key string used for alignment
    function normalizeKey(t: TimeInput): string {
        const rockDateTime = asRockDateTime(t);
        return rockDateTime.date.toISOString();
    }

    // Build lookup maps
    function toMap(arr: XYPoint[]): Map<string, number | null> {
        const m = new Map<string, number | null>();

        for (const { x, y } of arr) {
            const key = normalizeKey(x);

            if (m.has(key)) {
                // There is already an entry for the date/time
                // so add the values together.
                const v = m.get(key) as number | null;

                if (v === null) {
                    m.set(key, y);
                }
                else {
                    m.set(key, v + (y ?? 0));
                }
            }
            else {
                m.set(key, y);
            }
        }

        return m;
    }

    const mappedSeries = dataPoints.map(toMap);

    // Union + sort keys
    const allKeys = Enumerable.from(mappedSeries)
        .selectMany(s => s.keys())
        .distinct()
        .orderBy(k => k) // This should work because keys are in ISO format which are string sortable
        .toArray();

    // Values arrays aligned to keys
    return {
        labels: allKeys,
        values: mappedSeries.map(map => {
            let lastValue: number | null | undefined;
            console.log(`all keys`, allKeys);
            return Enumerable
                .from(allKeys)
                .select<number | null>(key => {
                    if (map.has(key)) {
                        const value = map.get(key)!;
                        lastValue = value;
                        return value;
                    }
                    else {
                        if (fillWith === "previousValueOrNull") {
                            return lastValue ?? null;
                        }
                        else if (fillWith === "previousValueOrZero") {
                            return lastValue ?? 0;
                        }
                        else {
                            lastValue = fillWith;
                            return lastValue;
                        }
                    }
                })
                .toArray();
        })
    };
}

// #endregion Utils