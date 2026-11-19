#!/bin/bash
# Hook: Block destructive bash commands before execution.
# Called by Claude Code as a PreToolUse hook on Bash.
# Exit 0 = allow, Exit 2 = block (stderr shown to user).

INPUT=$(cat)

# The JSON string value of "command", still JSON-escaped. Stops at the closing quote,
# so the fields that follow it (description, timeout) are not part of the command.
RAW=$(printf '%s' "$INPUT" | tr -d '\r\n' | sed -nE 's/.*"command"[[:space:]]*:[[:space:]]*"((\\.|[^"\\])*)".*/\1/p')

# Split the command into its simple commands, one per line, so that every rule below
# looks at one command at a time. Before this, a rule matched across the whole string,
# so "git push && gh api ... -f body=x" was read as a force push.
#   - Splits on ; | & && || and newlines. A nested command ($( ), ` `, ( )) becomes
#     its own line, with its own quoting, and the outer command continues around it.
#   - Does not split inside quotes. Quotes are removed, and whitespace inside a quoted
#     argument becomes \001, so the argument stays one word.
#   - For commands that never run their quoted text (git commit, gh...), that is all:
#     a commit message or a comment body cannot look like a command. For every other
#     command the quoted text is also checked as words, because the command could run
#     it (bash -c, eval, ssh, python -c, watch...).
#   - Here-document text that only feeds cat is data and is skipped. Any other
#     here-document text is checked line by line as it is written.
#   - If the quoting does not add up (a quote or nested command is still open at the
#     end), the whole command is also checked as one line, the way it used to be.
CMD=$(printf '%s' "$RAW" | LC_ALL=C awk '
function hexval(ch) { return index("0123456789abcdef", tolower(ch)) - 1 }
# Undo JSON string escaping.
function unescape(str,    out, j, len, c, d) {
    out = ""; len = length(str)
    for (j = 1; j <= len; j++) {
        c = substr(str, j, 1)
        if (c != "\\" || j == len) { out = out c; continue }
        d = substr(str, ++j, 1)
        if (d == "n") { out = out "\n" }
        else if (d == "t") { out = out "\t" }
        else if (d == "r" || d == "b" || d == "f") { out = out " " }
        else if (d == "u") {
            out = out sprintf("%c", hexval(substr(str, j + 3, 1)) * 16 + hexval(substr(str, j + 4, 1)))
            j += 4
        }
        else { out = out d }
    }
    return out
}
# Report whether a command only stores or shows its quoted text and never runs it:
# gh, grep, rg, and git commit, tag, merge, notes, log and grep.
function text_only(line) {
    gsub(/^ +/, "", line)
    while (line ~ /^[A-Za-z_][A-Za-z0-9_]*=[^ ]* /) { sub(/^[^ ]* /, "", line) }
    sub(/^(do|then|else|elif|if|while|!|[{]) /, "", line)
    if (line ~ /^(gh|grep|rg)( |$)/) { return 1 }
    return line ~ /^git( -[^ ]+( [^- ][^ ]*)?)* (commit|tag|merge|notes|log|grep)( |$)/
}
# Report whether every command around the current nested command is text-only.
function inside_text_only(    k) {
    for (k = depth - 1; k >= 0; k--) {
        if (!text_only(buf[k])) { return 0 }
    }
    return 1
}
# Print the command collected at the current nesting level, then start a new one.
# Any command that is not text-only could run its quoted text (bash -c, eval, ssh,
# python -c, watch, git rebase --exec...), so that text is printed again as words.
function emit(    line, words) {
    line = buf[depth]
    buf[depth] = ""
    gsub(/^ +| +$/, "", line)
    if (line == "") { return }
    print line
    if (!text_only(line)) {
        words = line
        gsub(SOFT, " ", words)
        gsub(/[ ()`"\047;|&]+/, " ", words)
        if (words != line) { print words }
    }
}
# Print text as one line without working out its quoting: quotes and separators
# become spaces, so anything in it that looks like a blocked command is blocked.
function emit_raw(line) {
    gsub(/[ \t\n()`"\047;|&]+/, " ", line)
    gsub(/^ | $/, "", line)
    if (line != "") { print line }
}
# A nested command starts: it gets its own line and its own quoting.
function open(kind) { depth++; buf[depth] = ""; closer[depth] = kind; saved[depth] = quote; quote = "" }
# A nested command ends: go back to the outer command and the quoting it was in.
function close_level() {
    emit()
    if (depth > 0) { quote = saved[depth]; depth--; buf[depth] = buf[depth] "_" }
}
# At a << operator: remember the word that ends the here-document.
function heredoc_start(    word, c, nl, rest) {
    i += 2
    pending++
    hd_strip[pending] = 0
    if (substr(s, i, 1) == "-") { hd_strip[pending] = 1; i++ }
    while (substr(s, i, 1) ~ /[ \t]/) { i++ }
    word = ""; hd_quoted[pending] = 0
    while (i <= n) {
        c = substr(s, i, 1)
        if (c ~ /[ \t\n;&|()<>]/) { break }
        if (c == "\047" || c == "\"" || c == "\\") { hd_quoted[pending] = 1 } else { word = word c }
        i++
    }
    i--
    hd_word[pending] = word
    nl = index(substr(s, i + 1), "\n")
    rest = (nl > 0) ? substr(s, i + 1, nl - 1) : substr(s, i + 1)
    # Text that cat only prints or saves is data. Piped onward, or printed for a
    # command around it that is not text-only (bash -c "$(cat <<EOF ...)"), it may be run.
    hd_data[pending] = (buf[depth] ~ /^ ?cat( |$)/ && index(rest, "|") == 0 && inside_text_only())
}
# At the end of the line that started them: read past the here-document bodies.
function heredoc_bodies(    k, nl, line, word) {
    for (k = 1; k <= pending; k++) {
        while (i < n) {
            nl = index(substr(s, i + 1), "\n")
            line = (nl > 0) ? substr(s, i + 1, nl - 1) : substr(s, i + 1)
            i += (nl > 0) ? nl : length(line)
            word = line
            if (hd_strip[k]) { sub(/^\t+/, "", word) }
            if (word == hd_word[k]) { break }
            if (!hd_data[k] || (!hd_quoted[k] && line ~ /\$\(|`/)) { emit_raw(line) }
        }
    }
    pending = 0
}
BEGIN { SOFT = sprintf("%c", 1) }
{ text = text $0 "\n" }
END {
    s = unescape(text); n = length(s)
    depth = 0; buf[0] = ""; quote = ""; pending = 0
    for (i = 1; i <= n; i++) {
        c = substr(s, i, 1); nx = substr(s, i + 1, 1); pv = substr(s, i - 1, 1)

        if (quote == "\047") {
            if (c == "\047") { quote = "" }
            else { buf[depth] = buf[depth] (c ~ /[ \t\n]/ ? SOFT : c) }
            continue
        }
        if (quote == "\"") {
            if (c == "\\") { buf[depth] = buf[depth] nx; i++ }
            else if (c == "\"") { quote = "" }
            else if (c == "$" && nx == "(") { i++; open(")") }
            else if (c == "`") { open("`") }
            else { buf[depth] = buf[depth] (c ~ /[ \t\n]/ ? SOFT : c) }
            continue
        }

        if (c == "\\") {
            if (nx != "\n") { buf[depth] = buf[depth] nx }
            i++
        }
        else if (c == "\047" || c == "\"") { quote = c }
        else if (c == "#" && (buf[depth] == "" || buf[depth] ~ / $/)) {
            while (i < n && substr(s, i + 1, 1) != "\n") { i++ }
        }
        else if (c == "<" && nx == "<" && substr(s, i + 2, 1) == "<") { buf[depth] = buf[depth] " <<< "; i += 2 }
        else if (c == "<" && nx == "<") { buf[depth] = buf[depth] " "; heredoc_start() }
        # A redirect is its own word, so "git push -f>out" still ends its flag at -f.
        else if (c == "<" || c == ">") { buf[depth] = buf[depth] " " c " " }
        else if (c == "`") {
            if (depth > 0 && closer[depth] == "`") { close_level() } else { open("`") }
        }
        else if (c == "(") { open(")") }
        else if (c == ")") { close_level() }
        else if (c == "\n") { emit(); if (pending > 0) { heredoc_bodies() } }
        else if (c == ";") { emit() }
        else if (c == "&" && (pv == ">" || nx == ">")) { buf[depth] = buf[depth] c }
        else if (c == "|" && pv == ">") { buf[depth] = buf[depth] c }
        else if (c == "&" || c == "|") { emit() }
        else if (c == " " || c == "\t") {
            if (buf[depth] !~ / $/) { buf[depth] = buf[depth] " " }
        }
        else { buf[depth] = buf[depth] c }
    }
    if (quote != "" || depth > 0) { emit_raw(s) }
    while (depth > 0) { close_level() }
    emit()
}')

# If the command could not be read, check the raw input as words rather than letting it through.
if [ -z "$RAW" ]; then
  CMD=$(printf '%s' "$INPUT" | tr '"\\(){}[],:;|&\r\n' ' ')
fi

# "git", with any global options (-C <path>, -c <name>=<value>, --no-pager...), up to the subcommand.
GIT='(^|[[:space:]])([^[:space:]]*[/\\])?[Gg][Ii][Tt](\.[Ee][Xx][Ee])?([[:space:]]+-[^[:space:]]+([[:space:]]+[^-[:space:]][^[:space:]]*)?)*[[:space:]]+'

# --- Git: force push ---
# Only the words of the push command itself count: --force, --force-with-lease,
# --force-if-includes, a short option group containing f (-f, -uf), or a +refspec.
if echo "$CMD" | grep -qE "${GIT}push([[:space:]].*)?[[:space:]](--force[^[:space:]]*|-[A-Za-z]*f[A-Za-z]*|\+[^[:space:]]+)([[:space:]]|\$)"; then
  echo "BLOCKED: Force push can overwrite remote history. Push normally or ask the user to confirm." >&2
  exit 2
fi

# --- Git: hard reset ---
if echo "$CMD" | grep -qE "${GIT}reset[[:space:]]+(.*[[:space:]])?--hard"; then
  echo "BLOCKED: git reset --hard discards all uncommitted changes. Stage or stash first." >&2
  exit 2
fi

# --- Git: discard all working changes ---
if echo "$CMD" | grep -qE "${GIT}checkout[[:space:]]+(--[[:space:]]+)?\.[[:space:]]*([0-9]*[[:space:]]*[<>].*)?\$"; then
  echo "BLOCKED: git checkout . discards all unstaged changes. Be specific about which files to restore." >&2
  exit 2
fi

# --- Git: force-delete branch ---
# Case-sensitive on purpose: -D force-deletes, but lowercase -d is the safe delete this message recommends.
if echo "$CMD" | grep -qE 'git\s+branch\s+-D\s'; then
  echo "BLOCKED: git branch -D force-deletes a branch even if unmerged. Use -d (lowercase) for safe delete." >&2
  exit 2
fi

# --- Git: clean untracked files ---
if echo "$CMD" | grep -qE "${GIT}clean[[:space:]]+(.*[[:space:]])?(-[A-Za-z]*f|--force)"; then
  echo "BLOCKED: git clean -f permanently deletes untracked files. Run git clean -n first to preview." >&2
  exit 2
fi

# --- Git: restore all files (discard all changes) ---
if echo "$CMD" | grep -qE "${GIT}restore[[:space:]]+(--[[:space:]]+)?\.[[:space:]]*([0-9]*[[:space:]]*[<>].*)?\$"; then
  echo "BLOCKED: git restore . discards all working tree changes. Be specific about which files to restore." >&2
  exit 2
fi

# --- Git: rebase onto main/develop ---
if echo "$CMD" | grep -qE "${GIT}rebase[[:space:]]+(.*[^[:alnum:]_-])?(main|master|develop)([^[:alnum:]_-]|\$)"; then
  echo "BLOCKED: Rebasing onto main/develop rewrites commit history. Merge instead, or ask the user to confirm." >&2
  exit 2
fi

# --- Git: amend a commit ---
if echo "$CMD" | grep -qE "${GIT}commit[[:space:]]+(.*[[:space:]])?--amend([[:space:]]|\$)"; then
  echo "BLOCKED: --amend rewrites the last commit. Create a new commit instead, or ask the user to confirm." >&2
  exit 2
fi

# --- Git: skip hooks ---
if echo "$CMD" | grep -qE "${GIT}([^[:space:]]+[[:space:]]+)+--no-verify([[:space:]]|\$)"; then
  echo "BLOCKED: --no-verify skips commit hooks (message format validation). Fix the issue instead of bypassing." >&2
  exit 2
fi

# --- Git: stash drop/clear ---
if echo "$CMD" | grep -qE "${GIT}stash[[:space:]]+(drop|clear)([[:space:]]|\$)"; then
  echo "BLOCKED: git stash drop/clear permanently deletes stashed work. Ask the user to confirm." >&2
  exit 2
fi

exit 0
