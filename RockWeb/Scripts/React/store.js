
import { createInjectableStore } from "redux-injectable-store";
import { applyMiddleware, compose } from "redux";

export default (middleware = [], initialState = {}) => {
  let devTools = (f) => f;

  // dev tool extension
  if (process.env.NODE_ENV !== "production") {
    if (typeof window === "object" && typeof window.devToolsExtension !== "undefined") {
      devTools = window.devToolsExtension();
    }
  }

  const enhancer = compose(applyMiddleware(...middleware), devTools);

  return createInjectableStore(initialState, enhancer);
};

