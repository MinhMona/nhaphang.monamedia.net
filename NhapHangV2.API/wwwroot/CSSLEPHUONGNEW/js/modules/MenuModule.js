export default function MenuModule() {
    const menuBar = document.querySelector('.hdBar')
    const hdMenu = document.querySelector('.hdMenu')
    const menuClose = document.querySelector('.menuClose')
    const bgPage = document.querySelector('.bgPage')
    const btnSubs = document.querySelectorAll('.subBtn');
    const body = document.querySelector('body');
    const hdControlsTop = document.querySelector('.hd-controls-top')

    let vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty("--vh", `${vh}px`);

    window.addEventListener("resize", () => {
        // if (window.innerWidth >= 1201) {
        //     closeMenu();
        // }
        // CheckDeviceModule()

        let vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty("--vh", `${vh}px`);
    })

    if (menuBar) {
        menuBar.onclick = () => {
            hdMenu.classList.add('active')
            bgPage.classList.add('active')
            body.classList.add('no-scroll')

            if (hdControlsTop) {
                hdControlsTop.style.zIndex = 2
            }
        }

        menuClose.onclick = () => {
            closeMenu()
        }

        bgPage.onclick = () => {
            closeMenu()
        }
    }

    function closeMenu() {
        hdMenu.classList.remove('active')
        bgPage.classList.remove('active')

        if (hdControlsTop) {
            hdControlsTop.style.zIndex = 7
        }

        $(".submenu").slideUp();

        btnSubs.forEach((btnSub) => {
            btnSub.classList.remove('active')
        })

        body.classList.remove('no-scroll')
    }

    $(document).ready(function () {
        $(".subBtn").click(function (e) {
            e.preventDefault();
            $(this).toggleClass("active");
            $(this).next(".submenu").stop().slideToggle();
        });
    });
}