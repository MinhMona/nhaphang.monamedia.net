export default function ImageSplitModule() {
    const imgSplits = document.querySelectorAll('.imgSplit')

    if (imgSplits.length > 0) {
        imgSplits.forEach((imgSplit) => {
            const imgEle = imgSplit.querySelector("img").src;

            for (let i = 0; i < 6; i++) {
                const imgItem = document.createElement("div");
                imgItem.className = "img-item";
                imgItem.innerHTML = `<img src="${imgEle}" alt="">`;
                imgSplit.appendChild(imgItem);
            }
        })
    }
}