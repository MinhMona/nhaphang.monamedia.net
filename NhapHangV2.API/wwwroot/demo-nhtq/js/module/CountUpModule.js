export default function CountUpModule() {
    var a = 0;
    let counterBlock = document.querySelector(".count-block");
    if (counterBlock) {
        let oTop = counterBlock.offsetTop - window.innerHeight;
        $(window).scroll(function() {
            if (a == 0 && $(window).scrollTop() > oTop) {
                $(".counter-up").each(function() {
                    var $this = $(this),
                        countTo = $this.attr("data-count");
                    $({
                        countNum: $this.text(),
                    }).animate({
                            countNum: countTo,
                        },

                        {
                            duration: 2000,
                            easing: "swing",
                            step: function() {
                                $this.text(Math.floor(this.countNum));
                            },
                            complete: function() {
                                $this.text(this.countNum);
                            },
                        }
                    );
                });
                a = 1;
            }
        });
    }
}