# paroller.js  
[![npm](https://img.shields.io/npm/v/paroller.js.svg)](https://www.npmjs.com/package/paroller.js)    
A lightweight jQuery plugin that enables parallax scrolling effect  
 - You can use it on elements with background property or on any other element  
 - While scrolling elements can move: vertical, horizontal  
 - Manipulated through html data-* attributes or jQuery options  
 - Mobile ready  
 - Easy to use  
  
**DEMO:** [Example page](https://tgomilar.github.io/paroller.js/), [More examples](https://github.com/tgomilar/paroller.js/issues/32)   
  
## Install  
Before closing ```</body>``` element include:  
  
1. [jQuery](http://jquery.com/download/)  
2. [jquery.paroller.js](https://github.com/tgomilar/paroller.js/tree/master/dist)  
  
  
#### npm  
```sh  
$ npm install paroller.js  
```  
  
#### Yarn  
```sh  
$ yarn add paroller.js  
```  
  
#### Bower  
```sh  
$ bower install paroller.js  
```  
#### CDN
[jsDelivr](https://www.jsdelivr.com/package/npm/paroller.js), 	[unpkg](https://unpkg.com/paroller.js@1.3.1/), [bundle.run](https://bundle.run/paroller.js)

## Use  
```javascript  
//a) initialize paroller.js 
$('.my-paroller').paroller();  

//b) initialize paroller.js and set attributes 
$(".my-element").paroller({ factor: 0.5, factorXs: 0.2, factorSm: 0.3, type: 'foreground', direction: 'horizontal' });  
```  
```html 
<!-- a) select element and set attributes --> 
<div class="my-paroller" 
    data-paroller-factor="0.4" 
    data-paroller-factor-xs="0.2" 
    data-paroller-factor-sm="0.3" 
    data-paroller-type="foreground" 
    data-paroller-direction="horizontal"
>

<!-- b) select element -->  
<div class="my-element"></div>  
```  
  
**npm and browserify**  
```sh  
require('paroller.js');  
```  
  
## Options  
### data attributes  
You can control parallax effect by data-paroller-* or jQuery values.  
  
| data-paroller-* | jQuery | value | default value |  
| ------ |----- | ------ | ------ |  
| data-paroller-factor | factor | number (+/-) | 0 |  
| data-paroller-factor-xs | factorXs | number (+/-)  | 0 |  
| data-paroller-factor-sm | factorSm | number (+/-)  | 0 |  
| data-paroller-factor-md | factorMd | number (+/-)  | 0 |  
| data-paroller-factor-lg | factorLg | number (+/-)  | 0 |  
| data-paroller-type | type | background, foreground | background |  
| data-paroller-direction | direction | vertical, horizontal | vertical |  
  
### data-paroller-factor  
Sets speed and distance of element's parallax effect on scroll. Value can be positive (0.3) or negative (-0.3). Less means slower. Different sign (+/-) means different direction (up/down, left/right).  
  
:warning: Since **factor** is multiplier **it must be set for paroller.js to have parallax effect**.   
  
### data-paroller-factor-*  
Sets paroller factor for selected breakpoint.   
  
|  | data-paroller-factor-* | jQuery option | window width breakpoint |  
| ------ | ------ | ------ | ------ |  
| Extra small | data-paroller-factor-xs | factorXs | <576px |  
| Small | data-paroller-factor-sm | factorSm | <=768px|  
| Medium | data-paroller-factor-md | factorMd | <=1024px|  
| Large | data-paroller-factor-lg | factorLg| <=1200px|  
| Extra Large | data-paroller-factor-xl | factorxl| <=1920px|  
  
### JavaScript  
```javascript  
// initialize paroller.js and set attributes for selected elements  
$(".paroller, [data-paroller-factor]").paroller({  
  factor: 0.2,            // multiplier for scrolling speed and offset, +- values for direction control  
  factorLg: 0.4,          // multiplier for scrolling speed and offset if window width is less than 1200px, +- values for direction control  
  type: 'foreground',     // background, foreground  
  direction: 'horizontal' // vertical, horizontal  
});  
  ```  
##### Set factor breakpoints  
  
```javascript  
// initialize paroller.js and set attributes for selected elements  
$(".paroller, [data-paroller-factor]").paroller({  
	 factor: 0.3,            // +/-, if no other breakpoint factor is set this value is selected  
	 factorXs: 0.1,          // factorXs, factorSm, factorMd, factorLg, factorXl      
	 factorSm: 0.2,          // factorXs, factorSm, factorMd, factorLg, factorXl      
	 factorMd: 0.3,          // factorXs, factorSm, factorMd, factorLg, factorXl      
	 factorLg: 0.4,          // factorXs, factorSm, factorMd, factorLg, factorXl
	 factorXl: 0.5  	 // factorXs, factorSm, factorMd, factorLg, factorXl
	 type: 'foreground',     // background, foreground  
	 direction: 'horizontal' // vertical, horizontal  
});  
  ```  
  
## :star: Show your work :star:  
Interesting showcase or project?   
  
[Share it here](https://github.com/tgomilar/paroller.js/issues/32) and let others see your creative work.   
  
:clap::clap::clap:  
  
License  
----  
  
MIT
