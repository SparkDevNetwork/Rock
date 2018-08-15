/*! animate.js v1.3.2 | (c) 2018 Josh Johnson | https://github.com/jshjohnson/animate.js */
(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(factory);
    } else if (typeof exports === 'object') {
        module.exports = factory(root);
    } else {
        root.Animate = factory();
    }
}(this, function() {
    'use strict';

    var Animate = function(userOptions) {
        var el = document.createElement('fakeelement');
        var defaultOptions = {
            target: '[data-animate]',
            animatedClass: 'js-animated',
            offset: [0.5, 0.5],
            delay: 0,
            remove: true,
            scrolled: false,
            reverse: false,
            onLoad: true,
            onScroll: true,
            onResize: false,
            disableFilter: null,
            callbackOnInit: function() {},
            callbackOnInView: function() {},
            callbackOnAnimate: function() {},
        };

        this.supports = 'querySelector' in document && 'addEventListener' in window && 'classList' in el && Function.prototype.bind;
        this.options = this._extend(defaultOptions, userOptions || {});
        this.elements = document.querySelectorAll(this.options.target);
        this.initialised = false;

        this.verticalOffset = this.options.offset;
        this.horizontalOffset = this.options.offset;

        // Offset can be [y, x] or the same value can be used for both
        if (this._isType('Array', this.options.offset)) {
            this.verticalOffset = this.options.offset[0];
            this.horizontalOffset = this.options.offset[1] ? this.options.offset[1] : this.options.offset[0];
        }

        this.throttledEvent = this._debounce(function() {
            this.render();
        }.bind(this), 15);
    };

    // Returns a function, that, as long as it continues to be invoked, will not
    // be triggered. The function will be called after it stops being called for
    // N milliseconds. If `immediate` is passed, trigger the function on the
    // leading edge, instead of the trailing.
    // @private
    // @author David Walsh
    // @link https://davidwalsh.name/javascript-debounce-function
    Animate.prototype._debounce = function(func, wait, immediate) {
        var timeout;
        return function() {
            var context = this;
            var args = arguments;
            var later = function() {
                timeout = null;
                if (!immediate) {
                    func.apply(context, args);
                }
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) {
                func.apply(context, args);
            }
        };
    };

    /**
     * Merges unspecified amount of objects into new object
     * @private
     * @return {Object} Merged object of arguments
     */
    Animate.prototype._extend = function() {
        var extended = {};
        var length = arguments.length;

        /**
         * Merge one object into another
         * @param  {Object} obj  Object to merge into extended object
         */
        var merge = function(obj) {
            for (var prop in obj) {
                if (Object.hasOwnProperty.call(obj, prop)) {
                    extended[prop] = obj[prop];
                }
            }
        };

        // Loop through each passed argument
        for (var i = 0; i < length; i++) {
            // Store argument at position i
            var obj = arguments[i];

            // If we are in fact dealing with an object, merge it. Otherwise throw error
            if (this._isType('Object', obj)) {
                merge(obj);
            } else {
                console.error('Custom options must be an object');
            }
        }

        return extended;
    };

    /**
     * Determines when an animation has completed
     * @author  David Walsh
     * @link https://davidwalsh.name/css-animation-callback
     * @private
     * @return {String} Appropriate 'animationEnd' event for browser to handle
     */
    Animate.prototype._whichAnimationEvent = function() {
        var t;
        var el = document.createElement('fakeelement');

        var animations = {
            animation: 'animationend',
            OAnimation: 'oAnimationEnd',
            MozAnimation: 'animationend',
            WebkitAnimation: 'webkitAnimationEnd'
        };

        for (t in animations) {
            if (Object.hasOwnProperty.call(animations, t)) {
                if (el.style[t] !== undefined) {
                    return animations[t];
                }
            }
        }
    };

    /**
     * Determines whether we have already scrolled past the element
     * @param  {HTMLElement}  el Element to test
     * @return {Boolean}
     */
    Animate.prototype._isAboveScrollPos = function(el) {
        var dimensions = el.getBoundingClientRect();
        var scrollPos = (window.scrollY || window.pageYOffset);

        return (dimensions.top + (dimensions.height * this.verticalOffset) < scrollPos);
    };

    /**
     * Determines the offset for a particular element considering
     * any attribute overrides. Falls back to config options otherwise
     * @param  {HTMLElement} el Element to get offset for
     * @return {Array}    An offset array of [Y,X] offsets
     */
    Animate.prototype._getElementOffset = function(el) {
        var elementOffset = el.getAttribute('data-animation-offset');
        var elementOffsetArray = [this.verticalOffset, this.horizontalOffset];

        if (elementOffset) {
            var stringArray = elementOffset.split(',');
            if (stringArray.length === 1) {
                elementOffsetArray = [parseFloat(stringArray[0]), parseFloat(stringArray[0])];
            } else {
                elementOffsetArray = [parseFloat(stringArray[0]), parseFloat(stringArray[1])];
            }
        }

        return elementOffsetArray;
    };

    /**
     * Determine whether an element is within the viewport
     * @param  {HTMLElement}  el Element to test for
     * @return {String} Position of scroll
     * @return {Boolean}
     */
    Animate.prototype._isInView = function(el) {
        // Dimensions
        var dimensions = el.getBoundingClientRect();
        var viewportHeight = (window.innerHeight || document.documentElement.clientHeight);
        var viewportWidth = (window.innerWidth || document.documentElement.clientWidth);

        // Offset
        var elementOffset = this._getElementOffset(el);
        var verticalOffset = elementOffset[0];
        var horizontalOffset = elementOffset[1];

        // Vertical
        var isInViewFromTop = (dimensions.bottom - (dimensions.height * verticalOffset)) > 0;
        var isInViewFromBottom = (dimensions.top + (dimensions.height * verticalOffset)) < viewportHeight;
        var isInViewVertically = isInViewFromTop && isInViewFromBottom;

        // Horizontal
        var isInViewFromLeft = (dimensions.right - (dimensions.width * horizontalOffset)) > 0;
        var isInViewFromRight = (dimensions.left + (dimensions.width * horizontalOffset)) < viewportWidth;
        var isInViewHorizontally = isInViewFromLeft && isInViewFromRight;

        return (isInViewVertically && isInViewHorizontally);
    };

    /**
     * Tests whether a DOM node's visibility attribute is set to true
     * @private
     * @param  {HTMLElement}  el Element to test
     * @return {Boolean}
     */
    Animate.prototype._isVisible = function(el) {
        var visibility = el.getAttribute('data-visibility');
        return visibility === 'true';
    };

    /**
     * Tests whether a DOM node has already been animated
     * @private
     * @param  {HTMLElement}  el Element to test
     * @return {Boolean}
     */
    Animate.prototype._hasAnimated = function(el) {
        var animated = el.getAttribute('data-animated');
        return animated === 'true';
    };

    /**
     * Test whether an object is of a give type
     * @private
     * @param  {String}  type Type to test for e.g. 'String', 'Array'
     * @param  {Object}  obj  Object to test type against
     * @return {Boolean}      Whether object is of a type
     */
    Animate.prototype._isType = function(type, obj) {
        var test = Object.prototype.toString.call(obj).slice(8, -1);
        return obj !== null && obj !== undefined && test === type;
    };

    /**
     * Add animation to given element
     * @private
     * @param {HTMLElement} el Element to target
     */
    Animate.prototype._addAnimation = function(el) {
        if (!this._isVisible(el)) {
            this._doCallback(this.options.callbackOnInView, el);

            var classes = el.getAttribute('data-animation-classes');
            if (classes) {
                el.setAttribute('data-visibility', true);
                var animations = classes.split(' ');
                var animationDelay = parseInt(el.getAttribute('data-animation-delay'), 10) || this.options.delay;
                var addAnimation = function(animation) {
                    el.classList.add(animation);
                };

                if (animationDelay && this._isType('Number', animationDelay) && animationDelay !== 0) {
                    setTimeout(function() {
                        animations.forEach(addAnimation);
                    }, animationDelay);
                } else {
                    animations.forEach(addAnimation);
                }

                this._completeAnimation(el);
            } else {
                console.error('No animation classes were given');
            }
        }
    };

    /**
     * Remove animation from given element
     * @private
     * @param {HTMLElement} el Element to target
     */
    Animate.prototype._removeAnimation = function(el) {
        var classes = el.getAttribute('data-animation-classes');
        if (classes) {
            el.setAttribute('data-visibility', false);
            el.removeAttribute('data-animated');
            var animations = classes.split(' ');
            var animationDelay = parseInt(el.getAttribute('data-animation-delay'), 10);
            var removeAnimation = function(animation) {
                el.classList.remove(animation);
            };

            animations.push(this.options.animatedClass);

            if (animationDelay && this._isType('Number', animationDelay)) {
                setTimeout(function() {
                    animations.forEach(removeAnimation);
                }, animationDelay);
            } else {
                animations.forEach(removeAnimation);
            }
        } else {
            console.error('No animation classes were given');
        }
    };

    /**
     * If valid callback has been passed, run it with optional element as a parameter
     * @private
     * @param  {Function}         fn Callback function
     */
    Animate.prototype._doCallback = function(fn) {
        var el = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : null;
        if (fn && this._isType('Function', fn)) {
            fn(el);
        } else {
            console.error('Callback is not a function');
        }
    };

    /**
     * Add class & data attribute to element on animation completion
     * @private
     * @param  {HTMLElement} el Element to target
     */
    Animate.prototype._completeAnimation = function(el) {
        // Store animation event
        var animationEvent = this._whichAnimationEvent();

        // When animation event has finished
        el.addEventListener(animationEvent, function() {
            var removeOverride = el.getAttribute('data-animation-remove');

            // If remove animations on completion option is turned on
            if (removeOverride !== 'false' && this.options.remove) {
                // Separate each class held in the animation classes attribute
                var animations = el.getAttribute('data-animation-classes').split(' ');
                var removeAnimation = function(animation) {
                    el.classList.remove(animation);
                };

                // Remove each animation from element
                animations.forEach(removeAnimation);
            }

            // Add animation complete class
            el.classList.add(this.options.animatedClass);
            // Set animated attribute to true
            el.setAttribute('data-animated', true);

            this._doCallback(this.options.callbackOnAnimate, el);
        }.bind(this));
    };

    /**
     * Remove event listeners
     * @public
     */
    Animate.prototype.removeEventListeners = function() {
        if (this.options.onResize) {
            window.removeEventListener('resize', this.throttledEvent, false);
        }

        if (this.options.onScroll) {
            window.removeEventListener('scroll', this.throttledEvent, false);
        }
    };

    /**
     * Add event listeners
     * @public
     */
    Animate.prototype.addEventListeners = function() {
        if (this.options.onLoad) {
            document.addEventListener('DOMContentLoaded', function() {
                this.render(true);
            }.bind(this));
        }

        if (this.options.onResize) {
            window.addEventListener('resize', this.throttledEvent, false);
        }

        if (this.options.onScroll) {
            window.addEventListener('scroll', this.throttledEvent, false);
        }
    };

    /**
     * Initializes Animate.js and adds event listeners
     * @public
     */
    Animate.prototype.init = function() {
        // If browser doesn't cut the mustard, let it fail silently
        if (!this.supports) return;

        this.initialised = true;

        this.addEventListeners();

        this._doCallback(this.options.callbackOnInit);
    };

    /**
     * Stop all running event listeners & resets options to null
     * @public
     */
    Animate.prototype.kill = function() {
        // If we haven't initialised, there is nothing to kill.
        if (!this.initialised) return;

        this.removeEventListeners();

        // Reset settings
        this.options = null;
        this.initialised = false;
    };

    /**
     * Toggles animations on an event
     * @public
     * @return {}
     */
    Animate.prototype.render = function(onLoad) {
        if (this.initialised) {
            // If a disability filter function has been passed...
            if (this.options.disableFilter && this._isType('Function', this.options.disableFilter)) {
                var test = this.options.disableFilter();
                // ...and it passes, kill render
                if (test === true) return;
            }

            // Grab all elements in the DOM with the correct target
            var els = this.elements;

            // Loop through all elements
            for (var i = els.length - 1; i >= 0; i--) {
                // Store element at location 'i'
                var el = els[i];

                // If element is in view
                if (this._isInView(el)) {
                    // Add those snazzy animations
                    this._addAnimation(el);
                } else if (this._hasAnimated(el)) {
                    // See whether it has a reverse override
                    var reverseOverride = el.getAttribute('data-animation-reverse');

                    if (reverseOverride !== 'false' && this.options.reverse) {
                        this._removeAnimation(el);
                    }
                } else if (onLoad) {
                    var animateScrolled = el.getAttribute('data-animation-scrolled');

                    // If this render has been triggered on load and the element is above our current
                    // scroll position and the `scrolled` option is set, animate it.
                    if ((this.options.scrolled || animateScrolled) && this._isAboveScrollPos(el)) {
                        this._addAnimation(el);
                    }
                }
            }
        }
    };

    return Animate;
}));
