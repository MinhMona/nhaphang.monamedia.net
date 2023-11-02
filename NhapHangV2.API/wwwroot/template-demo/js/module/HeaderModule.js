export default function HeaderModule() {
    const menuNavs = document.querySelectorAll(".header .menu-nav");
    if (menuNavs) {
        menuNavs.forEach((item) => {
            const menuLinks = item.querySelectorAll(".menu-item.dropdown .menu-link");
            menuLinks.forEach((item) => {
                const contentOld = item.innerHTML;
                const contentNew = `${contentOld} <i class="fas fa-caret-down"></i>`;
                item.innerHTML = contentNew;
            });
        });
    }
    const arrowIcon = document.querySelectorAll(
        ".mobile-nav .menu-list .menu-item a i"
    );
    if (arrowIcon) {
        arrowIcon.forEach((item) => {
            item.addEventListener("click", (e) => {
                e.preventDefault();
                const menu =
                    item.parentElement.parentElement.querySelectorAll(".menu-list")[0];
                $(menu).slideToggle();
                $(item.parentElement.parentElement).toggleClass("active");
            });
        });
    }
}