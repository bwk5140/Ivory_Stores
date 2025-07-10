window.cartStorage = {
    saveCart: function (key, cartData) {
        localStorage.setItem(key, cartData);
    },
    loadCart: function (key) {
        return localStorage.getItem(key);
    },
    clearCart: function (key) {
        localStorage.removeItem(key);
    }
};