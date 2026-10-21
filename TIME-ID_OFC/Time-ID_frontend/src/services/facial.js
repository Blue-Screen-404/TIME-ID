async function request(path, options = {}) {
  let response;
  try {
    response = await fetch(`/api/facial${path}`, {
      credentials: "same-origin", ...options,
      headers: { "Content-Type": "application/json", ...options.headers },
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch { throw new Error("Não foi possível conectar ao serviço local da empresa."); }
  const data = await response.json().catch(() => null);
  if (!response.ok) throw new Error(data?.title ?? "Não foi possível concluir a operação.");
  return data;
}
export const facialProfiles = () => request("/profiles");
export const enrollFace = (body) => request("/profiles", { method: "POST", body });
export const removeFace = (employeeId) => request(`/profiles/${employeeId}`, { method: "DELETE" });
export const facialPunch = (descriptor) => request("/terminal/punch", { method: "POST", body: { descriptor } });
