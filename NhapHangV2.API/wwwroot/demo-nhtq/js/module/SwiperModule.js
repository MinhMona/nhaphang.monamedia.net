import SearchModule from "./SearchModule.js";
export default function SwiperModule() {
  // function functionSlider(element, isLoop, isCenter, betWeenInit, autoplay, effects, breakpoint) {
  //     const swiperSlider = document.querySelectorAll(element)
  //     if (swiperSlider) {
  //         swiperSlider.forEach(item => {
  //             const swiper = item.querySelector('.swiper')
  //             const pagi = item.querySelector('.swiper-pagination')
  //             const next = item.querySelector('.swiper-next')
  //             const prev = item.querySelector('.swiper-prev')
  //             var slide = new Swiper(swiper, {
  //                 watchSlidesProgress: true,
  //                 speed: 1200,
  //                 autoplay: autoplay,
  //                 pagination: {
  //                     el: pagi,
  //                     type: 'bullets',
  //                     clickable: true,
  //                 },
  //                 navigation: {
  //                     nextEl: next,
  //                     prevEl: prev,
  //                 },
  //                 // slidesPerGroup: 2,
  //                 centeredSlides: isCenter,
  //                 loop: isLoop,
  //                 spaceBetween: betWeenInit,
  //                 effect: effects,
  //                 fadeEffect: {
  //                     crossFade: true
  //                 },
  //                 breakpoints: breakpoint,
  //             });
  //         })
  //     }
  // }

  // // element , isLoop, isCenter, betWeenInit, autoPlay, effects,breackpoint
  // functionSlider(".story-slider", false, false, 24, { delay: 3000 }, "slide", {
  //     0: {
  //         slidesPerView: 1.2,
  //         freeMode: true,
  //     },
  //     500: {
  //         slidesPerView: 2.2,

  //     },
  //     768: {
  //         slidesPerView: 3,

  //     },
  //     1200: {
  //         slidesPerView: 3,
  //     }
  // })

  // function sliderParalax() {
  //     const swiperSliderParalax = document.querySelectorAll("element")
  //     if (swiperSliderParalax) {
  //         swiperSliderParalax.forEach(item => {
  //             const swiper = item.querySelector('.swiper')
  //             const pagi = item.querySelector('.swiper-pagination')
  //             const next = item.querySelector('.swiper-next')
  //             const prev = item.querySelector('.swiper-prev')
  //             var slide = new swiper(swiper, {
  //                 slidesPerView: 1,
  //                 spaceBetween: 0,
  //                 // loopedSlides: 6,
  //                 loop: false,
  //                 autoplay: {
  //                     delay: 7000,
  //                 },
  //                 pagination: {
  //                     el: pagi,
  //                     type: "bullets",
  //                 },
  //                 navigation: {
  //                     nextEl: next,
  //                     prevEl: prev,
  //                 },
  //                 speed: 1000,
  //                 effect: "coverflow",
  //                 parallax: true,
  //                 grabCursor: true,
  //                 centeredSlides: true,
  //                 coverflowEffect: {
  //                     rotate: 0.05,
  //                     depth: 0,
  //                     stretch: 0,
  //                     modifier: 1,
  //                     slideShadows: false,
  //                 },
  //                 on: {
  //                     init: function() {
  //                         let swiper = this;
  //                         for (let i = 0; i < swiper.slides.length; i++) {
  //                             $(swiper.slides[i])
  //                                 .find(".hbn-img-inner")
  //                                 .attr({
  //                                     "data-swiper-parallax": 0.9 * swiper.width,
  //                                     "data-swiper-paralalx-opacity": 0.1,
  //                                 });
  //                         }
  //                         let index = swiper.activeIndex;
  //                         $(swiper.slides).removeClass("active");
  //                         $(swiper.slides[index]).addClass("active");
  //                     },
  //                     resize: function() {
  //                         this.update();
  //                     },
  //                 },
  //             })
  //         })
  //     }
  // }
  // sliderParalax(".hbn-slider")
  // var storySlider = new Swiper(".story-slider .swiper", {
  //     // freeMode: true,
  //     watchSlidesProgress: true,
  //     speed: 1200,
  //     autoplay: {
  //         delay: 3000,
  //     },
  //     pagination: {
  //         el: '.story-slider .swiper-pagination',
  //         type: 'bullets',
  //         clickable: true,
  //     },
  //     navigation: {
  //         nextEl: '.story-slider .swiper-next',
  //         prevEl: '.story-slider .swiper-prev',
  //     },
  //     // slidesPerGroup: 2,
  //     centeredSlides: false,
  //     loop: false,
  //     spaceBetween: 24,
  //     // effect: 'fade',
  //     fadeEffect: {
  //         crossFade: true
  //     },
  //     breakpoints: {
  //         0: {
  //             slidesPerView: 1.2,
  //             freeMode: true,
  //         },
  //         500: {
  //             slidesPerView: 2.2,

  //         },
  //         768: {
  //             slidesPerView: 3,

  //         },
  //         1200: {
  //             slidesPerView: 3,
  //         }
  //     }
  // });
  // Slider Control
  // topsSliderMain.controller.control = topSliderThumb;
  // topSliderThumb.controller.control = topsSliderMain;

  const swiperTextWrapper = new Swiper(".swiper.swiper-textWrapper", {
    // Optional parameters
    // direction: 'vertical',
    loop: true,
    // autoplay: {
    //   delay: 2500,
    //   disableOnInteraction: false,
    // },

    // Navigation arrows
    navigation: {
      prevEl: ".sec-register-slide-textWrapper-prev",
      nextEl: ".sec-register-slide-textWrapper-next",
    },

    // And if we need scrollbar
    scrollbar: {
      el: ".swiper-scrollbar",
    },
  });
  const evaluteSwiper = new Swiper(".swiper.evalute-swiper", {
    // Optional parameters
    // direction: 'vertical',
    // loop: true,
    // autoplay: {
    //   delay: 2500,
    //   disableOnInteraction: false,
    // },
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
      //evalute-swiper-pagination
    },
    // Navigation arrows
    // navigation: {
    //   prevEl: ".sec-register-slide-textWrapper-prev",
    //   nextEl: ".sec-register-slide-textWrapper-next",
    // },

    // And if we need scrollbar
    scrollbar: {
      el: ".swiper-scrollbar",
    },
  });

  const newsSwiper = new Swiper(".swiper.sec-news-inner-list", {
    // Optional parameters
    // direction: 'vertical',
    // loop: true,
    // autoplay: {
    //   delay: 2500,
    //   disableOnInteraction: false,
    // },
    slidesPerView: 1,
    spaceBetween: 30,
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
      //evalute-swiper-pagination
    },
    // Navigation arrows
    // navigation: {
    //   prevEl: ".sec-register-slide-textWrapper-prev",
    //   nextEl: ".sec-register-slide-textWrapper-next",
    // },

    // And if we need scrollbar
    scrollbar: {
      el: ".swiper-scrollbar",
    },
    breakpoints: {
      480: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      1024: {
        slidesPerView: 3,
        spaceBetween: 30,
      },
    },
  });

  const detailContentNewsElm = document.querySelector(".detail-content-news .swiper-wrapper");

  if (detailContentNewsElm) {
    const detailNewsSwiper = new Swiper(".swiper.detail-content-news-swiper", {
      // Optional parameters
      // direction: 'vertical',
      // loop: true,
      // autoplay: {
      //   delay: 2500,
      //   disableOnInteraction: false,
      // },
      slidesPerView: 2,
      // spaceBetween: 30,
      grid: {
        rows:
        detailContentNewsElm.children
            .length >5
            ? 2
            : 1,
      },
      pagination: {
        el: ".detail-content-news-controls-pagination",
        clickable: true,
        renderBullet: function (index, className) {
          return '<span class="' + className + '">' + (index + 1) + "</span>";
        },
      },
      // Navigation arrows
      navigation: {
        prevEl: ".detail-content-news-controls-button-prev",
        nextEl: ".detail-content-news-controls-button-next",
      },
  
      // And if we need scrollbar
      // scrollbar: {
      //   el: ".swiper-scrollbar",
      // },
      breakpoints: {
        680: {
          slidesPerView: 2,
        },
        1200: {
          slidesPerView: 3,
        }
      },
    });
  }


}
