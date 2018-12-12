'use strict';
let WinApp = {
    VIEWID: "__appview",
    viewport: null,
    dblclick: () => { },
    mousemove: (event) => { },
    render: function (canvasElement) {
        this.viewport = this.viewport || document.getElementById(this.VIEWID);
        if (this.viewport == null) {
            this.viewport = document.createElement("img");
            this.viewport.id = this.VIEWID;
            canvasElement.parentNode.insertBefore(this.viewport, canvasElement);
        }
        this.viewport.src = canvasElement.toDataURL("image/png");
    },
    callback: (json) => {
        var event = new MessageEvent("WinAppCallback", { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(json) });
        document.dispatchEvent(event);
    },
}