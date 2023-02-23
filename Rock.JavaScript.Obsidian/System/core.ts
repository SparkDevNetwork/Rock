import "systemjs";

declare type ReadyCallbackFn = () => void;

/**
 * A map for a package name to it's URL path.
 */
declare type PackageMap = {
    /** The name of the package that will be used during imports. */
    package: string;

    /** The alias to be used instead of the requested import name. */
    alias: string;

    /**
     * True if this is a wild card package. That is, if a '*' character was
     * found that will be used for substitution.
     */
    wildcard: boolean;
};

/**
 * Describes the export names from the vendor bundle.
 */
const vendorMaps: Record<string, string> = {
    "ant-design-vue": "AntDesignVue",
    "axios": "Axios",
    "luxon": "Luxon",
    "mitt": "Mitt",
    "vue": "Vue",
    "tslib": "TSLib"
};

/**
 * Describes the alias maps for package paths.
 */
const packageMaps: Record<string, string> = {
    "@Obsidian/Controls/*": "/Obsidian/Controls/*",
    "@Obsidian/Core/*": "/Obsidian/Core/*",
    "@Obsidian/Directives/*": "/Obsidian/Directives/*",
    "@Obsidian/Enums/*": "/Obsidian/Enums/*.js",
    "@Obsidian/Libs/*": "/Obsidian/Libs/*",
    "@Obsidian/FieldTypes/*": "/Obsidian/FieldTypes/*",
    "@Obsidian/PageState": "/Obsidian/PageState.js",
    "@Obsidian/SystemGuids/*": "/Obsidian/SystemGuids/*",
    "@Obsidian/Templates/*": "/Obsidian/Templates/*",
    "@Obsidian/Utility/*": "/Obsidian/Utility.js",
    "@Obsidian/Utility": "/Obsidian/Utility.js",
    "@Obsidian/ValidationRules": "/Obsidian/ValidationRules.js"
};

/**
 * The options that obsidian should be initialized with.
 */
interface IObsidianOptions {
    fingerprint?: string;
}

/**
 * Handles initialization of the Obsidian framework. This allows for blocks
 * and pages to delay their own initialization until the Obsidian framework
 * is ready. Currently SystemJS and other base requirements are bundled together,
 * but that could change in the future to be asynchronous loading of components.
 */
class Obsidian {
    /** Functions that will need to be called once Obsidian is ready. */
    private readonly callbacks: Array<ReadyCallbackFn> = [];

    /** True if Obsidian has fully initialized and is ready to render. */
    private isReady: boolean = false;

    /** The options Obsidian was initialized with. */
    private options!: Required<IObsidianOptions>;

    /** The package maps that have been parsed. */
    private packageMaps: PackageMap[];

    /** Creates a new instance of the Obsidian core framework. */
    constructor() {
        this.packageMaps = [];

        for (const packageId in packageMaps) {
            this.packageMaps.push({
                package: packageId.replace("*", ""),
                alias: packageMaps[packageId],
                wildcard: packageId.indexOf("*") !== -1
            });
        }
    }

    /**
     * Registers a callback to be called when the Obsidian framework is ready.
     *
     * @param callback The callback.
     */
    public onReady(callback: ReadyCallbackFn): void {
        if (this.isReady) {
            callback();
        }
        else {
            this.callbacks.push(callback);
        }
    }

    /**
     * Configures SystemJS to append the default extension.
     */
    private configureDefaultExtension(): void {
        const origResolve = System.constructor.prototype.resolve;
        const defaultExtension = ".js";
        const expectedExtensions = [defaultExtension, ".ts", ".css", ".json"];

        System.constructor.prototype.resolve = function (moduleId: string, ...args: unknown[]): string {
            const moduleWithoutQuery = moduleId.indexOf("?") !== -1 ? moduleId.substr(0, moduleId.indexOf("?")) : moduleId;
            const isPackage = moduleId.indexOf("/") === -1 && moduleId.indexOf("\\") === -1;
            const hasExtension = expectedExtensions.some(ext => moduleWithoutQuery.endsWith(ext));

            return origResolve.call(
                this,
                (hasExtension || isPackage) ? moduleId : `${moduleId}${defaultExtension}`,
                ...args
            );
        };
    }

    /**
     * Configures SystemJS to append the default extension.
     */
    private configureThumbprint(): void {
        const origResolve = System.constructor.prototype.resolve;
        const defaultExtension = ".js";
        const expectedExtensions = [defaultExtension, ".ts", ".css", ".json"];
        const options = this.options;

        System.constructor.prototype.resolve = function (moduleId: string, ...args: unknown[]): string {
            let url = origResolve.call(this, moduleId, ...args) as string;

            const hasExtension = expectedExtensions.some(ext => url.endsWith(ext));

            if (hasExtension) {
                if (url.indexOf("?") === -1) {
                    url += `?${options.fingerprint}`;
                }
                else {
                    url += `&${options.fingerprint}`;
                }
            }

            return url;
        };
    }

    /**
     * Configures any modules that are bundled in this script file.
     */
    private configureBundledMaps(): void {
        // Instruct SystemJS to accept the module name if it is a mapped one.
        const originalResolve = System.constructor.prototype.resolve;
        System.constructor.prototype.resolve = function (id: string, parentUrl?: string): string {
            if (vendorMaps[id] !== undefined) {
                return id;
            }

            return originalResolve(id, parentUrl);
        };

        // Intercept a request to instantiate a new module and if it is one
        // of our mapped modules then just return it from the bundle.
        const originalInstantiate = System.constructor.prototype.instantiate;
        System.constructor.prototype.instantiate = async function (url: string, parentUrl?: string): Promise<unknown> {
            if (vendorMaps[url] !== undefined) {
                const module = await System.import("/Obsidian/obsidian-vendor.js", parentUrl);

                return [[], function (exportFn: System.ExportFn): System.Declare {
                    return {
                        execute(): void {
                            exportFn(module[vendorMaps[url]]);
                        }
                    };
                }];
            }
            else {
                return await originalInstantiate.call(this, url, parentUrl);
            }
        };
    }

    /**
     * Configures any modules that are aliased to specific locations.
     */
    private configurePackageMaps(): void {
        const originalResolve = System.constructor.prototype.resolve;
        const parsedPackageMaps = this.packageMaps;

        // Instruct SystemJS to accept the package alias if it is a package.
        System.constructor.prototype.resolve = function (id: string, parentUrl?: string): string {
            const map = parsedPackageMaps.find(pkg => id.startsWith(pkg.package));

            if (!map) {
                return originalResolve(id, parentUrl);
            }

            if (map.wildcard) {
                if (map.alias.indexOf("*.js") !== -1) {
                    const packageSuffix = id.substring(map.package.length).split("/")[0];
                    const newAlias = `${map.alias.replace("*", packageSuffix)}${id.substring(map.package.length + packageSuffix.length)}`;
                    return originalResolve(newAlias);
                }
                else if (map.alias.indexOf("*") !== -1) {
                    const packageSuffix = id.substring(map.package.length);
                    return originalResolve(map.alias.replace("*", packageSuffix));
                }
                else {
                    const packageSuffix = id.substring(map.package.length);
                    return originalResolve(`${map.alias}/${packageSuffix}`);
                }
            }
            else {
                return originalResolve(map.alias);
            }
        };

        // Intercept a request to instantiate a new module and if it is one
        // of our mapped modules then just return it from the bundle.
        const originalInstantiate = System.constructor.prototype.instantiate;
        System.constructor.prototype.instantiate = async function (url: string, parentUrl?: string): Promise<unknown> {
            if (url.indexOf(".js/") !== -1) {
                let newUrl = url.substring(0, url.indexOf(".js/") + 3);
                const path = url.substring(url.indexOf(".js/") + 4, url.indexOf("?") !== -1 ? url.indexOf("?") : undefined);

                if (url.indexOf("?") !== -1) {
                    newUrl = newUrl + url.substring(url.indexOf("?"));
                }

                let module = await System.import(newUrl, parentUrl);

                return [[], function (exportFn: System.ExportFn): System.Declare {
                    return {
                        execute(): void {
                            const segments = path.split("/");

                            for (let i = 0; i < segments.length; i++) {
                                let segment = segments[i];

                                if (segment.endsWith(".js")) {
                                    segment = segment.substring(0, segment.length - 3);
                                }

                                //segment = segment[0].toUpperCase() + segment.substring(1);

                                module = module[segment];
                            }

                            exportFn(module);
                        }
                    };
                }];
            }
            else {
                return await originalInstantiate.call(this, url, parentUrl);
            }
        };
    }

    /**
     * Initialize the framework.
     */
    public init(options?: IObsidianOptions): void {
        this.options = {
            fingerprint: options?.fingerprint ?? ""
        };

        this.configureDefaultExtension();

        if (this.options.fingerprint !== "") {
            this.configureThumbprint();
        }

        this.configureBundledMaps();
        this.configurePackageMaps();

        this.isReady = true;

        // The concept of the callbacks is here due to backwards compatibility
        // as well as allowing for later requiring dynamically loading other
        // pre-requisites before initializing Obsidian.
        for (const callback of this.callbacks) {
            callback();
        }
    }
}

export default new Obsidian();
