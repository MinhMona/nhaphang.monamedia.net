export default function MenuHandler() {
  const barElm = document.querySelector("#menuBar");
  const overlayElm = document.querySelector("#overlay");
  const menuListElm = document.querySelector("#hd-nav-menu");
  const body = document.querySelector("body");
    const popup = document.querySelector(".popup");
    try {
        const buttonClose = popup.querySelectorAll("[target-close]");
        if (buttonClose.length) {
            buttonClose.forEach((btn) => {
                btn.addEventListener("click", (e) => {
                    const value = btn.getAttribute("target-close");

                    if (value === "never") {
                        document.cookie = "isRender=false";
                    }

                    popup.classList.remove("open");
                    body.classList.remove("no-scroll");
                    overlayElm.classList.remove("active");
                });
            });
        }
    } catch {

    }
  if (barElm) {
    barElm.addEventListener("click", () => {
      body.classList.add("no-scroll");
      menuListElm.classList.add("active");
      overlayElm.classList.add("active");
    });
  }

  if (overlayElm) {
    overlayElm.addEventListener("click", () => {
      body.classList.remove("no-scroll");
      menuListElm.classList.remove("active");
      overlayElm.classList.remove("active");
    });
  }

  if (popup) {
    if (popup.classList.contains("open")) {
      body.classList.add("no-scroll");
    } else {
      body.classList.remove("no-scroll");
    }
  }

  
}
