// A chat file with nothing wrong in it that the project's include never reaches: the gate fails,
// because a chat file the type check never read is a chat file nobody checked.
type Box = { organizationName?: string | null };

export function title(box: Box): string {
    return box.organizationName ?? "";
}
