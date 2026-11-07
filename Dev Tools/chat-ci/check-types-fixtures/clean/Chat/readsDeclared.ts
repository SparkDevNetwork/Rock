// A chat file reading only what its type declares: the gate passes it.
type Box = { organizationName?: string | null };

export function title(box: Box): string {
    return box.organizationName ?? "";
}
