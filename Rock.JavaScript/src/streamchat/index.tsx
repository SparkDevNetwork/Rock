import React from "react";
import ReactDOM from "react-dom/client";
import ChatComponent from "./ChatComponent";
import "./chatComponent.css";
import "./styles/main.css";
import type { ChatComponentProps } from "./ChatComponentProps";

const mount = (elementId: string, config: ChatComponentProps) => {
    const rootElement = document.getElementById(elementId);
    if (rootElement) {
        const rootInstance = ReactDOM.createRoot(rootElement);
        rootInstance.render(<ChatComponent {...config} />);
        return rootInstance;
    }
}

const unmount = (rootInstance: any) => {
    if (rootInstance) {
        rootInstance.unmount();
    }
};

export { mount, unmount };