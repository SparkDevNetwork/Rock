import { parse, Renderer, Tokens } from "@Obsidian/Libs/marked";
import { HtmlSanitizeOptions, HtmlTagRemovalMode, sanitizeHtml } from "@Obsidian/Core/Utilities/htmlSanitization";
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

/**
 * The tags that Markdown produces, without attributes that load, style or
 * position content, change focus order or change what screen readers announce.
 */
const chatMessageSanitizeOptions = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
    .allowTag("h1").allowTag("h2").allowTag("h3").allowTag("h4").allowTag("h5").allowTag("h6")
    .allowTag("ul").allowTag("ol").allowTag("li")
    .allowTag("table").allowTag("thead").allowTag("tbody").allowTag("tr").allowTag("th").allowTag("td")
    .allowTag("del")
    .allowAttribute("ol", "start")
    .allowAttribute("th", "align").allowAttribute("td", "align")
    .disallowAttribute(null, "dir")
    .disallowAttribute(null, "tabindex")
    .disallowAttribute(null, "role")
    .disallowAttribute(null, "aria-label").disallowAttribute(null, "aria-hidden").disallowAttribute(null, "aria-expanded")
    .disallowAttribute(null, "aria-haspopup").disallowAttribute(null, "aria-controls").disallowAttribute(null, "aria-describedby")
    .disallowAttribute(null, "aria-labelledby").disallowAttribute(null, "aria-valuemin").disallowAttribute(null, "aria-valuemax")
    .disallowAttribute(null, "aria-valuenow")
    .disallowAttribute("a", "target")
    .disallowAttribute(null, "style")
    .disallowAttribute(null, "class")
    .allowAttribute("code", "class")
    .setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap)
    .makeReadOnly();

const languageClassRegExp = /^language-[\w-]+$/;
const tableOrCodeTagRegExp = /<(table|code)[\s>]/i;

export function parseMarkdown(text: string): string {
    const html = parse(text, {
        renderer: new CustomRenderer()
    }) as string;

    return applyMessageClasses(sanitizeHtml(html, chatMessageSanitizeOptions) ?? "");
}

/**
 * Gives every table the Bootstrap table classes and keeps a class on code
 * only when it names a language.
 *
 * @param html The sanitized message HTML.
 * @returns The message HTML with only these classes.
 */
function applyMessageClasses(html: string): string {
    // Every message renders again on each streamed chunk, so skip the
    // second parse for the many messages with no table or code.
    if (!tableOrCodeTagRegExp.test(html)) {
        return html;
    }

    // The sanitizer can't filter attribute values, so a raw table or code
    // element could otherwise keep a class like navbar-fixed-top.
    const body = new DOMParser().parseFromString(html, "text/html").body;

    Array.from(body.querySelectorAll("table")).forEach(table => {
        table.className = "table table-bordered table-striped";
    });

    Array.from(body.querySelectorAll("code[class]")).forEach(code => {
        if (!languageClassRegExp.test(code.getAttribute("class") ?? "")) {
            code.removeAttribute("class");
        }
    });

    return body.innerHTML;
}

class CustomRenderer extends Renderer {
    private linkDepth = 0;

    override link(token: Tokens.Link): string {
        this.linkDepth++;
        const html = super.link(token);
        this.linkDepth--;

        return html;
    }

    // Images become links so nothing loads from another site until the
    // person clicks. Inside a link, the alt text is enough, since an
    // anchor inside another anchor makes the browser close the outer one.
    override image(token: Tokens.Image): string {
        const altText = token.tokens ? this.parser.parseInline(token.tokens, this.parser.textRenderer) : token.text;
        const text = altText.trim() || token.href;

        if (this.linkDepth > 0) {
            return escapeHtml(text);
        }

        const title = token.title ? ` title="${escapeHtml(token.title)}"` : "";

        return `<a href="${escapeHtml(token.href)}"${title}>${escapeHtml(text)}</a>`;
    }

    override checkbox(token: Tokens.Checkbox): string {
        return token.checked ? "[x] " : "[ ] ";
    }
}

