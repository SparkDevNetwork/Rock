#!/bin/bash
# Hook: Block destructive bash commands before execution.
# Called by Claude Code as a PreToolUse hook on Bash.
# Exit 0 = allow, Exit 2 = block (stderr shown to user).

INPUT=$(cat)
CMD=$(echo "$INPUT" | sed -n 's/.*"command"\s*:\s*"\(.*\)"/\1/p' | head -1)

# --- Git: force push ---
if echo "$CMD" | grep -qiE 'git\s+push\s+.*(--force|-f)\b'; then
  echo "BLOCKED: Force push can overwrite remote history. Push normally or ask the user to confirm." >&2
  exit 2
fi

# --- Git: hard reset ---
if echo "$CMD" | grep -qiE 'git\s+reset\s+--hard'; then
  echo "BLOCKED: git reset --hard discards all uncommitted changes. Stage or stash first." >&2
  exit 2
fi

# --- Git: discard all working changes ---
if echo "$CMD" | grep -qiE 'git\s+checkout\s+\.\s*$'; then
  echo "BLOCKED: git checkout . discards all unstaged changes. Be specific about which files to restore." >&2
  exit 2
fi

# --- Git: force-delete branch ---
if echo "$CMD" | grep -qiE 'git\s+branch\s+-D\s'; then
  echo "BLOCKED: git branch -D force-deletes a branch even if unmerged. Use -d (lowercase) for safe delete." >&2
  exit 2
fi

# --- Git: clean untracked files ---
if echo "$CMD" | grep -qiE 'git\s+clean\s+-f'; then
  echo "BLOCKED: git clean -f permanently deletes untracked files. Run git clean -n first to preview." >&2
  exit 2
fi

# --- Git: restore all files (discard all changes) ---
if echo "$CMD" | grep -qiE 'git\s+restore\s+\.\s*$'; then
  echo "BLOCKED: git restore . discards all working tree changes. Be specific about which files to restore." >&2
  exit 2
fi

# --- Git: rebase onto main/develop ---
if echo "$CMD" | grep -qiE 'git\s+rebase\s+.*(main|master|develop)\b'; then
  echo "BLOCKED: Rebasing onto main/develop rewrites commit history. Merge instead, or ask the user to confirm." >&2
  exit 2
fi

# --- Git: amend a commit ---
if echo "$CMD" | grep -qiE 'git\s+commit\s+.*--amend'; then
  echo "BLOCKED: --amend rewrites the last commit. Create a new commit instead, or ask the user to confirm." >&2
  exit 2
fi

# --- Git: skip hooks ---
if echo "$CMD" | grep -qiE 'git\s+.+--no-verify'; then
  echo "BLOCKED: --no-verify skips commit hooks (message format validation). Fix the issue instead of bypassing." >&2
  exit 2
fi

# --- Git: stash drop/clear ---
if echo "$CMD" | grep -qiE 'git\s+stash\s+(drop|clear)'; then
  echo "BLOCKED: git stash drop/clear permanently deletes stashed work. Ask the user to confirm." >&2
  exit 2
fi

exit 0
