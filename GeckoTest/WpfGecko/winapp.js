'use strict';
let WinApp = {
    _windows: {},
    DEFAULT_WINDOWID: "__appview",
    _registerWindow: function (id) {
        this._dispatchWindowEvent({ id: id, command: "create"});
    },
    _dispatchWindowEvent: function(json) {
        var event = new MessageEvent("WinAppCallback", { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(json) });
        document.dispatchEvent(event);
    },
    _onMouseMove: function (event) {
        for (var id in this._windows) {
            var handler = this._windows[id].mousemove;
            if (handler != null)
                handler.apply(this._windows[id], [event]);
        }
    },
    _onDoubleClick: function (id) {
        if (!(id in this._windows))
            return;
        var handler = this._windows[id].dblclick;
        if (handler == null)
            return;
        handler.apply(this._windows[id]);
    },
    _windowNotify: function (winData) {
        var id = winData.id;
        if (!(id in this._windows))
            return;
        var handler = this._windows[id].moveNotify;
        if (handler != null)
            handler.apply(this._windows[id], [{ x: winData.left, y: winData.top }]);

        var handler = this._windows[id].resizeNotify;
        if (handler != null)
            handler.apply(this._windows[id], [{ w: winData.width, h: winData.height }]);

        var handler = this._windows[id].opacityNotify;
        if (handler != null)
            handler.apply(this._windows[id], [{ opacity: winData.opacity }]);
    },
    createWindow: function (id) {
        id = id || this.DEFAULT_WINDOWID;
        this._registerWindow(id);
        var win = {
            id: id,
            viewport: null,
            dblclick: null,
            mousemove: null,
            moveNotify: null,
            resizeNotify: null,
            opacityNotify: null,
            render: function (canvasElement) {
                this.viewport = this.viewport || document.getElementById(this.viewId);
                if (this.viewport == null) {
                    this.viewport = document.createElement("img");
                    this.viewport.id = this.id;
                    canvasElement.parentNode.insertBefore(this.viewport, canvasElement);
                }
                this.viewport.src = canvasElement.toDataURL("image/png");
            },
            _dispatchEvent: function (command, parameters) {
                WinApp._dispatchWindowEvent({ id: this.id, command: command, parameters: JSON.stringify(parameters) });
            },
            move: function(x, y) {
                this._dispatchEvent("move", { x: x, y: y });
            },
            setWidth: function(w) {
                this._dispatchEvent("setWidth", { w: w });
            },
            setHeight: function (h) {
                this._dispatchEvent("setHeight", { h: h });
            },
            resize: function (w, h) {
                this._dispatchEvent("resize", { w: w, h: h });
            },
            setOpacity: function (opacity) {
                this._dispatchEvent("setOpacity", { opacity: opacity });
            },
        };
        this._windows[id] = win;
        return this._windows[id];
    },
}