import { computePosition, flip, type Placement } from "@floating-ui/dom";

export function createIconButton(icon: string): HTMLButtonElement {
    const button = document.createElement('button');

    button.classList.add('bn-button');
    button.innerHTML = `<i class="${icon}"></i>`;

    return button;
}

interface IDisposable {
    onDispose(callback: () => void): void;
    dispose(): void;
}

export type HoverOptions = {
    autoDismiss?: boolean;
    autoDestroy?: boolean;
    menu?: boolean;
};

export class Hover implements IDisposable {
    private readonly element: HTMLElement;
    private readonly options: HoverOptions;
    private hidden: boolean = true;
    private readonly disposeCallbacks: (() => void)[] = [];

    constructor(content: HTMLElement, container: HTMLElement, options?: HoverOptions) {
        this.options = options || {};

        this.element = document.createElement("div");
        this.element.classList.add("bn-hover");
        this.element.style.display = "none";

        if (this.options.menu) {
            this.element.classList.add("bn-hover-menu");
        }

        this.element.append(content);

        container.append(this.element);
    }

    public show(anchor: HTMLElement | DOMRect, placement?: Placement): void {
        if (this.hidden) {
            this.element.style.display = "block";
            this.hidden = false;

            if (this.options.autoDismiss) {
                setTimeout(() => {
                    document.addEventListener("click", this.onDocumentClick);
                }, 0);
            }
        }

        const reference = "x" in anchor
            ? { getBoundingClientRect: () => anchor }
            : anchor;

        computePosition(reference, this.element, {
            placement: placement || "right",
            middleware: [
                flip(),
            ]
        })
            .then(({ x, y }) => {
                this.element.style.left = `${x}px`;
                this.element.style.top = `${y}px`;
            });
    }

    public static showMenu(menu: Menu, container: HTMLElement, anchor: HTMLElement): Hover {
        const hover = new Hover(menu.element, container, {
            menu: true,
            autoDestroy: true,
            autoDismiss: true
        });

        hover.onDispose(() => menu.dispose());
        hover.show(anchor);

        return hover;
    }

    public hide(): void {
        if (this.hidden) {
            return;
        }

        this.element.style.display = "none";
        this.hidden = true;

        document.removeEventListener("click", this.onDocumentClick);

        if (this.options.autoDestroy) {
            this.dispose();
        }
    }

    public dispose(): void {
        this.hide();
        this.element.remove();

        document.removeEventListener("click", this.onDocumentClick);

        this.disposeCallbacks.forEach(callback => callback());
        this.disposeCallbacks.length = 0;
    }

    public onDispose(callback: () => void): void {
        this.disposeCallbacks.push(callback);
    }

    onDocumentClick = (event: MouseEvent): void => {
        if (!this.element.contains(event.target as Node)) {
            this.hide();
        }
    }
}

export class Menu implements IDisposable {
    public readonly element: HTMLElement;

    private readonly disposeCallbacks: (() => void)[] = [];
    private readonly listeners: { element: HTMLElement, listener: EventListener }[] = [];

    constructor() {
        this.element = document.createElement("ul");
        this.element.classList.add("bn-menu");
    }

    public dispose(): void {
        this.element.remove();

        this.listeners.forEach(({ element, listener }) => {
            element.removeEventListener("click", listener);
        });
        this.listeners.length = 0;

        this.disposeCallbacks.forEach(callback => callback());
        this.disposeCallbacks.length = 0;
    }

    public addItem(text: string | { text: string, icon?: string, type?: "danger" }, listener: EventListener): void {
        const item = document.createElement("li");
        item.classList.add("bn-menu-item");

        if (typeof text === "string") {
            item.textContent = text;
        }
        else {
            if (text.icon) {
                const icon = document.createElement("i");
                icon.className = text.icon;
                item.prepend(icon);
            }

            const span = document.createElement("span");
            span.textContent = text.text;
            item.appendChild(span);

            if (text.type === "danger") {
                item.classList.add("bn-menu-item-danger");
            }
        }

        item.addEventListener("click", listener);
        item.addEventListener("mousedown", ev => ev.preventDefault());

        this.element.appendChild(item);
        this.listeners.push({ element: item, listener });
    }

    public onDispose(callback: () => void): void {
        this.disposeCallbacks.push(callback);
    }
}
