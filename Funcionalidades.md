# Funcionalidades do TIMEID

Este documento descreve o que está implementado no TIMEID. O sistema reúne gestão de funcionários e controle de ponto em uma interface React atendida por uma API ASP.NET Core. Para instalar e abrir o sistema, consulte o [README.md](README.md). A organização do código está em [Arch.md](Arch.md); ideias futuras estão em [Roadmap.md](Roadmap.md).

## Acesso ao sistema

| Funcionalidade | Como funciona |
|---|---|
| Login | A pessoa informa e-mail e senha. O servidor valida a conta e cria uma sessão por cookie; páginas internas exigem uma sessão válida. |
| Sair | Encerra a sessão e volta à tela de login. A sessão também pode expirar ou ser invalidada quando dados de segurança mudam. |
| Recuperar senha | Em **Esqueceu a senha?**, a pessoa informa o e-mail, solicita um código de seis dígitos, confirma o código e define uma nova senha. O código expira em 15 minutos e é de uso único. O envio exige configuração SMTP no servidor; a resposta da solicitação não revela se o e-mail está cadastrado. |
| Autenticação em duas etapas | Em **Configurações**, a pessoa pode ativar códigos temporários de um aplicativo autenticador. Ao ativar, recebe códigos de recuperação de uso único. Também pode desativar a proteção mediante verificação. |
| Perfil | Pelo menu no canto superior direito, a pessoa pode ver e atualizar os dados permitidos de seu perfil e a foto. O administrador pode alterar o próprio nome e e-mail; o usuário comum pode alterar a foto. O perfil de permissão não é trocado nessa tela. |

O controle **Lembrar de mim** aparece no formulário, mas ainda não altera a duração da sessão. As senhas são guardadas como hashes, e o servidor confere as permissões em cada operação protegida.

## Navegação e tela inicial

Depois do login, a barra lateral abre as áreas disponíveis para o perfil da conta. O cabeçalho mostra a área atual, a busca geral para administradores, notificações internas quando ativadas e o menu de perfil. A busca geral leva à consulta de funcionários.

Em **Início**, o painel mostra a quantidade de funcionários cadastrados e inativos, aniversariantes do dia e departamentos. Também apresenta os últimos funcionários cadastrados, atividades recentes e aniversariantes. Os números vêm dos dados salvos, não da imagem usada como referência visual.

## Funcionários e departamentos

### Funcionários e Listar

As duas abas permitem consultar funcionários. **Funcionários** também apresenta indicadores; **Listar** prioriza a tabela. É possível buscar por nome, CPF, matrícula, cargo ou departamento e filtrar por departamento e situação. A tabela tem paginação e ações para editar ou excluir.

O administrador pode alterar a situação para **inativo**. A exclusão é recusada quando o funcionário possui histórico de ponto ou férias, para preservar esses registros. CPF válido, CPF único, matrícula única e e-mail único são conferidos pelo servidor.

### Cadastrar

O cadastro e a edição usam três etapas:

1. **Dados pessoais:** nome, CPF, nascimento, RG, telefone, e-mail e endereço. CEP, rua, número, complemento, bairro, cidade e UF são preenchidos manualmente; não há busca automática de CEP.
2. **Cargo e departamento:** matrícula, departamento, cargo, regime presencial, híbrido ou remoto, admissão, carga semanal, situação e atividades da função. Há sugestões de cargos e descrições editáveis; no regime híbrido, devem ser escolhidos os dias presenciais.
3. **Acesso ao sistema:** mostra um resumo e oferece a criação opcional de uma conta com senha inicial e perfil de acesso. Uma conta já existente pode ser vinculada pelo mesmo e-mail. O cadastro do funcionário não envia convite por e-mail.

### Departamentos

O administrador pode criar, buscar, editar e excluir departamentos. Há exemplos para preencher nome e descrição. É possível definir como líder um funcionário ativo do próprio departamento e consultar a composição de cada equipe. Um departamento que ainda contém funcionários não pode ser excluído.

## Ponto

Em **Ponto**, a jornada é registrada na sequência **Entrada → Início do intervalo → Retorno do intervalo → Saída**, usando o horário do servidor na referência de Brasília. O administrador escolhe o funcionário; uma conta comum vinculada a funcionário registra apenas o próprio ponto.

A tela permite consultar marcações por data e mostra o espelho das horas em intervalos concluídos. O administrador pode corrigir uma marcação sem mudar seu dia nem a sequência dos horários, sempre com justificativa. Também pode excluir apenas a última marcação do dia, com justificativa. Há um intervalo mínimo de 30 segundos entre registros; funcionário inativo ou em férias aprovadas não registra ponto.

O cálculo exibido soma pares concluídos de marcações. A versão atual não calcula jornada atravessando a meia-noite, banco de horas, horas extras, atrasos ou folha de pagamento.

## Reconhecimento facial e terminal

A aba **Reconhecimento facial** está disponível para administrador e gestor. Mostra cadastros biométricos, quantidade de marcações faciais da semana, relatório semanal e acesso ao terminal local. Somente o administrador pode cadastrar e remover padrões faciais de funcionários ativos presenciais ou híbridos; antes do cadastro, a interface pede conferência de nome, idade e matrícula.

No endereço `/terminal`, a câmera detecta o rosto e tenta registrar automaticamente a próxima marcação do dia. O terminal aceita a operação apenas a partir do próprio computador do servidor. O navegador processa a imagem da câmera, mas envia ao servidor o descritor facial necessário para a comparação; não salva uma fotografia. Os descritores cadastrados são protegidos no armazenamento local. Se o terminal falhar, o ponto pelo aplicativo continua disponível.

O reconhecimento é baseado em imagem 2D e não implementa prova de vida. Antes de usar biometria em uma operação real, a empresa precisa avaliar precisão, riscos de fraude, base legal, retenção dos dados e uma alternativa de registro.

## Férias e feriados

Em **Férias**, o funcionário vinculado a uma conta pode solicitar e acompanhar seus períodos. Pode alterar ou cancelar uma solicitação enquanto ela estiver pendente. O administrador pode cadastrar para funcionários e aprovar ou recusar solicitações. Há filtro por situação, limite demonstrativo de 1 a 14 dias corridos por solicitação e bloqueio de períodos sobrepostos não recusados.

Em **Feriados**, todos os perfis com acesso à área podem consultar o calendário e filtrar pelo ano. O administrador pode criar, editar e excluir feriados com abrangência nacional, estadual, municipal ou da empresa.

## Relatórios

O administrador pode gerar relatórios de **funcionários, departamentos, férias, feriados e ponto**. Há ainda um relatório personalizado de funcionários, no qual escolhe as colunas. A maioria usa um período de consulta; o relatório de departamentos mostra a composição atual.

Depois de gerar o relatório, é possível baixar um arquivo **CSV** ou usar a impressão do navegador para imprimir ou salvar como PDF. A exportação não gera um arquivo `.xlsx`.

## Configurações, notificações e auditoria

Em **Configurações**, cada pessoa pode alterar a própria senha, ativar ou desativar a autenticação em duas etapas, escolher o modo claro ou escuro e ligar ou desligar notificações internas. O modo escuro é o padrão. A troca de tema exibe a logo durante a transição visual.

O administrador também pode editar os dados da empresa e baixar um backup completo. As notificações da interface incluem solicitações de férias pendentes e aniversariantes; não são enviadas por e-mail ou SMS. A página mostra atividades recentes do sistema para o administrador e da própria conta para os demais perfis. Alterações relevantes também ficam registradas para auditoria.

O backup é um JSON com os dados locais, inclusive informações pessoais e de contas. A restauração é manual, substituindo o arquivo de dados com o servidor parado.

## Perfis de acesso

| Perfil | Acesso principal |
|---|---|
| Administrador | Gerencia funcionários, departamentos, ponto, férias, feriados, relatórios, dados da empresa, backup e cadastros faciais. |
| Gestor | Pode consultar a área de reconhecimento facial e o relatório semanal, sem cadastrar ou remover padrões. Também vê as áreas comuns disponíveis na barra lateral, sujeitas às regras de acesso do servidor. |
| Usuário | Consulta o próprio painel e os próprios registros de ponto e férias, consulta feriados e configura a conta. Precisa estar vinculado a um funcionário para registrar ponto ou solicitar férias. |

## Dados e limites da versão atual

O servidor salva contas, funcionários, departamentos, marcações, férias, feriados, preferências e auditoria em `TIME-ID_OFC/Time-ID_backend/App_Data/timeid.json`. Esse arquivo é local e não deve ser enviado ao Git. A implementação foi feita para **uma instância do servidor**; não há sincronização entre computadores ou banco SQL ativo. O caminho dos dados pode ser alterado com `TIMEID_DATA_PATH`.

As telas de login, recuperação de senha, painel, terminal e áreas internas estão implementadas. Funcionalidades mencionadas no [Roadmap.md](Roadmap.md), como banco de dados para várias instâncias e regras avançadas de jornada, são planos e não devem ser consideradas prontas.
