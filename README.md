# TIMEID

Sistema acadêmico de gestão de pessoas e controle de jornada, com interface em **React** e servidor em **C# / ASP.NET Core 8**.

**Para abrir o sistema, siga [Como executar o projeto](#como-executar-o-projeto).** O acesso começa no login; depois de validar o e-mail e a senha, o sistema abre a tela inicial com o menu lateral.

## Como executar o projeto

### 1. Prepare o computador

Instale o **SDK .NET 8.0.425** ou uma atualização da faixa **8.0.4xx**, conforme `global.json`. O SDK é o conjunto de ferramentas que prepara e executa o projeto; instalar somente o runtime não basta.

Reabra o terminal após a instalação. Para conferir, digite:

```powershell
dotnet --version
```

Use um navegador como Edge, Chrome ou Firefox. Na primeira execução, mantenha a internet disponível para baixar as bibliotecas do projeto. **A interface React já está preparada e incluída no repositório: não é necessário instalar Node.js nem executar npm para usar o sistema.**

### 2. Abra a pasta do projeto

Se baixou um ZIP, extraia os arquivos. No VS Code, escolha **Arquivo > Abrir Pasta** e selecione a pasta que contém este `README.md`, `global.json` e `TIME-ID_OFC`.

Clique em **Terminal > Novo Terminal**. O terminal é o espaço onde você digita o comando para ligar o sistema. Ele deve estar na pasta principal do projeto.

### 3. Ligue o sistema

Copie o comando inteiro e pressione Enter:

```powershell
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

Aguarde aparecer `Now listening on: http://localhost:5000`. Mantenha esse terminal aberto enquanto usar o sistema. O mesmo programa C# entrega as telas React e processa as informações.

Se aparecer **“Uma política de Controle de Aplicativo bloqueou este arquivo” (`0x800711C7`)**, o Windows impediu a execução. Nesse caso, o servidor não está ligado: veja [Bloqueio pelo Controle Inteligente de Aplicativos](#bloqueio-pelo-controle-inteligente-de-aplicativos) antes de tentar abrir o navegador.

### 4. Entre pelo login

Acesse **[http://localhost:5000](http://localhost:5000)** no navegador. `localhost` significa o seu próprio computador.

Na primeira utilização, estas contas de demonstração estão disponíveis:

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | `teste@timeid.local` | `TimeId@123` |
| Usuário | `usuario@timeid.local` | `TimeId@123` |

Digite o e-mail e a senha e clique em **Entrar**. A senha diferencia maiúsculas de minúsculas. Deixe o campo de código de autenticação vazio, a menos que tenha ativado a autenticação em duas etapas para essa conta.

- Com os dados corretos, você entra na tela inicial e pode usar o menu lateral.
- Com os dados incorretos, permanece no login e recebe uma mensagem.
- Para encerrar a sessão, use **Sair** no menu lateral.

As contas de demonstração são criadas apenas quando o arquivo de dados ainda não existe. Se alterar o e-mail ou a senha, use os novos dados nos próximos acessos: as alterações continuam salvas após reiniciar. As páginas internas exigem autenticação; uma sessão válida permite recarregar a página sem entrar novamente. Após expiração da sessão ou reinício do servidor, faça login de novo.

### 5. Encerre quando terminar

No terminal, pressione **Ctrl+C**. Fechar somente o navegador não desliga o servidor.

Para usar outro dia, repita o comando do passo 3 e abra o endereço do passo 4. Os cadastros permanecem salvos no computador.

### Se algo não funcionar

| Situação | Como resolver |
|---|---|
| “Uma política de Controle de Aplicativo bloqueou este arquivo” / `0x800711C7` | Consulte o diagnóstico abaixo. Trocar npm, PowerShell ou a porta não resolve a assinatura da DLL bloqueada. |
| Erro de privacidade ou certificado no navegador | Para esta execução local, digite exatamente `http://localhost:5000`, sem HTTPS. Se o bloqueio for do Windows, consulte a seção abaixo. |
| “dotnet não é reconhecido” | Instale o SDK .NET e reabra o terminal ou o VS Code. |
| “A compatible .NET SDK was not found” | Confira `dotnet --list-sdks` e instale uma versão da faixa 8.0.4xx. |
| “O arquivo de projeto não existe” / `MSB1009` | Abra o terminal na pasta que contém este README e copie o comando completo. |
| Falha ao baixar bibliotecas | Confira a conexão e execute o comando novamente. |
| Porta 5000 em uso | Encerre a execução anterior com Ctrl+C antes de iniciar outra. |
| Arquivo em uso / `MSB3021` / `MSB3027` | Pare a execução anterior; se usou o depurador do VS Code, pressione Shift+F5. |
| “Conexão recusada” no navegador | Confira se o servidor está ligado e use `http://localhost:5000`. |
| E-mail ou senha inválidos | Confira maiúsculas, espaços e se você já alterou as credenciais. Com duas etapas ativadas, informe também um código válido. |
| O sistema voltou ao login | A sessão pode ter expirado, a senha ter mudado ou o servidor ter reiniciado. Entre novamente. |

### Bloqueio pelo Controle Inteligente de Aplicativos

No diagnóstico feito em 12/10/2026, o comando de inicialização chegou à execução do C#, mas o Windows bloqueou `Time-ID_backend/bin/Debug/net8.0/TIME-ID_OFC.dll` com uma `FileLoadException` e o código `0x800711C7`. Os registros de **CodeIntegrity** mostraram o evento **3077** (requisitos de assinatura não atendidos) e o evento **3118** (Smart App Control). A DLL gerada localmente não possui assinatura digital Authenticode.

Isso é uma regra de segurança do Windows, não uma política de privacidade do TIMEID ou um problema de Node.js. A compilação pode terminar normalmente e, mesmo assim, a execução ser bloqueada. O comando de inicialização do passo 3 continua sendo o correto; não há uma mudança de comando no README que autorize a DLL perante essa política.

**Para resolver mantendo a proteção:** a versão executável precisa ser assinada com um certificado de assinatura de código de um fornecedor confiável, conforme as [orientações oficiais da Microsoft](https://learn.microsoft.com/en-us/windows/apps/develop/smart-app-control/code-signing-for-smart-app-control). Essa etapa depende de um certificado ou serviço de assinatura autorizado; o repositório não inclui um. Em computador administrado pela faculdade ou empresa, encaminhe a mensagem e o caminho do arquivo à equipe responsável para definir uma distribuição aprovada.

A Microsoft informa que o Smart App Control não oferece uma exceção individual para simplesmente permitir um aplicativo bloqueado. Consulte as [perguntas frequentes oficiais](https://support.microsoft.com/en-us/windows/security/threat-malware-protection/smart-app-control-frequently-asked-questions). Não é necessário alterar a estrutura React/C# para diagnosticar esse caso. Não foi desativada nenhuma proteção do Windows.

Para consultar o motivo no Windows, abra o **Visualizador de Eventos > Logs de Aplicativos e Serviços > Microsoft > Windows > CodeIntegrity > Operational** e procure os eventos no horário da tentativa. Para conferir apenas a compilação, sem iniciar o servidor, execute:

```powershell
dotnet build ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

Após disponibilizar uma versão aprovada para essa política, siga as instruções de execução fornecidas com ela. Não confunda a aprovação de uma versão assinada com a recompilação do código: compilar novamente gera novos binários, que precisam passar pelo processo de assinatura/aprovação novamente.

O endereço `http://localhost:5102` usado em verificações anteriores era de um servidor temporário de teste. A execução normal documentada usa **http://localhost:5000** e só estará disponível quando o terminal mostrar `Now listening on`.

## Primeiro uso e funcionalidades

Os números mostrados nas telas vêm dos cadastros salvos. Uma instalação nova começa sem funcionários ou departamentos; os nomes e quantidades da imagem de referência não são dados reais do sistema.

1. Entre como administrador e abra **Departamentos** para criar os setores.
2. Em **Cadastrar**, preencha as três etapas: dados pessoais, cargo/departamento (incluindo regime presencial, híbrido ou remoto) e acesso ao sistema. A criação de uma conta é opcional; informe uma senha inicial se desejar habilitar o login do funcionário. Não é enviado convite por e-mail.
3. Use **Funcionários** ou **Listar** para pesquisar, filtrar, editar, inativar e excluir cadastros. O CPF é validado, e CPF, matrícula e e-mail duplicados são recusados. Se houver histórico de ponto ou férias, inative o funcionário em vez de excluí-lo.
4. Em **Ponto**, registre entrada, início do intervalo, retorno do intervalo e saída. O administrador seleciona o funcionário; o usuário comum registra apenas o próprio ponto. Para o terminal presencial, use o perfil administrador para cadastrar o rosto de colaboradores presenciais ou híbridos e abra `http://localhost:5000/terminal` no computador ligado à câmera. O terminal funciona sem login do colaborador; o registro pelo app permanece disponível para home office ou indisponibilidade do terminal. Para vincular uma conta existente ao cadastro, use o mesmo e-mail no funcionário.
5. Cadastre os **Feriados** e gerencie os pedidos em **Férias**. O administrador pode aprovar ou recusar solicitações; o usuário comum acompanha as próprias e pode alterar ou excluir apenas as pendentes.

| Tela | O que está disponível |
|---|---|
| Início | Quantidades de funcionários, inativos, departamentos, aniversariantes, últimos cadastros e atividades recentes. |
| Funcionários / Listar | Consulta com busca, filtros e paginação; cadastro, edição, inativação e exclusão com proteção do histórico. |
| Cadastrar | Formulário em três etapas com dados pessoais, CEP, rua, número, complemento, bairro, cidade e UF; sugestões de cargos com atividades editáveis, opção de cargo personalizado e conta opcional. Endereços anteriores são preservados para revisão. |
| Departamentos | Cadastro, edição, exclusão e escolha de líder entre funcionários ativos do setor. Setores com funcionários não podem ser excluídos. |
| Ponto | Marcações no horário do servidor, consulta por data, espelho com horas dos intervalos concluídos e correções administrativas com justificativa. |
| Reconhecimento facial | Área exclusiva para Administrador e Gestor. O gestor consulta os cadastros e o relatório semanal; apenas o administrador cadastra ou remove padrões faciais. As imagens são processadas no navegador local, os descritores são cifrados no armazenamento local, e colaboradores remotos não entram no fluxo facial. |
| Férias | Cadastro, consulta, edição, exclusão e aprovação/recusa; limite demonstrativo de 14 dias corridos por solicitação (incluindo início e fim), validado também pelo servidor para ambos os perfis, e verificação de sobreposição de períodos. |
| Feriados | Cadastro, edição, exclusão e consulta por ano. |
| Relatórios | Funcionários, departamentos, férias, feriados, ponto e relatório personalizado com escolha de colunas; exportação CSV e impressão/PDF pelo navegador. |
| Configurações | Dados da empresa, notificações internas, tema claro/escuro com superfícies grafite e destaques verdes, backup, troca de senha, autenticação em duas etapas e histórico de atividades. |

Os relatórios de funcionários usam a data de admissão; os de férias consideram períodos que cruzam a consulta. O relatório de departamentos mostra a situação atual. O CSV pode ser aberto em programas de planilha; não é um arquivo `.xlsx`. Para obter PDF, use a opção de imprimir e escolha **Salvar como PDF** no navegador.

### Perfis de acesso

As permissões são verificadas também pelo servidor, não apenas pelo menu.

- **Administrador:** gerencia funcionários, departamentos, férias, feriados, relatórios, dados da empresa e backup; registra e corrige pontos.
- **Gestor:** consulta apenas a área de reconhecimento facial e o relatório semanal; não cadastra nem remove padrões faciais.
- **Usuário:** consulta os próprios dados de funcionário, registra e consulta o próprio ponto, solicita férias, consulta feriados e configura a própria conta. A conta precisa estar vinculada a um funcionário para registrar ponto ou solicitar férias.

O terminal reconhece padrões faciais 2D e não inclui verificação de prova de vida. Antes de usar as marcações como controle oficial, a empresa deve avaliar a precisão e o risco de fraude com fotos ou telas, definir retenção e base legal para biometria e manter o ponto pelo app como alternativa. Dados biométricos são dados pessoais sensíveis pela LGPD.

No canto superior direito, clique no perfil e escolha **Ver perfil**. O administrador pode editar o próprio nome, e-mail e foto; o usuário comum pode alterar somente a foto nesse modal. O perfil de acesso não é alterado ali. As fotos aceitam PNG, JPEG ou WebP de até 2 MB.

O modal de edição bloqueia a interação com o restante da página até **Salvar** ou **Cancelar**; clicar fora ou pressionar Esc não o fecha. Alteração de senha e autenticação em duas etapas ficam em **Configurações**. Ao ativar duas etapas, cadastre a chave em um aplicativo autenticador e guarde os códigos de recuperação exibidos; cada código de recuperação só pode ser usado uma vez.

### Dados salvos e backup

O servidor grava as informações em `TIME-ID_OFC/Time-ID_backend/App_Data/timeid.json`. Esse arquivo contém contas, senhas protegidas por hash, funcionários, departamentos, marcações, férias, feriados, preferências e auditoria. Ele não entra no Git. As alterações de perfil também permanecem salvas.

Em **Configurações > Backup**, o administrador pode baixar uma cópia completa. Guarde-a em local restrito: ela inclui dados pessoais e informações de segurança das contas. A restauração é manual: pare o servidor, preserve uma cópia do arquivo atual e substitua `App_Data/timeid.json` pelo backup antes de iniciar novamente.

Esta persistência local foi feita para uma única instância do servidor. Não execute dois servidores gravando no mesmo arquivo. Para testes ou outra instalação, a configuração `TIMEID_DATA_PATH` permite escolher outro arquivo.

### Limites da versão atual

- O ponto segue quatro marcações no mesmo dia, no horário de Brasília. Correções preservam a sequência e exigem justificativa; só a última marcação do dia pode ser excluída. Há um intervalo mínimo de 30 segundos entre registros para evitar duplicação por clique.
- O espelho soma pares concluídos. Jornada atravessando meia-noite, banco de horas, horas extras, atrasos e folha de pagamento ainda não são calculados.
- As notificações aparecem dentro do sistema; não há envio de e-mail ou SMS. Não há integração com CEP, biometria ou reconhecimento facial.
- Há dois perfis de acesso. A indicação de líder de departamento não cria um terceiro perfil de permissões.
- A estrutura atual usa arquivo JSON local, sem banco SQL ou sincronização entre servidores.

## Arquitetura e desenvolvimento

A arquitetura, a estrutura atual dos diretórios e os procedimentos de build e testes estão documentados em [Arch.md](Arch.md).

## Próximas etapas

- Banco de dados para implantação com múltiplas instâncias.
- Regras avançadas de jornada, banco de horas e folha de pagamento.
- Reconhecimento facial, geolocalização e controle de visitantes.
- Monitoramento de segurança, alertas externos e assistente de RH com IA.

## Equipe

**Blue-Screen-404** — projeto acadêmico de desenvolvimento de software e gestão de pessoas.

Arthur Ribeiro de Oliveira, Pedro Henrique Cardoso, José Flavio, João pedro Leal, Gabriel Rodrigues
