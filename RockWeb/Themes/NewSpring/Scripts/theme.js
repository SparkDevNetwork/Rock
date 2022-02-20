document.addEventListener("DOMContentLoaded", function(){
    // var navWrapper = $('#navigation-wrapper');
    // var offset = navWrapper.height();
    // console.log(offset);
//   var navigation = document.querySelector('#zone-navigation');
//   if(navigation != null) {
//       var navigationHeight = navigation.offsetHeight;
//   }

//   var header = document.querySelector('#navigation-secondary');
//   if(header != null) {
//       var headerHeight = header.offsetHeight;
//   }

  if (window.innerWidth > 667) {
      // Desktop
      offset = '30px';
  } else {
      // Mobile
      offset = '15px';
  }

  window.scroll = new SmoothScroll('a[data-scroll]', {
      speed: 500,
      speedAsDuration: true,
    //   header: '#navigation-wrapper',
      offset: offset,
      easing: 'easeInOutCubic'
  });
});
