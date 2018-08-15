# Animate.js ![Build Status](https://travis-ci.org/jshjohnson/Animate.svg?branch=develop)

Trigger animations on elements when they are in view 👓.

[Demo](https://joshuajohnson.co.uk/Animate/)

----
### Interested in writing your own JavaScript plugins? Check out [ES6.io](https://ES6.io/friend/JOHNSON) for great tutorials!
----

## Setup
```html
<script src="/assets/js/dist/animate.js"></script>
<script>
    var animate = new Animate({        
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
        disableFilter: false,
        callbackOnInit: function() {},
        callbackOnInView: function(el) {},
        callbackOnAnimate: function(el) {},
    });
    animate.init();
</script>
```

## Installation
To install via NPM, run `npm install --save-dev animate.js` 

## Options
#### target
Type: `String` Default: `[data-animate]`

Element(s) reference to target (`querySelectorAll` is called against this value). Once this element is in view, add animations.

#### animatedClass
Type: `String` Default: `js-animated`

Class to be added to element once animation has completed.

#### offset
Type: `Array/Number` Default: `[0.5, 0.5]`

The vertical and horizontal percentages of the element that needs to be in the viewport before the animation triggers. If a single number is passed instead of an array, that number will be used for both the vertical and horizontally offset.

*Examples*

```js
// Trigger animations when 50% of an elements height 
// is within the viewport and 100% of its width:
var animate = new Animate({
    target: '[data-animate]',
    animatedClass: 'visible',
    offset: [0.5, 1],
});

// Trigger animations when 100% of an elements height 
// is within the viewport and 25% of its width:
var animate = new Animate({
    target: '[data-animate]',
    animatedClass: 'visible',
    offset: [1, 0.25],
});

// Trigger animations when 50% of an elements height 
// is within the viewport and 50% of its width:
var animate = new Animate({
    target: '[data-animate]',
    animatedClass: 'visible',
    offset: 0.5,
});
```

####  delay
Type: `Number` Default: `0`

Milisecond delay before animation is added to element in view.

####  remove
Type: `Boolean` Default: `true`

Whether animation classes should removed when the animations complete.

####  reverse
Type: `Boolean` Default: `false`

Once the element has left the top of the viewport (by the same offset), remove the animations from element. When the element comes back into view, it will animate again.

####  scrolled
Type: `Boolean` Default: `false`

Animate any elements that a user has already scrolled past on load. This will only trigger if the `onLoad` option (see below) is `true`.

#### onLoad
Type: `Boolean` Default: `true`

Whether to fire on DOMContentLoaded.

#### onScroll
Type: `Boolean` Default: `true`

Whether to fire on scroll.

#### onResize
Type: `Boolean` Default: `false`

Whether to fire on resize.

### disableFilter
Type: `Function` Default: `null`

Function to determine whether Animate should animate elements.

*Example*

```js
// Function to determine whether we are on a mobile device
var isMobile = function() {
    if (window.matchMedia("(max-width: 480px)").matches) {
        return true;
    } else {
        return false;
    }
};

// Disable Animate.js if isMobile returns true
var animate = new Animate({
    onResize: true,
    disableFilter: isMobile,
});
```

#### callbackOnInit
Type: `Function` Default: `function(){}`

Function to run once Animate.js initialises

#### callbackOnInView
Type: `Function` Default: `function(el){}`

Function to run once the element is in the viewport (pass parameter to access the element).

#### callbackOnAnimate 
Type: `Function` Default: `function(el){}`

Function to run once animation has completed (pass parameter to access the animated element).

## Element overrides

##### `data-animate`

Default way of targeting an element to animate (no value required). This can be overridden to be a custom attribute or class.

##### `data-animation-classes`

Animations to be added to element when it is in view. To add multiple classes, seperate each class with a space (as you would normally).

### Optional element overrides
##### `data-animation-delay`

Overide the plugin `delay` option per element.

##### `data-animation-offset`

Override the plugin `offset` option per element.

##### `data-animation-remove`

Overide the plugin `removeAnimations` option per element.

##### `data-animation-reverse`

Overide the plugin `reverse` option per element.

#### Examples
```html
<div data-animate data-animation-classes="animated fadeIn"></div>
<div data-animate data-animation-classes="animated tada" data-animation-delay="1000"></div>
<div data-animate data-animation-classes="animated bounce" data-animation-offset="0.2, 0.5"></div>
<div data-animate data-animation-classes="animated bounce" data-animation-remove="true"></div>
```

## Methods
#### init();
Initialises event listeners.
#### kill();
Kills event listeners and resets options.
#### render();
Adds/removes animations without the need for event listeners.

## Browser compatibility
Animate.js is supported in modern browsers from IE9 and above (i.e. browsers that support CSS animations). Due to discrepencies in support for `Element.classList`, I would recommend including the very good [classList polyfill](https://github.com/eligrey/classList.js/) before you include animate.js. I would also suggest using Modernizr to feature detect CSS animations/transitions and apply override styling for browsers that do not support those features.

Using SCSS, this may look like this:

```css
.animate {
    opacity: 0;
    .no-csstransitions &, .no-cssanimations &  {
        opacity: 1;
    }
}
```

## Development
To setup a local environment: clone this repo, navigate into it's directory in a terminal window and run the following command:

* ```npm install```

### Gulp tasks
* ```gulp dev```
* ```gulp test```
* ```gulp build```

## Contributions
In lieu of a formal styleguide, take care to maintain the existing coding style. Add unit tests for any new or changed functionality. Lint and test your code using Gulp...bla bla bla

## License
MIT License 
