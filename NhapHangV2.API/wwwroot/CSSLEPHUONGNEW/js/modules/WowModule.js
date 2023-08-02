export default function WowModule() {
    var wow = new WOW(
        {
            boxClass: 'wow',      // default
            animateClass: 'animated', // default
            offset: 0,          // default
            mobile:false,
            live: false,       // default
          
        }
    )
    wow.init();
}