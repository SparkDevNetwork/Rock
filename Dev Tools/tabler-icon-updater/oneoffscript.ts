import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

interface Icon {
  StyleClass: string;
  SearchTerms: string[];
}

interface IconFile {
  Icons: Icon[];
}

// paths
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const inputPath = path.resolve(__dirname, "tabler-icons.json");
const outputPath = path.resolve(__dirname, "styleclass-searchterms.json");

// read + parse input
const raw = fs.readFileSync(inputPath, "utf-8");
const data: IconFile = JSON.parse(raw);

// build map
const result: Record<string, string[]> = {};

for (const icon of data.Icons) {
  if (!icon.StyleClass) continue;

  result[icon.StyleClass] = icon.SearchTerms ?? [];
}

// write output
fs.writeFileSync(
  outputPath,
  JSON.stringify(result, null, 2),
  "utf-8"
);

console.log(`✅ Wrote ${Object.keys(result).length} entries to ${outputPath}`);
