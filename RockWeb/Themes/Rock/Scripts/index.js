import "react-hot-loader/patch";
import React from "react";
import ReactDOM from "react-dom";
import { AppContainer } from "react-hot-loader";

import App from "./Hello";

ReactDOM.render(
  <AppContainer><App /></AppContainer>,
  document.getElementById("react-root")
);

if (module.hot) {
  module.hot.accept("./Hello", () => {
    const NextApp = require("./Hello").default;
    ReactDOM.render(
      <AppContainer><NextApp /></AppContainer>,
      document.getElementById("react-root")
    );
  });
}
