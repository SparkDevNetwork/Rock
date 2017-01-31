/* global Rock */
import Bootstrap from "./Bootstrap";

let app;
const load = () => {
  if (!Rock) return;

  const pageId = Rock.settings.get("pageId");
  // bootstrap the page
  if (!app) app = new Bootstrap({ pageId });
};

// load everything up
// written as a function for easy early returns
if (!window.fetch) {
  System.import("whatwg-fetch").then(load);
} else {
  load();
}

// handle local development inline reloads
if (module.hot) {
  module.hot.accept(() => {
    // eslint-disable-next-line
    if (app) app.render();
  });
}
