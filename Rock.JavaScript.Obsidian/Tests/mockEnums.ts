export function mockEnums(mocker: (moduleName: string, factory?: () => unknown, options?: jest.MockOptions) => typeof jest): void {
    mocker("@Obsidian/Types/Controls/controlLazyMode", () => ({
        ControlLazyMode: {
            Lazy: "lazy",
            Eager: "eager",
            OnDemand: "onDemand"
        }
    }));

    mocker("@Obsidian/Types/Controls/pickerDisplayStyle", () => ({
        PickerDisplayStyle: {
            Auto: "auto",
            List: "list",
            Condensed: "condensed"
        }
    }));
}