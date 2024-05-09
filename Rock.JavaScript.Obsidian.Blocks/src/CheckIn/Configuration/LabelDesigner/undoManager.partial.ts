import { Ref, ref } from "vue";
import { LabelBag, LabelFieldBag } from "./types.partial";
import { Guid } from "@Obsidian/Types";
import { areEqual } from "@Obsidian/Utility/guid";

export class UndoSnapshotManager<T> {
    private readonly states: string[] = [];

    private currentStateIndex: number = 0;

    public constructor(initialState: T) {
        this.states.push(JSON.stringify(initialState));
    }

    public get canUndo(): boolean {
        return this.currentStateIndex > 0;
    }

    public get canRedo(): boolean {
        return this.currentStateIndex + 1 < this.states.length;
    }

    public snapshot(state: T): T {
        // Remove old redo state information that will be replaced.
        this.states.splice(this.currentStateIndex + 1, this.states.length - this.currentStateIndex - 1);

        this.states.push(JSON.stringify(state));
        this.currentStateIndex += 1;

        return this.getCurrentState();
    }

    public undo(): T {
        if (this.currentStateIndex > 0) {
            this.currentStateIndex -= 1;
        }

        return this.getCurrentState();
    }

    public redo(): T {
        if (this.currentStateIndex + 1 < this.states.length) {
            this.currentStateIndex += 1;
        }

        return this.getCurrentState();
    }

    public getCurrentState(): T {
        return JSON.parse(this.states[this.currentStateIndex]) as T;
    }
}

interface IReadOnlyRef<T> extends Ref<T> {
    readonly value: T;
}

export class ObjectUndoManager<TObject> {
    private snapshot: UndoSnapshotManager<TObject>;

    public readonly canUndo: IReadOnlyRef<boolean>;

    public readonly canRedo: IReadOnlyRef<boolean>;

    public readonly current: IReadOnlyRef<TObject>;

    public constructor(originalValue: TObject) {
        this.snapshot = new UndoSnapshotManager(originalValue);
        this.canUndo = ref(false);
        this.canRedo = ref(false);
        this.current = ref(originalValue) as Ref<TObject>;
    }

    public apply(fn: (value: TObject) => void): void {
        // Work on a copy so we don't corrupt the existing object if the
        // function errors half way through.
        const originalJson = JSON.stringify(this.current.value);
        const value = JSON.parse(originalJson);

        fn(value);

        // If nothing actually changed, don't try to take another snapshot
        // otherwise we lose our redo history. This happens when performing
        // and undo or redo operation because we then get notified of the change
        // again.
        if (JSON.stringify(value) === originalJson) {
            return;
        }

        this.snapshot.snapshot(value);
        this.updateStateValues();
    }

    public undo(): void {
        if (this.snapshot.canUndo) {
            this.snapshot.undo();
            this.updateStateValues();
        }
    }

    public redo(): void {
        if (this.snapshot.canRedo) {
            this.snapshot.redo();
            this.updateStateValues();
        }
    }

    private updateStateValues(): void {
        (this.current as Ref<TObject>).value = this.snapshot.getCurrentState();
        (this.canUndo as Ref<boolean>).value = this.snapshot.canUndo;
        (this.canRedo as Ref<boolean>).value = this.snapshot.canRedo;

        this.currentValueChanged();
    }

    protected currentValueChanged(): void {
        /* Intentionally blank so that child classes can override. */
    }
}

export class LabelUndoManager extends ObjectUndoManager<LabelBag> {
    private selectedFieldGuid?: Guid;

    public readonly selectedField: IReadOnlyRef<LabelFieldBag | undefined>;

    public constructor(label: LabelBag) {
        super(label);

        this.selectedField = ref<LabelFieldBag>();
    }

    public selectField(fieldGuid?: Guid | undefined): void {
        this.selectedFieldGuid = fieldGuid;
        (this.selectedField as Ref<LabelFieldBag | undefined>).value = this.current.value.fields
            ?.find(f => areEqual(f.guid, this.selectedFieldGuid));
    }

    public applyToSelectedField(fn: (value: LabelFieldBag) => void): void {
        this.apply(label => {
            const field = label.fields?.find(f => areEqual(f.guid, this.selectedFieldGuid));

            if (field) {
                fn(field);
            }
        });
    }

    protected override currentValueChanged(): void {
        super.currentValueChanged();

        (this.selectedField as Ref<LabelFieldBag | undefined>).value = this.current.value.fields
            ?.find(f => areEqual(f.guid, this.selectedFieldGuid));
    }
}
