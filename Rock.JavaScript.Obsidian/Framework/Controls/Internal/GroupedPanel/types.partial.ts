/**
 * Represents a single group produced by the grouping computation.
 * Both the list and board views consume this shape.
 */
export type GroupedGrid = {
    gridData: {
        rows: Record<string, unknown>[];
    };
    iconCssClass?: string | null;
    iconStyle?: string | null;
    key?: string | null;
    label?: string | null;
    order?: number | null;
    photoUrl?: string | null;
    textColorCssClass?: string | null;
    type?: string | null;
};