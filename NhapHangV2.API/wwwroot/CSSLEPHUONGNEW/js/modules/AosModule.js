export default function AosModule() {
    /** ANIMATION ON SCROLL */
    AOS.init({
        duration: 800,
        once: 'true',
        disable: function () {
            return $(window).width() <= 1200;
        },
    });
    /** ANIMATION ON SCROLL - END */
}
