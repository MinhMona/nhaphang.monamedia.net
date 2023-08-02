export default function ParallaxModule() {
    var image = document.getElementsByClassName('parallaxImg');
    new simpleParallax(image, {
        delay: .6,
        transition: 'cubic-bezier(0,0,0,1)',
        overflow: true,
    });
}