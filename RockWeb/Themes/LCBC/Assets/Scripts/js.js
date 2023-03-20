// create mutation observer to watch for changes in .content-collection-view 
console.log('js.js loaded')
// Select the node that will be observed for mutations
const targetNode = document.querySelector('.content-collection-view');

// Options for the observer (which mutations to observe)
const config = { attributes: true, childList: true, subtree: true };

// Callback function to execute when mutations are observed
const callback = (mutationList, observer) => {
  for (const mutation of mutationList) {
    if (mutation.type === 'childList') {
      console.log('A child node has been added or removed.');
    } else if (mutation.type === 'attributes') {
      console.log(`The ${mutation.attributeName} attribute was modified.`);
    }

    // if .radio-inline input is checked, add class to .radio-inline
    // const radioInline = document.querySelectorAll('.radio-inline');
    // radioInline.forEach((radio) => {
    //     if (radio.querySelector('input').checked) {
    //         radio.classList.add('checked');
    //     } else {
    //         radio.classList.remove('checked');
    //     }
    //     }
    // )

  }
};

// Create an observer instance linked to the callback function
const observer = new MutationObserver(callback);

// Start observing the target node for configured mutations
observer.observe(targetNode, config);