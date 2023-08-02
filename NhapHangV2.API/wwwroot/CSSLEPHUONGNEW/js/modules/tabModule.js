export default function tabModule() {
    const tabs = document.querySelectorAll('.tab');
    const tabPanels = document.querySelectorAll('.tab-panel');
    if (tabs) {
        tabs.forEach((tab, index) => {
            const tabPanel = tabPanels[index];

            tab.onclick = function () {
                document.querySelector('.tab.active').classList.remove('active');
                document.querySelector('.tab-panel.active').classList.remove('active');

                this.classList.add("active");
                tabPanel.classList.add("active");

                const ourMenu = new Swiper('.our-menu-panel .tab-panel.active .swiper-container', {
                    slidesPerView: 1.2,
                    slidesPerGroup: 1,
                    speed: 1200,
                    delay: 1200,

                    breakpoints: {
                        550: {
                            slidesPerView: 2.2,
                            slidesPerGroup: 2,
                        },
                        769: {
                            slidesPerView: 3.2,
                            slidesPerGroup: 3,
                        },
                        1200: {
                            slidesPerView: 4,
                            slidesPerGroup: 4,
                        }
                    },

                    navigation: {
                        nextEl: '.our-menu-panel .tab-panel.active .swiper-navi.next',
                        prevEl: '.our-menu-panel .tab-panel.active .swiper-navi.prev',
                    },
                });
            };
        });
    }
}