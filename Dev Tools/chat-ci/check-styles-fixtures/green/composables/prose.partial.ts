// Words that carry a number after a hash, as an issue or a room is written, are not colors.
export function issueLabel(issue: number): string {
    return issue > 0 ? `Issue #${issue}` : "Issue #1234";
}
