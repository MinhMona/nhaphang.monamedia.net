import { CountUp } from "../libs/countUp/countUp.min.js";
export default function CountingModule() {
  let num = document.querySelectorAll(".countNum");
  num.forEach((v) => {
    let n = parseInt(v.textContent);
    var countUp = new CountUp(v, n, 3, 2.5, {
      separator: ".",
    });
    let check = true;
    window.addEventListener("scroll", () => {
      if (v.getBoundingClientRect().top < window.innerHeight && check) {
        countUp.start();
        check = false;
      }
    });
  });
}
