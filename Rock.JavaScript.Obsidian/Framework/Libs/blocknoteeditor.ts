interface IObsidian {
    options: { fingerprint: string };
    nativeImport: (url: string) => Promise<unknown>;
}

interface IBlockNoteEditorModule {
    createEditor: (container: HTMLElement) => void;
}

let modulePromise: Promise<IBlockNoteEditorModule> | null = null;

export async function createEditor(container: HTMLElement): Promise<void> {
    if (!modulePromise) {
        const url = `/Scripts/Rock/blocknoteeditor.esm.js?${(window["Obsidian"] as IObsidian).options.fingerprint}`;
        modulePromise = (window["Obsidian"] as IObsidian).nativeImport(url) as Promise<IBlockNoteEditorModule>;
    }

    const module = await modulePromise;

    return module.createEditor(container);
}
