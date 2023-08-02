export default function FireWorkModule() {
    let fireworks = document.querySelectorAll('.fireworkJS')

    if (fireworks.length > 0) {
        fireworks.forEach((firework) => {
            for (let i = 0; i < 24; i++) {
                firework.innerHTML += `<p class="tua"><span class="tua-wrap"></span></p>`;

                let tua = firework.querySelectorAll(".tua");
                tua.forEach((lv, li) => {
                    let deg = (li * 15);
                    let delay = li * 0.1;
                    lv.style.transform = `translate(-50%, 0%) rotate(` + deg + `deg)`;
                    lv.style.animationDelay = delay + `s`;
                });
            }
        })
    }
}