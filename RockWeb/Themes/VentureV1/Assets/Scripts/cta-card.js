export default function ctaCards() {
    $(document).ready( function() {
        let width = $(document).width();
        let isMobile = false;

        if (width < 992) {
            isMobile = true;
        }

        $('.js-poster-card:not(.is-mobile)').each((i, obj) => {
            if (isMobile) {
                $(obj).addClass('is-mobile');
            } else {
                let cardBody = $(obj).find('.js-poster-card-body');
                let cardHidden = $(obj).find('.js-poster-card-hidden')
                let cardHiddenHeight = $(cardHidden).height();
                $(cardBody).css('transform',`translateY(${cardHiddenHeight}px)`);

                $(obj).on('mouseenter', () => {
                    $(cardBody).css('transform',`translateY(0)`);
                    $(cardHidden).css('opacity',`1`);
                });

                $(obj).on('mouseleave', () => {
                    $(cardBody).css('transform',`translateY(${cardHiddenHeight}px)`);
                    $(cardHidden).css('opacity',`0`);
                });
            }
        });
    });
}
