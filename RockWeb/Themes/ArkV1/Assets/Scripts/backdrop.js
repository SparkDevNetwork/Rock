
export default function backdrop() {
    $(document).ready(()=> {
        resizeBackdrops();
        let basewidth = $( document ).width();
        let basebp = '';

        if (basewidth < 768) {
            basebp = 'xs';
        } else if (basewidth < 992) {
            basebp = 'sm';
        } else if (basewidth < 1200) {
            basebp = 'md';
        } else {
            basebp = 'lg';
        }

        $(window).on('resize', () => {
            let width = $( document ).width();
            let bp = ''

            if (width < 768) {
                bp = 'xs';
            } else if (width < 992) {
                bp = 'sm';
            } else if (width < 1200) {
                bp = 'md';
            } else {
                bp = 'lg';
            }

            if (bp == 'xs' || basebp != bp) {
                resizeBackdrops();
                basebp = bp;
            }
        })
    });

    function resizeBackdrops() {
        $('.js-backdrop').each((i, obj) => {
            var height = $(obj).height();
            var offset = (height * Math.sin(0.0872665)) / 2; // 5deg in radians = 0.0872665
            offset = offset * .85;
            $(obj).css('margin',`${offset}px`);
        });
    }
}
