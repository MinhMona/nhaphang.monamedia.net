export default function HeaderModule() {
    const hd = document.querySelector('.hd');
    const menuCateFilter = document.querySelector('.menu-cate-filter');
    const commentSticky = document.querySelector('.comment-header-wrap');
    const podcastsAudioJS = document.querySelector('.podcastsAudioJS');

    let lastScroll = 0;
    window.onscroll = () => {
        let currentScroll = document.documentElement.scrollTop || document.body.scrollTop; // Get Current Scroll Value

        if (currentScroll > 0 && lastScroll <= currentScroll) {
            // down
            lastScroll = currentScroll;
            hd.classList.add('out')
            hd.classList.remove('in')

            // if (menuFilter) {
            //     menuFilter.classList.add('out')
            //     menuFilter.classList.remove('in')
            // }

            if (menuCateFilter) {
                menuCateFilter.classList.add('out')
                menuCateFilter.classList.remove('in')
            }

            if (commentSticky) {
                commentSticky.classList.remove('in')
            }

            if (podcastsAudioJS) {
                podcastsAudioJS.classList.add('bottom')
            }

        } else {
            // up
            lastScroll = currentScroll;
            hd.classList.remove('out')
            hd.classList.add('in')

            // if (menuFilter) {
            //     menuFilter.classList.remove('out')
            //     menuFilter.classList.add('in')
            // }

            if (menuCateFilter) {
                menuCateFilter.classList.remove('out')
                menuCateFilter.classList.add('in')
            }

            if (commentSticky) {
                commentSticky.classList.add('in')
            }

            if (podcastsAudioJS) {
                podcastsAudioJS.classList.remove('bottom')
            }
        }

        if (hd) {
            if (window.scrollY > 0) {
                hd.classList.add("small");
            } else {
                hd.classList.remove("small");
            }
        }
    }
}