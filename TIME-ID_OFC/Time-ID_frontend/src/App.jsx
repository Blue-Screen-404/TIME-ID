import { useEffect, useState } from "react";
import { login, getCurrentUser, logout } from "./services/auth";
import "./App.css";
import iconeUsuario from "./assets/icone-usuario.png";
import iconeSenha from "./assets/icone-senha.png";
import Workspace from "./workspace/Workspace";

function App() {
  const [paginaAtual, setPaginaAtual] = useState(window.location.pathname);
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
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
    function onExpired() { setUser(null); setPassword(""); setCode(""); navegar("login", true); }
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
    try { setUser(await login(email, password, code)); setPassword(""); setCode(""); navegar("inicio", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  async function handleLogout() {
    setBusy(true); setError("");
    try { await logout(); setUser(null); setPassword(""); setCode(""); navegar("login", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  if (checking) return <main className="carregando-sessao" role="status">Verificando sessão...</main>;
  if (user) {
    return <>{error && <p className="erro-sessao" role="alert">{error}</p>}<Workspace paginaAtual={paginaAtual} onUpdateProfile={setUser} user={user} aoNavegar={navegar} aoSair={handleLogout} busy={busy} /></>;
  }
  return (
    <main className="pagina-login">
      <section className="lado-esquerdo">
        <div className="marca">
          <span className="icone-logo">◷</span>

          <h1>
            TIME<strong>ID</strong>
          </h1>
        </div>

        <div className="apresentacao">
          <h2>
            Mais controle, mais pessoas,
            <br />
            mais resultados.
          </h2>

          <p>
            Sistema de ponto e gestão de pessoas
            <br />
            para empresas de todos os tamanhos.
          </p>
        </div>
      </section>

      <section className="lado-direito">
        <div className="card-login">
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
                <div className="campo-login">
                  <img src={iconeUsuario} alt="" />

                  <input
                    type="email"
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

                <div className="campo-login">
                  <img src={iconeSenha} alt="" />

                  <input
                    type="password"
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

                <label className="login-code">Código de autenticação (se ativado)<input type="text" autoComplete="one-time-code" maxLength={64} value={code} onChange={e => setCode(e.target.value)} placeholder="Código do app ou de recuperação" disabled={busy}/><small>Preencha apenas se ativou a autenticação em duas etapas nas configurações.</small></label>
                <button type="submit" disabled={busy}>
                  {busy ? "Aguarde..." : "Entrar"}
                </button>
              </form>

        </div>
      </section>
    </main>
  );
}

export default App;