export * from "chart.js/auto";
export type { AnnotationPluginOptions } from "chartjs-plugin-annotation";

import { Chart } from "chart.js/auto";
import annotationPlugin from "chartjs-plugin-annotation";

Chart.register(annotationPlugin);
