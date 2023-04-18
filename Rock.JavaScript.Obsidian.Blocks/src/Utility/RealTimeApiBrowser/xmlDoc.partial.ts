export class XmlDocMember {
    private document: Document;

    private elements: ElementDoc[];

    constructor(xml: string) {
        const parser = new DOMParser();
        this.document = parser.parseFromString(xml, "text/xml");

        if (this.document.documentElement.localName !== "member") {
            throw new Error("Invalid documentation member XML: Incorrect root element.");
        }

        if (!this.document.documentElement.getAttribute("name")) {
            throw new Error("Invalid documentation member XML: Missing member name attribute.");
        }

        this.elements = this.parseContent(this.document.documentElement);
    }

    private parseContent(xml: Node): ElementDoc[] {
        const elements: ElementDoc[] = [];

        xml.childNodes.forEach(node => {
            let element: ElementDoc | null = null;

            switch (node.nodeType) {
                case Node.ELEMENT_NODE:
                    if (!(node instanceof Element)) {
                        break;
                    }

                    element = this.parseElementNode(node);
                    break;

                case Node.TEXT_NODE:
                    element = new TextDoc(trimLines(node.textContent, true, " "));
                    break;

                case Node.CDATA_SECTION_NODE:
                    element = new TextDoc(node.textContent ?? "");
                    break;

                default:
                    break;
            }

            if (element) {
                elements.push(element);
            }
        });

        return elements;
    }

    private parseElementNode(node: Element): ElementDoc | null {
        const attributes = getNodeAttributes(node);

        switch (node.localName) {
            case "summary":
                return new SummaryDoc(this.parseContent(node), attributes);

            case "returns":
                return new ReturnsDoc(this.parseContent(node), attributes);

            case "example":
                return new ExampleDoc(this.parseContent(node), attributes);

            case "exception":
                return new ExceptionDoc(this.parseContent(node), attributes);

            case "see":
                return new SeeDoc(this.parseContent(node), attributes);

            case "seealso":
                return new SeeAlsoDoc(this.parseContent(node), attributes);

            case "param":
                return new ParamDoc(this.parseContent(node), attributes);

            case "paramref":
                return new ParamRefDoc(attributes);

            case "c":
                return new InlineCodeDoc(node.textContent ?? "", attributes);

            case "code":
                return new CodeDoc(trimLines(node.textContent, false, "\n"), attributes);

            default:
                return null;
        }
    }

    public getDoc(name: string, attributes?: Record<string, string>): ElementDoc | null {
        return this.elements
            .find(e => {
                if (e.nodeName !== name) {
                    return false;
                }

                if (attributes) {
                    for (const key of Object.keys(attributes)) {
                        if (e.attributes[key] !== attributes[key]) {
                            return false;
                        }
                    }
                }

                return true;
            }) ?? null;
    }
}

export interface IDocVisitor {
    visitText?(text: TextDoc): boolean | void;

    visitInlineCode?(code: InlineCodeDoc): boolean | void;

    visitCode?(code: CodeDoc): boolean | void;

    visitSummary?(summary: SummaryDoc): boolean | void;

    visitReturns?(returns: ReturnsDoc): boolean | void;

    visitException?(exception: ExceptionDoc): boolean | void;

    visitExample?(example: ExampleDoc): boolean | void;

    visitSee?(see: SeeDoc): boolean | void;

    visitSeeAlso?(see: SeeAlsoDoc): boolean | void;

    visitParam?(param: ParamDoc): boolean | void;

    visitParamRef?(paramRef: ParamRefDoc): boolean | void;
}

export abstract class ElementDoc {
    public readonly nodeName: string;

    public readonly attributes: Record<string, string>;

    constructor(nodeName: string, attributes: Record<string, string>) {
        this.nodeName = nodeName;
        this.attributes = attributes;
    }

    public abstract accept(visitor: IDocVisitor): void;

    public abstract toString(): string;
}

export class TextDoc extends ElementDoc {
    public readonly content: string;

    constructor(content: string) {
        super("#text", {});

        this.content = content;
    }

    public override accept(visitor: IDocVisitor): void {
        visitor.visitText?.(this);
    }

    public override toString(): string {
        return this.content;
    }
}

export class InlineCodeDoc extends ElementDoc {
    public readonly content: string;

    constructor(content: string, attributes: Record<string, string>) {
        super("c", attributes);

        this.content = content;
    }

    public override accept(visitor: IDocVisitor): void {
        visitor.visitInlineCode?.(this);
    }

    public override toString(): string {
        return this.content;
    }
}

export class CodeDoc extends ElementDoc {
    public readonly content: string;

    constructor(content: string, attributes: Record<string, string>) {
        super("code", attributes);

        this.content = content;
    }

    public override accept(visitor: IDocVisitor): void {
        visitor.visitCode?.(this);
    }

    public override toString(): string {
        return this.content;
    }
}

export class ParamRefDoc extends ElementDoc {
    public readonly name: string;

    constructor(attributes: Record<string, string>) {
        super("paramref", attributes);

        this.name = attributes["name"] ?? "";
    }

    public override accept(visitor: IDocVisitor): void {
        visitor.visitParamRef?.(this);
    }

    public override toString(): string {
        return this.name;
    }
}

export abstract class ContainerDoc extends ElementDoc {
    public readonly elements: ElementDoc[];

    constructor(nodeName: string, elements: ElementDoc[], attributes: Record<string, string>) {
        super(nodeName, attributes);

        this.elements = elements;
    }

    public override accept(visitor: IDocVisitor): void {
        for (const element of this.elements) {
            element.accept(visitor);
        }
    }

    public override toString(): string {
        return this.elements.map(e => e.toString().trim()).join(" ");
    }
}

export class SummaryDoc extends ContainerDoc {
    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("summary", elements, attributes);
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitSummary?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class ReturnsDoc extends ContainerDoc {
    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("returns", elements, attributes);
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitReturns?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class ExampleDoc extends ContainerDoc {
    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("example", elements, attributes);
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitExample?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class ParamDoc extends ContainerDoc {
    public readonly name: string;

    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("param", elements, attributes);

        this.name = attributes["name"] ?? "";
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitParam?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class ExceptionDoc extends ContainerDoc {
    public readonly cref: string;

    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("exception", elements, attributes);

        this.cref = attributes["cref"] ?? "";
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitException?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class SeeDoc extends ContainerDoc {
    public readonly cref: string;
    public readonly href: string;
    public readonly langword: string;

    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("See", elements, attributes);

        this.cref = attributes["cref"] ?? "";
        this.href = attributes["href"] ?? "";
        this.langword = attributes["langword"] ?? "";
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitSee?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

export class SeeAlsoDoc extends ContainerDoc {
    public readonly cref: string;
    public readonly href: string;

    constructor(elements: ElementDoc[], attributes: Record<string, string>) {
        super("SeeAlso", elements, attributes);

        this.cref = attributes["cref"] ?? "";
        this.href = attributes["href"] ?? "";
    }

    public override accept(visitor: IDocVisitor): void {
        if (visitor.visitSeeAlso?.(this) !== false) {
            super.accept(visitor);
        }
    }
}

function getNodeAttributes(node: Element): Record<string, string> {
    const attributes: Record<string, string> = {};

    for (const key of node.getAttributeNames()) {
        attributes[key] = node.getAttribute(key) ?? "";
    }

    return attributes;
}

function trimWhitespaceTo(content: string, index: number): string {
    if (!content) {
        return content;
    }

    for (let i = 0; i < index; i++) {
        if (content[i] !== " " && content[i] !== "\t") {
            return content.substring(i);
        }
    }

    return content.substring(index);
}

function trimLines(content: string | undefined | null, plainText: boolean, joinWith: string): string {
    if (content === undefined || content === null) {
        return "";
    }

    const lines = content.split(/\r\n|\n/).filter(l => !plainText || l !== "");

    if (lines.length === 0) {
        return "";
    }

    // Remove leading and trailing empty lines which are used for wrapping in the doc XML.
    if (lines[0].trim().length === 0) {
        if (plainText) {
            lines.splice(0, 1, "");
        }
        else {
            lines.splice(0, 1);
        }
    }

    if (lines.length === 0) {
        return "";
    }

    if (lines[lines.length - 1].trim().length === 0) {
        lines.splice(lines.length - 1, 1);
    }

    if (lines.length === 0) {
        return "";
    }

    // Get indent.
    let indent = lines[0].search(/[^\s]/);
    if (indent <= 4 && lines[0] !== "" && lines[0][0] !== "\t") {
        indent = 0;
    }

    if (indent < 0) {
        indent = 0;
    }

    const result = lines.map(line => {
        if (line === "") {
            return line;
        }
        else if (line.length < indent) {
            return "";
        }
        else {
            return trimWhitespaceTo(line, indent);
        }
    }).join(joinWith);

    return plainText
        ? result.replace(/\s{2,}/g, " ")
        : result;
}
