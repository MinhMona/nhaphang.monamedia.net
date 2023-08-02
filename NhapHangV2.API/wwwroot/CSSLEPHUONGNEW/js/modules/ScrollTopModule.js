export default function ScrollTopModule() {
    const scrollTopBtn = document.querySelector('.moveToTop');
    const socialFixed = document.querySelector('.socialFixed');

    const toggleClass = () => {
        pageYOffset >= 300 ? scrollTopBtn.classList.add('active') : scrollTopBtn.classList.remove('active');

        if (socialFixed) {
            pageYOffset >= 300 ? socialFixed.classList.add('active') : socialFixed.classList.remove('active');
        }
    };
    const scrollTop = () => {
        if (pageYOffset > 0) {
            window.scrollTo(0, pageYOffset - pageYOffset / 8);
            requestAnimationFrame(scrollTop);
        }
    };
    document.addEventListener('DOMContentLoaded', toggleClass);
    window.addEventListener('scroll', toggleClass);
    scrollTopBtn.addEventListener('click', scrollTop);
}
