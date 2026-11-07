// A chat file reading a member its type does not declare: the gate fails on it.
type Box = { organizationName?: string | null };

export function title(box: Box): string {
    return box.organizationTitle ?? "";
}
