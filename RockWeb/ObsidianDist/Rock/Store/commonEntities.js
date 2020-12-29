define(["require", "exports", "./Generators.js"], function (require, exports, Generators_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.commonEntityModules = exports.commonEntities = void 0;
    // The common entity configs that will be used with generateCommonEntityModule to create store modules
    exports.commonEntities = [
        { namespace: 'campuses', apiUrl: '/api/obsidian/v1/commonentities/campuses' },
        { namespace: 'definedTypes', apiUrl: '/api/obsidian/v1/commonentities/definedTypes' }
    ];
    exports.commonEntityModules = {};
    // Generate a module for each config
    for (var _i = 0, commonEntities_1 = exports.commonEntities; _i < commonEntities_1.length; _i++) {
        var commonEntity = commonEntities_1[_i];
        exports.commonEntityModules[commonEntity.namespace] = Generators_js_1.generateCommonEntityModule(commonEntity);
    }
});
//# sourceMappingURL=commonEntities.js.map