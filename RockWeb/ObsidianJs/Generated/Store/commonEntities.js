System.register(["./Generators.js"], function (exports_1, context_1) {
    "use strict";
    var Generators_js_1, commonEntities, commonEntityModules;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Generators_js_1_1) {
                Generators_js_1 = Generators_js_1_1;
            }
        ],
        execute: function () {
            // The common entity configs that will be used with generateCommonEntityModule to create store modules
            exports_1("commonEntities", commonEntities = [
                { namespace: 'campuses', apiUrl: '/api/obsidian/v1/commonentities/campuses' },
                { namespace: 'definedTypes', apiUrl: '/api/obsidian/v1/commonentities/definedTypes' }
            ]);
            exports_1("commonEntityModules", commonEntityModules = {});
            // Generate a module for each config
            for (var _i = 0, commonEntities_1 = commonEntities; _i < commonEntities_1.length; _i++) {
                var commonEntity = commonEntities_1[_i];
                commonEntityModules[commonEntity.namespace] = Generators_js_1.generateCommonEntityModule(commonEntity);
            }
        }
    };
});
//# sourceMappingURL=CommonEntities.js.map