(function ($) {
    "use strict";
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};
    Rock.controls.emailEditor = Rock.controls.emailEditor || {};
    Rock.controls.emailEditor.$currentImageComponent = $(false);

    var imageComponentAttributeKey = Object.freeze({
        assetStorageProviderId: "data-image-AssetStorageProviderId",
        filename: "data-image-filename",
        guid: "data-image-guid",
        height: "data-image-height",
        iconPath: "data-image-IconPath",
        id: "data-image-id",
        key: "data-image-Key",
        name: "data-image-name",
        resizeMode: "data-image-resizemode",
        url: "data-image-Url",
        width: "data-image-width",
    });

    var imageAttributeKey = Object.freeze({
        cssWidth: "data-imgcsswidth",
    });

    Rock.controls.emailEditor.imageComponentHelper = (function () {
        /**
         * Gets the image component jquery object.
         */
        function getImageComponent$() {
            return Rock.controls.emailEditor.$currentImageComponent;
        }

        /**
         * Gets the <img> jQuery object.
         */
        function getImage$() {
            return getImageComponent$().find("img");
        }

        /**
         * Gets a value determining whether an asset is shown in the component.
         */
        function isAssetShownInComponent() {
            var $imageComponent = getImageComponent$();
            return !!$imageComponent.attr(imageComponentAttributeKey.iconPath) || !!$imageComponent.attr(imageComponentAttributeKey.assetStorageProviderId);
        }

        /**
         * Gets a value determining whether an uploaded image is shown in the component.
         */
        function isImageShownInComponent() {
            return !isAssetShownInComponent();
        }

        /**
         * Shows a placeholder image in the component.
         */
        function showPlaceholderImageInComponent() {
            clearAssetDataAttributes();
            clearImageDataAttributes();

            getImageComponent$().html('<img src="/Assets/Images/image-placeholder.jpg" alt="" />');

            // Apply styles to image from the current image component settings.
            Rock.controls.emailEditor.imageComponentHelper.setImageCss();
        }

        /**
         * Clears the image component data attributes added for asset images.
         */
        function clearAssetDataAttributes() {
            var $imageComponent = getImageComponent$();
            var $img = getImage$();

            $imageComponent.removeAttr(imageComponentAttributeKey.assetStorageProviderId);
            $imageComponent.removeAttr(imageComponentAttributeKey.key);
            $imageComponent.removeAttr(imageComponentAttributeKey.iconPath);
            $imageComponent.removeAttr(imageComponentAttributeKey.name);
            $imageComponent.removeAttr(imageComponentAttributeKey.url);
            $img.removeAttr("src", undefined);
        }

        /**
         * Clears the image component data attributes added for uploaded images.
         */
        function clearImageDataAttributes() {
            var $imageComponent = getImageComponent$();
            var $img = getImage$();

            $imageComponent.attr(imageComponentAttributeKey.id, undefined);
            $imageComponent.attr(imageComponentAttributeKey.filename, undefined);
            $img.attr("src", undefined);
        }

        /**
         * A helper object for working with the image component properties on the right side of the email editor.
         */
        var propertyHelper = Object.freeze({
            getPropertiesContainer$: function () {
                return $("#emaileditor-properties");
            },

            width: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-imgcsswidth");
                },

                setValue: function (value) {
                    if (value === "full") {
                        this.getProperty$().val(1);
                    }
                    else {
                        this.getProperty$().val(0);
                    }
                },

                getValue: function () {
                    return this.getProperty$().val();
                }
            }),

            align: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-imagealign");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                }
            }),

            imageWidth: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-imagewidth");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsInt: function () {
                    return parseInt(this.getValue());
                }
            }),

            imageHeight: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-imageheight");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsInt: function () {
                    return parseInt(this.getProperty$().val());
                }
            }),

            resizeMode: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-resizemode");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                }
            }),

            altText: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-alt");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                }
            }),

            marginTop: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-margin-top");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsPixels: function () {
                    return Rock.controls.util.getValueAsPixels(this.getValue());
                }
            }),

            marginLeft: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-margin-left");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsPixels: function () {
                    return Rock.controls.util.getValueAsPixels(this.getValue());
                }
            }),

            marginRight: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-margin-right");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsPixels: function () {
                    return Rock.controls.util.getValueAsPixels(this.getValue());
                }
            }),

            marginBottom: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-margin-bottom");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                },

                getValueAsPixels: function () {
                    return Rock.controls.util.getValueAsPixels(this.getValue());
                }
            }),

            link: Object.freeze({
                getProperty$: function () {
                    return propertyHelper.getPropertiesContainer$().find("#component-image-link");
                },

                setValue: function (value) {
                    this.getProperty$().val(value);
                },

                getValue: function () {
                    return this.getProperty$().val();
                }
            }),

            image: (function () {
                function isOptionSelected($option) {
                    var classList = $option.attr("class");
                    return classList && classList.includes("btn-primary");
                }

                function selectOption($option) {
                    $option.removeClass("btn-default");
                    $option.addClass("btn-primary");
                }

                function deselectOption($option) {
                    $option.removeClass("btn-primary");
                    $option.addClass("btn-default");
                }

                function getButtonGroup$() {
                    return propertyHelper.getPropertiesContainer$().find(".js-btn-group-image-type");
                }

                function getAssetPicker$() {
                    return propertyHelper.getPropertiesContainer$().find(".js-component-asset-manager");
                }

                function getAssetUploaderRemoveButton$() {
                    return getAssetPicker$().find("a.js-picker-select-none");
                }

                function getAssetUploaderThumbnail$() {
                    return getAssetPicker$().find(".js-asset-thumbnail");
                }

                function getAssetUploaderThumbnailName$() {
                    return getAssetPicker$().find(".js-asset-thumbnail-name");
                }

                function getImageUploader$() {
                    return propertyHelper.getPropertiesContainer$().find("#componentImageUploader");
                }

                function getImageUploaderRemoveButton$() {
                    return getImageUploader$().find(".imageupload-remove a");
                }

                function getImageUploaderThumbnailImage$() {
                    return getImageUploader$().find(".imageupload-thumbnail-image");
                }

                return Object.freeze({
                    /**
                     * Sets the thumbnail value of the asset picker.
                     *
                     * Note: Use `clearAssetValue` for clearing the value.
                     * @param {string} thumbnailImageUrl The thumbnail image URL.
                     * @param {string} thumbnailFileName The thumbnail file name.
                     */
                    setAssetThumbnail: function (thumbnailImageUrl, thumbnailFileName) {
                        if (thumbnailImageUrl) {
                            getAssetUploaderThumbnail$().attr("style", "background-image:url('" + thumbnailImageUrl + "')");
                        }
                        else {
                            getAssetUploaderThumbnail$().attr("style", "");
                        }

                        var $assetThumbnailName = getAssetUploaderThumbnailName$();

                        $assetThumbnailName.text(thumbnailFileName);
                        $assetThumbnailName.attr("title", thumbnailFileName);

                        if (thumbnailFileName) {
                            $assetThumbnailName.removeClass("file-link-default");
                            $assetThumbnailName.attr("style", "background-color: transparent");
                        }
                        else {
                            $assetThumbnailName.addClass("file-link-default");
                            $assetThumbnailName.attr("style", "");
                        }
                    },

                    /**
                     * Sets the thumbnail value of the image uploader.
                     *
                     * Note: Use `clearImageValue` for clearing the value.
                     * @param {string} thumbnailImageUrl The thumbnail image path.
                     */
                    setImageThumbnail: function (thumbnailImageUrl) {
                        if (thumbnailImageUrl) {
                            getImageUploaderThumbnailImage$().css("background-image", "url('" + thumbnailImageUrl + "')");
                        }
                        else {
                            getImageUploaderThumbnailImage$().css("background-image", "");
                        }
                    },

                    /**
                     * Gets the "Image" option that switches from asset picker to image uploader.
                     */
                    getImageOption$: function () {
                        return getButtonGroup$().find(".js-image-picker-type-image");
                    },

                    /**
                     * Gets the "Asset" option that switches from image uploader to asset picker.
                     */
                    getAssetOption$: function () {
                        return getButtonGroup$().find(".js-image-picker-type-asset");
                    },

                    /**
                     * Gets a value determining whether the "Image" option is selected.
                     */
                    isImageOptionSelected: function () {
                        return isOptionSelected(this.getImageOption$());
                    },

                    /**
                     * Gets a value determining whether the "Asset" option is selected.
                     */
                    isAssetButtonSelected: function () {
                        return isOptionSelected(this.getAssetOption$());
                    },

                    /**
                     * Selects the "Asset" option and shows the asset picker.
                     */
                    selectAssetOption: function () {
                        selectOption(this.getAssetOption$());
                        deselectOption(this.getImageOption$());

                        getAssetPicker$().show();
                        getImageUploader$().hide();
                    },

                    /**
                     * Selects the "Image" option and shows the image uploader.
                     */
                    selectImageOption: function () {
                        selectOption(this.getImageOption$());
                        deselectOption(this.getAssetOption$());

                        getImageUploader$().show();
                        getAssetPicker$().hide();
                    },

                    /**
                     * Clears the asset picker value.
                     */
                    clearAssetValue: function () {
                        var $removeButton = getAssetUploaderRemoveButton$();

                        // If there is a remove button, then simulate a click to clear the asset.
                        if ($removeButton.length > 0) {
                            var href = $removeButton.attr("href");

                            if (href && href.startsWith("javascript:")) {
                                window.location.href = href;
                            }
                            else {
                                $removeButton.click();
                            }
                        }
                        // Otherwise, just clear the thumbnail and file name.
                        else {
                            this.setAssetThumbnail("", "");
                        }
                    },

                    /**
                     * Clears the image uploader value.
                     */
                    clearImageValue: function () {
                        var $removeButton = getImageUploaderRemoveButton$();

                        if ($removeButton) {
                            var href = $removeButton.attr("href");
                            if (href && href.startsWith("javascript:")) {
                                window.location.href = href;
                            }
                            else {
                                $removeButton.click();
                            }
                        }
                        else {
                            this.setImageThumbnail("/Assets/Images/no-picture.svg");
                        }
                    },

                    /**
                     * Hides the asset picker.
                     */
                    hideAssetPicker: function () {
                        getAssetPicker$().hide();
                    }
                });
            })()
        });

        var exports = {
            /**
             * Initializes event handlers for the image component.
             */
            initializeEventHandlers: function () {
                var self = this;

                // Set up button group handlers for switching between Image and Asset image uploader types.
                propertyHelper.image.getImageOption$().off("click").on("click", function (e) {
                    propertyHelper.image.clearImageValue();
                    propertyHelper.image.clearAssetValue();

                    propertyHelper.image.selectImageOption();

                    // Show the placeholder image each time the image picker type changes.
                    showPlaceholderImageInComponent();

                    return false;
                });
                propertyHelper.image.getAssetOption$().off("click").on("click", function (e) {
                    propertyHelper.image.clearImageValue();
                    propertyHelper.image.clearAssetValue();

                    propertyHelper.image.selectAssetOption();

                    // Show the placeholder image each time the image picker type changes.
                    showPlaceholderImageInComponent();

                    return false;
                });

                $("#component-image-imgcsswidth, #component-image-imagealign").on("change", function (e) {
                    self.setImageCss();
                });

                $("#component-image-imageheight, #component-image-imagewidth, #component-image-resizemode, #component-image-alt").on("change", function (e) {
                    if (isImageShownInComponent() || propertyHelper.image.isImageOptionSelected()) {
                        self.setImageSrc();
                    }
                    else if (isAssetShownInComponent() || propertyHelper.image.isAssetButtonSelected()) {
                        self.setAssetImageSrc();
                    }
                });

                $("#component-image-margin-top,#component-image-margin-left,#component-image-margin-right,#component-image-margin-bottom").on("change", function (e) {
                    $(this).val(parseFloat($(this).val()) || "");
                    self.setMargins();
                });

                $("#component-image-link").on("blur", function () {
                    self.setImageWrapAnchor();
                });
            },

            /**
             * Handles the event when an image component is clicked.
             *
             * This will assign the initial settings that configure the <img> element.
             *
             * @param {any} $imageComponent
             * @returns {any} Returns the $imageComponent back to the caller.
             */
            setProperties: function ($imageComponent) {
                // This assigns the global reference to the current image component selected in the wizard.
                Rock.controls.emailEditor.$currentImageComponent = $imageComponent;

                var $img = getImage$();

                if (isAssetShownInComponent()) {
                    var iconPath = $imageComponent.attr(imageComponentAttributeKey.iconPath);
                    var fileName = $imageComponent.attr(imageComponentAttributeKey.name) || "";
                    propertyHelper.image.setAssetThumbnail(iconPath, fileName);

                    // Ensure the "Asset" option is selected.
                    propertyHelper.image.selectAssetOption();
                } else {
                    propertyHelper.image.setImageThumbnail($img.attr("src") || "");

                    // Ensure the "Image" option is selected.
                    propertyHelper.image.selectImageOption();
                }

                propertyHelper.width.setValue($img.attr(imageAttributeKey.cssWidth) || "full");
                propertyHelper.align.setValue($imageComponent.css("text-align"));
                propertyHelper.imageWidth.setValue($imageComponent.attr(imageComponentAttributeKey.width));
                propertyHelper.imageHeight.setValue($imageComponent.attr(imageComponentAttributeKey.height));
                propertyHelper.resizeMode.setValue($imageComponent.attr(imageComponentAttributeKey.resizeMode));
                propertyHelper.altText.setValue($img.attr("alt"));

                var imageEl = $imageComponent[0];

                propertyHelper.marginTop.setValue(parseFloat(imageEl.style["margin-top"]) || "");
                propertyHelper.marginLeft.setValue(parseFloat(imageEl.style["margin-left"]) || "");
                propertyHelper.marginRight.setValue(parseFloat(imageEl.style["margin-right"]) || "");
                propertyHelper.marginBottom.setValue(parseFloat(imageEl.style["margin-bottom"]) || "");

                if ($img.parent().is("a")) {
                    propertyHelper.link.setValue($img.parent().attr("href"));
                }
                else {
                    propertyHelper.link.setValue("");
                }

                // Return this to the previous function caller.
                return $imageComponent;
            },

            /**
             * Handles the event when an image is picked or removed.
             *
             * @param {any} e The event.
             * @param {any} data The image data.
             * @returns {void}
             */
            handleImageUpdate: function (e, data) {
                this.setImageFromData(data);
            },

            /**
             * Handles the event when an asset is picked, removed, or when *any* post-back occurs.
             *
             * @param {any} e The event.
             * @param {any} data The asset data.
             */
            handleAssetUpdate: function (e, data) {
                if (data !== undefined && typeof (data) === "string") {
                    var parsedData = "";

                    if (data.includes("AssetStorageProviderId")) {
                        parsedData = JSON.parse(data);
                    }

                    // maybe check for an access denied error here and display it.
                    this.setAssetFromData(e, parsedData);
                }

                // This handler is called when *any* post-back occurs and will cause the asset picker to be visible.
                // Ensure the asset picker is hidden if an asset is not used in this image component.
                if (!propertyHelper.image.isAssetButtonSelected() && !isAssetShownInComponent()) {
                    propertyHelper.image.hideAssetPicker();
                }
            },

            /**
             * Styles the image component and <img> element according to its configured settings.
             */
            setImageCss: function () {
                var $img = getImage$();

                if (propertyHelper.width.getValue() === "0") {
                    $img.css("width", "auto");
                    $img.attr(imageAttributeKey.cssWidth, "image");
                }
                else {
                    $img.css("width", "100%");
                    $img.attr(imageAttributeKey.cssWidth, "full");
                }

                getImageComponent$().css("text-align", propertyHelper.align.getValue());
            },

            /**
             * Sets the image component element's margins according to its configured settings.
             */
            setMargins: function () {
                getImageComponent$()
                    .css("margin-top", propertyHelper.marginTop.getValueAsPixels())
                    .css("margin-left", propertyHelper.marginLeft.getValueAsPixels())
                    .css("margin-right", propertyHelper.marginRight.getValueAsPixels())
                    .css("margin-bottom", propertyHelper.marginBottom.getValueAsPixels());
            },

            /**
             * Updates the image component element's data attributes from the provided data,
             * and updates the <img> src attribute.
             *
             * @param {any} data The data used to updated the element.
             */
            setImageFromData: function (data) {
                var $imageComponent = getImageComponent$();
                var $img = getImage$();

                // Clear the data attributes on the img and image component elements that do not apply to images.
                clearAssetDataAttributes();

                // Store the data values as attributes on the img and image component elements.
                $imageComponent.attr(imageComponentAttributeKey.id, data ? data.response().result.Id : null);
                $imageComponent.attr(imageComponentAttributeKey.guid, data ? data.response().result.Guid : null);
                $imageComponent.attr(imageComponentAttributeKey.filename, data ? data.response().result.FileName : null);

                this.setImageSrc();
            },

            /**
             * Updates the image component element's data attributes from the provided data,
             * and updates the <img> src attribute.
             *
             * @param {any} e Unused event args.
             * @param {any} data The data used to updated the element.
             */
            setAssetFromData: function (e, data) {
                var $imageComponent = getImageComponent$();
                var $img = getImage$();

                // Clear the data attributes on the img and image component elements that do not apply to assets.
                clearImageDataAttributes();

                // Store the data values as attributes on the img and image component elements.
                $imageComponent.attr(imageComponentAttributeKey.assetStorageProviderId, data ? data.AssetStorageProviderId : null);
                $imageComponent.attr(imageComponentAttributeKey.key, data ? data.Key : null);
                $imageComponent.attr(imageComponentAttributeKey.iconPath, data ? data.IconPath : null);
                $imageComponent.attr(imageComponentAttributeKey.name, data ? data.Name : null);
                $imageComponent.attr(imageComponentAttributeKey.url, data ? data.Url : null);

                this.setAssetImageSrc();
            },

            setImageWrapAnchor: function () {
                var $img = getImage$();
                var imageLinkUrl = propertyHelper.link.getValue();

                if (imageLinkUrl) {
                    if ($img.parent().is("a")) {
                        $img.parent().attr("href", imageLinkUrl);
                    }
                    else {
                        var linkTag = '<a href="' + imageLinkUrl + '"></a>';
                        $img.wrap(linkTag);
                    }
                }
                else {
                    if ($img.parent().is("a")) {
                        $img.unwrap();
                    }
                }
            },

            /**
             * Sets the <img> src. Should only be called when the image uploader type is "Image".
             */
            setImageSrc: function () {
                if (!isImageShownInComponent() && !propertyHelper.image.isImageOptionSelected()) {
                    return;
                }

                var $imageComponent = getImageComponent$();

                var imageWidth = propertyHelper.imageWidth.getValueAsInt() || "";
                $imageComponent.attr(imageComponentAttributeKey.width, imageWidth);

                var imageHeight = propertyHelper.imageHeight.getValueAsInt() || "";
                $imageComponent.attr(imageComponentAttributeKey.height, imageHeight);

                var imageResizeMode = propertyHelper.resizeMode.getValue();
                $imageComponent.attr(imageComponentAttributeKey.resizeMode, imageResizeMode);

                var imageUrl;
                var binaryFileId = $imageComponent.attr(imageComponentAttributeKey.id);

                if (binaryFileId) {
                    imageUrl = Rock.settings.get("baseUrl") + "GetImage.ashx?isBinaryFile=T";
                    imageUrl += "&guid=" + $imageComponent.attr(imageComponentAttributeKey.guid);
                    imageUrl += "&fileName=" + $imageComponent.attr(imageComponentAttributeKey.filename);

                    if (imageWidth) {
                        imageUrl += "&width=" + imageWidth;
                    }
                    if (imageHeight) {
                        imageUrl += "&height=" + imageHeight;
                    }
                    if (imageResizeMode) {
                        imageUrl += "&mode=" + imageResizeMode;
                    }
                }
                else {
                    imageUrl = $($(".js-emaileditor-iframe").contents().find("#editor-toolbar").find(".component-image").attr("data-content")).prop("src");
                }

                if (imageUrl) {
                    getImage$()
                        .attr("src", imageUrl)
                        .attr("alt", propertyHelper.altText.getValue());
                }
                else {
                    showPlaceholderImageInComponent();
                }
            },

            /**
             * Sets the asset's img element CSS and other attributes (such as "src") from the controls that modify the image styles
             * as well as the current data attribute values on the img.
             */
            setAssetImageSrc: function () {
                if (!isAssetShownInComponent() && !propertyHelper.image.isAssetButtonSelected()) {
                    return;
                }

                var $imageComponent = getImageComponent$();

                var imageWidth = propertyHelper.imageWidth.getValueAsInt() || "";
                $imageComponent.css("width", imageWidth);
                $imageComponent.attr(imageComponentAttributeKey.width, imageWidth);

                var imageHeight = propertyHelper.imageHeight.getValueAsInt() || "";
                $imageComponent.css("height", imageHeight);
                $imageComponent.attr(imageComponentAttributeKey.height, imageHeight);

                var imageResizeMode = propertyHelper.resizeMode.getValue();
                $imageComponent.attr(imageComponentAttributeKey.resizeMode, imageResizeMode);

                var imageUrl = $imageComponent.attr(imageComponentAttributeKey.url);

                if (imageUrl) {
                    getImage$()
                        .attr("src", imageUrl)
                        .attr("alt", propertyHelper.altText.getValue());
                }
                else {
                    showPlaceholderImageInComponent();
                }
            }
        };

        return exports;
    }());
}(jQuery));


