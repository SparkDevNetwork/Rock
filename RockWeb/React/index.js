import Bootstrap from "./Bootstrap";

let pageId;
let app;
const load = () => {
  // this layout or version of rock doesn't support react pages
  if (!pageId && !document.getElementById("__page_id__")) {
    console.warn("no page id found to determined blocks to load");
    return;
  }
  
  // parse initial state from server for block info
  if (!pageId) {
    const el = document.getElementById("__page_id__");
    pageId = JSON.parse(el.innerText);
  }
  
  // bootstrap the page
  if (!app) app = new Bootstrap({ pageId });
};

// load everything up
// written as a function for easy early returns
load();

// handle local development inline reloads
if (module.hot) {
  module.hot.accept(() => {
    // eslint-disable-next-line
    if (app) app.render();
  });
}
