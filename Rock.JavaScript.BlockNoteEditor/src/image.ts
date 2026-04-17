import { createBlockConfig, createBlockSpec, createImageBlockConfig as createImageBlockConfigBN, imageParse, imageRender, imageToExternalHTML, type ImageOptions } from "@blocknote/core";

const baseImageBlockConfig = createImageBlockConfigBN({});

// Creates an image block spec with the additional fileGuid option.
const createImageBlockConfig = createBlockConfig(
    (_ctx: ImageOptions = {}) => {
        const config = {
            type: baseImageBlockConfig.type,
            propSchema: {
                ...baseImageBlockConfig.propSchema,

                fileGuid: {
                    default: undefined,
                    type: "string" as const,
                },
            },
            content: baseImageBlockConfig.content,
        };

        return config;
    }
);

export const createImageBlockSpec = createBlockSpec(
    createImageBlockConfig,
    (config) => ({
        meta: {
            fileBlockAccept: ["image/*"],
        },
        parse: imageParse(config),
        render: imageRender(config) as any,
        toExternalHTML: imageToExternalHTML(config) as any,
        runsBefore: ["file"],
    })
);
