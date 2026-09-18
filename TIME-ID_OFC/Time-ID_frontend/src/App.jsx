import "./App.css";

function App() {
  return (
    <main className="pagina-login">

      {/* Lado esquerdo da tela */}
      <section className="lado-esquerdo">

        <h1>TIMEID</h1>

        <h2>
          Mais controle, mais pessoas,
          mais resultados.
        </h2>

        <p>
          Sistema de ponto e gestão de pessoas
          para empresas de todos os tamanhos.
        </p>

      </section>


      {/* Lado direito da tela */}
      <section className="lado-direito">

        <div className="card-login">

          <h1>TIMEID</h1>

          <h2>Bem-vindo!</h2>

          <p>
            Faça seu login para acessar o sistema.
          </p>

          <input
            type="text"
            placeholder="Usuário ou e-mail"
          />

          <input
            type="password"
            placeholder="Senha"
          />

          <button>
            Entrar
          </button>

        </div>

      </section>

    </main>
  );
}

export default App;
