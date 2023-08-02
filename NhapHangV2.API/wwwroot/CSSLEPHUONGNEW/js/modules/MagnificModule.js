export default function MagnificModule() {
    jQuery(document).ready(function ($) {
        $('.popup-with-zoom-anim').magnificPopup({
            type: 'inline',
            preloader: false,
            removalDelay: 100,
            mainClass: 'my-mfp-zoom-in',
            modal: false,

            fixedBgPos: true,
            fixedContentPos: true,
        });

        $(document).on('click', '.dismiss', function (e) {
            e.preventDefault();
            $.magnificPopup.close();
        });

        // Youtube
        $('.popup-youtube').magnificPopup({
            // disableOn: 700,
            type: 'iframe',
            mainClass: 'mfp-fade',
            removalDelay: 160,
            preloader: false,

            fixedBgPos: true,
            fixedContentPos: true,
        });
    })
}