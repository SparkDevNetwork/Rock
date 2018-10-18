/*!
 * @copyright Copyright (c) 2017 IcoMoon.io
 * @license   Licensed under MIT license
 *            See https://github.com/Keyamoon/svgxuse
 * @version   1.2.6
 */
/*jslint browser: true */
/*global XDomainRequest, MutationObserver, window */
!function(){"use strict";if("undefined"!=typeof window&&window.addEventListener){var v=Object.create(null),e,t,n=function(){clearTimeout(t),t=setTimeout(e,100)},b=function(){},E=function(){var e;window.addEventListener("resize",n,!1),window.addEventListener("orientationchange",n,!1),b=window.MutationObserver?((e=new MutationObserver(n)).observe(document.documentElement,{childList:!0,subtree:!0,attributes:!0}),function(){try{e.disconnect(),window.removeEventListener("resize",n,!1),window.removeEventListener("orientationchange",n,!1)}catch(e){}}):(document.documentElement.addEventListener("DOMSubtreeModified",n,!1),function(){document.documentElement.removeEventListener("DOMSubtreeModified",n,!1),window.removeEventListener("resize",n,!1),window.removeEventListener("orientationchange",n,!1)})},g=function(e){
// In IE 9, cross origin requests can only be sent using XDomainRequest.
// XDomainRequest would fail if CORS headers are not set.
// Therefore, XDomainRequest should only be used with cross origin requests.
function t(e){var t;return void 0!==e.protocol?t=e:(t=document.createElement("a")).href=e,t.protocol.replace(/:/g,"")+t.host}var n,o,i;return window.XMLHttpRequest&&(n=new XMLHttpRequest,o=t(location),i=t(e),n=void 0===n.withCredentials&&""!==i&&i!==o?XDomainRequest||void 0:XMLHttpRequest),n},L="http://www.w3.org/1999/xlink",o;// holds xhr objects to prevent multiple requests
e=function(){function i(){
// If done with making changes, start watching for chagnes in DOM again
0===(l-=1)&&(// if all xhrs were resolved
b(),// make sure to remove old handlers
E())}function e(e){return function(){!0!==v[e.base]&&(e.useEl.setAttributeNS(L,"xlink:href","#"+e.hash),e.useEl.hasAttribute("href")&&e.useEl.setAttribute("href","#"+e.hash))}}function t(o){return function(){var e=document.body,t=document.createElement("x"),n;o.onload=null,t.innerHTML=o.responseText,(n=t.getElementsByTagName("svg")[0])&&(n.setAttribute("aria-hidden","true"),n.style.position="absolute",n.style.width=0,n.style.height=0,n.style.overflow="hidden",e.insertBefore(n,e.firstChild)),i()}}function n(e){return function(){e.onerror=null,e.ontimeout=null,i()}}var o,r,u="",s,d,a,l=0,c,h,f,m,w;for(b(),// stop watching for changes to DOM
// find all use elements
m=document.getElementsByTagName("use"),a=0;a<m.length;a+=1){try{r=m[a].getBoundingClientRect()}catch(e){
// failed to get bounding rectangle of the use element
r=!1}o=(f=(d=m[a].getAttribute("href")||m[a].getAttributeNS(L,"href")||m[a].getAttribute("xlink:href"))&&d.split?d.split("#"):["",""])[0],s=f[1],c=r&&0===r.left&&0===r.right&&0===r.top&&0===r.bottom,r&&0===r.width&&0===r.height&&!c?(m[a].hasAttribute("href")&&m[a].setAttributeNS(L,"xlink:href",d),o.length&&(
// schedule updating xlink:href
!0!==(w=v[o])&&
// true signifies that prepending the SVG was not required
setTimeout(e({useEl:m[a],base:o,hash:s}),0),void 0===w&&void 0!==(h=g(o))&&(w=new h,(v[o]=w).onload=t(w),w.onerror=n(w),w.ontimeout=n(w),w.open("GET",o),w.send(),l+=1))):c?o.length&&v[o]&&setTimeout(e({useEl:m[a],base:o,hash:s}),0):void 0===v[o]?
// remember this URL if the use element was not empty and no request was sent
v[o]=!0:v[o].onload&&(
// if it turns out that prepending the SVG is not necessary,
// abort the in-progress xhr.
v[o].abort(),delete v[o].onload,v[o]=!0)}m="",l+=1,i()},o=function(){window.removeEventListener("load",o,!1),// to prevent memory leaks
t=setTimeout(e,0)},"complete"!==document.readyState?
// The load event fires when all resources have finished loading, which allows detecting whether SVG use elements are empty.
window.addEventListener("load",o,!1):
// No need to add a listener if the document is already loaded, initialize immediately.
o()}}();