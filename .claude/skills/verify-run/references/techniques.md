# Techniques

How to send requests the page would never offer, and how to keep separate logins. `/verify-plan` copies the techniques a plan uses into its Techniques section, so a human tester has them too.

Most crafted-request tests run in the browser's developer tools (or an agent's JavaScript tool) on the page that hosts the block, logged in as the user the test names.

## WebForms: hidden field tampering (WF-HF)

1. Find the hidden field: `document.querySelector('input[type=hidden][id$="_hfFieldName"]')` (the server id ends with the field name, for example `hfGroupId`).
2. Set its value: `el.value = '123';`
3. Click the page's normal button (Save, Delete, etc.). The postback sends the tampered value.

If the button opens a confirm modal, tamper after the modal opens and before clicking its confirm button.

## WebForms: postback argument tampering (WF-PB)

Some actions are postbacks with an argument (`__EVENTARGUMENT`), for example `javascript:__doPostBack('ctl00$main$ctl23$ctl01$ctl06$upnlContent','Flag^123')`.

1. Find an existing link or button that makes the postback (right-click > Inspect) and copy its `__doPostBack(target, argument)` call.
2. Run it in the console with a changed argument: `__doPostBack('<target>', '<tampered argument>')`.

Call `__doPostBack` directly; it also skips client-side confirm dialogs. Replaying a postback for a control that is not rendered for this user is rejected by ASP.NET event validation (an "Invalid postback" error page) before the block's own code runs. That proves nothing about the block's check; note it and tamper from a page where the control is rendered.

## Obsidian: block action replay (OB-ACT)

Obsidian blocks call `POST /api/v2/BlockActions/{pageGuid}/{blockGuid}/{ActionName}` with a JSON body `{ "__context": { "pageParameters": { ... } }, ...arguments }`. The request is authenticated by the login cookie only.

1. Open the Network tab, perform the normal action once, and find the `BlockActions/.../{ActionName}` request. Copy its URL and request body.
2. Replay it from the console with changed values:

   ```js
   await fetch('/api/v2/BlockActions/<pageGuid>/<blockGuid>/<ActionName>', {
       method: 'POST',
       headers: { 'Content-Type': 'application/json' },
       body: JSON.stringify({ __context: { pageParameters: { /* as captured, or tampered */ } }, /* arguments */ })
   }).then(async r => r.status + ' ' + await r.text());
   ```

3. If the action is not reachable from the UI for this user, get the page and block Guids from the database (see `verify-plan/references/locating-ui.md`) and write the body from the action's parameter names in the block's C# source.

Keys: actions accept IdKeys, Guids, and (unless the site disables predictable ids) plain integer ids. IdKeys for existing rows can be copied from grid data responses in the Network tab.

A blocked request normally returns HTTP 400, 403 or 404 with an error message, or 200 with nothing changed. Always confirm in the UI or database that nothing changed, and that the same request with a legitimate value still works.

## Anonymous and other-user requests

- Anonymous: `fetch(url, { credentials: 'omit' })` from any page on the site, or a scripted session that never logs in.
- Another user without switching the browser: use a scripted session (below).

## Scripted HTTP sessions

The browser has one set of cookies, so it is logged in as one user at a time. A scripted session keeps its own cookies, which lets a test act as two users at once and lets parallel workers avoid fighting over the browser login.

PowerShell:

```powershell
$base = '<siteUrl>'
$body = @{ Username = 'pfoster'; Password = 'password'; Persisted = $false } | ConvertTo-Json
Invoke-WebRequest "$base/api/Auth/Login" -Method Post -ContentType 'application/json' -Body $body -SessionVariable pete | Out-Null

# Obsidian block action as Pete:
Invoke-WebRequest "$base/api/v2/BlockActions/<pageGuid>/<blockGuid>/<ActionName>" -Method Post -ContentType 'application/json' -Body '<json>' -WebSession $pete
```

A session file per user (`curl -c pete.txt -b pete.txt`) works the same way in bash. Keep session files in the scratchpad, never in the repo.

WebForms postbacks are harder to script (they need `__VIEWSTATE`, `__EVENTVALIDATION` and the other hidden fields from a fresh GET of the page). Do them in the browser unless a recipe in `ui-recipes.md` shows a scripted way.

## Checking results in the database

Read-only SQL is a good way to confirm a change did or did not happen (for example `SELECT [Id], [Value], [ModifiedDateTime] FROM [DefinedValue] WHERE [Id] = ...`). Compare `ModifiedDateTime` before and after to catch writes that left values unchanged.
