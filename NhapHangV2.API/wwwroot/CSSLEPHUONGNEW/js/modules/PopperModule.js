export default function PopperModule() {
    const popperButtons = document.querySelectorAll(".myButton");
    // const popperSection = document.querySelector("#popper-section");
    // const popperArrow = document.querySelector("#popper-arrow");

    let popperInstance = null;

    //create popper instance
    function createInstance(popperButton, popperPopup) {
        popperInstance = Popper.createPopper(popperButton, popperPopup, {
            placement: "right", //preferred placement of popper
            modifiers: [
                {
                    name: "offset", //offsets popper from the reference/button
                    options: {
                        offset: [0, 8]
                    }
                },
                {
                    name: "flip", //flips popper with allowed placements
                    options: {
                        allowedAutoPlacements: ["right", "left", "top", "bottom"],
                        rootBoundary: "viewport"
                    }
                }
            ]
        });
    }

    //destroy popper instance
    function destroyInstance() {
        if (popperInstance) {
            popperInstance.destroy();
            popperInstance = null;
        }
    }

    //show and create popper
    function showPopper(popperButton, popperPopup) {
        popperPopup.setAttribute("show-popper", "");
        // popperArrow.setAttribute("data-popper-arrow", "");
        createInstance(popperButton, popperPopup);
    }

    //hide and destroy popper instance
    function hidePopper(popperPopup) {
        popperPopup.removeAttribute("show-popper");
        // popperArrow.removeAttribute("data-popper-arrow");
        destroyInstance();
    }

    //toggle show-popper attribute on popper to hide or show it with CSS
    function togglePopper(popperButton, popperPopup) {
        if (popperPopup.hasAttribute("show-popper")) {
            hidePopper(popperPopup);
        } else {
            showPopper(popperButton, popperPopup);
        }
    }
    //execute togglePopper function when clicking the popper reference/button
    popperButtons.forEach((popperButton) => {
        if (window.innerWidth < 1201) {
            popperButton.addEventListener("click", function (e) {
                e.stopPropagation();
                e.preventDefault();
                const parent = popperButton.closest(".popper-parent");
    
                if (parent) {
                    const popperPopup = parent.querySelector(".popper-popup");
                    if (popperPopup) {
                        popperPopup.classList.add('ani')
                        // togglePopper(popperButton, popperPopup);
                        showPopper(popperButton, popperPopup);
                    }
                }
            });

            window.addEventListener("click", function (e) {
                const popperPopups = document.querySelectorAll(".popper-popup");

                popperPopups.forEach((popperPopup) => {
                    popperPopup.classList.remove('ani')
                    hidePopper(popperPopup);
                })
            })
        } else {
            popperButton.addEventListener("mouseenter", function (e) {
                e.preventDefault();
                const parent = popperButton.closest(".popper-parent");
    
                if (parent) {
                    const popperPopup = parent.querySelector(".popper-popup");
                    if (popperPopup) {
                        popperPopup.classList.add('ani')
                        showPopper(popperButton, popperPopup);
                    }
                }
            });

            popperButton.addEventListener("mouseleave", function (e) {
                e.preventDefault();
                const parent = popperButton.closest(".popper-parent");
    
                if (parent) {
                    const popperPopup = parent.querySelector(".popper-popup");
            
                    if (popperPopup) {
                        // togglePopper(popperPopup);
                        popperPopup.classList.remove('ani')
                        // hidePopper(popperPopup);
                    }
                }
            });
        }
    })
}