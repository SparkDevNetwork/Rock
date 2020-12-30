import { Component } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';

const fieldTypeComponentPaths: Record<Guid, Component> = {};

export type FieldTypeModule = {
    fieldTypeGuid: Guid;
    component: Component;
};

export function registerFieldType(fieldTypeGuid: Guid, component: Component) {
    const dataToExport: FieldTypeModule = {
        fieldTypeGuid: fieldTypeGuid,
        component: component
    };

    if (fieldTypeComponentPaths[fieldTypeGuid]) {
        console.error(`Field type "${fieldTypeGuid}" is already registered`);
    }
    else {
        fieldTypeComponentPaths[fieldTypeGuid] = component;
    }

    return dataToExport;
}

export function getFieldTypeComponent(fieldTypeGuid: Guid): Component | null {
    return fieldTypeComponentPaths[fieldTypeGuid];
}