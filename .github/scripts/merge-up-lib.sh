# shellcheck shell=bash
#
# Shared merge-up helpers for private-ship.yml and private-merge-up-check.yml.
#
# Ship it and the merge-up check must merge up in exactly the same way, or the
# check would warn about the wrong things. So every merge-up step, every
# automatic resolution, and every comment explaining one comes from this one
# file instead of a copy in each workflow.
#
# Each job loads this file from the commit its workflow file came from
# ($GITHUB_SHA), not from the commit it checks out. A PR branch cut before this
# file existed still gets the helpers that match the workflow running it.
#
# Automatic resolutions. When a merge-up step conflicts, a file is resolved
# automatically only in two ways, and every time it happens the PR gets a
# comment showing both sides and what was chosen:
#   1. Version lines. In Rock.Version/AssemblySharedInfo.cs and .tt and in
#      Directory.Build.props, a conflict whose lines are ALL version numbers,
#      comments or blank lines keeps the branch being merged into. Each branch
#      owns its version; that is how people resolved these by hand in nearly
#      every merge-up since mid-2025. Any other line in a conflict leaves it a
#      conflict. Set up only inside these jobs (.git/info/attributes), so
#      nobody's local merges change.
#   2. Recorded resolutions (git rerere), kept on the mirror's `rerere-cache`
#      branch: resolutions people made in earlier merges on public, and AI
#      proposals a person approved. A step is only finished when EVERY
#      conflicted file is covered and no conflict markers are left.
#
# Requires: GH_TOKEN (the mirror), RUNNER_TEMP, GITHUB_REPOSITORY. For the
# public branches, TOKEN (the Ship Bot token).

MU_CHECK_MARKER="<!-- merge-up-check -->"
# AI resolution comments have one marker per conflict; see mu_ai_marker.
MU_PUBLIC_COMMIT_URL="https://github.com/SparkDevNetwork/Rock/commit/"

# Sets the paths every helper uses. Call it at the top of each step.
mu_init() {
  MU_GITDIR=$(git rev-parse --absolute-git-dir)
  MU_MIRROR="https://x-access-token:${GH_TOKEN}@github.com/$GITHUB_REPOSITORY.git"
  MU_PUBLIC="https://x-access-token:${TOKEN}@github.com/SparkDevNetwork/Rock.git"
  MU_CACHE="$RUNNER_TEMP/rerere"
  MU_META="$MU_CACHE/resolutions.json"
  MU_LOG_URL="${GITHUB_SERVER_URL:-https://github.com}/$GITHUB_REPOSITORY/actions/runs/$GITHUB_RUN_ID"
}

# Once per job, before any merge-up: turns on rerere, restores the recorded
# resolutions from the `rerere-cache` branch, and installs the version-line
# merge driver.
mu_setup() {
  mu_init
  git config rerere.enabled true
  # Replayed files stay marked as conflicted, so merge_step can check every
  # file was covered before it finishes a merge.
  git config rerere.autoupdate false
  # Never let git's own cleanup delete recordings mid-job.
  git config gc.auto 0

  mkdir -p "$MU_GITDIR/rr-cache" "$MU_CACHE"
  if git ls-remote --exit-code "$MU_MIRROR" refs/heads/rerere-cache > /dev/null; then
    git fetch -q "$MU_MIRROR" "+refs/heads/rerere-cache:refs/remotes/origin/rerere-cache"
    # -m: today's time on every file, so nothing restored looks old.
    git archive refs/remotes/origin/rerere-cache | tar -x -m -C "$MU_CACHE"
    if [ -d "$MU_CACHE/rr-cache" ]; then cp -R "$MU_CACHE/rr-cache/." "$MU_GITDIR/rr-cache/"; fi
  else
    echo "No rerere-cache branch yet. Starting with no recorded resolutions."
  fi
  [ -f "$MU_META" ] || echo '{}' > "$MU_META"
  [ -f "$MU_CACHE/trained.json" ] || echo '{}' > "$MU_CACHE/trained.json"
  echo "Recorded resolutions: $(ls "$MU_GITDIR/rr-cache" | wc -l)"

  mu_install_version_driver
}

# The version-line merge driver (see the top of this file).
mu_install_version_driver() {
  local driver="$RUNNER_TEMP/version-merge-driver.sh"
  cat > "$driver" <<'DRIVER'
#!/bin/bash
# git merge driver. $1 base, $2 the branch merged into (also the result),
# $3 the branch being merged, $4 the file's path.
base=$1 ours=$2 theirs=$3 path=$4
original=$(mktemp)
cp "$ours" "$original"
if git merge-file -L "${MERGE_INTO:-into}" -L base -L "${MERGE_FROM:-from}" "$ours" "$base" "$theirs"; then
  exit 0
fi

# Conflicts are left in $ours. Only take over when every line that differs
# between the two sides of every conflict is a version line, a comment or
# blank. (Git can sweep an unchanged line into a conflict when changes sit
# next to it; a line that is the same on both sides is kept either way.)
if ! awk '
  function clean(s) { sub(/^\357\273\277/, "", s); sub(/\r$/, "", s); return s }
  function allowed(s) {
    return s ~ /^[ \t]*$/ || s ~ /^[ \t]*\/\// \
      || s ~ /<(Version|InformationalVersion|AssemblyVersion|FileVersion)>/ \
      || s ~ /Assembly(File|Informational)?Version\(/ \
      || s ~ /^[ \t]*int (major|minor|build|revision) = [0-9]+;[ \t]*$/
  }
  /^<<<<<<< / { side = 1; split("", a); split("", b); next }
  side == 1 && /^=======$/ { side = 2; next }
  side && /^>>>>>>> / {
    for (l in a) if (!(l in b) && !allowed(l)) bad = 1
    for (l in b) if (!(l in a) && !allowed(l)) bad = 1
    side = 0
    next
  }
  side == 1 { a[clean($0)] = 1 }
  side == 2 { b[clean($0)] = 1 }
  END { exit bad }' "$ours"; then
  exit 1
fi

conflicted=$(mktemp)
cp "$ours" "$conflicted"
cp "$original" "$ours"
git merge-file --ours "$ours" "$base" "$theirs"

# merge_step sets MERGE_DRIVER_LOG, so the comment can show what was chosen.
# One entry per file: in a merge with several merge bases, git first calls
# this for its own internal merge of those bases, and the real merge comes
# last, so the last call wins.
if [ -n "$MERGE_DRIVER_LOG" ]; then
  mkdir -p "$MERGE_DRIVER_LOG"
  n=$(printf %s "$path" | git hash-object --stdin)
  printf '%s\n' "$path" > "$MERGE_DRIVER_LOG/$n.path"
  cp "$conflicted" "$MERGE_DRIVER_LOG/$n.conflicted"
  cp "$ours" "$MERGE_DRIVER_LOG/$n.result"
fi
exit 0
DRIVER
  chmod +x "$driver"
  git config merge.rock-version.name "Rock version lines: keep the branch being merged into"
  git config merge.rock-version.driver "\"$driver\" %O %A %B %P"
  mkdir -p "$MU_GITDIR/info"
  {
    echo "Rock.Version/AssemblySharedInfo.cs merge=rock-version"
    echo "Rock.Version/AssemblySharedInfo.tt merge=rock-version"
    echo "Directory.Build.props merge=rock-version"
  } >> "$MU_GITDIR/info/attributes"
}

# Saves to the `rerere-cache` branch. $1 is the commit message, $2 a function
# that changes the files in the checkout of that branch it is given. Several
# jobs write to the branch, so the change is re-applied to the latest version
# and pushed again (never forced) if another job saved first.
mu_save_cache() {
  local message=$1 apply=$2 out="$RUNNER_TEMP/rerere-out" attempt
  for attempt in 1 2 3; do
    rm -rf "$out"
    git init -q "$out"
    if git ls-remote --exit-code "$MU_MIRROR" refs/heads/rerere-cache > /dev/null; then
      git -C "$out" fetch -q "$MU_MIRROR" refs/heads/rerere-cache
      git -C "$out" checkout -q -b rerere-cache FETCH_HEAD
    else
      git -C "$out" checkout -q --orphan rerere-cache
    fi
    [ -f "$out/resolutions.json" ] || echo '{}' > "$out/resolutions.json"

    "$apply" "$out"

    {
      echo "# Recorded conflict resolutions"
      echo
      echo "Written by .github/workflows/private-merge-up-check.yml. Read by it and by private-ship.yml."
      echo "Never ship or merge this branch."
      echo
      echo "- rr-cache/: git rerere's recordings, one folder per conflict."
      echo "- resolutions.json: for each recording, the file, where it was learned from, and the PR."
      echo "- trained.json: the last commit learned from, per branch."
      echo "- proposed/pr-N/: AI-proposed resolutions waiting for a person to approve them."
      echo
      echo "To drop a bad recording, delete its rr-cache folder and its resolutions.json entry."
    } > "$out/README.md"

    git -C "$out" add -A
    if git -C "$out" diff --cached --quiet; then
      echo "Nothing new to save."
      return 0
    fi
    git -C "$out" -c user.name="github-actions[bot]" -c user.email="41898283+github-actions[bot]@users.noreply.github.com" \
      commit -q -m "$message"
    if git -C "$out" push -q "$MU_MIRROR" HEAD:refs/heads/rerere-cache; then
      echo "Saved to rerere-cache."
      return 0
    fi
    echo "rerere-cache changed while saving. Trying again."
  done
  echo "Couldn't save to rerere-cache."
  return 1
}

# Prints one "<from> <into>" line per merge-up, in the order to run them, for
# a PR into $1. A branch is only merged onward after every branch feeding it
# is done.
mu_merge_plan() {
  BASE=$1 python3 - <<'PY'
import json, os, sys
base = os.environ["BASE"]
chain = json.loads(os.environ["MERGE_CHAIN"])

# Every branch this change must reach.
reach, todo = {base}, [base]
while todo:
    for nxt in chain.get(todo.pop(), []):
        if nxt not in reach:
            reach.add(nxt)
            todo.append(nxt)

parents = {n: [p for p in reach if n in chain.get(p, [])] for n in reach}
done, order = [base], []
while len(done) < len(reach):
    ready = sorted(n for n in reach if n not in done and all(p in done for p in parents[n]))
    if not ready:
        sys.exit("MERGE_CHAIN contains a loop.")
    for n in ready:
        order.append(n)
        done.append(n)

for n in order:
    for p in sorted(parents[n], key=done.index):
        print(p, n)
PY
}

# One merge-up step: merges $1 into $2 (both local branches) as a real merge
# commit. Writes what happened to the folder $3 and sets STEP to:
#   clean     merged with no conflicts
#   resolved  every conflict was resolved automatically and the merge was
#             committed; the folder says how, for the explanation comment
#   conflict  at least one conflict wasn't covered; the folder lists the files,
#             what kind of conflict each is, and their conflicts. Then, by
#             MU_ON_CONFLICT:
#               abort (default)  the merge is aborted
#               keep             the merge is left in progress (the AI
#                                resolver works on it)
#               ours             each conflicted file takes the version of the
#                                branch being merged into, and the merge is
#                                committed, so later steps can still be probed
mu_merge_step() {
  local from=$1 into=$2 dir=$3 conflicted unresolved i f
  git switch -q "$into"
  # Any recording rerere uses below gets a new timestamp, which is how the
  # comment can say which recordings were replayed.
  find "$MU_GITDIR/rr-cache" -type f -exec touch -d 2000-01-01 {} +
  rm -rf "$dir"
  mkdir -p "$dir"
  echo "$from" > "$dir/from"
  echo "$into" > "$dir/into"

  export MERGE_DRIVER_LOG="$dir/driver" MERGE_FROM="$from" MERGE_INTO="$into"
  if git merge --no-ff --no-edit "$from" > "$dir/merge.log" 2>&1; then
    unset MERGE_DRIVER_LOG
    cat "$dir/merge.log"
    if [ -d "$dir/driver" ]; then
      git rev-parse HEAD > "$dir/commit"
      : > "$dir/files"
      STEP=resolved
    else
      STEP=clean
    fi
    return 0
  fi
  unset MERGE_DRIVER_LOG
  cat "$dir/merge.log"

  conflicted=$(git diff --name-only --diff-filter=U)
  unresolved=$(git rerere remaining)
  if [ -n "$conflicted" ] && [ -z "$unresolved" ]; then
    unresolved=$(printf '%s\n' "$conflicted" | xargs -d '\n' grep -lE '^(<<<<<<<|>>>>>>>) ' 2> /dev/null || true)
  fi

  if [ -n "$conflicted" ] && [ -z "$unresolved" ]; then
    printf '%s\n' "$conflicted" | xargs -d '\n' git add --
    git commit -q --no-edit
    git rev-parse HEAD > "$dir/commit"
    printf '%s\n' "$conflicted" > "$dir/files"
    find "$MU_GITDIR/rr-cache" -type f -name 'postimage*' -newermt 2001-01-01 \
      | sed -E 's#.*/rr-cache/([^/]+)/postimage#\1#' > "$dir/ids"
    echo "Resolved automatically: $(echo $conflicted)"
    STEP=resolved
    return 0
  fi

  # A merge that failed without any conflicted file (rare: for example, an
  # untracked file in the way) is reported as a conflict with no file list.
  printf '%s\n' "${unresolved:-$conflicted}" | sed '/^$/d' > "$dir/files"
  : > "$dir/resolved"
  if [ -n "$unresolved" ]; then
    printf '%s\n' "$conflicted" | grep -vxF -f "$dir/files" > "$dir/resolved" || true
  fi
  if [ -d "$dir/driver" ]; then
    cat "$dir/driver/"*.path >> "$dir/resolved"
  fi

  # For each file: its conflict as a diff for the comment, and each
  # conflicting chunk on one line, so a run without the PR can be compared
  # chunk by chunk.
  i=0
  while IFS= read -r f; do
    i=$((i + 1))
    git diff -- "$f" | head -n 200 > "$dir/diff-$i" || true
    if [ -f "$f" ]; then
      awk '/^<<<<<<< /{inside=1; chunk=""; next}
           inside && /^>>>>>>> /{inside=0; print chunk; next}
           inside{chunk = chunk $0 "\\n"}' "$f" > "$dir/chunks-$i"
    fi
  done < "$dir/files"
  mu_classify_conflicts "$from" "$into" "$dir"

  # rerere's ids for the conflicts it couldn't resolve, and a key made from
  # them. The same conflict has the same key on any PR (leftover drift hits
  # several at once), so Claude is only asked once per conflict. An id is
  # "<hash of the conflict>" or "<hash>.<variant>"; the variant only counts
  # earlier unresolved tries in this job, so the key leaves it out.
  tr '\0' '\n' < "$MU_GITDIR/MERGE_RR" > "$dir/merge-rr" 2> /dev/null || true
  cut -f1 "$dir/merge-rr" | sed 's/\..*$//' | sort | git hash-object --stdin > "$dir/key"

  case "${MU_ON_CONFLICT:-abort}" in
    keep) ;;
    ours)
      if [ -s "$dir/files" ]; then
        mu_take_into_side "$dir/files"
      else
        git merge --abort > /dev/null 2>&1 || true
      fi ;;
    *) git merge --abort ;;
  esac
  STEP=conflict
}

# Writes $3/kinds: one "<path><TAB><kind><TAB><detail>" line per conflicted
# file in $3/files, for a merge of $1 into $2 that is still in progress.
#   content       both sides changed the same lines. Recorded and AI-proposed
#                 resolutions can cover it.
#   deleted-into  the branch being merged into deleted the file and the other
#                 side changed it. Detail: "<commit><TAB><subject>" of the
#                 deletion.
#   deleted-from  the branch being merged deleted it, and the branch merged
#                 into changed it. Same detail.
#   other         a conflict with no conflicting lines to show (for example a
#                 binary file, or a file where the other side has a folder).
# Only "content" conflicts can be recorded by rerere, so only they can be
# resolved automatically or by an AI proposal.
mu_classify_conflicts() {
  local from=$1 into=$2 dir=$3 f stages deleted_by
  : > "$dir/kinds"
  while IFS= read -r f; do
    [ -n "$f" ] || continue
    stages=$(git ls-files -u -- "$f" | awk '{print $3}' | sort -u | tr -d '\n')
    case "$stages" in
      *2*3*)
        if [ -f "$f" ] && grep -qE '^<<<<<<< ' "$f"; then
          printf '%s\tcontent\t\n' "$f" >> "$dir/kinds"
        else
          printf '%s\tother\t\n' "$f" >> "$dir/kinds"
        fi ;;
      *3*)
        deleted_by=$(git log -1 --diff-filter=D --format='%h%x09%s' "$into" -- "$f" 2> /dev/null || true)
        printf '%s\tdeleted-into\t%s\n' "$f" "$deleted_by" >> "$dir/kinds" ;;
      *2*)
        deleted_by=$(git log -1 --diff-filter=D --format='%h%x09%s' "$from" -- "$f" 2> /dev/null || true)
        printf '%s\tdeleted-from\t%s\n' "$f" "$deleted_by" >> "$dir/kinds" ;;
      *)
        printf '%s\tother\t\n' "$f" >> "$dir/kinds" ;;
    esac
  done < "$dir/files"
}

# True when every conflicted file in the step folder $1 is a "content"
# conflict, the only kind a recorded or AI-proposed resolution can cover.
mu_all_content_conflicts() {
  [ -s "$1/kinds" ] && ! cut -f2 "$1/kinds" | grep -qvx content
}

# True when Claude can propose a resolution for the conflicting step folder
# $1: every file is a conflict between lines, and rerere is tracking each one,
# so an approved proposal can be recorded and replayed.
mu_can_propose() {
  mu_all_content_conflicts "$1" || return 1
  ! mu_has_protected_file "$1" || return 1
  [ -z "$(cut -f2 "$1/merge-rr" 2> /dev/null | grep -vxF -f - "$1/files" || true)" ]
}

# Prints "<pr> <status>" for the proposal that covers conflict key $1, if
# any: one queued earlier in this run ("queued"), or one saved on the
# rerere-cache branch ("pending", waiting for approval, or "failed": Claude
# couldn't propose one). A proposal on a closed PR doesn't count, so the
# conflict is proposed again for an open one.
mu_find_proposal() {
  local key=$1 file pr status
  if [ -f "$RUNNER_TEMP/propose-keys" ]; then
    pr=$(awk -v k="$key" '$1 == k { print $2; exit }' "$RUNNER_TEMP/propose-keys")
    if [ -n "$pr" ]; then
      echo "$pr queued"
      return 0
    fi
  fi
  for file in "$MU_CACHE"/proposed/pr-*/proposal.json; do
    [ -f "$file" ] || continue
    [ "$(jq -r '.key // empty' "$file")" = "$key" ] || continue
    pr=$(jq -r '.pr' "$file")
    status=$(jq -r '.status // empty' "$file")
    if [ "$(gh pr view "$pr" -R "$GITHUB_REPOSITORY" --json state -q .state 2> /dev/null)" = "OPEN" ]; then
      echo "$pr ${status:-pending}"
      return 0
    fi
  done
}

# Finishes a conflicted merge by giving each file in $1 the version of the
# branch being merged into (deleted, if that branch deleted it). Only used to
# probe the merge-up steps after a conflict; never pushed.
mu_take_into_side() {
  local f
  while IFS= read -r f; do
    [ -n "$f" ] || continue
    if git ls-files -u -- "$f" | awk '{print $3}' | grep -qx 2; then
      git checkout -q --ours -- "$f"
      git add -- "$f"
    else
      git rm -q --cached -- "$f" > /dev/null 2>&1 || true
      rm -f -- "$f"
    fi
  done < "$1"
  # Files a recorded resolution already covered in this merge stay marked as
  # conflicted (see mu_setup); keep what it wrote.
  git diff --name-only --diff-filter=U | sort -u | xargs -r -d '\n' git add --
  # Without this, git would record these stand-in resolutions as if a person
  # had made them, and later dry runs in this job would replay them.
  git rerere clear
  git -c rerere.enabled=false commit -q --no-edit
}

# Runs every step of the plan file $1, writing each step's folder under $2.
# Every target branch starts from the live public version. Sets CHAIN to clean
# or conflict. The first conflicting step's folder is $2/conflict.
#
# Like Ship it, it stops at the first conflict, unless $3 is "probe". Then it
# goes on to estimate the later steps, so a PR shows all its conflicts at once
# instead of one at a time: the conflicting step takes the side of the branch
# being merged into, and each later conflict is written to $2/probe-NN (and
# handled the same way). That's an estimate, because the real resolution may
# differ. Automatic resolutions found after the first conflict aren't kept.
mu_run_chain() {
  local plan=$1 out=$2 mode=$3 t n=0 total from into step
  mkdir -p "$out"
  git switch -q --detach
  for t in $(awk '{print $2}' "$plan" | awk '!seen[$0]++'); do
    git branch -f "$t" "public/$t"
  done
  total=$(wc -l < "$plan")
  CHAIN=clean
  while read -r from into; do
    n=$((n + 1))
    step="$out/step-$(printf '%02d' "$n")"
    if [ "$mode" = "probe" ]; then
      MU_ON_CONFLICT=ours mu_merge_step "$from" "$into" "$step"
    else
      mu_merge_step "$from" "$into" "$step"
    fi

    if [ "$STEP" = "clean" ]; then
      rm -rf "$step"
    elif [ "$STEP" = "conflict" ] && [ "$CHAIN" = "clean" ]; then
      CHAIN=conflict
      mv "$step" "$out/conflict"
      if [ "$n" -lt "$total" ]; then touch "$out/conflict/more-steps"; fi
      if [ "$mode" != "probe" ]; then break; fi
      touch "$out/conflict/probed"
    elif [ "$STEP" = "conflict" ]; then
      mv "$step" "$out/probe-$(printf '%02d' "$n")"
    elif [ "$CHAIN" = "conflict" ]; then
      rm -rf "$step"
    fi
  done < "$plan"
}

# True when the folder $1 holds any automatically resolved step.
mu_has_resolutions() {
  ls "$1"/step-*/commit > /dev/null 2>&1
}

# Says where the recorded resolution of file $2, replayed in step $1, came from.
mu_resolution_source() {
  local d=$1 f=$2 id entry pr commit source by link
  while IFS= read -r id; do
    entry=$(jq -c --arg id "$id" --arg f "$f" '.[$id] | select(. != null and .path == $f)' "$MU_META")
    [ -n "$entry" ] || continue
    pr=$(printf '%s' "$entry" | jq -r '.pr // empty')
    commit=$(printf '%s' "$entry" | jq -r '.commit // empty')
    source=$(printf '%s' "$entry" | jq -r '.source // empty')
    by=$(printf '%s' "$entry" | jq -r '.approvedBy // empty')
    if [ "$source" = "ai" ]; then
      echo "resolved with the AI-proposed resolution that \`$by\` approved on #$pr"
      return 0
    fi
    link="[\`${commit:0:7}\`]($MU_PUBLIC_COMMIT_URL$commit)"
    if [ -n "$pr" ]; then
      echo "resolved the way #$pr resolved it (in $link)"
    else
      echo "resolved the way public commit $link resolved it"
    fi
    return 0
  done < "$d/ids"
  echo "resolved by a recorded resolution (where it was learned from wasn't recorded)"
}

# Markdown explaining every automatically resolved step under $1. $2 is a URL
# prefix for the merge commits, or empty when they were never pushed.
mu_render_resolutions() {
  local root=$1 commit_url=$2 d from into commit f n
  echo "In each diff, the \`-\` lines between \`<<<<<<<\` and \`>>>>>>>\` are both sides of the conflict (first the branch being merged into, then the branch being merged), and the \`+\` lines are what was chosen."
  for d in "$root"/step-*; do
    [ -f "$d/commit" ] || continue
    from=$(cat "$d/from") into=$(cat "$d/into") commit=$(cat "$d/commit")
    echo
    if [ -n "$commit_url" ]; then
      echo "#### Merging \`$from\` into \`$into\` ([\`${commit:0:7}\`]($commit_url$commit))"
    else
      echo "#### Merging \`$from\` into \`$into\`"
    fi
    echo
    while IFS= read -r f; do
      echo "- \`$f\`: $(mu_resolution_source "$d" "$f")"
    done < "$d/files"
    for n in $(ls "$d/driver" 2> /dev/null | sed -n 's/\.path$//p'); do
      echo "- \`$(cat "$d/driver/$n.path")\`: only version lines conflicted, so \`$into\` kept its own version"
    done

    while IFS= read -r f; do
      echo
      echo "<details><summary><code>$f</code>: both sides and what was chosen</summary>"
      echo
      echo '````diff'
      git show --remerge-diff --format= "$commit" -- "$f" | head -n 300
      echo '````'
      echo
      echo "</details>"
    done < "$d/files"
    for n in $(ls "$d/driver" 2> /dev/null | sed -n 's/\.path$//p'); do
      f=$(cat "$d/driver/$n.path")
      echo
      echo "<details><summary><code>$f</code>: both sides and what was chosen</summary>"
      echo
      echo '````diff'
      echo "--- a/$f"
      echo "+++ b/$f"
      git diff --no-index --no-color -- "$d/driver/$n.conflicted" "$d/driver/$n.result" | sed -n '/^@@/,$p' | head -n 300 || true
      echo '````'
      echo
      echo "</details>"
    done
  done
}

# Cuts the comment file $1 down to GitHub's limit (65536 characters).
mu_cap_comment() {
  if [ "$(wc -c < "$1")" -gt 60000 ]; then
    head -c 60000 "$1" > "$1.tmp"
    printf '\n\n(Cut short to fit in a comment. See the [workflow log](%s).)\n' "$MU_LOG_URL" >> "$1.tmp"
    mv "$1.tmp" "$1"
  fi
}

# Creates, edits or (with no body file) deletes the one comment on PR $1 that
# carries the marker $2.
mu_set_comment() {
  local pr=$1 marker=$2 body=$3 keep="" id
  for id in $(gh api --paginate "repos/$GITHUB_REPOSITORY/issues/$pr/comments?per_page=100" \
      -q ".[] | select(.body | contains(\"$marker\")) | .id"); do
    if [ -n "$body" ] && [ -z "$keep" ]; then
      keep=$id
    else
      gh api -X DELETE "repos/$GITHUB_REPOSITORY/issues/comments/$id" > /dev/null
    fi
  done
  [ -n "$body" ] || return 0
  mu_cap_comment "$body"
  if [ -n "$keep" ]; then
    gh api -X PATCH "repos/$GITHUB_REPOSITORY/issues/comments/$keep" -F "body=@$body" > /dev/null
  else
    gh api -X POST "repos/$GITHUB_REPOSITORY/issues/$pr/comments" -F "body=@$body" > /dev/null
  fi
}

# Adds ($3 = true) or removes the label $2 on PR $1.
mu_set_label() {
  local pr=$1 label=$2 on=$3
  if [ "$on" = "true" ]; then
    gh api -X POST "repos/$GITHUB_REPOSITORY/issues/$pr/labels" -f "labels[]=$label" > /dev/null
  else
    gh api -X DELETE "repos/$GITHUB_REPOSITORY/issues/$pr/labels/$label" > /dev/null 2>&1 || true
  fi
}

# Puts the repo back to a clean, detached state.
mu_reset_repo() {
  git merge --abort > /dev/null 2>&1 || true
  git rebase --abort > /dev/null 2>&1 || true
  git reset -q --hard
  git clean -fdq
  git switch -q --detach
}

# The dry run of Ship it for PR $1 into $2, from branch $3, written under $4:
#   update-conflicts   bringing the PR up to date with public conflicted
#   pr-files           the files the PR changes
#   run/               the merge-up (run/conflict, run/step-NN)
#                      and run/probe-NN, later conflicts found by probing
#   control/           on a conflict, the same merge-up without the PR
# With MU_ON_CONFLICT=keep it stops at the first conflict with the merge still
# in progress, and skips both the probing and the run without the PR.
# Sets DRY_RUN to "skipped" when there is nothing to check (not shippable, or
# already on public), otherwise "done". It never returns non-zero for that, so
# a real failure inside still stops the job.
mu_dry_run() {
  local pr=$1 base=$2 head=$3 work=$4
  DRY_RUN=done
  rm -rf "$work"
  mkdir -p "$work"
  mu_reset_repo

  case "$base" in
    develop|release-*|hotfix-*) ;;
    *)
      echo "Targets $base, which Ship it doesn't ship. Nothing to check."
      DRY_RUN=skipped; return 0 ;;
  esac

  git fetch -q "$MU_MIRROR" "+refs/pull/$pr/head:refs/pr/$pr"
  git switch -q --detach "refs/pr/$pr"

  # 1. Bring the PR up to date, the same three ways Ship it does. Automatic
  # resolutions are only for merge-up steps: a conflict with the PR's own
  # target is the author's to resolve.
  if git merge-base --is-ancestor "public/$base" HEAD; then
    :
  elif [ "$(git rev-list --count --merges "public/$base..HEAD")" -gt 0 ]; then
    if ! git -c rerere.enabled=false merge --no-ff -m "Merge branch '$base' into $head" "public/$base" > /dev/null 2>&1; then
      git diff --name-only --diff-filter=U > "$work/update-conflicts"
      git merge --abort || true
    fi
  elif ! git -c rerere.enabled=false rebase -q "public/$base" > /dev/null 2>&1; then
    git diff --name-only --diff-filter=U > "$work/update-conflicts"
    git rebase --abort || true
  fi
  if [ -f "$work/update-conflicts" ]; then
    if [ ! -s "$work/update-conflicts" ]; then
      echo "(no conflicted file: see the workflow log)" > "$work/update-conflicts"
    fi
    return 0
  fi

  if [ "$(git rev-list --count "public/$base..HEAD")" -eq 0 ]; then
    echo "Every commit is already on public $base. Nothing to check."
    DRY_RUN=skipped; return 0
  fi

  # 2. Merge up.
  if [ -z "$MERGE_CHAIN" ]; then
    echo "MERGE_CHAIN isn't set, so the merge-up wasn't checked."
    return 0
  fi
  git diff --name-only "public/$base" HEAD > "$work/pr-files"
  git branch -f "$base" HEAD
  mu_merge_plan "$base" > "$work/plan.txt"
  echo "Merge plan:"
  cat "$work/plan.txt"
  if [ "$MU_ON_CONFLICT" = "keep" ]; then
    mu_run_chain "$work/plan.txt" "$work/run"
    return 0
  fi
  mu_run_chain "$work/plan.txt" "$work/run" probe

  # 3. On a conflict, the same merge-up from public alone, to tell this PR's
  # conflicts from leftover drift.
  if [ "$CHAIN" = "conflict" ]; then
    mu_reset_repo
    git branch -f "$base" "public/$base"
    mu_run_chain "$work/plan.txt" "$work/control"
  fi
  return 0
}

# What kind of conflict file $2 is, in the conflicting step folder $1, as a
# short plain note (empty for an ordinary conflict between lines).
mu_kind_note() {
  local d=$1 f=$2 kind detail sha subject from into
  from=$(cat "$d/from") into=$(cat "$d/into")
  kind=$(awk -F'\t' -v f="$f" '$1 == f { print $2; exit }' "$d/kinds" 2> /dev/null)
  detail=$(awk -F'\t' -v f="$f" '$1 == f { print $3 "\t" $4; exit }' "$d/kinds" 2> /dev/null)
  sha=$(printf '%s' "$detail" | cut -f1)
  subject=$(printf '%s' "$detail" | cut -f2)
  case "$kind" in
    deleted-into)
      echo "deleted in \`$into\`${sha:+ by \`$sha\` ($subject)}, but changed in \`$from\`" ;;
    deleted-from)
      echo "deleted in \`$from\`${sha:+ by \`$sha\` ($subject)}, but changed in \`$into\`" ;;
    other)
      echo "no conflicting lines to show (for example, a binary file)" ;;
  esac
}

# The conflicting step folder $1 as a markdown list of its files, each with a
# kind note when it isn't an ordinary conflict. With $2 (the first conflicting
# step's folder, for probed steps), a file that conflicted there too is marked:
# the probe stood in for that resolution, so this may just be its echo.
mu_render_conflict_files() {
  local d=$1 first=$2 f note
  while IFS= read -r f; do
    note=$(mu_kind_note "$d" "$f")
    if [ -n "$first" ] && grep -qxF -- "$f" "$first/files"; then
      note="${note:+$note; }it conflicts in the merge above too, so this may go away once that's resolved"
    fi
    echo "- \`$f\`${note:+ ($note)}"
  done < "$d/files"
}

# What to do about the conflicting step folder $1, for PR $3 into $2, as a
# markdown list. MU_AI_STATUS ("<pr> <status>", from mu_find_proposal or
# mu_check_pr) says where Claude's proposal for this conflict stands; without
# it (Ship it's failure message) the text is general.
mu_render_ways_forward() {
  local d=$1 base=$2 pr=$3 from into ai_pr ai_status
  from=$(cat "$d/from") into=$(cat "$d/into")
  ai_pr=${MU_AI_STATUS%% *}
  ai_status=${MU_AI_STATUS#* }
  if cut -f2 "$d/kinds" 2> /dev/null | grep -qx 'deleted-into'; then
    echo "- **A file this PR changes no longer exists in \`$into\`.** It was usually replaced there (for example, a WebForms block converted to Obsidian), so the change has to be made again in its replacement: open a PR into \`$into\` with the same change. Claude can't propose a resolution and none can be recorded, because git can only record conflicting lines, not a choice to keep a file deleted."
    echo "- Then ship this PR with \`ship-only\`, and merge it up by hand in SmartGit, keeping the file deleted when merging into \`$into\`."
    return 0
  elif cut -f2 "$d/kinds" 2> /dev/null | grep -qx 'deleted-from'; then
    echo "- **\`$from\` deletes a file that \`$into\` still changes.** Decide by hand whether \`$into\` should keep it. Claude can't propose a resolution for that choice, and none can be recorded."
    echo "- Then ship with \`ship-only\`, and do the merge-up by hand in SmartGit."
    return 0
  elif ! mu_all_content_conflicts "$d"; then
    echo "- Ship with \`ship-only\`, then do the merge-up by hand in SmartGit. These conflicts have no conflicting lines git can record, so Claude can't propose a resolution and none can be recorded."
    return 0
  elif mu_has_protected_file "$d"; then
    echo "- Ship with \`ship-only\`, then do the merge-up by hand in SmartGit. Claude can't propose a resolution here, because one of the files is AI configuration (such as \`CLAUDE.md\` or \`.claude/\`), which the AI action replaces before Claude runs."
    return 0
  fi

  mu_ai_status_line "$pr" "$ai_pr" "$ai_status"
  echo "- Or add \`ship-only\` to ship to \`$base\` without merging up, then do the merge-up by hand in SmartGit. When that merge reaches public, its resolution is recorded, and Ship it replays it if the identical conflict comes up again."
}

# The "ways forward" line for where Claude's proposal stands: for PR $1, the
# proposal is on PR $2 with status $3 (queued, pending or failed; anything
# else gives the general line).
mu_ai_status_line() {
  local pr=$1 ai_pr=$2 ai_status=$3
  case "$ai_status" in
    queued)
      if [ "$ai_pr" = "$pr" ]; then
        echo "- Claude is proposing a resolution now. It'll be posted here in a few minutes, and nothing uses it until a person approves it with the \`approve-resolution\` label."
      else
        echo "- Claude is proposing a resolution on #$ai_pr, which hits the same conflict. Once someone approves it there, this PR uses it too."
      fi ;;
    pending)
      if [ "$ai_pr" = "$pr" ]; then
        echo "- Claude proposed a resolution (see the AI-proposed resolution comment). If it's right, add the \`approve-resolution\` label, then \`ship\`."
      else
        echo "- Claude proposed a resolution on #$ai_pr for the same conflict. Approve it there with \`approve-resolution\`, and this PR uses it too."
      fi ;;
    failed)
      if [ "$ai_pr" = "$pr" ]; then
        echo "- Claude couldn't propose a resolution for this conflict (see the AI resolution comment), and won't try again unless the conflict changes."
      else
        echo "- Claude couldn't propose a resolution for this conflict (see #$ai_pr), and won't try again unless the conflict changes."
      fi ;;
    *)
      echo "- The merge-up check has Claude propose a resolution for this conflict automatically. Approve it with the \`approve-resolution\` label, then add \`ship\` again." ;;
  esac
}

# After a proposal for PR $1 finishes with status $2 (pending or failed),
# updates the merge-up check comment, which still says Claude is proposing one.
# Best effort: the next check rewrites the whole comment anyway.
mu_refresh_check_comment() {
  local pr=$1 status=$2 body="$RUNNER_TEMP/check-comment.md" queued new
  gh api --paginate "repos/$GITHUB_REPOSITORY/issues/$pr/comments?per_page=100" \
    -q ".[] | select(.body | contains(\"$MU_CHECK_MARKER\")) | .body" > "$body" 2> /dev/null || return 0
  [ -s "$body" ] || return 0
  queued=$(mu_ai_status_line "$pr" "$pr" queued)
  new=$(mu_ai_status_line "$pr" "$pr" "$status")
  grep -qxF -- "$queued" "$body" || return 0
  awk -v old="$queued" -v new="$new" '$0 == old { print new; next } { print }' "$body" > "$body.tmp"
  mv "$body.tmp" "$body"
  mu_set_comment "$pr" "$MU_CHECK_MARKER" "$body" || true
}

# The marker for the AI resolution comment about conflict key $1. One comment
# per conflict, so a later proposal or failure for another conflict on the
# same PR never replaces an earlier one (or its approval).
mu_ai_marker() {
  echo "<!-- ai-resolution key=$1 -->"
}

# Files that claude-code-action replaces, before Claude runs, with their
# version from the PR's target branch (it treats the PR as untrusted). In the
# middle of a merge-up the working files are another branch's, so the action
# would change these files itself.
MU_AI_PROTECTED_PATHS=".claude .mcp.json .claude.json .gitmodules .ripgreprc CLAUDE.md CLAUDE.local.md .husky"

# True when a conflicted file in the step folder $1 is one of those files, so
# whatever Claude saw of it wasn't the conflict.
mu_has_protected_file() {
  local f p
  while IFS= read -r f; do
    for p in $MU_AI_PROTECTED_PATHS; do
      case "$f" in "$p"|"$p"/*) return 0 ;; esac
    done
  done < "$1/files"
  return 1
}

# Saves the protected files as they are now (in the folder $1), so
# mu_restore_protected can undo whatever the AI action does to them.
mu_save_protected() {
  local dir=$1 p present=""
  mkdir -p "$dir"
  for p in $MU_AI_PROTECTED_PATHS; do
    if [ -e "$p" ]; then present="$present $p"; fi
  done
  echo "$present" > "$dir/present"
  if [ -n "$present" ]; then
    # Unquoted on purpose: one argument per path.
    tar -cf "$dir/protected.tar" $present
  fi
}

# Puts the protected files back exactly as mu_save_protected saw them,
# including removing any the AI action added.
mu_restore_protected() {
  local dir=$1 p
  for p in $MU_AI_PROTECTED_PATHS; do
    rm -rf -- "$p"
  done
  if [ -f "$dir/protected.tar" ]; then
    tar -xf "$dir/protected.tar"
  fi
}

# Writes the merge-up check comment for PR $1 (into $2) from the dry run in $3.
mu_render_check_comment() {
  local pr=$1 base=$2 work=$3 c cc p from into same_step drift_files pr_files i j f notes kind_note total drift
  {
    echo "$MU_CHECK_MARKER"
    if [ -s "$work/update-conflicts" ]; then
      echo "### Merge-up check: conflicts with \`$base\`"
      echo
      echo "Bringing this PR up to date with the live public \`$base\` hits conflicts in:"
      sed 's/^/- `/; s/$/`/' "$work/update-conflicts"
      echo
      echo "Merge \`$base\` into your branch locally, resolve the conflicts, and push. The merge-up can't be checked until then."
    elif [ -d "$work/run/conflict" ]; then
      c="$work/run/conflict"
      from=$(cat "$c/from") into=$(cat "$c/into")
      cc="$work/control/conflict"
      same_step=false
      if [ -d "$cc" ] && [ "$(cat "$cc/from")" = "$from" ] && [ "$(cat "$cc/into")" = "$into" ]; then
        same_step=true
      fi

      echo "### Merge-up check: conflict merging \`$from\` into \`$into\`"
      echo
      echo "Shipping this PR with \`ship\` would stop at this merge, and nothing would be pushed."
      echo

      # A conflicting chunk is leftover drift when the same merge without the
      # PR has the identical chunk in the same file.
      drift_files=0 pr_files=0
      if [ -s "$c/files" ]; then
        echo "Conflicts in:"
        i=0
        while IFS= read -r f; do
          i=$((i + 1))
          notes=""
          grep -qxF -- "$f" "$work/pr-files" || notes="this PR doesn't change this file"
          j=""
          if [ "$same_step" = true ]; then
            j=$(grep -nxF -- "$f" "$cc/files" | head -1 | cut -d: -f1)
          fi
          total=$(cat "$c/chunks-$i" 2> /dev/null | wc -l)
          if [ -z "$j" ]; then
            drift=0
          elif [ "$total" -eq 0 ]; then
            # No conflict markers (for example, deleted on one side): compare
            # the file as a whole.
            drift=1 total=1
          else
            drift=$(grep -cxF -f "$cc/chunks-$j" "$c/chunks-$i" 2> /dev/null || true)
            drift=${drift:-0}
          fi
          if [ "$total" -gt 0 ] && [ "$drift" -ge "$total" ]; then
            notes="${notes:+$notes; }conflicts the same way without this PR"
            drift_files=$((drift_files + 1))
          elif [ "$drift" -gt 0 ]; then
            notes="${notes:+$notes; }$drift of $total conflicting chunks are there without this PR too"
            drift_files=$((drift_files + 1))
            pr_files=$((pr_files + 1))
          else
            pr_files=$((pr_files + 1))
          fi
          kind_note=$(mu_kind_note "$c" "$f")
          if [ -n "$kind_note" ]; then
            notes="$kind_note${notes:+; $notes}"
          fi
          echo "- \`$f\`${notes:+ ($notes)}"
        done < "$c/files"
      else
        echo "The merge failed without a conflicted file. See the [workflow log]($MU_LOG_URL)."
      fi
      if [ -s "$c/resolved" ]; then
        echo
        echo "These other files in the same merge would be resolved automatically, but a merge is only finished automatically when every file is covered:"
        sed 's/^/- `/; s/$/`/' "$c/resolved"
      fi

      echo
      if [ "$drift_files" -gt 0 ] && [ "$pr_files" -eq 0 ]; then
        echo "**Leftover drift, not this PR.** This merge conflicts the same way without this PR, because an earlier change was never merged up from \`$from\` to \`$into\`. Someone needs to merge \`$from\` into \`$into\` by hand on public first; this check then runs again by itself."
      elif [ "$drift_files" -gt 0 ]; then
        echo "**Partly leftover drift.** The conflicts marked as there without this PR come from an earlier change that was never merged up from \`$from\` to \`$into\`. The rest come from this PR."
      elif [ -d "$cc" ] && [ "$same_step" = false ]; then
        echo "**Leftover drift elsewhere.** Even without this PR, merging \`$(cat "$cc/from")\` into \`$(cat "$cc/into")\` already conflicts, so the merge-up chain isn't fully merged up. That needs a merge by hand on public first."
      elif [ -d "$cc" ]; then
        echo "Without this PR this merge also conflicts, but in other places, so the conflicts listed here come from this PR's changes. The chain also has leftover drift that needs a merge by hand on public."
      else
        echo "Without this PR this merge is clean, so these conflicts come from this PR's changes."
      fi

      # Only ordinary conflicts have lines worth showing. For the other kinds
      # git's diff just says "Unmerged path", and the note above says it all.
      i=0
      while IFS= read -r f; do
        i=$((i + 1))
        [ -s "$c/diff-$i" ] || continue
        [ -z "$(mu_kind_note "$c" "$f")" ] || continue
        echo
        echo "<details><summary><code>$f</code>: the conflict</summary>"
        echo
        echo '````diff'
        cat "$c/diff-$i"
        echo '````'
        echo
        echo "</details>"
      done < "$c/files"

      echo
      echo "Ways forward:"
      mu_render_ways_forward "$c" "$base" "$pr"

      # Later steps, estimated by keeping `into`'s side of this conflict.
      if [ -f "$c/probed" ] && ls -d "$work"/run/probe-* > /dev/null 2>&1; then
        echo
        echo "### Probably also conflicts later"
        echo
        echo "Estimated by keeping \`$into\`'s side of the conflict above, so this can change once it's really resolved:"
        for p in "$work"/run/probe-*; do
          echo
          echo "Merging \`$(cat "$p/from")\` into \`$(cat "$p/into")\`:"
          mu_render_conflict_files "$p" "$c"
        done
      elif [ -f "$c/probed" ]; then
        echo
        echo "The later merge-up steps look clean (estimated by keeping \`$into\`'s side of the conflict above)."
      elif [ -f "$c/more-steps" ]; then
        echo
        echo "Later merge-up steps weren't checked, because they come after this one."
      fi
    fi

    if mu_has_resolutions "$work/run"; then
      echo
      echo "### Resolved automatically"
      echo
      echo "Ship it will resolve these conflicts automatically. Check that each resolution is right for this PR before adding \`ship\`."
      echo
      mu_render_resolutions "$work/run" ""
    fi

    echo
    echo "<sub>Dry run of Ship it against the live public branches; nothing was pushed. Updated whenever this PR or a public branch changes. [Workflow log]($MU_LOG_URL)</sub>"
  } > "$work/comment.md"
}

# Re-runs merge commit $1 and records how each conflict in it was resolved.
# Works like git's contrib/rerere-train.sh.
mu_train() {
  local m=$1 recording pr now
  # Skip clean merges cheaply, without touching the working files.
  if git merge-tree --write-tree --no-messages "$m^1" "$m^2" > /dev/null 2>&1; then
    return 0
  fi

  git checkout -q -f --detach "$m^1"
  git clean -fdq
  git merge --no-ff --no-commit "$m^2" > /dev/null 2>&1 || true

  git diff --name-only --diff-filter=U | while IFS= read -r f; do
    # Deleted as the resolution: rerere can't record that.
    git cat-file -e "$m:$f" 2> /dev/null || continue
    [ -f "$f" ] || continue
    if ! grep -qE '^<<<<<<< ' "$f"; then
      # rerere already replayed a recording here. Same result as the merge
      # commit: nothing new. Different: this person resolved it differently,
      # so the newer resolution replaces the recording.
      if [ "$(git hash-object --path="$f" -- "$f")" = "$(git rev-parse "$m:$f")" ]; then
        continue
      fi
      git rerere forget -- "$f"
    fi
    git checkout -q "$m" -- "$f"
  done

  # MERGE_RR lists "<recording id><TAB><path>" for each conflict rerere is
  # about to record.
  recording=$(tr '\0' '\n' < "$MU_GITDIR/MERGE_RR" 2> /dev/null || true)
  git rerere

  if [ -n "$recording" ]; then
    pr=$(mu_pr_for_merge "$m")
    now=$(date -u +%Y-%m-%dT%H:%M:%SZ)
    printf '%s\n' "$recording" | while IFS="$(printf '\t')" read -r id path; do
      mu_has_postimage "$id" || continue
      echo "Recorded the resolution of $path from ${m:0:10}${pr:+ (PR #$pr)}."
      jq --arg id "$id" --arg path "$path" --arg commit "$m" --arg pr "$pr" --arg at "$now" \
        '.[$id] = {path: $path, commit: $commit, pr: (if $pr == "" then null else ($pr | tonumber) end), recordedAt: $at}' \
        "$MU_META" > "$MU_META.tmp"
      mv "$MU_META.tmp" "$MU_META"
    done
  fi
  git reset -q --hard
}

# True when the recording $1 ("<folder>" or "<folder>.<variant>") has a resolution.
mu_has_postimage() {
  local dir=${1%%.*}
  local variant=${1#"$dir"}
  [ -f "$MU_GITDIR/rr-cache/$dir/postimage$variant" ]
}

# Finds the PR a merge commit's resolution came from, when it can.
#   "Merge branch 'release-20.0' into bugfix-x": the PR from bugfix-x, which
#   merged its target in to resolve a conflict.
#   A merge into a shared branch: a merge-up done by hand. The PR is the
#   closed `merge-up-conflict` PR whose shipped commit this merge carried
#   across (in its second parent but not its first).
mu_pr_for_merge() {
  local m=$1 into pr shipped
  into=$(git log -1 --format=%s "$m" | sed -nE "s/^Merge (remote-tracking )?branch '[^']+' into (.+)$/\2/p")
  case "$into" in
    "") return 0 ;;
    develop|release-*|hotfix-*) ;;
    *)
      gh pr list -R "$GITHUB_REPOSITORY" -s all --head "$into" -L 1 --json number -q '.[0].number // empty' || true
      return 0 ;;
  esac
  for pr in $(gh pr list -R "$GITHUB_REPOSITORY" -s closed -l merge-up-conflict -L 30 --json number -q '.[].number' || true); do
    shipped=$(gh api "repos/$GITHUB_REPOSITORY/issues/$pr/comments?per_page=100" -q '.[].body' \
      | grep -oE 'shipped-sha: [0-9a-f]{40}' | tail -1 | cut -d' ' -f2 || true)
    [ -n "$shipped" ] || continue
    if git merge-base --is-ancestor "$shipped" "$m^2" 2> /dev/null && ! git merge-base --is-ancestor "$shipped" "$m^1" 2> /dev/null; then
      echo "$pr"
      return 0
    fi
  done
}

# Copies every finished recording (a conflict plus its resolution) into the
# rerere-cache checkout $1, merges resolutions.json and trained.json into it,
# and leaves anything already there in place.
mu_apply_recordings() {
  local out=$1 dir name
  mkdir -p "$out/rr-cache"
  for dir in "$MU_GITDIR"/rr-cache/*/; do
    [ -d "$dir" ] || continue
    ls "$dir" | grep -q '^postimage' || continue
    name=$(basename "$dir")
    mkdir -p "$out/rr-cache/$name"
    find "$dir" -maxdepth 1 -type f \( -name 'preimage*' -o -name 'postimage*' \) -exec cp {} "$out/rr-cache/$name/" \;
  done
  jq -s '.[0] * .[1]' "$out/resolutions.json" "$MU_META" > "$out/resolutions.tmp"
  mv "$out/resolutions.tmp" "$out/resolutions.json"
  [ -f "$out/trained.json" ] || echo '{}' > "$out/trained.json"
  jq -s '.[0] * .[1]' "$out/trained.json" "$MU_CACHE/trained.json" > "$out/trained.tmp"
  mv "$out/trained.tmp" "$out/trained.json"
}

# Dry run for PR $1 (into $2, from branch $3), then sets its label and its
# merge-up check comment from the result.
mu_check_pr() {
  local pr=$1 base=$2 head=$3 work="$RUNNER_TEMP/pr-$1" has_conflict=false key
  mu_dry_run "$pr" "$base" "$head" "$work"
  if [ "$DRY_RUN" = "skipped" ]; then
    mu_set_label "$pr" merge-up-conflict false
    mu_set_comment "$pr" "$MU_CHECK_MARKER" ""
    return 0
  fi

  if [ -s "$work/update-conflicts" ] || [ -d "$work/run/conflict" ]; then
    has_conflict=true
  fi

  # A conflict Claude can resolve gets a proposal automatically, once per
  # conflict: not again for a conflict that already has one (waiting for
  # approval, or failed), whether on this PR or another. Queued PRs are
  # written to propose.txt for the `propose` job.
  MU_AI_STATUS=""
  if [ -d "$work/run/conflict" ] && mu_can_propose "$work/run/conflict"; then
    key=$(cat "$work/run/conflict/key")
    MU_AI_STATUS=$(mu_find_proposal "$key")
    if [ -z "$MU_AI_STATUS" ]; then
      echo "$pr $base $head" >> "$RUNNER_TEMP/propose.txt"
      echo "$key $pr" >> "$RUNNER_TEMP/propose-keys"
      MU_AI_STATUS="$pr queued"
    fi
    echo "AI proposal: $MU_AI_STATUS"
  fi

  mu_set_label "$pr" merge-up-conflict "$has_conflict"
  if [ "$has_conflict" = "true" ] || mu_has_resolutions "$work/run"; then
    mu_render_check_comment "$pr" "$base" "$work"
    mu_set_comment "$pr" "$MU_CHECK_MARKER" "$work/comment.md"
  else
    mu_set_comment "$pr" "$MU_CHECK_MARKER" ""
  fi
  echo "PR #$pr: $( [ "$has_conflict" = "true" ] && echo conflict || echo clean )"
}
