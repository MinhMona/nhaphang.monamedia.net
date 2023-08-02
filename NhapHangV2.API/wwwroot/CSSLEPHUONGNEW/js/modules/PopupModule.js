export default function PopupModule() {
    const openPops = document.querySelectorAll('.openPop')
    const closePops = document.querySelectorAll('.popupClose')
    const pageBG = document.querySelector('.pageBG')
    const popupBox = document.querySelector('.popJS')

    if (openPops.length > 0) {
        openPops.forEach((openPop) => {
            openPop.onclick = (e) => {
                e.preventDefault()
                console.log('123');
                popupBox.classList.add('active')
                pageBG.classList.add('active')
            }
        })
    }

    if (closePops.length > 0) {
        closePops.forEach((closePop) => {
            closePop.onclick = () => {
                closePopForm()
            }
        })

        pageBG.onclick = () => {
            closePopForm()
        }
    }

    function closePopForm() {
        popupBox.classList.remove('active')
        pageBG.classList.remove('active')
    }
}