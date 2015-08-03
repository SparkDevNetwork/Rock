$(document).ready(function() {
  $('.js-vimeothumb').each(function(){
    var $this = $(this)
    var vimeoId = $this.attr('data-vimeo-id')
    var img = ''

    $.getJSON('//vimeo.com/api/v2/video/' + vimeoId + '.json?callback=?', function(data) {
      img = '<img src="' + data[0].thumbnail_large + '" alt="' + data[0].title + '">'
      run()
    })

    function run() {
      $this
        .append('<a href="https://vimeo.com/'+vimeoId+'">'+img+'</a>')
        .children('a').magnificPopup({
          disableOn: 700,
          type: 'iframe',
          mainClass: 'mfp-fade',
          removalDelay: 160,
          preloader: false,

          fixedContentPos: false
        })
    }
  })

})
