import { Ref, ref } from "vue";
import { Guid } from "@Obsidian/Types";
import { areEqual } from "@Obsidian/Utility/guid";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";
import { LabelDetail } from "./types.partial";

/**
 * Simple interface to force a reference to be read-only so it can't be
 * changed by callers.
 */
interface IReadOnlyRef<T> extends Ref<T> {
    readonly value: T;
}

/**
 * Simple snapshot state undo/redo support. This takes a JSON snapshot of the
 * object and will then restore that object later. This should not be used with
 * extremely large objects as that will consume a lot of memory.
 */
export class UndoSnapshotManager<T> {
    /** The snapshots that have been taken, including the original value. */
    private readonly snapshots: string[] = [];

    /** The snapshots index that represents the current state. */
    private currentStateIndex: number = 0;

    /**
     * Creates a new snapshot manager for the object.
     *
     * @param initialState The initial object state.
     */
    public constructor(initialState: T) {
        this.snapshots.push(JSON.stringify(initialState));
    }

    /**
     * `true` if the manager can undo and return to a previous state.
     */
    public get canUndo(): boolean {
        return this.currentStateIndex > 0;
    }

    /**
     * `true` if the manager can redo and return to a state prior to undo.
     */
    public get canRedo(): boolean {
        return this.currentStateIndex + 1 < this.snapshots.length;
    }

    /**
     * Takes a snapshot of the object and places it in the undo stack. If any
     * data exists forward in the stack (such as a previous undo operation)
     * then those snapshots will be lost.
     *
     * @param state The object state that should have a snapshot taken.
     */
    public snapshot(state: T): void {
        // Remove old redo state information that will be replaced.
        this.snapshots.splice(this.currentStateIndex + 1, this.snapshots.length - this.currentStateIndex - 1);

        this.snapshots.push(JSON.stringify(state));
        this.currentStateIndex += 1;
    }

    /**
     * Moves back to the previous snapshot.
     */
    public undo(): void {
        if (this.currentStateIndex > 0) {
            this.currentStateIndex -= 1;
        }
    }

    /**
     * Moves forward to the next snapshot available in the stack.
     */
    public redo(): void {
        if (this.currentStateIndex + 1 < this.snapshots.length) {
            this.currentStateIndex += 1;
        }
    }

    /**
     * Gets the object state as represented by the current snapshot. Modifying
     * this object does nothing until you call the snapshot function.
     *
     * @returns The current object state being tracked.
     */
    public getCurrentState(): T {
        return JSON.parse(this.snapshots[this.currentStateIndex]) as T;
    }
}

/**
 * Provides object-state undo management. This happens by way of snapshots after
 * modifications are requested via the {@link mutate} function.
 */
export class ObjectUndoManager<TObject> {
    /** The object managing the snapshots. */
    private snapshot: UndoSnapshotManager<TObject>;

    /**
     * `true` if an undo to an older state of the object is possible.
     */
    public readonly canUndo: IReadOnlyRef<boolean>;

    /**
     * `true` if a redo operation to a newer state of the object is possible.
     */
    public readonly canRedo: IReadOnlyRef<boolean>;

    /**
     * The current value being tracked. Any changes made directly to this
     * object may be lost. To make changes call the {@link mutate} function.
     */
    public readonly current: IReadOnlyRef<TObject>;

    /**
     * Creates a new manager instance to track undo/redo logic for the object.
     *
     * @param initialValue The initial value to begin tracking.
     */
    public constructor(initialValue: TObject) {
        this.snapshot = new UndoSnapshotManager(initialValue);
        this.canUndo = ref(false);
        this.canRedo = ref(false);
        this.current = ref(initialValue) as Ref<TObject>;
    }

    /**
     * Mutates an object and then creates a new undo/redo state after the
     * changes have been completed.
     *
     * @param fn The function to call to mutate the object. The object to be mutated will be passed as the parameter.
     */
    public mutate(fn: (value: TObject) => void): void {
        // Work on a copy so we don't corrupt the existing object if the
        // function errors half way through.
        const originalJson = JSON.stringify(this.current.value);
        const value = JSON.parse(originalJson);

        fn(value);

        // If nothing actually changed, don't try to take another snapshot
        // otherwise we lose our redo history. This happens when performing
        // and undo or redo operation because we often get notified of the change
        // again.
        if (JSON.stringify(value) === originalJson) {
            return;
        }

        this.snapshot.snapshot(value);
        this.updateStateValues();
    }

    /**
     * Moves back to a previous state of the object.
     */
    public undo(): void {
        if (this.snapshot.canUndo) {
            this.snapshot.undo();
            this.updateStateValues();
        }
    }

    /**
     * Moves forward to a future state of the object.
     */
    public redo(): void {
        if (this.snapshot.canRedo) {
            this.snapshot.redo();
            this.updateStateValues();
        }
    }

    /**
     * Update the ref values and notify any subclasses that values have changed
     * so they can perform any updates of their own.
     */
    private updateStateValues(): void {
        (this.current as Ref<TObject>).value = this.snapshot.getCurrentState();
        (this.canUndo as Ref<boolean>).value = this.snapshot.canUndo;
        (this.canRedo as Ref<boolean>).value = this.snapshot.canRedo;

        this.currentValueChanged();
    }

    /**
     * Called by the super object when the current value property has changed.
     */
    protected currentValueChanged(): void {
        // Intentionally blank so that child classes can override.
    }
}

/**
 * Undo manager for the {@link LabelDetail} object type. This provides some
 * additional functionality to make it easier to modify labels.
 */
export class LabelUndoManager extends ObjectUndoManager<LabelDetail> {
    /** The currently selected field identifier. */
    private selectedFieldGuid?: Guid;

    /** A reference to the currently selected field or undefined. */
    public readonly selectedField: IReadOnlyRef<LabelFieldBag | undefined>;

    /**
     * Creates a new undo manager for {@link LabelDetail}.
     *
     * @param label The initial label content.
     */
    public constructor(label: LabelDetail) {
        super(label);

        this.selectedField = ref<LabelFieldBag>();
    }

    /**
     * Selects or deselects a field in the label.
     *
     * @param fieldGuid The field to be selected or `undefined` to deselect.
     */
    public selectField(fieldGuid?: Guid | undefined): void {
        this.selectedFieldGuid = fieldGuid;

        const field = this.current.value.labelData.fields
            .find(f => areEqual(f.guid, this.selectedFieldGuid));

            (this.selectedField as Ref<LabelFieldBag | undefined>).value = field;
    }

    /**
     * Mutates the currently selected field and then creates a new undo/redo
     * checkpoint for the label.
     *
     * @param fn The function to call to mutate the field.
     */
    public mutateSelectedField(fn: (value: LabelFieldBag) => void): void {
        this.mutate(label => {
            const field = label.labelData.fields
                .find(f => areEqual(f.guid, this.selectedFieldGuid));

            if (field) {
                fn(field);
            }
        });
    }

    protected override currentValueChanged(): void {
        super.currentValueChanged();

        const field = this.current.value.labelData.fields
            .find(f => areEqual(f.guid, this.selectedFieldGuid));

        (this.selectedField as Ref<LabelFieldBag | undefined>).value = field;
    }
}
