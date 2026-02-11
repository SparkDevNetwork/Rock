### How it work?
This script is an update script that incrementally keeps your Tabler outline icon data in sync rather than recreating it from scratch. It scans the Tabler outline SVG directory to determine the current set of available icons, loads the existing `styleclass-searchterms.json`, and only adds entries for icons that are newly introduced or previously missing, leaving all existing keywords untouched and making the process idempotent. For each missing icon, it uses the OpenAI Chat Completions API to generate a small, normalized set of human-friendly search terms based on the icon’s name and likely visual shape, then writes those additions back to the search-terms file in a stable, sorted format. After the search terms are updated, the script rebuilds `tabler-icons.json` deterministically from the now-complete search-terms mapping and the SVG files themselves, embedding the SVG markup alongside each icon’s title, style class, and keywords. The result is a safe, repeatable update process that can be rerun at any time to pick up new icons while preserving all existing data and ensuring the icon picker always has a complete, up-to-date library.


### Instructions
1. Navigate to `Rock/Dev Tools/tabler-icon-updater`.  
2. Open the project in VS Code.
3. Open the `package.json`
4. Increment the `@tabler/icons-webfont` version to the desired version.
Note: Only version increments are supported—you cannot downgrade. If you mess up, just discard your Git changes and retry.
5. Run `npm install` to install the updated package.
6. In the terminal, set the OpenAI API key: `$env:OPENAI_KEY=""`
Note: You can use the triumph OpenAI Key in RDM 
7. Run `npm run generateIcon`.
8. Restart Rock and clear cache then make sure it applies.
9. Done.
