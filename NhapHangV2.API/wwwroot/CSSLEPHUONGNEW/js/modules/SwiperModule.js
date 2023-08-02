export default function SwiperModule() {
    const banner = new Swiper(".banner-slide .swiper-container", {
        effect: "fade",
        fadeEffect: { crossFade: true },
        disableOnInteraction: false,
        speed: 2200,
        delay: 7000,
        autoplay: true,
    
        pagination: {
          el: ".banner-slide .swiper-pagination",
          clickable: true,
        },
    });

    const priceList = document.querySelector('.price-list-slider .swiper-container')

    if (priceList && window.innerWidth > 601) {
      const priceListSwiper = new Swiper(priceList, {
        slidesPerView: 'auto',
        disableOnInteraction: false,
        speed: 1200,
        delay: 7000,
        autoplay: false,

        // pagination: {
        //   el: ".gift-car-slide-js .swiper-pagination",
        //   clickable: true,
        // },
      });
    }

    // const lpNews = document.querySelector('.lp-news-box .swiper-container')

    // if (lpNews) {
    //   const lpNewsSwiper = new Swiper(lpNews, {
    //     slidesPerView: 'auto',
    //     disableOnInteraction: false,
    //     speed: 1200,
    //     delay: 7000,
    //     autoplay: false,

    //     pagination: {
    //       el: ".lp-news-box .swiper-pagination",
    //       clickable: true,
    //     },
    //   });
    // }

    const swiperGenerals = document.querySelectorAll('.swiper-general')
    if (swiperGenerals.length > 0) {
      swiperGenerals.forEach((ele) => {
        let container = ele.querySelector('.swiper-container');
        let pagination = ele.querySelector('.swiper-pagination');

        let mySwiper = new Swiper(container, {
          slidesPerView: 'auto',
          disableOnInteraction: false,
          speed: 1200,
          delay: 7000,
          autoplay: false,
  
          pagination: {
            el: pagination,
            clickable: true,
          },
        });
      })
    }

    const orderProcess = document.querySelectorAll('.order-process-item')
    if (orderProcess.length > 0) {
      // console.log(orderProcess.length);
      if (orderProcess.length == 6) {
        
      }
    }

    const qSearchs = document.querySelectorAll('.q-search-item-js')
    const qSearchsCurrent = document.querySelector('.q-search-current')
    if (qSearchs.length > 0) {
      qSearchs.forEach((ele) => {
        ele.onclick = function() {
          let imgSrc = ele.querySelector('img').src
          qSearchsCurrent.src = imgSrc
        }
      })
    }
}
