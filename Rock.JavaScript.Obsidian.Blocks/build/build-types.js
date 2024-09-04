const DeclarationBuilder = require("../../Rock.JavaScript.Obsidian/build/build-tools").DeclarationBuilder;

async function main() {
    const builder = new DeclarationBuilder();

    builder.arguments = ["--noEmit"];
    builder.importProject(builder.resolveProjectFile("."), ref => ref.match(/[/\\\\]Rock.JavaScript.Obsidian[/\\\\]/) == null);

    const result = await builder.build();

    process.exit(result.success ? 0 : 1);
}

main();
