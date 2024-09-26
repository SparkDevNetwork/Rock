
export default function primarySwiper() {
    $(document).ready(()=> {
        $('.js-primary-swiper').each((i, obj) => {
            var backdropItem = $(obj).find('.backdrop').first();
            if (backdropItem.length > 0) {
                const nextButton = $(obj).find('.swiper-button-next');
                const prevButton = $(obj).find('.swiper-button-prev');
                placeNavigation(backdropItem, nextButton, prevButton);

                $(window).on('resize', () => {
                    placeNavigation(backdropItem, nextButton, prevButton);
                });
            }
        })
    });

    function placeNavigation(backdropItem, nextButton, prevButton) {
        const backdropHeight = backdropItem.height();
        $(nextButton).css({'top':`${backdropHeight / 2}px`,'transform':'translateY(50%)'});
        $(prevButton).css({'top':`${backdropHeight / 2}px`,'transform':'translateY(50%)'});

        if ($(nextButton).hasClass('swiper-button-disabled') && $(prevButton).hasClass('swiper-button-disabled')) {
            $(nextButton).css({'display':'none'});
            $(prevButton).css({'display':'none'});
        } else {
            $(nextButton).css({'display':'flex'});
            $(prevButton).css({'display':'flex'});
        }
    }
}
