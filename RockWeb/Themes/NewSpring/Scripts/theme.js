document.addEventListener("DOMContentLoaded", function(){
  var navigation = document.querySelector('#zone-navigation');
  if(navigation != null) {
      var navigationHeight = navigation.offsetHeight;
  }

  var header = document.querySelector('#navigation-secondary');
  if(header != null) {
      var headerHeight = header.offsetHeight;
  }

  if (window.innerWidth > 667) {
      // Desktop
      offset = navigationHeight + headerHeight + 25 + "px";
  } else {
      // Mobile
      offset = headerHeight + 10 + "px";
  }

  window.scroll = new SmoothScroll('a[data-scroll]', {
      speed: 500,
      speedAsDuration: true,
      offset: offset,
      easing: 'easeInOutCubic'
  });
});