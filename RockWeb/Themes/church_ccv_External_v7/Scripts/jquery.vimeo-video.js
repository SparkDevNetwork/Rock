(function ( $ ) {
  $.fn.vimeoVideo = function( options ) {

    var settings = $.extend({
      linkClass: 'popup-vimeo hover-playicon',
    }, options );

    this.each(function() {

      var $this = $(this),
          vimeoId = $this.attr('data-vimeo-id')

      if (!vimeoId) {
        throw new Error('data-vimeo-id is required on all nodes pass to vimeoVideo');
      }

      $.ajax({
        dataType: 'jsonp',
        url: 'https://vimeo.com/api/v2/video/'+vimeoId+'.json',
      })
      .done(function(data) {
        parseData(data);
      })
      .error(function(jqXHR, textStatus, errorThrown) {
        console.log(errorThrown);
      });

      function parseData( data ) {
        var video = data[0];
        var r = '<a href="'+video.url+'" class="'+settings.linkClass+'"><img src="'+video.thumbnail_large+'" alt="'+video.title+'"></a><p>'+video.title+'</p>';
        $this.append(r)

        // Reenable popup on newly created nodes
        $('.popup-vimeo').magnificPopup({
          disableOn: 320,
          type: 'iframe',
          mainClass: 'mfp-fade',
          removalDelay: 160,
          preloader: false,
          fixedContentPos: false
        });
      }

    });
  };
}( jQuery ));
