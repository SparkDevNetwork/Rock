// @prepros-prepend "Vendor/moment.js"

var programBlog = {
  init: function(){
    var _this = this
    this.loadFirstPost()
    $(window).one('programBlogFirstPostLoaded', function(){
      _this.loadAllPosts()
    })
  },
  loadFirstPost: function(){
    var _this = this
    var url = '//ccvprogramblog.dreamhosters.com/?json=get_recent_posts&count=1&callback=?'
    $.getJSON(url, function(data){
      var formatted = _this.formatPost(data.posts[0])
      $('.js-wp-container').prepend(formatted)
      $(window).trigger('programBlogFirstPostLoaded')
    })
  },
  loadAllPosts: function(){
    var _this = this
    var url = '//ccvprogramblog.dreamhosters.com/?json=get_recent_posts&callback=?'
    var formattedPosts = ''
    $.getJSON(url, function(data){
      // starting with one to skip the first post that was already loaded
      for (var i = 1; i < data.posts.length; i++) {
        formattedPosts += _this.formatPost(data.posts[i])
      };
      $('.js-wp-loading-icon').remove()
      $('.js-wp-container').append(formattedPosts)
      $(window).trigger('programBlogAllLoaded')
    })
  },
  formatPost: function(post){
    var result  = '<article class="wordpress-post">'
        result += '<h1 class="page-header">'+post.title+' <small style="display:inline-block;">'+moment(post.date).format('MMMM Do, YYYY')+'</small></h1>'
        result += '<div>'+post.content+'</div>'
        result += '</article>'
    return result
  }
}

programBlog.init()


$(window).on('programBlogAllLoaded', function() {

  // Wrap default wp images with lightbox class
  $('a img[class*="wp-image"]').parent().each(function() {
    var $this = $(this)
    if ($this.parent().hasClass('lightbox-gallery') || $this.hasClass('lightbox')) return
    else $this.addClass('lightbox')
  });

  // Default wp gallery markup to CCV gallery markup
  $('.wordpress-post').each(function(index, el) {
    var imgs = $(el).find('img.attachment-thumbnail')
    if (imgs) {
      imgs.parent().wrapAll('<div class="lightbox-gallery">').each(function() {
        var $this = $(this);
        var url = $this.find('img').attr('src').replace('-150x150', '');
        $this.attr('href', url);
      });
    }
  });

  $('.lightbox-gallery').each(function() {
    $(this).magnificPopup({
      delegate: 'a',
      type: 'image',
      tLoading: 'Loading image #%curr%...',
      mainClass: 'mfp-img-mobile',
      gallery: {
        enabled: true,
        navigateByImgClick: true,
        preload: [0, 1] // Will preload 0 - before current, and 1 after the current image
      },
      image: {
        tError: '<a href="%url%">The image #%curr%</a> could not be loaded.',
        verticalFit: true,
        titleSrc: function(item) {
          return item.el.find('img').attr('alt');
        }
      }
    });
  });

  $('.js-wp-container').fitVids();

});
