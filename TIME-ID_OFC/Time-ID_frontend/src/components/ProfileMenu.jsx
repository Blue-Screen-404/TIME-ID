import { useEffect, useRef, useState } from "react";
import { ChevronDown, UserRound, Pencil, Camera } from "lucide-react";
import { updateProfile } from "../services/auth";
import "./ProfileMenu.css";

export default function ProfileMenu({ user, onUpdate }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const [mode, setMode] = useState(null);
  const [draft, setDraft] = useState(null);
  const [saving, setSaving] = useState(false);
  const [reading, setReading] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const dialog = useRef(null);
  const menu = useRef(null);
  const trigger = useRef(null);
  const readVersion = useRef(0);
  const admin = user.role === "Administrador";
  const initials = user.username.split(/\s+/).slice(0, 2).map(word => word[0]).join("").toUpperCase();

  useEffect(() => {
    if (!menuOpen) return;
    function outside(event) { if (!menu.current?.contains(event.target)) setMenuOpen(false); }
    function escape(event) { if (event.key === "Escape") { setMenuOpen(false); trigger.current?.focus(); } }
    document.addEventListener("pointerdown", outside);
    document.addEventListener("keydown", escape);
    return () => { document.removeEventListener("pointerdown", outside); document.removeEventListener("keydown", escape); };
  }, [menuOpen]);

  useEffect(() => {
    if (!mode) return;
    const element = dialog.current;
    element.showModal();
    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => { element.close(); document.body.style.overflow = previous; };
  }, [Boolean(mode)]);

  function openProfile() {
    setDraft({ username: user.username, email: user.email, photoUrl: user.photoUrl ?? null });
    setMode("view"); setMenuOpen(false); setError(""); setNotice("");
  }
  function closeProfile() {
    readVersion.current++;
    setMode(null); setReading(false); setError("");
    // O diálogo devolve o foco ao gatilho do perfil.
    requestAnimationFrame(() => trigger.current?.focus());
  }
  async function selectPhoto(event) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;
    setError("");
    if (!["image/png", "image/jpeg", "image/webp"].includes(file.type) || file.size > 2 * 1024 * 1024) {
      setError("Escolha uma foto PNG, JPEG ou WebP de até 2 MB."); return;
    }
    const version = ++readVersion.current;
    setReading(true);
    try {
      const photo = await new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(new Error("Não foi possível ler a foto."));
        reader.readAsDataURL(file);
      });
      await new Promise((resolve, reject) => {
        const image = new Image(); image.onload = resolve;
        image.onerror = () => reject(new Error("O arquivo não é uma imagem válida.")); image.src = photo;
      });
      if (version === readVersion.current) setDraft(current => ({ ...current, photoUrl: photo }));
    } catch (err) { if (version === readVersion.current) setError(err.message); }
    finally { if (version === readVersion.current) setReading(false); }
  }
  async function save(event) {
    event.preventDefault();
    if (mode !== "edit" || saving || reading) return;
    setSaving(true); setError("");
    try {
      const payload = admin ? { ...draft, username: draft.username.trim(), email: draft.email.trim() } : { photoUrl: draft.photoUrl };
      const updated = await updateProfile(payload);
      onUpdate(updated); closeProfile(); setNotice("Perfil atualizado com sucesso.");
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  return <div className="profile-control" ref={menu}>
    <button ref={trigger} type="button" className="perfil profile-trigger" aria-expanded={menuOpen} aria-controls="profile-options" onClick={() => setMenuOpen(open => !open)}>
      <span className="profile-avatar">{user.photoUrl ? <img src={user.photoUrl} alt="" /> : initials}</span>
      <span className="profile-identity"><strong>{user.username}</strong><span>{user.role}</span></span>
      <ChevronDown size={18} aria-hidden="true" />
    </button>
    {menuOpen && <div id="profile-options" className="profile-options">
      <button type="button" onClick={openProfile}><UserRound size={18} aria-hidden="true" />Ver perfil</button>
    </div>}
    <span className="profile-notice" role="status">{notice}</span>
    <dialog ref={dialog} className="profile-dialog" aria-labelledby="profile-title" aria-describedby="profile-description" onCancel={event => event.preventDefault()}>
      {draft && <form onSubmit={save}>
        <header className="profile-modal-header"><span className="profile-modal-icon"><UserRound size={24} aria-hidden="true" /></span><div><h2 id="profile-title">{mode === "edit" ? "Editar perfil" : "Meu perfil"}</h2><p id="profile-description">{admin ? "Gerencie seus dados e sua foto de perfil." : "Você pode alterar apenas sua foto de perfil."}</p></div></header>
        <div className="profile-modal-body">
          <div className="profile-photo-row"><span className="profile-avatar profile-avatar-large">{draft.photoUrl ? <img src={draft.photoUrl} alt="Prévia da foto do perfil" /> : initials}</span>
            <div><strong>{user.role}</strong>{mode === "edit" && <><label className="profile-photo-label"><Camera size={16} aria-hidden="true" />Alterar foto<input type="file" aria-label="Escolher foto de perfil" accept="image/png,image/jpeg,image/webp" onChange={selectPhoto} disabled={saving || reading} /></label><small>PNG, JPEG ou WebP · até 2 MB</small>{draft.photoUrl && <button className="profile-remove" type="button" disabled={saving || reading} onClick={() => setDraft(current => ({ ...current, photoUrl: null }))}>Remover foto</button>}</>}</div>
          </div>
          <label className="profile-field">Nome<input autoComplete="name" value={draft.username} readOnly={mode !== "edit" || !admin} disabled={saving} required minLength={2} maxLength={100} onChange={event => setDraft({ ...draft, username: event.target.value })} /></label>
          <label className="profile-field">E-mail<input type="email" autoComplete="email" value={draft.email} readOnly={mode !== "edit" || !admin} disabled={saving} required maxLength={254} onChange={event => setDraft({ ...draft, email: event.target.value })} /></label>
          <p className="profile-hint">{admin ? "Se alterar o e-mail, use o novo endereço no próximo login." : "Nome, e-mail e tipo de acesso são somente leitura."}</p>
          {error && <p className="profile-error" role="alert">{error}</p>}
          {reading && <p role="status">Preparando foto...</p>}
        </div>
        <footer className="profile-modal-footer"><button type="button" className="profile-secondary" onClick={closeProfile} disabled={saving}>Cancelar</button>{mode === "view" ? <button type="button" className="profile-primary" onClick={event => { event.preventDefault(); setMode("edit"); }}><Pencil size={16} aria-hidden="true" />{admin ? "Editar perfil" : "Editar foto"}</button> : <button type="submit" className="profile-primary" disabled={saving || reading}>{saving ? "Salvando..." : "Salvar"}</button>}</footer>
      </form>}
    </dialog>
  </div>;
}
