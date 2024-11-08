const DeclarationBuilder = require("../../Rock.JavaScript.Obsidian/build/build-tools").DeclarationBuilder;

const mainProject = "Framework";

async function main() {
    const builder = new DeclarationBuilder();
    builder.importProject(builder.resolveProjectFile(mainProject));

    const result = await builder.build();

    process.exit(result.success ? 0 : 1);
}

main();
