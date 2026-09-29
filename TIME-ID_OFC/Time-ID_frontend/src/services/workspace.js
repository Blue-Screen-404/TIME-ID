export async function api(path = "", options = {}) {
  let response;
  try { response = await fetch(`/api/workspace${path}`, { credentials: "same-origin", ...options, headers: { "Content-Type": "application/json", ...options.headers }, body: options.body === undefined ? undefined : JSON.stringify(options.body) }); }
  catch { throw new Error("Não foi possível conectar ao servidor. Tente novamente."); }
  if (response.status === 401) { window.dispatchEvent(new Event("timeid:expired")); throw new Error("Sua sessão expirou. Entre novamente."); }
  if (!response.ok) {
    const error = await response.json().catch(() => null);
    throw new Error(error?.errors ? Object.values(error.errors).flat().join(" ") : error?.title ?? "Não foi possível concluir a operação.");
  }
  return response.status === 204 ? null : response.json();
}
export async function download(path, name) {
  const response = await fetch(`/api/workspace${path}`, { credentials: "same-origin" });
  if (!response.ok) throw new Error("Não foi possível exportar. Confira suas permissões e o período.");
  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement("a"); link.href = url; link.download = name; link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
export const dateLabel = value => value ? new Date(`${value}T12:00:00`).toLocaleDateString("pt-BR") : "—";
export const timeLabel = value => new Date(value).toLocaleTimeString("pt-BR", { timeZone: "America/Sao_Paulo", hour: "2-digit", minute: "2-digit" });
export const dayKey = value => new Intl.DateTimeFormat("en-CA", { timeZone: "America/Sao_Paulo", year: "numeric", month: "2-digit", day: "2-digit" }).format(new Date(value));
export const normalize = value => String(value ?? "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
