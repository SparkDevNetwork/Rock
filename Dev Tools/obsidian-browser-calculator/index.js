import browserslist from "browserslist";
import { coerce as coerceVersion } from "semver";
// No direct usage import; use agents for usage data
import { agents } from "caniuse-lite/dist/unpacker/agents.js";

// Generate browserslist for all non-dead browsers
const fullNotDead = browserslist("cover 100% and not dead");
const browserVersionCoverage = fullNotDead.map(entry => {
    const [name, version] = entry.split(' ');
    const usage = agents[name]?.usage_global[version] || 0;

    return { name, version: version.split('-')[0], usage };
});

// Normalize the browserVersionCoverage to sum to 100%
const usageSum = browserVersionCoverage.reduce((sum, item) => sum + item.usage, 0);
browserVersionCoverage.forEach(item => {
    item.usage = (item.usage / usageSum) * 100;
});

// Sort by usage descending to prioritize browsers contributing most to coverage.
browserVersionCoverage.sort((a, b) => b.usage - a.usage);

// Generate the list of browsers needed to reach target coverage.
const includedBrowsers = [];
const targetCoverage = 99.0;
for (const item of browserVersionCoverage) {
    includedBrowsers.push(item);

    if (includedBrowsers.reduce((sum, i) => sum + i.usage, 0) >= targetCoverage) {
        break;
    }
}

// Sort included browsers ascending by name and then by version descending.
includedBrowsers.sort((a, b) => {
    if (a.name === b.name) {
        // Compare versions in descending order
        const versionA = coerceVersion(a.version);
        const versionB = coerceVersion(b.version);
        return versionB.compare(versionA);
    }

    return a.name.localeCompare(b.name);
});

// Reduce to a single entry per browser (oldest version).
const browserMap = {};
for (const item of includedBrowsers) {
    const { name, version } = item;
    if (!browserMap[name] || coerceVersion(version).compare(coerceVersion(browserMap[name])) < 0) {
        browserMap[name] = version;
    }
}

let browserlistQuery = Object.entries(browserMap)
    .map(([name, version]) => `${name} >= ${version}`)
    .join(', ');

// Print the final list of browsers and versions and then the final coverage percentage.
console.log("Browsers included:");
let totalCoverage = 0;
for (const item of includedBrowsers) {
    totalCoverage += item.usage;
    console.log(`${item.name} ${item.version}: ${item.usage.toFixed(2)}%`);
}
console.log(`\nTotal coverage of included browsers: ${totalCoverage.toFixed(2)}%`);

console.log(`\nBrowserlist: ${browserlistQuery}`);
