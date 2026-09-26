import ProfileMenu from "../components/ProfileMenu";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import "./Inicio.css";
import "./Cadastro.css";
import {
  ArrowLeft,
  Building2,
  KeyRound,
  MapPin,
  UserRoundPlus,
} from "lucide-react";

function Cadastro({ aoNavegar, aoSair, user, busy, onUpdateProfile }) {
  const [etapaAtual, setEtapaAtual] = useState(1);

  function proximaEtapa() {
    if (etapaAtual < 3) {
      setEtapaAtual(etapaAtual + 1);
    }
  }

  function etapaAnterior() {
    if (etapaAtual > 1) {
      setEtapaAtual(etapaAtual - 1);
    }
  }

  function finalizarCadastro(event) {
    event.preventDefault();

    alert("Demonstração concluída. Nenhum dado foi salvo e nenhum convite foi enviado.");
    aoNavegar("inicio");
  }

  return (
    <main className="dashboard">
      <Sidebar pagina="cadastro" aoNavegar={aoNavegar} aoSair={aoSair} busy={busy} />

      <section className="conteudo-dashboard">
        <header className="barra-superior">
          <input
            type="search"
            placeholder="Buscar funcionário, departamento..."
          />

          <ProfileMenu user={user} onUpdate={onUpdateProfile} />
        </header>

        <main className="pagina-cadastro">
          <section className="area-formulario">
            <header className="cabecalho-cadastro">
              <button
                type="button"
                className="botao-voltar"
                onClick={() => aoNavegar("inicio")}
                aria-label="Voltar para o início"
              >
                <ArrowLeft size={24} />
              </button>

              <div>
                <h2>Cadastro de Funcionário</h2>

                <p>
                  Formulário demonstrativo: os dados não serão salvos e nenhum convite será enviado.
                </p>
              </div>
            </header>

            <section className="formulario-cadastro">
              <div className="etapas-cadastro">
                <button
                  type="button"
                  className={`etapa ${
                    etapaAtual === 1 ? "etapa-ativa" : ""
                  } ${etapaAtual > 1 ? "etapa-concluida" : ""}`}
                  onClick={() => setEtapaAtual(1)}
                >
                  <span>1</span>
                  <strong>Dados Pessoais</strong>
                </button>

                <button
                  type="button"
                  className={`etapa ${
                    etapaAtual === 2 ? "etapa-ativa" : ""
                  } ${etapaAtual > 2 ? "etapa-concluida" : ""}`}
                  onClick={() => setEtapaAtual(2)}
                >
                  <span>2</span>
                  <strong>Cargo e Departamento</strong>
                </button>

                <button
                  type="button"
                  className={`etapa ${
                    etapaAtual === 3 ? "etapa-ativa" : ""
                  }`}
                  onClick={() => setEtapaAtual(3)}
                >
                  <span>3</span>
                  <strong>Acesso ao Sistema</strong>
                </button>
              </div>

              <form onSubmit={finalizarCadastro}>
                {etapaAtual === 1 && (
                  <>
                    <section className="secao-formulario">
                      <h3>
                        <UserRoundPlus size={22} />
                        Informações Pessoais
                      </h3>

                      <div className="campos-duplos">
                        <label>
                          Nome completo *
                          <input
                            type="text"
                            placeholder="Digite o nome completo"
                          />
                        </label>

                        <label>
                          CPF *
                          <input
                            type="text"
                            placeholder="000.000.000-00"
                          />
                        </label>

                        <label>
                          Data de nascimento *
                          <input
                            type="text"
                            placeholder="dd/mm/aaaa"
                          />
                        </label>

                        <label>
                          RG
                          <input
                            type="text"
                            placeholder="Digite o RG"
                          />
                        </label>

                        <label>
                          Telefone *
                          <input
                            type="text"
                            placeholder="(00) 00000-0000"
                          />
                        </label>

                        <label>
                          E-mail *
                          <input
                            type="email"
                            placeholder="exemplo@dominio.com"
                          />
                        </label>
                      </div>
                    </section>

                    <section className="secao-formulario">
                      <h3>
                        <MapPin size={22} />
                        Endereço
                      </h3>

                      <div className="campos-endereco">
                        <label className="campo-cep">
                          CEP
                          <input
                            type="text"
                            placeholder="00000-000"
                          />
                        </label>

                        <label className="campo-rua">
                          Rua
                          <input
                            type="text"
                            placeholder="Digite o nome da rua"
                          />
                        </label>

                        <label>
                          Número
                          <input type="text" placeholder="Nº" />
                        </label>

                        <label>
                          Bairro
                          <input
                            type="text"
                            placeholder="Digite o bairro"
                          />
                        </label>

                        <label>
                          Cidade
                          <select defaultValue="">
                            <option value="" disabled>
                              Selecione a cidade
                            </option>

                            <option value="sao-paulo">
                              São Paulo
                            </option>

                            <option value="rio-de-janeiro">
                              Rio de Janeiro
                            </option>
                          </select>
                        </label>
                      </div>
                    </section>
                  </>
                )}

                {etapaAtual === 2 && (
                  <section className="secao-formulario">
                    <h3>
                      <Building2 size={22} />
                      Cargo e Departamento
                    </h3>

                    <div className="campos-duplos">
                      <label>
                        Departamento *
                        <select defaultValue="">
                          <option value="" disabled>
                            Selecione o departamento
                          </option>

                          <option value="administrativo">
                            Administrativo
                          </option>

                          <option value="financeiro">
                            Financeiro
                          </option>

                          <option value="recursos-humanos">
                            Recursos Humanos
                          </option>

                          <option value="tecnologia">
                            Tecnologia
                          </option>
                        </select>
                      </label>

                      <label>
                        Cargo *
                        <input
                          type="text"
                          placeholder="Digite o cargo"
                        />
                      </label>

                      <label>
                        Data de admissão *
                        <input type="date" />
                      </label>

                      <label>
                        Gestor responsável
                        <select defaultValue="">
                          <option value="" disabled>
                            Selecione o gestor
                          </option>

                          <option value="gabriel">
                            {user.username}
                          </option>
                        </select>
                      </label>

                      <label>
                        Tipo de contrato *
                        <select defaultValue="">
                          <option value="" disabled>
                            Selecione o tipo
                          </option>

                          <option value="clt">CLT</option>
                          <option value="temporario">
                            Temporário
                          </option>
                          <option value="estagio">Estágio</option>
                        </select>
                      </label>

                      <label>
                        Carga horária semanal *
                        <select defaultValue="">
                          <option value="" disabled>
                            Selecione a carga horária
                          </option>

                          <option value="20">20 horas</option>
                          <option value="30">30 horas</option>
                          <option value="40">40 horas</option>
                          <option value="44">44 horas</option>
                        </select>
                      </label>
                    </div>
                  </section>
                )}

                {etapaAtual === 3 && (
                  <section className="secao-formulario">
                    <h3>
                      <KeyRound size={22} />
                      Acesso ao Sistema
                    </h3>

                    <p className="texto-secao">
                      Defina as informações de acesso do novo
                      funcionário ao sistema.
                    </p>

                    <div className="campos-duplos">
                      <label>
                        Nome de usuário *
                        <input
                          type="text"
                          placeholder="Digite o nome de usuário"
                        />
                      </label>

                      <label>
                        Perfil de acesso *
                        <select defaultValue="">
                          <option value="" disabled>
                            Selecione o perfil
                          </option>

                          <option value="usuario">Usuário</option>
                          <option value="lider">Líder</option>
                          <option value="administrador">
                            Administrador
                          </option>
                        </select>
                      </label>

                      <label>
                        Senha *
                        <input
                          type="password"
                          placeholder="Crie uma senha"
                        />
                      </label>

                      <label>
                        Confirmar senha *
                        <input
                          type="password"
                          placeholder="Digite a senha novamente"
                        />
                      </label>
                    </div>

                    <label className="enviar-convite">
                      <input type="checkbox" defaultChecked />
                      Enviar convite de acesso para o e-mail do
                      funcionário.
                    </label>
                  </section>
                )}

                <footer className="rodape-formulario">
                  {etapaAtual === 1 ? (
                    <button
                      type="button"
                      className="botao-cancelar"
                      onClick={() => aoNavegar("inicio")}
                    >
                      Cancelar
                    </button>
                  ) : (
                    <button
                      type="button"
                      className="botao-cancelar"
                      onClick={etapaAnterior}
                    >
                      Voltar
                    </button>
                  )}

                  {etapaAtual < 3 ? (
                    <button
                      type="button"
                      className="botao-proximo"
                      onClick={proximaEtapa}
                    >
                      Próximo passo
                    </button>
                  ) : (
                    <button
                      type="submit"
                      className="botao-proximo"
                    >
                      Finalizar cadastro
                    </button>
                  )}
                </footer>
              </form>
            </section>
          </section>

          <aside className="painel-informacoes">
            <section className="card-talentos">
              <UserRoundPlus size={48} />

              <h3>Cadastre novos talentos no seu time!</h3>

              <p>
                Com o TIMEID, o controle de ponto e a gestão de
                pessoas ficam mais simples e eficientes.
              </p>
            </section>
          </aside>
        </main>
      </section>
    </main>
  );
}

export default Cadastro;