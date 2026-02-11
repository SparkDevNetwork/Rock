import * as fs from "fs";
import * as path from "path";

type SearchTermsJson = Record<string, string[]>;

type IconsPickerJson = {
  StyleClassPrefix: string;
  Icons: Array<{
    Title: string;
    SearchTerms: string[];
    StyleClass: string;
    IconSvg: string;
  }>;
};

const PROMPT: string =
  "I'm making a icon picker control for the tabler icons. The control will have a search filter at the top. Please provide 1-4 keywords for the icons below. Only provide keywords that a person would be likely to use to find the icon. Don't return more keywords than is needed. Keep in mind a person might select to use the icon based on it's shape and not only the name provided. For example settings could be used to find the icon cog";

const OUTLINE_DIR = path.resolve(
  process.cwd(),
  "node_modules/@tabler/icons/icons/outline"
);

const SEARCHTERMS_PATH = path.resolve(process.cwd(), "styleclass-searchterms.json");
const TABLER_ICONS_JSON_PATH = path.resolve("../../RockWeb/Assets/Icons/Libraries/tabler-icons.json");

/**
 * Calls OpenAI GPT API with input and range, using API key from environment variable.
 * Expects OPENAI_KEY to be set.
 */
export async function GPTPrompt(input: string, range: string): Promise<string> {
  const apiKey = process.env.OPENAI_KEY;
  if (!apiKey) return "Error: OPENAI_KEY environment variable not set.";

  const url = "https://api.openai.com/v1/chat/completions";
  const payload = {
    model: "gpt-4.1-mini",
    messages: [{ role: "user", content: `${input}: ${range}` }],
    max_tokens: 300,
    temperature: 0.7,
  };

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${apiKey}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    const json: any = await response.json();
    if (!json.choices?.[0]?.message) return "Error: Unexpected API response.";
    return String(json.choices[0].message.content ?? "").trim();
  } catch (e: any) {
    return "Error: " + (e.message || e);
  }
}

function ensurePathExists(p: string, label: string) {
  if (!fs.existsSync(p)) throw new Error(`${label} not found: ${p}`);
}

function GetAllSvgBaseNames(dir: string): Set<string> {
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  const names = entries
    .filter((e) => e.isFile() && e.name.toLowerCase().endsWith(".svg"))
    .map((e) => path.basename(e.name, ".svg"));
  return new Set(names);
}

function GetAllSearchTermsJson(filePath: string): SearchTermsJson {
  const raw = fs.readFileSync(filePath, "utf8");
  const parsed = JSON.parse(raw);
  if (typeof parsed !== "object" || parsed === null || Array.isArray(parsed)) {
    throw new Error(`Expected an object at top-level in ${filePath}`);
  }
  return parsed as SearchTermsJson;
}

function writeSearchTermsJson(filePath: string, data: SearchTermsJson) {
  const sorted: SearchTermsJson = {};
  for (const key of Object.keys(data).sort()) sorted[key] = data[key];
  fs.writeFileSync(filePath, JSON.stringify(sorted, null, 2) + "\n", "utf8");
}

function stripTiPrefix(key: string): string {
  return key.startsWith("ti-") ? key.slice(3) : key;
}

function normalizeKeywords(words: string[]): string[] {
  const seen = new Set<string>();
  const out: string[] = [];
  for (const w of words) {
    const k = w.trim().toLowerCase();
    if (!k) continue;
    if (seen.has(k)) continue;
    seen.add(k);
    out.push(k);
  }
  return out;
}

function parseKeywordsFromGpt(text: string): string[] {
  const trimmed = text.trim();

  if (trimmed.startsWith("[") && trimmed.endsWith("]")) {
    try {
      const arr = JSON.parse(trimmed);
      if (Array.isArray(arr)) return normalizeKeywords(arr.map((x) => String(x))).slice(0, 4);
    } catch {
      // ignore
    }
  }

  const lines = trimmed
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter(Boolean);

  const listish = lines
    .map((l) => l.replace(/^[-*•]\s+/, "").replace(/^\d+[\).\]]\s+/, ""))
    .filter(Boolean);

  if (listish.length > 1) return normalizeKeywords(listish).slice(0, 4);

  const parts = trimmed.split(/[,;|]/g).map((p) => p.trim());
  return normalizeKeywords(parts).slice(0, 4);
}

async function generateKeywordsForIcon(iconBaseName: string): Promise<string[]> {
  const hint = `Icon name: "${iconBaseName}".`;
  const resp = await GPTPrompt(PROMPT, hint);
  if (resp.startsWith("Error:")) throw new Error(resp);

  const keywords = parseKeywordsFromGpt(resp);
  if (keywords.length === 0) return normalizeKeywords(iconBaseName.split("-")).slice(0, 4);
  return keywords;
}

function readSvg(iconBaseName: string): string {
  const svgPath = path.join(OUTLINE_DIR, `${iconBaseName}.svg`);
  if (!fs.existsSync(svgPath)) throw new Error(`SVG not found for ${iconBaseName}: ${svgPath}`);
  return fs.readFileSync(svgPath, "utf8").trim();
}

function readTablerIconsJson(p: string): IconsPickerJson {
  if (!fs.existsSync(p)) {
    // Create a new one if not present
    return { StyleClassPrefix: "ti", Icons: [] };
  }
  const raw = fs.readFileSync(p, "utf8");
  const parsed = JSON.parse(raw) as IconsPickerJson;

  if (!parsed || typeof parsed !== "object" || !Array.isArray(parsed.Icons)) {
    throw new Error(`Invalid tabler-icons.json format at ${p}`);
  }
  if (!parsed.StyleClassPrefix) parsed.StyleClassPrefix = "ti";
  return parsed;
}

function writeTablerIconsJson(p: string, data: IconsPickerJson) {
  // Sort by Title for stable output
  data.Icons.sort((a, b) => a.Title.localeCompare(b.Title));
  fs.writeFileSync(p, JSON.stringify(data, null, 2) + "\n", "utf8");
}

function rebuildIconPickerJson(
  searchTerms: SearchTermsJson,
  svgNames: Set<string>
): IconsPickerJson {
  const icons: IconsPickerJson["Icons"] = [];

  for (const styleClass of Object.keys(searchTerms).sort()) {
    if (!styleClass.startsWith("ti-")) continue; // enforce your prefix

    const title = stripTiPrefix(styleClass); // e.g. "a-b-2"
    if (!svgNames.has(title)) continue; // skip orphaned entries

    const svg = readSvg(title);
    icons.push({
      Title: title,
      SearchTerms: normalizeKeywords(searchTerms[styleClass] ?? []).slice(0, 4),
      StyleClass: styleClass,
      IconSvg: svg,
    });
  }

  return { StyleClassPrefix: "ti", Icons: icons };
}

async function main() {
  ensurePathExists(OUTLINE_DIR, "Outline icons directory");
  ensurePathExists(SEARCHTERMS_PATH, "Search terms JSON");

  const svgNames = GetAllSvgBaseNames(OUTLINE_DIR);
  const searchTerms = GetAllSearchTermsJson(SEARCHTERMS_PATH);

  // 1) Add missing icons to styleclass-searchterms.json (idempotent)
  const jsonIconNames = new Set<string>();
  for (const key of Object.keys(searchTerms)) jsonIconNames.add(stripTiPrefix(key));

  const missingInJson = [...svgNames].filter((svg) => !jsonIconNames.has(svg)).sort();

  let addedCount = 0;
  if (missingInJson.length > 0) {
    console.log(`Found ${missingInJson.length} missing icons in searchterms. Generating...`);

    for (const iconBase of missingInJson) {
      const key = `ti-${iconBase}`;
      if (Object.prototype.hasOwnProperty.call(searchTerms, key)) continue; // idempotent

      try {
        const keywords = await generateKeywordsForIcon(iconBase);
        searchTerms[key] = keywords;
        addedCount++;
        console.log(`+ ${key}: [${keywords.join(", ")}]`);
      } catch (e: any) {
        console.warn(`! Failed for ${key}: ${e?.message ?? e}`);
      }
    }

    if (addedCount > 0) {
      writeSearchTermsJson(SEARCHTERMS_PATH, searchTerms);
      console.log(`Updated ${SEARCHTERMS_PATH} (+${addedCount})`);
    }
  } else {
    console.log("No missing icons in styleclass-searchterms.json");
  }

  // 2) Write tabler-icons.json from the (now updated) styleclass-searchterms.json
  //    This ensures it always includes:
  //    - Title
  //    - SearchTerms
  //    - StyleClass
  //    - IconSvg (actual svg file contents)
  const rebuilt = rebuildIconPickerJson(searchTerms, svgNames);

  // Optional: keep existing file around but overwrite deterministically
  writeTablerIconsJson(TABLER_ICONS_JSON_PATH, rebuilt);

  console.log(`Wrote ${TABLER_ICONS_JSON_PATH} (${rebuilt.Icons.length} icons)`);

}

main().catch((e) => {
  console.error(e);
  process.exitCode = 1;
});