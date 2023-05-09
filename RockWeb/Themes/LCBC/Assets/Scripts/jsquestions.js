const observer = new MutationObserver(mutations => {
    const body = document.querySelector('body');
    const pendingBlocks = body.getAttribute('data-obsidian-pending-blocks');
    if (pendingBlocks === '0') {
      // update url with categoryGuid on click
      // const categories = document.querySelectorAll('.filter-category input');
      // categories.forEach(category => {
      //   category.addEventListener('change', () => {
      //     const categoryValue = category.value;
      //     if (categoryValue === '') {
      //       console.log("Reset Filters");
      //       categoryChange('', categoryList)
      //     }
      //   });
      // });
  
      observer.disconnect();
    }
  });
  
  observer.observe(document.querySelector('body'), { attributes: true });