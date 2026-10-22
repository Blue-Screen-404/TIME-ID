import { useEffect, useRef, useState } from "react";
import { ChevronLeft, ChevronRight, Plus, ShieldCheck } from "lucide-react";
export function Heading({ title, subtitle, action, label = "Adicionar" }) { return <div className="work-heading"><div><h2>{title}</h2><p>{subtitle}</p></div>{action && <button className="btn primary" onClick={action}><Plus size={17} />{label}</button>}</div>; }
export function Badge({ children, good = true }) { return <span className={`status-badge ${good ? "good" : "muted"}`}>{children}</span>; }
export function Empty({ text = "Nenhum registro encontrado.", action }) { return <div className="empty-state"><p>{text}</p>{action}</div>; }
export function Table({ columns, rows, render, pageSize = 8, empty }) {
  const [page, setPage] = useState(1); const pages = Math.max(1, Math.ceil(rows.length / pageSize)); const current = Math.min(page, pages);
  useEffect(() => setPage(1), [rows.length]);
  return <><div className="work-table"><table><thead><tr>{columns.map(c => <th key={c} scope="col">{c}</th>)}</tr></thead><tbody>{rows.slice((current - 1) * pageSize, current * pageSize).map(render)}</tbody></table></div>{!rows.length && <Empty text={empty} />}{rows.length > pageSize && <div className="pagination"><button className="icon-button" aria-label="Página anterior" disabled={current === 1} onClick={() => setPage(current - 1)}><ChevronLeft size={18} /></button><span>Página {current} de {pages} · {rows.length} registros</span><button className="icon-button" aria-label="Próxima página" disabled={current === pages} onClick={() => setPage(current + 1)}><ChevronRight size={18} /></button></div>}</>;
}
export function Field({ label, children, ...props }) { return <label className="work-field"><span>{label}</span>{children ?? <input {...props} />}</label>; }
export function Modal({ title, children, onCancel, onSubmit, busy, submitLabel = "Salvar", error }) {
  const ref = useRef(null);
  useEffect(() => { const element = ref.current; const focus = document.activeElement; element.showModal(); const previous = document.body.style.overflow; document.body.style.overflow = "hidden"; return () => { element.close(); document.body.style.overflow = previous; focus?.focus(); }; }, []);
  return <dialog className="work-modal" ref={ref} aria-labelledby="work-modal-title" onCancel={e => e.preventDefault()}><form onSubmit={onSubmit}><header><h2 id="work-modal-title">{title}</h2></header><div className="modal-body"><fieldset disabled={busy}>{children}</fieldset>{error && <p className="form-error" role="alert">{error}</p>}</div><footer><button type="button" className="btn secondary" disabled={busy} onClick={onCancel}>Cancelar</button><button className="btn primary" type="submit" disabled={busy}>{busy ? "Salvando..." : submitLabel}</button></footer></form></dialog>;
}
export function SecurityCard() { return <aside className="security-tile"><ShieldCheck size={30} /><h3>Gestão com responsabilidade</h3><p>Controle de acesso e histórico das alterações para acompanhar sua equipe.</p><div className="security-wave" /></aside>; }
export function Stat({ icon: Icon, value, label }) { return <div className="stat"><span className="stat-icon"><Icon size={23} /></span><div><strong>{value}</strong><span>{label}</span></div></div>; }
