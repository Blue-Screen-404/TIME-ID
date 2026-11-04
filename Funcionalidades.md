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

### Assistente de dúvidas

Na área autenticada, o botão **Ajuda TIME-ID**, no canto inferior direito, abre um chat para dúvidas sobre o funcionamento do sistema. O backend envia ao Gemini a pergunta, o histórico recente e os documentos do projeto (`Funcionalidades.md`, `Arch.md`, `README.md` e `Roadmap.md`). O assistente deve responder em português simples, com foco direto na dúvida e instruções práticas quando necessário. Perguntas que não são sobre o TIME-ID ou não têm resposta na documentação recebem uma mensagem curta indicando o limite do assistente. A chave da API fica somente no backend, configurada por segredo local ou variável de ambiente.

## Funcionários e departamentos

### Funcionários e Listar

As duas abas permitem consultar funcionários. **Funcionários** também apresenta indicadores; **Listar** prioriza a tabela. É possível buscar por nome, CPF, matrícula, cargo ou departamento e filtrar por departamento e situação. A tabela tem paginação e ações para editar ou excluir.

O administrador pode alterar a situação para **inativo**. A exclusão é recusada quando o funcionário possui histórico de ponto ou férias, para preservar esses registros. CPF válido, CPF único, matrícula única e e-mail único são conferidos pelo servidor.

### O que posso fazer em Funcionários?

Na tela **Funcionários**, o administrador acompanha os indicadores e consulta a lista de pessoas. Use a busca para localizar por nome, CPF, matrícula, cargo ou departamento; os filtros ajudam a separar por setor e situação. Use o lápis para editar. Para cadastrar alguém, abra **Cadastrar** no menu. A exclusão só é permitida quando a pessoa não tem histórico de ponto ou férias; caso contrário, altere a situação para inativo.

### Cadastrar

O cadastro e a edição usam três etapas:

1. **Dados pessoais:** nome, CPF, nascimento, RG, telefone, e-mail e endereço. CEP, rua, número, complemento, bairro, cidade e UF são preenchidos manualmente; não há busca automática de CEP.
2. **Cargo e departamento:** matrícula, departamento, cargo, regime presencial, híbrido ou remoto, admissão, carga semanal, situação e atividades da função. Há sugestões de cargos e descrições editáveis; no regime híbrido, devem ser escolhidos os dias presenciais.
3. **Acesso ao sistema:** mostra um resumo e oferece a criação opcional de uma conta com senha inicial e perfil de acesso. Uma conta já existente pode ser vinculada pelo mesmo e-mail. O cadastro do funcionário não envia convite por e-mail.

### O que posso fazer na aba Departamentos?

Se você usa o perfil Administrador, na aba **Departamentos** pode buscar setores pelo nome, ver a descrição e a quantidade de funcionários, conferir líderes e equipes ativas e criar, editar ou apagar setores. Para apagar, o setor precisa estar sem funcionários cadastrados.

### Criar, editar ou excluir um departamento

Para criar um setor, abra **Departamentos**, clique em **Novo departamento**, dê um nome e, se quiser, escreva uma descrição. Depois, clique em **Salvar**. Para encontrar um setor, pesquise pelo nome. Nos cartões, o lápis edita o nome, a descrição ou o líder. O líder precisa ser um funcionário ativo do setor. A lixeira apaga o setor, mas só quando não há funcionários cadastrados nele. Essa tela aparece para o perfil Administrador.

## Ponto

Em **Ponto**, a jornada é registrada na sequência **Entrada → Início do intervalo → Retorno do intervalo → Saída**, usando o horário do servidor na referência de Brasília. O administrador escolhe o funcionário; uma conta comum vinculada a funcionário registra apenas o próprio ponto.

A tela permite consultar marcações por data e mostra o espelho das horas em intervalos concluídos. O administrador pode corrigir uma marcação sem mudar seu dia nem a sequência dos horários, sempre com justificativa. Também pode excluir apenas a última marcação do dia, com justificativa. Há um intervalo mínimo de 30 segundos entre registros; funcionário inativo ou em férias aprovadas não registra ponto.

O cálculo exibido soma pares concluídos de marcações. A versão atual não calcula jornada atravessando a meia-noite, banco de horas, horas extras, atrasos ou folha de pagamento.

### Como bater ou registrar o ponto?

Abra **Ponto** e clique em **Registrar ponto**. A tela indica qual é a próxima marcação: **Entrada**, **Início do intervalo**, **Retorno do intervalo** ou **Saída**; faça cada uma na ordem mostrada. O administrador escolhe o funcionário antes de registrar. Para uma conta de funcionário, o registro é feito para a própria pessoa e exige que a conta esteja vinculada ao cadastro do funcionário.

## Reconhecimento facial e terminal

A aba **Reconhecimento facial** está disponível para administrador e gestor. Mostra cadastros biométricos, quantidade de marcações faciais da semana, relatório semanal e acesso ao terminal local. Somente o administrador pode cadastrar e remover padrões faciais de funcionários ativos presenciais ou híbridos; antes do cadastro, a interface pede conferência de nome, idade e matrícula.

No endereço `/terminal`, a câmera detecta o rosto e tenta registrar automaticamente a próxima marcação do dia. O terminal aceita a operação apenas a partir do próprio computador do servidor. O navegador processa a imagem da câmera, mas envia ao servidor o descritor facial necessário para a comparação; não salva uma fotografia. Os descritores cadastrados são protegidos no armazenamento local. Se o terminal falhar, o ponto pelo aplicativo continua disponível.

O reconhecimento é baseado em imagem 2D e não implementa prova de vida. Antes de usar biometria em uma operação real, a empresa precisa avaliar precisão, riscos de fraude, base legal, retenção dos dados e uma alternativa de registro.

### Câmeras e alertas

A página **Câmeras** é exclusiva do Administrador. Na primeira etapa, ela usa a webcam ligada ao computador do administrador quando ele inicia o monitoramento. O navegador compara os rostos capturados com os padrões faciais cadastrados de funcionários ativos presenciais ou híbridos. Se não encontrar uma correspondência confiável, grava um alerta com horário e origem; alertas repetidos da webcam são agrupados por um minuto. Não são guardados quadros nem vídeos. “Sem correspondência” não confirma que a pessoa não seja funcionária: iluminação, ângulo ou qualidade da imagem podem impedir o reconhecimento.

O administrador também pode guardar cadastros futuros de câmeras RTSP ou ONVIF, com endereço e credenciais protegidos no armazenamento local. Esses cadastros ainda não exibem vídeo. A reprodução e análise de câmeras IP/NVR dependem de integrar e testar o conector compatível com os equipamentos do local.

## Férias e feriados

Em **Férias**, o funcionário vinculado a uma conta pode solicitar e acompanhar seus períodos. Pode alterar ou cancelar uma solicitação enquanto ela estiver pendente. O administrador pode cadastrar para funcionários e aprovar ou recusar solicitações. Há filtro por situação, limite demonstrativo de 1 a 14 dias corridos por solicitação e bloqueio de períodos sobrepostos não recusados.

### Solicitar férias

Para pedir férias, abra **Férias**, clique em **Solicitar férias**, escolha as datas de início e fim e clique em **Salvar**. O período pode ter de 1 a 14 dias corridos e não pode se sobrepor a outro pedido ainda válido. Você acompanha a aprovação nessa mesma tela e pode editar ou cancelar o pedido enquanto ele estiver como **Solicitada**. Para pedir pela sua conta, ela precisa estar ligada ao seu cadastro de funcionário. O administrador também pode cadastrar férias para um funcionário e aprovar ou recusar pedidos.

### Consultar, alterar ou cancelar um pedido de férias

Abra **Férias** para ver os períodos e a situação de cada pedido. Use o filtro de situação para localizar pedidos solicitados, aprovados ou recusados. Se o pedido ainda estiver aguardando análise, use **Editar férias** para mudar as datas ou **Excluir** para cancelar. Depois de aprovado ou recusado, somente o administrador pode alterar o registro.

Em **Feriados**, todos os perfis com acesso à área podem consultar o calendário e filtrar pelo ano. O administrador pode criar, editar e excluir feriados com abrangência nacional, estadual, municipal ou da empresa.

## Relatórios

O administrador pode gerar relatórios de **funcionários, departamentos, férias, feriados e ponto**. Há ainda um relatório personalizado de funcionários, no qual escolhe as colunas. A maioria usa um período de consulta; o relatório de departamentos mostra a composição atual.

Depois de gerar o relatório, é possível baixar um arquivo **CSV** ou usar a impressão do navegador para imprimir ou salvar como PDF. A exportação não gera um arquivo `.xlsx`.

### Como gerar ou baixar um relatório?

Abra **Relatórios**, escolha o tipo de relatório e as datas inicial e final e clique em **Gerar relatório**. Depois de ver os resultados, use **CSV** para baixar os dados ou **Imprimir / PDF** para imprimir ou salvar como PDF. No relatório **Personalizado**, marque as colunas que deseja incluir. O relatório de departamentos mostra a composição atual da equipe, sem depender do período escolhido.

## Configurações, notificações e auditoria

Em **Configurações**, cada pessoa pode alterar a própria senha, ativar ou desativar a autenticação em duas etapas, escolher o modo claro ou escuro e ligar ou desligar notificações internas. O modo escuro é o padrão. A troca de tema exibe a logo durante a transição visual.

O administrador também pode editar os dados da empresa e baixar um backup completo. As notificações da interface incluem solicitações de férias pendentes e aniversariantes; não são enviadas por e-mail ou SMS. A página mostra atividades recentes do sistema para o administrador e da própria conta para os demais perfis. Alterações relevantes também ficam registradas para auditoria.

O backup é um JSON com os dados locais, inclusive informações pessoais e de contas. A restauração é manual, substituindo o arquivo de dados com o servidor parado.

### O que posso mudar em Configurações?

Em **Configurações**, você pode escolher o tema claro ou escuro, ligar ou desligar as notificações internas, alterar sua senha e ativar a autenticação em duas etapas. Ao ativar a proteção em duas etapas, guarde os códigos de recuperação: cada código só pode ser usado uma vez. O administrador também pode atualizar os dados da empresa e baixar um backup.

## Perfis de acesso

| Perfil | Acesso principal |
|---|---|
| Administrador | Gerencia funcionários, departamentos, ponto, férias, feriados, relatórios, dados da empresa, backup e cadastros faciais. |
| Gestor | Pode consultar a área de reconhecimento facial e o relatório semanal, sem cadastrar ou remover padrões. Também vê as áreas comuns disponíveis na barra lateral, sujeitas às regras de acesso do servidor. |
| Usuário | Consulta o próprio painel e os próprios registros de ponto e férias, consulta feriados e configura a conta. Precisa estar vinculado a um funcionário para registrar ponto ou solicitar férias. |

## Dados e limites da versão atual

O servidor salva contas, funcionários, departamentos, marcações, férias, feriados, preferências e auditoria em `TIME-ID_OFC/Time-ID_backend/App_Data/timeid.json`. Esse arquivo é local e não deve ser enviado ao Git. A implementação foi feita para **uma instância do servidor**; não há sincronização entre computadores ou banco SQL ativo. O caminho dos dados pode ser alterado com `TIMEID_DATA_PATH`.

As telas de login, recuperação de senha, painel, terminal e áreas internas estão implementadas. Funcionalidades mencionadas no [Roadmap.md](Roadmap.md), como banco de dados para várias instâncias e regras avançadas de jornada, são planos e não devem ser consideradas prontas.
