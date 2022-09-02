import { argv } from "process";
import * as ts from "typescript";
import { createSolutionBuilderHost } from "./typescript";

/**
 * Perform a one-time build of the project.
 *
 * @param project The path to the project to be built.
 */
function buildProject(project: string): void {
    const builder = ts.createSolutionBuilder(createSolutionBuilderHost(), [project], {});
    const result = builder.build();

    ts.sys.exit(result);
}

buildProject(argv[2]);
