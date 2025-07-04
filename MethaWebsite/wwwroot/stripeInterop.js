let stripe;
let card;

window.initializeCardElement = function (publicKey) {
    stripe = Stripe(publicKey);
    const elements = stripe.elements();
    card = elements.create("card");
    card.mount("#card-element");
};
window.confirmCardSetup = async function (clientSecret) {
    return await stripe.confirmCardSetup(clientSecret, {
        payment_method: { card: card }
    });
};
window.tokenizeCard = async function () {
    if (!stripe || !card) {
        return { error: "Card not initialized" };
    }

    const result = await stripe.createPaymentMethod({
        type: "card",
        card: card,
    });

    if (result.error) {
        return { error: result.error.message };
    }

    return {
        paymentMethodId: result.paymentMethod.id,
        fingerprint: result.paymentMethod.card.fingerprint,
    };
};