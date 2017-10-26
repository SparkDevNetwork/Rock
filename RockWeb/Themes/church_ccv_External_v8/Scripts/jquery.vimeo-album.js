// note: this only works for public videos on public albums

(function ( $ ) {

  $.fn.vimeoAlbum = function( vimeoAlbum, options ) {

    var _this = this;

    if (!vimeoAlbum) {
      throw new Error('Vimeo album ID required');
    }

    var settings = $.extend({
      rowClass: 'row',
      itemClass: 'col-md-4',
      linkClass: 'popup-vimeo hover-playicon',
      rowLength: 3
    }, options );

    var videoHtml = [];

    $.ajax({
      dataType: 'jsonp',
      url: 'https://vimeo.com/api/v2/album/'+vimeoAlbum+'/videos.json',
    })
    .done(function(data) {
      parseData(data);
    })
    .error(function(jqXHR, textStatus, errorThrown) {
      console.log(errorThrown);
    });

    function parseData( data ) {

      // Add individual items
      for (var i = 0; i < data.length; i++) {
        var video = data[i];
        var r = '<div class="'+settings.itemClass+'"><a href="'+video.url+'" class="'+settings.linkClass+'"><img src="'+video.thumbnail_large+'" alt="'+video.title+'"></a><p>'+video.title+'</p></div>';
        videoHtml.push(r);
      };

      // Wrap items in rows
      var result = '<div class="'+settings.rowClass+'">';
      for (var i = 0; i < data.length; i++) {
        if (i % settings.rowLength === 0) {
          result += '</div><div class="'+settings.rowClass+'">'
        }
        result += videoHtml[i];
      };
      result += '</div>'

      _this.append(result);

      // Reenable magnific popup
      $('.popup-vimeo').magnificPopup({
        disableOn: 320,
        type: 'iframe',
        mainClass: 'mfp-fade',
        removalDelay: 160,
        preloader: false,
        fixedContentPos: false
      });

    }
  };
}( jQuery ));
