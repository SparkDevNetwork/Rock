import { ComputedRef, Ref, DefineComponent, VNode, RendererNode, RendererElement, Slot, ComponentOptionsMixin, VNodeProps, AllowedComponentProps, ComponentCustomProps, UnwrapRef, WritableComputedRef } from 'vue';
import { ObjectSchema } from 'yup';

interface ValidationResult {
    errors: string[];
    valid: boolean;
}
declare type MaybeReactive<T> = Ref<T> | ComputedRef<T> | T;
declare type SubmitEvent = Event & {
    target: HTMLFormElement;
};
declare type GenericValidateFunction = (value: any) => boolean | string | Promise<boolean | string>;
interface FormState<TValues> {
    values: TValues;
    errors: Partial<Record<keyof TValues, string | undefined>>;
    dirty: Partial<Record<keyof TValues, boolean>>;
    touched: Partial<Record<keyof TValues, boolean>>;
    submitCount: number;
}
interface SetFieldValueOptions {
    force: boolean;
}
interface FormActions<TValues> {
    setFieldValue<T extends keyof TValues>(field: T, value: TValues[T], opts?: Partial<SetFieldValueOptions>): void;
    setFieldError: (field: keyof TValues, message: string | undefined) => void;
    setErrors: (fields: Partial<Record<keyof TValues, string | undefined>>) => void;
    setValues<T extends keyof TValues>(fields: Partial<Record<T, TValues[T]>>): void;
    setFieldTouched: (field: keyof TValues, isTouched: boolean) => void;
    setTouched: (fields: Partial<Record<keyof TValues, boolean>>) => void;
    setFieldDirty: (field: keyof TValues, isDirty: boolean) => void;
    setDirty: (fields: Partial<Record<keyof TValues, boolean>>) => void;
    resetForm: (state?: Partial<FormState<TValues>>) => void;
}
interface FormValidationResult<TValues> {
    errors: Partial<Record<keyof TValues, string>>;
    valid: boolean;
}
interface SubmissionContext<TValues extends Record<string, any> = Record<string, any>> extends FormActions<TValues> {
    evt: SubmitEvent;
}
declare type SubmissionHandler<TValues extends Record<string, any> = Record<string, any>> = (values: TValues, ctx: SubmissionContext<TValues>) => any;
interface FormContext<TValues extends Record<string, any> = Record<string, any>> extends FormActions<TValues> {
    register(field: any): void;
    unregister(field: any): void;
    values: TValues;
    fields: ComputedRef<Record<keyof TValues, any>>;
    submitCount: Ref<number>;
    schema?: Record<keyof TValues, GenericValidateFunction | string | Record<string, any>> | ObjectSchema<TValues>;
    validateSchema?: (shouldMutate?: boolean) => Promise<Record<keyof TValues, ValidationResult>>;
    validate(): Promise<FormValidationResult<TValues>>;
    meta: ComputedRef<{
        dirty: boolean;
        touched: boolean;
        valid: boolean;
        pending: boolean;
        initialValues: TValues;
    }>;
    isSubmitting: Ref<boolean>;
    handleSubmit(cb: SubmissionHandler<TValues>): (e?: SubmitEvent) => Promise<void>;
}

interface ValidationOptions {
    name?: string;
    values?: Record<string, any>;
    bails?: boolean;
    skipIfEmpty?: boolean;
    isInitial?: boolean;
}
/**
 * Validates a value against the rules.
 */
declare function validate(value: any, rules: string | Record<string, any> | GenericValidateFunction, options?: ValidationOptions): Promise<ValidationResult>;

interface FieldContext {
    field: string;
    value: any;
    form: Record<string, any>;
    rule?: {
        name: string;
        params?: Record<string, any> | any[];
    };
}
declare type ValidationRuleFunction = (value: any, params: any[] | Record<string, any>, ctx: FieldContext) => boolean | string | Promise<boolean | string>;
declare type ValidationMessageGenerator = (ctx: FieldContext) => string;

/**
 * Adds a custom validator to the list of validation rules.
 */
declare function defineRule(id: string, validator: ValidationRuleFunction): void;

interface VeeValidateConfig {
    bails: boolean;
    generateMessage: ValidationMessageGenerator;
    validateOnInput: boolean;
    validateOnChange: boolean;
    validateOnBlur: boolean;
    validateOnModelUpdate: boolean;
}
declare const configure: (newConf: Partial<VeeValidateConfig>) => void;

declare const Field: DefineComponent<{
    as: {
        type: (ObjectConstructor | StringConstructor)[];
        default: any;
    };
    name: {
        type: StringConstructor;
        required: true;
    };
    rules: {
        type: (ObjectConstructor | FunctionConstructor | StringConstructor)[];
        default: any;
    };
    validateOnMount: {
        type: BooleanConstructor;
        default: boolean;
    };
    validateOnBlur: {
        type: BooleanConstructor;
        default: any;
    };
    validateOnChange: {
        type: BooleanConstructor;
        default: any;
    };
    validateOnInput: {
        type: BooleanConstructor;
        default: any;
    };
    validateOnModelUpdate: {
        type: BooleanConstructor;
        default: any;
    };
    bails: {
        type: BooleanConstructor;
        default: () => boolean;
    };
    label: {
        type: StringConstructor;
        default: any;
    };
    uncheckedValue: {
        type: any;
        default: any;
    };
}, () => VNode<RendererNode, RendererElement, {
    [key: string]: any;
}> | Slot | VNode<RendererNode, RendererElement, {
    [key: string]: any;
}>[], unknown, {}, {}, ComponentOptionsMixin, ComponentOptionsMixin, Record<string, any>, string, VNodeProps & AllowedComponentProps & ComponentCustomProps, Readonly<{
    label: string;
    name: string;
    uncheckedValue: any;
    validateOnMount: boolean;
    bails: boolean;
    validateOnInput: boolean;
    validateOnChange: boolean;
    validateOnBlur: boolean;
    validateOnModelUpdate: boolean;
    as: any;
    rules: any;
} & {}>, {
    label: string;
    uncheckedValue: any;
    validateOnMount: boolean;
    bails: boolean;
    validateOnInput: boolean;
    validateOnChange: boolean;
    validateOnBlur: boolean;
    validateOnModelUpdate: boolean;
    as: any;
    rules: any;
}>;

declare const Form: DefineComponent<{
    as: {
        type: StringConstructor;
        default: string;
    };
    validationSchema: {
        type: ObjectConstructor;
        default: any;
    };
    initialValues: {
        type: ObjectConstructor;
        default: any;
    };
    initialErrors: {
        type: ObjectConstructor;
        default: any;
    };
    initialDirty: {
        type: ObjectConstructor;
        default: any;
    };
    initialTouched: {
        type: ObjectConstructor;
        default: any;
    };
    validateOnMount: {
        type: BooleanConstructor;
        default: boolean;
    };
}, (this: any) => VNode<RendererNode, RendererElement, {
    [key: string]: any;
}> | Slot | VNode<RendererNode, RendererElement, {
    [key: string]: any;
}>[], unknown, {}, {}, ComponentOptionsMixin, ComponentOptionsMixin, Record<string, any>, string, VNodeProps & AllowedComponentProps & ComponentCustomProps, Readonly<{
    validateOnMount: boolean;
    as: string;
    initialValues: Record<string, any>;
    validationSchema: Record<string, any>;
    initialErrors: Record<string, any>;
    initialDirty: Record<string, any>;
    initialTouched: Record<string, any>;
} & {}>, {
    validateOnMount: boolean;
    as: string;
    initialValues: Record<string, any>;
    validationSchema: Record<string, any>;
    initialErrors: Record<string, any>;
    initialDirty: Record<string, any>;
    initialTouched: Record<string, any>;
}>;

declare const ErrorMessage: DefineComponent<{
    as: {
        type: StringConstructor;
        default: any;
    };
    name: {
        type: StringConstructor;
        required: true;
    };
}, () => VNode<RendererNode, RendererElement, {
    [key: string]: any;
}> | Slot | VNode<RendererNode, RendererElement, {
    [key: string]: any;
}>[], unknown, {}, {}, ComponentOptionsMixin, ComponentOptionsMixin, Record<string, any>, string, VNodeProps & AllowedComponentProps & ComponentCustomProps, Readonly<{
    name: string;
    as: string;
} & {}>, {
    as: string;
}>;

interface FieldOptions<TValue = any> {
    initialValue: TValue;
    validateOnValueUpdate: boolean;
    validateOnMount?: boolean;
    bails?: boolean;
    type?: string;
    valueProp?: MaybeReactive<TValue>;
    uncheckedValue?: MaybeReactive<TValue>;
    label?: MaybeReactive<string>;
}
interface FieldState<TValue = any> {
    value: TValue;
    dirty: boolean;
    touched: boolean;
    errors: string[];
}
declare type RuleExpression = MaybeReactive<string | Record<string, any> | GenericValidateFunction>;
/**
 * Creates a field composite.
 */
declare function useField<TValue = any>(name: MaybeReactive<string>, rules?: RuleExpression, opts?: Partial<FieldOptions<TValue>>): {
    fid: number;
    name: MaybeReactive<string>;
    value: Ref<UnwrapRef<TValue>> | WritableComputedRef<TValue>;
    meta: {
        touched: boolean;
        dirty: boolean;
        valid: boolean;
        pending: boolean;
        initialValue?: any;
    };
    errors: Ref<string[]>;
    errorMessage: ComputedRef<string>;
    type: string;
    valueProp: any;
    uncheckedValue: any;
    checked: ComputedRef<boolean>;
    idx: number;
    resetField: (state?: Partial<FieldState<TValue>>) => void;
    handleReset: () => void;
    validate: () => Promise<ValidationResult>;
    handleChange: (e: unknown) => Promise<ValidationResult>;
    handleBlur: () => void;
    handleInput: (e: unknown) => void;
    setValidationState: (result: ValidationResult) => ValidationResult;
    setTouched: (isTouched: boolean) => void;
    setDirty: (isDirty: boolean) => void;
};

interface FormOptions<TValues extends Record<string, any>> {
    validationSchema?: Record<keyof TValues, GenericValidateFunction | string | Record<string, any>> | ObjectSchema<TValues>;
    initialValues?: MaybeReactive<TValues>;
    initialErrors?: Record<keyof TValues, string | undefined>;
    initialTouched?: Record<keyof TValues, boolean>;
    initialDirty?: Record<keyof TValues, boolean>;
    validateOnMount?: boolean;
}
declare function useForm<TValues extends Record<string, any> = Record<string, any>>(opts?: FormOptions<TValues>): {
    errors: ComputedRef<{ [P in keyof TValues]?: string; }>;
    meta: ComputedRef<{
        pending: boolean;
        dirty: boolean;
        touched: boolean;
        valid: boolean;
        initialValues: TValues;
    }>;
    values: TValues;
    isSubmitting: Ref<boolean>;
    submitCount: Ref<number>;
    validate: () => Promise<FormValidationResult<TValues>>;
    handleReset: () => void;
    resetForm: (state?: Partial<FormState<TValues>>) => void;
    handleSubmit: (fn?: SubmissionHandler<TValues>) => (e: unknown) => Promise<void>;
    submitForm: (e: unknown) => Promise<void>;
    setFieldError: (field: keyof TValues, message: string | undefined) => void;
    setErrors: (fields: Partial<Record<keyof TValues, string | undefined>>) => void;
    setFieldValue: <T extends keyof TValues = string>(field: T, value: TValues[T], { force }?: {
        force: boolean;
    }) => void;
    setValues: (fields: Partial<TValues>) => void;
    setFieldTouched: (field: keyof TValues, isTouched: boolean) => void;
    setTouched: (fields: Partial<Record<keyof TValues, boolean>>) => void;
    setFieldDirty: (field: keyof TValues, isDirty: boolean) => void;
    setDirty: (fields: Partial<Record<keyof TValues, boolean>>) => void;
};

declare function useResetForm<TValues extends Record<string, any> = Record<string, any>>(): (state?: Partial<FormState<TValues>>) => void;

/**
 * If a field is dirty or not
 */
declare function useIsFieldDirty(path?: MaybeReactive<string>): ComputedRef<boolean>;

/**
 * If a field is touched or not
 */
declare function useIsFieldTouched(path?: MaybeReactive<string>): ComputedRef<boolean>;

/**
 * If a field is validated and is valid
 */
declare function useIsFieldValid(path?: MaybeReactive<string>): ComputedRef<boolean>;

/**
 * If the form is submitting or not
 */
declare function useIsSubmitting(): ComputedRef<boolean>;

/**
 * Validates a single field
 */
declare function useValidateField(path?: MaybeReactive<string>): () => Promise<ValidationResult>;

/**
 * If the form is dirty or not
 */
declare function useIsFormDirty(): ComputedRef<boolean>;

/**
 * If the form is touched or not
 */
declare function useIsFormTouched(): ComputedRef<boolean>;

/**
 * If the form has been validated and is valid
 */
declare function useIsFormValid(): ComputedRef<boolean>;

/**
 * Validate multiple fields
 */
declare function useValidateForm<TValues extends Record<string, any> = Record<string, any>>(): () => Promise<FormValidationResult<TValues>>;

/**
 * The number of form's submission count
 */
declare function useSubmitCount(): ComputedRef<number>;

/**
 * Gives access to a field's current value
 */
declare function useFieldValue<TValue = any>(path?: MaybeReactive<string>): ComputedRef<TValue>;

/**
 * Gives access to a form's values
 */
declare function useFormValues<TValues extends Record<string, any> = Record<string, any>>(): ComputedRef<Partial<TValues>>;

/**
 * Gives access to all form errors
 */
declare function useFormErrors<TValues extends Record<string, any> = Record<string, any>>(): ComputedRef<{}>;

/**
 * Gives access to a single field error
 */
declare function useFieldError(path?: MaybeReactive<string>): ComputedRef<string>;

declare function useSubmitForm<TValues extends Record<string, any> = Record<string, any>>(cb: SubmissionHandler<TValues>): (e?: SubmitEvent) => Promise<void>;

export { ErrorMessage, Field, Form, FormActions, FormContext, FormState, FormValidationResult, ValidationResult, configure, defineRule, useField, useFieldError, useFieldValue, useForm, useFormErrors, useFormValues, useIsFieldDirty, useIsFieldTouched, useIsFieldValid, useIsFormDirty, useIsFormTouched, useIsFormValid, useIsSubmitting, useResetForm, useSubmitCount, useSubmitForm, useValidateField, useValidateForm, validate };
