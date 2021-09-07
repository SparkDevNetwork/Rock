window.RegisterObsidianPlugin = function (depPaths, callback) {
    depPaths = depPaths || [];

    System.register(depPaths, function (_export) {
        const deps = [];
        const setters = [];

        for (let i = 0; i < depPaths.length; i++) {
            deps.push(null);
            setters.push(function (depModule) {
                deps[i] = depModule;
            });
        }

        return {
            setters,
            execute: function () {
                _export(callback.apply(this, deps));
            }
        };
    });

};
