window.generatePdfFromHtml = async (elementId) => {
    window.scrollTo(0, 0);
    const { jsPDF } = window.jspdf;
    const element = document.getElementById(elementId);
    const canvas = await html2canvas(element, {
        width: element.scrollWidth,
        height: element.scrollHeight,
        windowWidth: element.scrollWidth,
        windowHeight: element.scrollHeight,
        scale: 1
    });
    const doc = new jsPDF({
        orientation: "portrait",
        unit: "px",
        format: [canvas.width, canvas.height] // dynamically match canvas size
    });
    const imgData = canvas.toDataURL("image/png");

    const img = new Image();
    const pdfWidth = doc.internal.pageSize.getWidth();
    const pdfHeight = doc.internal.pageSize.getHeight();

    const widthRatio = pdfWidth / canvas.width;
    const heightRatio = pdfHeight / canvas.height;
    const ratio = Math.min(widthRatio, heightRatio);

    const imgWidth = canvas.width * ratio;
    const imgHeight = canvas.height * ratio;

    img.src = imgData;
    img.onload = () => {
        doc.addImage(img, "PNG", 70, 70, imgWidth, imgHeight);
        doc.save("Order-Invoice.pdf");
    };
};
