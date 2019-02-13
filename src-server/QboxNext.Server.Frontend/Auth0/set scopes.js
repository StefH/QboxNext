function (user, context, callback) {
    var req = context.request;

    // Get requested scopes
    var scopes = (req.query && req.query.scope) || (req.body && req.body.scope);

    // Normalize scopes into an array
    scopes = (scopes && scopes.split(" ")) || [];

    // Restrict the access token scopes according to the current user
    context.accessToken.scope = restrictScopes(user, scopes);

    callback(null, user, context);

    function restrictScopes(user, requested) {
        return ['openid', 'read:data'];
    }
}