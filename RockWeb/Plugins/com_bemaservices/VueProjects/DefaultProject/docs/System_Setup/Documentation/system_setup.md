### Browser Extensions for Vue Development
 - Vue Dev Tools
This extension is useful for inspecting the properties, data, and method output of a vue instance for development and debugging.
[Firefox](https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/)
[Chrome](https://chrome.google.com/webstore/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd?hl=en)

### Configure Your System
1 Install Node JS.
Vue uses Node Package Manager to install dependencies and run in a live server for development.  You can install this by downloading the package at [https://nodejs.org/en/](https://nodejs.org/en/) or running the following commands.
```noEditor
curl -o- https://raw.githubusercontent.com/creationix/nvm/v0.33.11/install.sh | bash
nvm install stable
```

2 Open terminal to the project folder.  In the default project it would be DefaultProject and run the following command to download required dependencies.  The node-modules folder is included in .gitignore.
```noteEditor
npm install
```
