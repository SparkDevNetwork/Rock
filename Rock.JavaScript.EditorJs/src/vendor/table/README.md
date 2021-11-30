![](https://badgen.net/badge/Editor.js/v2.0/blue)

# Table tool
Table Block for the [Editor.js](https://editorjs.io).

![](https://capella.pics/619031fb-be62-436f-90db-c5e8aa8dd31b.jpg)

## Installation

### Install via NPM or Yarn

Get the package

```shell
npm i --save-dev @editorjs/table
```
or
```shell
yarn add @editorjs/table --dev
```

Include module in your application

```javascript
const Table = require('@editorjs/table');
```

### Upload to your project's source dir
1. Download folder `dist` from repository
2. Add `dist/bundle.js` file to your page.

## Usage
Add a new Tool to the `tools` property of the Editor.js initial config.

```javascript
var editor = EditorJS({
  ...
  
  tools: {
    ...
    table: {
      class: Table,
    }
  }
  
  ...
});
```

Or init Table Tool with additional settings

```javascript
var editor = EditorJS({
  ...
  
  tools: {
    ...
    table: {
      class: Table,
      inlineToolbar: true,
      config: {
        rows: 2,
        cols: 3,
      },
    },
  },
  
  ...
});
```

## Config Params

| Field              | Type     | Description          |
| ------------------ | -------- | ---------------------------------------- |
| rows               | `number` | initial number of rows. by default `2`   |
| cols               | `number` | initial number of columns. by default `2`|

## Output data
This Tool returns `data` with following format

| Field     | Type         | Description           |
| --------- | ------------ | ----------------------------------------- |
| withHeadings | `boolean` | Uses the first line as headings |
| content   | `string[][]` | two-dimensional array with table contents |

```json
{
    "type" : "table",
    "data" : {
        "withHeadings": true,
        "content" : [ [ "Kine", "Pigs", "Chicken" ], [ "1 pcs", "3 pcs", "12 pcs" ], [ "100$", "200$", "150$" ] ]
    }
}
```
