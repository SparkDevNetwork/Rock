export type GenericServerFunctions = {
    [name: string]: (...args: unknown[]) => unknown;
};

// eslint-disable-next-line @typescript-eslint/ban-types
export type ServerFunctions<T> = { [K in keyof T]: T[K] extends Function ? T[K] : never };

