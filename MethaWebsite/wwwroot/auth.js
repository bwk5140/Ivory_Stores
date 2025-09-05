window.getAccessToken = async function () {
    const account = msalInstance.getAllAccounts()[0];
    if (!account) return null;
    const response = await msalInstance.acquireTokenSilent({
        account,
        scopes: ["your-scope"]
    });
    return response.accessToken;
};
