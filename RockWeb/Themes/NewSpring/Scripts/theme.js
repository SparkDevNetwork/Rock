  @import './_layout.js'
  @import './_append-query.js'
	@import './_swiper.js'
	@import './_navigation.js'
	@import './_likes.js'

	var swipers = document.querySelectorAll('.swiper-container');
    
    if (swipers.length) {
        var swiper = new Swiper('.swiper-container', {
          slidesPerView: 3.3,
          spaceBetween: 15,
          centeredSlides: true,
          roundLengths: true,
          pagination: {
            el: '.swiper-pagination',
            clickable: true,
          },
          navigation: {
            nextEl: '.swiper-next',
            prevEl: '.swiper-prev',
          },
          breakpoints: {
            1024: {
              slidesPerView: 2.3,
            },
            768: {
              slidesPerView: 2.3,
              pagination: {
                dynamicBullets: true,
              },
            },
            667: {
              slidesPerView: 1.15,
              pagination: {
                dynamicBullets: true,
              },
            }
          }
        });
    }