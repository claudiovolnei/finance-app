(() => {
  const SECURITY_SCHEMA = "Bearer";
  const TOKEN_KEY = "finance.swagger.jwt";

  const setBearerToken = (token) => {
    if (!token || !window.ui?.authActions) return;

    const bearerValue = token.startsWith("Bearer ") ? token : `Bearer ${token}`;

    window.ui.authActions.authorize({
      [SECURITY_SCHEMA]: {
        name: SECURITY_SCHEMA,
        schema: {
          type: "http",
          scheme: "bearer",
          bearerFormat: "JWT"
        },
        value: bearerValue
      }
    });
  };

  const originalFetch = window.fetch.bind(window);
  window.fetch = async (...args) => {
    const response = await originalFetch(...args);

    try {
      const [request] = args;
      const requestUrl = typeof request === "string" ? request : request.url;

      if (requestUrl?.includes("/auth/login") && response.ok) {
        const body = await response.clone().json();
        if (body?.token) {
          localStorage.setItem(TOKEN_KEY, body.token);
          setBearerToken(body.token);
        }
      }
    } catch (_) {
      // ignore parsing errors
    }

    return response;
  };

  window.addEventListener("load", () => {
    const storedToken = localStorage.getItem(TOKEN_KEY);
    if (storedToken) {
      setBearerToken(storedToken);
    }
  });
})();
