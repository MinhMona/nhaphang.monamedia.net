export default function CountDownDateModule() {
    

    const countDowns = document.querySelectorAll('.countdown');
    if (countDowns) {
        countDowns.forEach(function (countDown) {
            const daysEl = countDown.querySelector('.daysJS');
            const hoursEl = countDown.querySelector('.hoursJS');
            const minsEL = countDown.querySelector('.minutesJS');
            const secondsEL = countDown.querySelector('.secondsJS');
            const timeEnd = countDown.getAttribute("data-timeInDay");

            function countdown() {
                const timeEndDate = new Date(timeEnd);
                const currentDate = new Date();
            
                const totalSeconds = (timeEndDate - currentDate) / 1000;
                const minutes = Math.floor(totalSeconds/ 60) % 60;
                const hours = Math.floor(totalSeconds /3600) % 24;
                const days = Math.floor(totalSeconds /3600/ 24);
                const seconds = Math.floor(totalSeconds) % 60;
                
                if (totalSeconds < 0) {
                    daysEl.innerText = '00'
                    hoursEl.innerText = '00'
                    minsEL.innerText = '00'
                    secondsEL.innerText = '00'
                } else {
                    daysEl.innerText = ('0' + days).slice(-2);
                    hoursEl.innerText = ('0' + hours).slice(-2);
                    minsEL.innerText = ('0' + minutes).slice(-2);
                    secondsEL.innerText = ('0' + seconds).slice(-2);
                    // daysEl.innerText = days
                    // hoursEl.innerText = hours
                    // minsEL.innerText = minutes
                    // secondsEL.innerText = seconds
                }
                // console.log(seconds.length)
                
            }
            setInterval(countdown, 1000);
        })
    }
}