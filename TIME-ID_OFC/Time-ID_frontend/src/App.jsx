import { useEffect, useState } from "react";
import { login, getCurrentUser, logout } from "./services/auth";
import { LockKeyhole, Mail, ShieldCheck, UsersRound } from "lucide-react";
import pessoasLogin from "./assets/pessoas-login.png";
import "./App.css";
import Workspace from "./workspace/Workspace";
import { FacialTerminal } from "./workspace/Facial";

function App() {
  const [paginaAtual, setPaginaAtual] = useState(window.location.pathname);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [user, setUser] = useState(null);
  const [busy, setBusy] = useState(false);
  const [checking, setChecking] = useState(true);
  const [error, setError] = useState("");

  function navegar(para, replace = false) {
    const path = `/${para}`;
    window.history[replace ? "replaceState" : "pushState"]({}, "", path);
    setPaginaAtual(path);
  }

  useEffect(() => {
    let active = true;
    async function checkSession() {
      if (window.location.pathname === "/terminal") { if (active) setChecking(false); return; }
      try {
        const current = await getCurrentUser();
        if (!active) return;
        setUser(current);
        if (!current) navegar("login", true);
        else if (["/", "/login"].includes(window.location.pathname)) navegar("inicio", true);
      } catch (err) {
        if (active) { setUser(null); setError(err.message); navegar("login", true); }
      } finally { if (active) setChecking(false); }
    }
    function onHistory() { setPaginaAtual(window.location.pathname); checkSession(); }
    function onVisible() { if (document.visibilityState === "visible") checkSession(); }
    function onExpired() { setUser(null); setPassword(""); navegar("login", true); }
    window.addEventListener("timeid:expired", onExpired);
    checkSession();
    const interval = window.setInterval(checkSession, 60000);
    window.addEventListener("popstate", onHistory);
    window.addEventListener("pageshow", onHistory);
    document.addEventListener("visibilitychange", onVisible);
    return () => {
      active = false;
      window.removeEventListener("timeid:expired", onExpired);
      window.clearInterval(interval);
      window.removeEventListener("popstate", onHistory);
      window.removeEventListener("pageshow", onHistory);
      document.removeEventListener("visibilitychange", onVisible);
    };
  }, []);

  async function handleLogin(event) {
    event.preventDefault(); setBusy(true); setError("");
    try { setUser(await login(email, password)); setPassword(""); navegar("inicio", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  async function handleLogout() {
    setBusy(true); setError("");
    try { await logout(); setUser(null); setPassword(""); navegar("login", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  if (window.location.pathname === "/terminal") return <FacialTerminal/>;
  if (checking) return <main className="carregando-sessao" role="status">Verificando sessão...</main>;
  if (user) {
    return <>{error && <p className="erro-sessao" role="alert">{error}</p>}<Workspace paginaAtual={paginaAtual} onUpdateProfile={setUser} user={user} aoNavegar={navegar} aoSair={handleLogout} busy={busy} /></>;
  }
  return (
    <main className="pagina-login">
      <section className="lado-esquerdo">
        <div className="marca">
          <span className="icone-logo" aria-hidden="true">◷</span>

          <h1>
            TIME<strong>ID</strong>
          </h1>
        </div>

        <div className="apresentacao">
          <h2>
            Mais controle,
            <br />
            Mais tempo para o
            <br />
            <strong>que importa.</strong>
          </h2>

          <p>
            Sistema de ponto e <strong>gestão de pessoas</strong>
            <br />
            para empresas de <strong>todos os tamanhos.</strong>
          </p>
        </div>

        <div className="relogio-destaque" aria-label="Relógio decorativo animado">
          <span className="ponteiro ponteiro-horas" />
          <span className="ponteiro ponteiro-minutos" />
          <span className="miolo-relogio" />
        </div>

        <img
          className="pessoas-login"
          src={pessoasLogin}
          alt="Três colaboradores do TIMEID"
        />

        <div className="beneficios-login">
          <div className="beneficio-login">
            <ShieldCheck size={30} aria-hidden="true" />
            <p><strong>Segurança e confiabilidade</strong><span>Seus dados protegidos em todas as etapas.</span></p>
          </div>
          <div className="beneficio-login">
            <UsersRound size={30} aria-hidden="true" />
            <p><strong>Mais de 1M+ de usuários</strong><span>Em empresas de todo o Brasil</span></p>
          </div>
        </div>
      </section>

      <section className="lado-direito">
        <div className="card-login" id="form-login">
          <div className="marca-card">
            <span className="icone-logo">◷</span>

            <h1>
              TIME<strong>ID</strong>
            </h1>
          </div>

              <h2>Bem-vindo!</h2>

              <p>Faça seu login para acessar o sistema.</p>

              {error && <p className="mensagem-erro" role="alert">{error}</p>}
              <form onSubmit={handleLogin}>
                <label className="rotulo-campo" htmlFor="email">E-mail</label>
                <div className="campo-login">
                  <Mail aria-hidden="true" />

                  <input
                    type="email"
                    id="email"
                    name="email"
                    aria-label="E-mail"
                    autoComplete="username"
                    required
                    maxLength={254}
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    disabled={busy}
                    placeholder="E-mail"
                  />
                </div>

                <label className="rotulo-campo" htmlFor="senha">Senha</label>
                <div className="campo-login">
                  <LockKeyhole aria-hidden="true" />

                  <input
                    type="password"
                    id="senha"
                    name="password"
                    aria-label="Senha"
                    autoComplete="current-password"
                    required
                    maxLength={256}
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    disabled={busy}
                    placeholder="Senha"
                  />
                </div>

                <div className="opcoes-login">
                  <label>
                    <input type="checkbox" />
                    Lembrar de mim
                  </label>

                  <button type="button" className="link-senha">
                    Esqueceu a senha?
                  </button>
                </div>

                <button type="submit" disabled={busy}>
                  {busy ? "Aguarde..." : "Entrar  →"}
                </button>
              </form>

              <div className="acesso-seguro">
                <span />
                <p><ShieldCheck size={18} /> Acesso seguro — Seus dados protegidos.</p>
                <span />
              </div>

        </div>
      </section>
    </main>
  );
}

export default App;
