import { UserManager } from "oidc-client-ts";
import { useEffect, useState } from "react"

const keycloakBaseUrl = import.meta.env.VITE_KEYCLOAK_URL || "https://localhost:8080";
const userManager = new UserManager({
    authority: `${keycloakBaseUrl}/realms/hrassistant`,
    client_id: "hrassistant-app",
    redirect_uri: window.location.origin,
    response_type: "code", // Enables PKCE
    scope: "openid profile email"
})

const Login = () => {

    const [auth, setAuth] = useState({
        token: null,
        expiresAt: null
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleSubmit = (e) => {
        e.preventDefault();
        userManager.signinRedirect();
    }

    useEffect(() => {
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has("code")) {
            setLoading(true);

            userManager.signinRedirectCallback()
                .then(user => {
                    console.log(user)
                    setAuth({ ...auth, token: user.access_token, expiresAt : user.expires_at});

                    window.history.replaceState({}, document.title, window.location.pathname);
                })
                .catch(err => {
                    console.log("Token error:", error);
                    setError(error.message);
                })
                .finally(() => setLoading(false))
        }
    }, [])

    if (loading) return <div>Exchanging authorization code for access token...</div>;
    if (error) return <div>Authentication failed: {error}</div>;

    if (auth.token) {
        return (
            <div style={{ wordBreak: "break-all", padding: "20px" }}>
                <h2>Successfully Authenticated!</h2>
                <p><strong>Access Token:</strong></p>
                <textarea rows={6} cols={60} value={auth.token} readOnly />
            </div>
        );
    }
    return (
        <form onSubmit={handleSubmit}>
            <button type="submit">Login</button>
        </form>
    )
}

export default Login