export default function ScrollToSectionModule() {
    const speed = 800;
    // NẾU CÓ ĐỊA CHỈ ID TRÊN THANH URL THÌ SCROLL XUỐNG
    const hash = window.location.hash;
    if ($(hash).length) scrollToID(hash, speed);
    // TÌM ĐỊA CHỈ ID VÀ SCROLL XUỐNG NẾU CÓ CLASS
    $('.btn-scroll').on('click', function (e) {
        e.preventDefault();

        $(this).addClass('current');

        const href = $(this).find('> a').attr('href') || $(this).attr('href');
        const id = href.slice(href.lastIndexOf('#'));
        if ($(id).length) {
            scrollToID(id, speed);
        } else {
            // window.location.replace(`/${id}`);
            window.location.href = href;
        }
    });
    // HÀM SCROLL CHO MƯỢT MÀ
    function scrollToID(id, speed) {
        const offSet = $('.header').outerHeight();
        const section = $(id).offset();
        const targetOffset = section.top - offSet + 130;
        $('html,body').animate({ scrollTop: targetOffset }, speed);
    }


    function runLine() {
        var bullets = document.querySelectorAll('.btn-scroll');

        $(".titleJS").each(function (i) {
            if ($(window).scrollTop() > $(this).offset().top - 500) {
                var _this = this;
                
                bullets.forEach(function (bullet, index) {
                    bullet.classList.remove('current')
                    if(index == i) {
                        bullet.classList.add('current')
                    }
                })
            } else {}
        })
    }
    runLine();

    $(window).scroll(function () {
        runLine();
    })
}
