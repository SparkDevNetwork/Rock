(function () {
    window.Obsidian = window.Obsidian || {};

    /**
     * If there is already a component with the given name, then an error is thrown.
     * @param {string} name
     * @param {object} namespaceObject
     */
    const confirmNameAvailability = (name, namespaceObject) => {
        if (!name) {
            throw `A component without a name cannot be registered`;
        }

        if (name in namespaceObject) {
            throw `A component using the key "${name}" is already registered`;
        }

        return;
    };

    /**
     * Templates are Vue components that have no (or extrememly limited) local state. Their purpose is to provide HTML
     * patterns. Templates are a base layer dependency and should not use other components except for other templates.
     * An example would be a paneled block or an alert (HTML pattern only).
     */
    Obsidian.Templates = {
        /**
         * Add a template to the Obsidian template collection
         * @param {object} template
         */
        registerTemplate(template) {
            confirmNameAvailability(template.name, Obsidian.Templates);
            Obsidian.Templates[template.name] = template;
        },
    };

    /**
     * Elements are Vue components that are self contained except perhaps to use a template. They should not use APIs,
     * or other functionality. Consider elements to be one step above raw HTML inputs: adding a label, emiting events,
     * and very fundamental logic. An example would be a DropDownList (a select element with label and option tag
     * rendering).
     */
    Obsidian.Elements = {
        /**
         * Add an element to the Obsidian element collection
         * @param {object} element
         */
        registerElement(element) {
            confirmNameAvailability(element.name, Obsidian.Elements);
            Obsidian.Elements[element.name] = element;
        },
    };

    /**
     * Controls are Vue components that can use templates and elements. Controls are a step above elements and may contain
     * business logic and make API calls.
     */
    Obsidian.Controls = {
        /**
         * Add a control to the Obsidian control collection
         * @param {object} control
         */
        registerControl(control) {
            confirmNameAvailability(control.name, Obsidian.Controls);
            Obsidian.Controls[control.name] = control;
        },

        /**
         * Generate and add a common entity picker. Common entities are those stored in the Obsidian store.
         * @param {any} entityName The entity name (ex: Campus)
         * @param {any} getOptionsFunc A function called with the store as a parameter that should return the
         * options object list for the drop down list.
         */
        registerCommonEntityPicker(entityName, getOptionsFunc) {
            Obsidian.Controls.registerControl({
                name: `${entityName}Picker`,
                components: {
                    DropDownList: Obsidian.Elements.DropDownList
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    required: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        internalValue: '',
                        isLoading: false
                    };
                },
                computed: {
                    options() {
                        return getOptionsFunc(this.$store);
                    }
                },
                methods: {
                    onChange: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                watch: {
                    value: function () {
                        this.internalValue = this.modelValue;
                    }
                },
                template:
                    `<DropDownList v-model="internalValue" @change="onChange" :disabled="isLoading" :label="label" :options="options" />`
            });
        }
    };

    /**
     * Fields are controls, but more dynamically sourced, for the purpose of automatic form generation from server side FieldTypes. For example: Fields
     * are commonly used for generating a block attribute settings form.
     */
    Obsidian.Fields = {
        /**
         * Add a field to the Obsidian field collection
         * @param {string} fieldTypeGuid
         * @param {object} field
         */
        registerField(fieldTypeGuid, field) {
            confirmNameAvailability(field.name, Obsidian.Fields);
            confirmNameAvailability(fieldTypeGuid, Obsidian.Fields);

            Obsidian.Fields[field.name] = field;
            Obsidian.Fields[fieldTypeGuid] = field;
        },
    };

    /**
     * Block components are the front end objects responsible for interfacing with server side blocks. These serve the same purpose as
     * traditional DotNet Framework RockBlocks and can contain business logic, controls, elements, templates, etc.
     */
    Obsidian.Blocks = {
        /**
         * Add a block to the Obsidian block collection
         * @param {object} block 
         */
        registerBlock(block) {
            confirmNameAvailability(block.name, Obsidian.Blocks);

            if (block.props) {
                throw `${block.name} has props. Block components cannot have props.`;
            }

            Obsidian.Blocks[block.name] = block;
        },
    };

    /**
     * This is the AJAX interface to allow communication to the Rock server from the browser. It is centralized to common functions (rather than
     * simply using Axios within individual components) to allow tools like performance metrics to provide better insights.
     */
    Obsidian.Http = {
        /**
         * Make an API call. This is only place Axios (or AJAX library) should be referenced to allow tools like performance metrics to provide
         * better insights.
         * @param {string} method The HTTP method, such as GET
         * @param {string} url The endpoint to access, such as /api/campuses/
         * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
         * @param {any} data This will be the body of the request
         */
        doApiCall(method, url, params, data) {
            return axios({ method, url, data, params });
        },

        /**
         * Make a GET HTTP request
         * @param {string} url The endpoint to access, such as /api/campuses/
         * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
         */
        get(url, params) {
            return Obsidian.Http.doApiCall('GET', url, params);
        },

        /**
         * Make a POST HTTP request
         * @param {string} url The endpoint to access, such as /api/campuses/
         * @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
         * @param {any} data This will be the body of the request
         */
        post(url, params, data) {
            return Obsidian.Http.doApiCall('POST', url, params, data);
        }
    };

    let _isPageReady = false;
    const _onPageReadyCallbacks = [];

    /**
     * Schedules a callback to be executed once the page has been initialized.
     * @param {Function} callback
     */
    Obsidian.onPageReady = function (callback) {
        if (_isPageReady) {
            callback();
        }

        _onPageReadyCallbacks.push(callback);
    };

    /**
     * This should be called once per page with data from the server that pertains to the entire page. This includes things like
     * page parameters and context entities.
     * @param {object} pageData
     */
    Obsidian.initializePage = async function (pageData) {
        await Obsidian.Store.dispatch('initialize', { pageData });
        await Vue.nextTick();
        _isPageReady = true;

        while (_onPageReadyCallbacks.length) {
            const callback = _onPageReadyCallbacks.pop();
            callback();
        }
    };

    /**
     * This should be called once per block on the page. The config contains configuration provided by the block's server side logic
     * counterpart.  This adds the block to the page and allows it to begin initializing.
     * @param {object} config
     */
    Obsidian.initializeBlock = function (config) {
        return Vue.createApp({
            name: `Root.${config.blockFileIdentifier}`,
            components: {
                RockBlock: Obsidian.Controls.RockBlock
            },
            data() {
                return {
                    config: config
                };
            },
            template: `<RockBlock :config="config" />`
        })
            .use(Obsidian.Store)
            .mount(config.rootElement);
    };
})();
