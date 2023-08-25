export default function ParallaxModule() {
    if (window.innerWidth > 1201) {
        var image = document.getElementsByClassName('parallaxImg');
        if (image) {
            new simpleParallax(image, {
                delay: .6,
                transition: 'cubic-bezier(0,0,0,1)',
                overflow: true,
            });
        }
    }
}