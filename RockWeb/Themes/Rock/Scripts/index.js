import { render } from "react-dom";
import { AppContainer } from "react-hot-loader";

import App from "./Hello";

render(
  <AppContainer><App /></AppContainer>,
  document.getElementById("react-root"),
);

if (module.hot) {
  module.hot.accept("./Hello", () => {
    // eslint-disable-next-line
    const NextApp = require("./Hello").default;
    render(
      <AppContainer><NextApp /></AppContainer>,
      document.getElementById("react-root"),
    );
  });
}
