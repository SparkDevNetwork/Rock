$('[data-like]').on('click', function(){
    event.preventDefault();

    // Save HTML Elements of Like Button and Split into Icon/Count
    var likeHtml = $(this)[0].innerHTML.split('</i>');
    likeHtml[0] = likeHtml[0] + '</i>';
    
    if ($(this).find('i').hasClass('far')) {
        // Change Icon Class & Text Color
        likeHtml[0] = likeHtml[0].replace('far','fas text-danger');
        // Increment if not liked
        likeHtml[1] = Number(likeHtml[1]) + 1
    } else {
        // Change Icon Class & Text Color
        likeHtml[0] = likeHtml[0].replace('fas text-danger','far');
        // Decrement if liked  

        likeCount = Number(likeHtml[1]);   

        if (likeCount == 1) {
            // If going to zero, set to blank instead of zero
            likeHtml[1] = '';
        } else {
            // If not, decrement
            likeHtml[1] = Number(likeHtml[1]) - 1;
        }
    }

    // Update like UI
    $(this)[0].innerHTML = likeHtml.join(' ');
});