import "./App.css";
import iconeUsuario from "./assets/icone-usuario.png";
import iconeSenha from "./assets/icone-senha.png";

function App() {
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

          <form>
            <div className="campo-login">
              <img src={iconeUsuario} alt="" />

              <input
                type="text"
                placeholder="Usuário ou e-mail"
              />
            </div>

            <div className="campo-login">
              <img src={iconeSenha} alt="" />

              <input
                type="password"
                placeholder="Senha"
              />
            </div>

            <div className="opcoes-login">
              <label>
                <input type="checkbox" />
                Lembrar de mim
              </label>

              <a href="/">Esqueceu a senha?</a>
            </div>

            <button type="button">Entrar</button>
          </form>
        </div>
      </section>
    </main>
  );
}

export default App;