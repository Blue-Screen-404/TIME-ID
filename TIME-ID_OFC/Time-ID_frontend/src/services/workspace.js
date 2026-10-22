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
  if (!response.ok) {
    const error = await response.json().catch(() => null);
    if (response.status === 401) window.dispatchEvent(new Event("timeid:expired"));
    throw new Error(error?.title ?? "Não foi possível exportar. Confira suas permissões e o período.");
  }
  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement("a"); link.href = url; link.download = name; link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
export function downloadAttendancePdf(data, date, employeeId = "") {
  const employees = data.employees.filter(e => (!employeeId || e.id === employeeId) && data.punches.some(p => p.employeeId === e.id && dayKey(p.at) === date));
  if (!employees.length) throw new Error("Não há marcações nesta data para gerar o espelho.");
  const esc = value => String(value ?? "").replace(/\\/g, "\\\\").replace(/\(/g, "\\(").replace(/\)/g, "\\)").replace(/[\r\n]/g, " ");
  const pdfBytes = value => Uint8Array.from(value, char => char.charCodeAt(0) & 255);
  const pages = employees.map(employee => {
    const punches = data.punches.filter(p => p.employeeId === employee.id && dayKey(p.at) === date).sort((a, b) => a.at.localeCompare(b.at));
    const lines = [data.settings?.name || "TIMEID", data.settings?.cnpj ? `CNPJ: ${data.settings.cnpj}` : "", "ESPELHO DE PONTO", `Data: ${new Date(`${date}T12:00:00`).toLocaleDateString("pt-BR")}`, `Colaborador: ${employee.name}`, `CPF: ${employee.cpf || "Não informado"}    Matrícula: ${employee.registration || "—"}`, "", "MARCAÇÕES REGISTRADAS"];
    for (const punch of punches) {
      const time = new Date(punch.at).toLocaleTimeString("pt-BR", { timeZone: "America/Sao_Paulo", hour: "2-digit", minute: "2-digit", second: "2-digit" });
      lines.push(`${time}  ${punch.kind}  (${punch.source === "Facial local" ? "Terminal facial" : "Aplicativo"})`);
      if (punch.correction) lines.push(`   Correção: ${punch.correction}`);
    }
    let worked = 0;
    for (let i = 0; i + 1 < punches.length; i += 2) worked += new Date(punches[i + 1].at) - new Date(punches[i].at);
    lines.push("", `Total em intervalos concluídos: ${Math.floor(worked / 3600000)}h ${Math.floor(worked / 60000) % 60}min`, "", "Espelho interno gerado a partir das marcações registradas no TIMEID.", `Gerado em ${new Date().toLocaleString("pt-BR", { timeZone: "America/Sao_Paulo" })} (Brasília)`);
    return `BT\n/F1 11 Tf\n50 750 Td\n14 TL\n${lines.map(line => `(${esc(line)}) Tj\nT*\n`).join("")}ET`;
  });
  const objects = ["<< /Type /Catalog /Pages 2 0 R >>", `<< /Type /Pages /Kids [${pages.map((_, i) => `${5 + i * 2} 0 R`).join(" ")}] /Count ${pages.length} >>`, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"];
  for (const page of pages) {
    const streamId = objects.length + 1;
    objects.push(`<< /Length ${pdfBytes(page).length} >>\nstream\n${page}\nendstream`);
    objects.push(`<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents ${streamId} 0 R >>`);
  }
  let pdf = "%PDF-1.4\n%TIMEID\n";
  const offsets = [0];
  objects.forEach((object, i) => { offsets.push(pdf.length); pdf += `${i + 1} 0 obj\n${object}\nendobj\n`; });
  const xref = pdf.length;
  pdf += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n${offsets.slice(1).map(offset => `${String(offset).padStart(10, "0")} 00000 n \n`).join("")}trailer\n<< /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF`;
  const blob = new Blob([pdfBytes(pdf)], { type: "application/pdf" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a"); link.href = url; link.download = `espelho-de-ponto-${date}.pdf`; link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
export const dateLabel = value => value ? new Date(`${value}T12:00:00`).toLocaleDateString("pt-BR") : "—";
export const timeLabel = value => new Date(value).toLocaleTimeString("pt-BR", { timeZone: "America/Sao_Paulo", hour: "2-digit", minute: "2-digit" });
export const dayKey = value => new Intl.DateTimeFormat("en-CA", { timeZone: "America/Sao_Paulo", year: "numeric", month: "2-digit", day: "2-digit" }).format(new Date(value));
export const normalize = value => String(value ?? "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
