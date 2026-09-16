# Overview

This tool is used to generate the browserlist string that will be used when compiling Obsidian. It finds the top 99% of browsers and uses that data to generate the string.

To use, run the following commands.

```sh
npm update --save
npm run start
```

This will update to the latest browser database package and then run the tool. Any changes to `package.json` or `package-lock.json` should be commited.
