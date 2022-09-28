When adding new files to this folder, the following rules must be followed.

1. Ensure the file name does not conflict with a reserved keyword name. If it does, append "Utils" or something similar to it (for example, stringUtils.ts).
2. Add the file to the index.ts so it re-exports the contents of the file. You must follow the same pattern, which is to use the PascalCase version of the filename as the named export.
