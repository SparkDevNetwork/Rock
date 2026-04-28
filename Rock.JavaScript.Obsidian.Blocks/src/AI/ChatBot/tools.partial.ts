import { parse, Renderer } from "@Obsidian/Libs/marked";
import { asFormattedString } from "@Obsidian/Utility/numberUtils";
import { ExtendedChatMessageBag, ToolCall } from "./types";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

export function getDebugLogText(message: ExtendedChatMessageBag | null): string {
    if (!message || !message.logs || message.logs.length === 0) {
        return "No logs available.";
    }

    let logText = "";

    for (const log of message.logs) {
        logText += `<span class="log-time">@${asFormattedString(log.timestamp)}ms</span>`;
        logText += `<span class="log-category" title="${log.category}">${log.logLevelName}</span>`;
        logText += `<span class="log-message">${escapeHtml(log.message!)}</span>\n`;
    }

    return logText;
}

export function getDebugToolCalls(message: ExtendedChatMessageBag | null): ToolCall[] {
    const toolInvokingRegExp = /^Function ([a-zA-Z0-9_-]+) invoking\.$/;
    const toolArgumentsRegExp = /^Function ([a-zA-Z0-9_-]+) arguments: ([\s\S]+)$/;
    const toolResultRegExp = /^Function ([a-zA-Z0-9_-]+) result: ([\s\S]+)$/;
    const toolErrorRegExp = /^Function ([a-zA-Z0-9_-]+) failed. Error: ([\s\S]+)$/;
    const toolCompletedRegExp = /^Function ([a-zA-Z0-9_-]+) completed. Duration: (.+)$/;

    const logs = message?.logs?.map(l => l.message ?? "") ?? [];

    if (logs.length === 0) {
        return [];
    }

    const tools: ToolCall[] = [];
    let currentTool: ToolCall | null = null;

    for (const log of logs) {
        if (currentTool === null) {
            const match = toolInvokingRegExp.exec(log);
            if (match) {
                currentTool = {
                    name: match[1],
                    args: "{}",
                    result: "",
                    error: "",
                    duration: ""
                };
            }

            continue;
        }

        // Look for the arguments for this tool call.
        let match = toolArgumentsRegExp.exec(log);
        if (match) {
            currentTool.args = match[2];

            continue;
        }

        // Look for the result of this tool call.
        match = toolResultRegExp.exec(log);
        if (match) {
            currentTool.result = match[2];

            continue;
        }

        // Look for an error for this tool call.
        match = toolErrorRegExp.exec(log);
        if (match) {
            currentTool.error = match[2];

            continue;
        }

        // Look for the completion of this tool call.
        match = toolCompletedRegExp.exec(log);
        if (match) {
            currentTool.duration = match[2];

            // Add the completed tool call to the list.
            tools.push(currentTool);
            currentTool = null;

            continue;
        }
    }

    return tools;
}

export function parseMarkdown(text: string): string {
    return parse(text, {
        renderer: new CustomRenderer()
    }) as string;
}

class CustomRenderer extends Renderer {
    override table(token): string {
        const html = super.table(token);

        return html.replace("<table>", `<table class="table table-bordered table-striped">`);
    }
}

