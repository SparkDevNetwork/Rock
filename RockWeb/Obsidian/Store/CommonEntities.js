System.register(["./Generators"], function (exports_1, context_1) {
    "use strict";
    var Generators_1, commonEntities, commonEntityModules;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Generators_1_1) {
                Generators_1 = Generators_1_1;
            }
        ],
        execute: function () {
            exports_1("commonEntities", commonEntities = [
                { namespace: 'campuses', apiUrl: '/api/v2/CommonEntities/Campuses' },
                { namespace: 'definedTypes', apiUrl: '/api/v2/CommonEntities/DefinedTypes' }
            ]);
            exports_1("commonEntityModules", commonEntityModules = {});
            for (var _i = 0, commonEntities_1 = commonEntities; _i < commonEntities_1.length; _i++) {
                var commonEntity = commonEntities_1[_i];
                commonEntityModules[commonEntity.namespace] = Generators_1.generateCommonEntityModule(commonEntity);
            }
        }
    };
});
//# sourceMappingURL=CommonEntities.js.map