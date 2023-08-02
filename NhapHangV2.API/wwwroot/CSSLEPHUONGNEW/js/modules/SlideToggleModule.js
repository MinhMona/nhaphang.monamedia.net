export default function SlideToggleModule() {
    $(document).ready(function () {
        $(document).ready(function () {
            $('.toggleHide').hide();
            $('.toggleOnclick').click(function (e) {
                e.preventDefault();
    
                if (!$(this).closest('.faq-item').find('.toggleHide').is(':visible')) {
                    $('.toggleHide').slideUp()
                }
    
                $(this).toggleClass('active');
                $(this).next('.toggleHide').stop().slideToggle();
            })
    
            const itemActive = document.querySelector('.wdsContent.active')
            if (itemActive) {
                $(itemActive).find('.wdsMoreHide').stop().slideToggle();
            }

            $('.wdsMore').click(function (e) {
                e.stopPropagation();
                e.preventDefault();

                let parent = $(this).closest('.wdsContent')
                if (parent) {
                    if (!$(parent).find('.wdsMoreHide').is(':visible')) {
                        $('.wdsMoreHide').slideUp()
                        $('.wdsMoreHide').closest('.wdsContent').removeClass('active')
                    }
                    parent.toggleClass('active')
                    parent.find('.wdsMoreHide').stop().slideToggle();
                }
            })
        });
    });
}