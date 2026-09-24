import { useEffect, useState } from "react";
import { login, getCurrentUser, logout } from "./services/auth";
import "./App.css";
import iconeUsuario from "./assets/icone-usuario.png";
import iconeSenha from "./assets/icone-senha.png";
import Inicio from "./pages/Inicio";

function App() {
  const estaNaPaginaInicial =
    window.location.pathname !== "/login";

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [user, setUser] = useState(null);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (estaNaPaginaInicial) {
      setBusy(false);
      return;
    }

    let active = true;

    getCurrentUser()
      .then((currentUser) => {
        if (active) setUser(currentUser);
      })
      .catch((err) => {
        if (active) setError(err.message);
      })
      .finally(() => {
        if (active) setBusy(false);
      });

    return () => {
      active = false;
    };
  }, [estaNaPaginaInicial]);

  async function handleLogin(event) {
    event.preventDefault();

    setBusy(true);
    setError("");

    try {
      const currentUser = await login(email, password);
      setUser(currentUser);
      setPassword("");
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  async function handleLogout() {
    setBusy(true);
    setError("");

    try {
      await logout();
      setUser(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  if (estaNaPaginaInicial) {
    return <Inicio />;
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

          {user ? (
            <section
              className="sessao-usuario"
              aria-label="Sessão do usuário"
            >
              <h2>Bem-vindo, {user.username}!</h2>
              <p>{user.email}</p>
              <p>Login realizado com sucesso.</p>

              <button
                type="button"
                onClick={handleLogout}
                disabled={busy}
              >
                {busy ? "Aguarde..." : "Sair"}
              </button>
            </section>
          ) : (
            <>
              <h2>Bem-vindo!</h2>

              <p>Faça seu login para acessar o sistema.</p>

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

                <button type="submit" disabled={busy}>
                  {busy ? "Aguarde..." : "Entrar"}
                </button>
              </form>
            </>
          )}
        </div>
      </section>
    </main>
  );
}

export default App;