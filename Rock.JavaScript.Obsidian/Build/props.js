import {parse} from 'vue-docgen-api';
import { readdirSync } from "node:fs";
import {join, extname} from "node:path";



let dir = './Framework/Controls/'

const files = readdirSync(dir)

const vueFiles = files.filter(f => extname(f).toLowerCase() == '.vue')

console.log(vueFiles, "\n\n-------\n\n")

vueFiles.forEach(async (fileName) => {
    const filePath = dir + fileName;

    let componentInfo = await parse(filePath, {
        addScriptHandlers: [
            function(docs, compDef, astPath, opts) {
                // console.log(astPath.tokens.find((token, i, tokens) => (token.value == "standardAsyncPickerProps" && tokens[i-1].value == "...")), '\n\n')
            }
        ]
    });

    console.log(componentInfo, "\n\n---------\n\n");
})