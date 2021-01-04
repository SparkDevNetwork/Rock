import { Component } from '../Vendor/Vue/vue.js';
import { Guid, normalize } from '../Util/Guid.js';

const fieldTypeComponentPaths: Record<Guid, Component> = {};

export type FieldTypeModule = {
    fieldTypeGuid: Guid;
    component: Component;
};

export function registerFieldType(fieldTypeGuid: Guid, component: Component) {
    const normalizedGuid = normalize(fieldTypeGuid);

    const dataToExport: FieldTypeModule = {
        fieldTypeGuid: normalizedGuid,
        component: component
    };

    if (fieldTypeComponentPaths[normalizedGuid]) {
        console.error(`Field type "${fieldTypeGuid}" is already registered`);
    }
    else {
        fieldTypeComponentPaths[normalizedGuid] = component;
    }

    return dataToExport;
}

export function getFieldTypeComponent(fieldTypeGuid: Guid): Component | null {
    const field = fieldTypeComponentPaths[normalize(fieldTypeGuid)];

    if (field) {
        return field;
    }

    console.error(`Field type "${fieldTypeGuid}" was not found`);
    return null;
}