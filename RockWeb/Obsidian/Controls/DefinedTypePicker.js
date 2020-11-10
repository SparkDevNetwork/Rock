Obsidian.Controls.registerCommonEntityPicker(
    'DefinedType',
    store => store.getters['definedTypes/all'].map(dt => ({
        key: dt.Guid,
        value: dt.Guid,
        text: dt.Name
    })));
