async function request(path, options = {}) {
  let response;
  try {
    response = await fetch(`/api/auth/${path}`, {
      ...options,
      credentials: "same-origin",
    });
  } catch {
    throw new Error("Não foi possível conectar ao servidor. Tente novamente.");
  }

  if (response.status === 401 && (path === "me" || path === "logout")) {
    return null;
  }
  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("E-mail ou senha inválidos.");
    }
    if (response.status === 400) {
      throw new Error("Confira o formato do e-mail e preencha a senha.");
    }
    throw new Error("Não foi possível concluir a solicitação. Tente novamente.");
  }
  if (response.status === 204) {
    return null;
  }
  return response.json();
}

export function login(email, password) {
  return request("login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
}

export function getCurrentUser() {
  return request("me");
}

export function logout() {
  return request("logout", { method: "POST" });
}
