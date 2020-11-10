Obsidian.Controls.registerCommonEntityPicker(
    'Campus',
    store => store.getters['campuses/all'].map(c => ({
        key: c.Guid,
        value: c.Guid,
        text: c.Name
    })));
