export * from "chart.js/auto";
export type { AnnotationPluginOptions } from "chartjs-plugin-annotation";

import { Chart } from "chart.js/auto";

import annotationPlugin from "chartjs-plugin-annotation";
Chart.register(annotationPlugin);

import "chartjs-adapter-luxon";

export { default as ChartDataLabels } from "chartjs-plugin-datalabels";
export type { Options as ChartDataLabelsOptions, LabelOptions as ChartDataLabelOptions } from "chartjs-plugin-datalabels/types/options";

