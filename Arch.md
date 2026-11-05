# Arquitetura do TIME-ID

Este documento descreve a organização atual do sistema e o caminho dos dados entre a interface, a API e o armazenamento local. O TIME-ID é uma aplicação web local: o servidor ASP.NET Core entrega os arquivos estáticos do React e atende às chamadas da API na mesma origem.

## Visão geral

```text
Navegador
  ├── Interface React (arquivos compilados em Time-ID_backend/wwwroot)
  └── chamadas HTTP /api/* com cookie de sessão
          ↓
ASP.NET Core 8
  ├── Controllers: autenticação, área de trabalho, recursos faciais e ajuda
  ├── Services: autenticação, perfil, ajuda/RAG e serviços auxiliares
  ├── Dominios e DTOs: entidades e contratos da API
  └── Operations/DataStore + TimeIdDbContext: validação, estado e persistência SQLite
          ↓
App_Data/timeid.sqlite
```

O frontend não possui um servidor de produção separado. Depois de preparado, seus arquivos são servidos pelo mesmo processo C# que implementa a API. Por isso, a execução documentada usa apenas `dotnet run`.

## Estrutura do repositório

```text
TIME-ID/
├── Arch.md                         # arquitetura e instruções para desenvolvimento
├── README.md                       # uso do sistema
├── global.json                     # faixa do SDK .NET usada pelo projeto
├── scripts/                        # testes de integração em PowerShell
├── artifacts/                      # dados temporários usados pelos testes
└── TIME-ID_OFC/
    ├── Time-ID_backend/
    │   ├── Controllers/            # endpoints HTTP da API
    │   ├── DTOs/                   # contratos de entrada e saída
    │   ├── Dominios/               # entidades agrupadas por área funcional
    │   ├── Operations/             # DataStore, modelos e autenticação em duas etapas
    │   ├── Repositories/           # componentes de acesso a dados
    │   ├── Services/               # autenticação e serviços auxiliares
    │   ├── Views/                  # recursos de interface do backend
    │   ├── App_Data/               # dados e chaves locais gerados em execução
    │   ├── wwwroot/                # frontend compilado e modelos faciais
    │   ├── Program.cs              # configuração e inicialização da aplicação
    │   └── TIME-ID_OFC.csproj
    └── Time-ID_frontend/
        ├── src/pages/              # telas de entrada e cadastro
        ├── src/workspace/          # telas funcionais e componentes da área logada
        ├── src/components/         # componentes compartilhados
        ├── src/services/           # chamadas HTTP para a API
        ├── scripts/                # preparação dos modelos faciais no build
        └── package.json            # dependências e scripts Vite
```

## Interface e comunicação

O frontend é escrito em React, JavaScript e CSS. O Vite gera o pacote estático, que é publicado em `Time-ID_backend/wwwroot`. As funções em `src/services/auth.js`, `workspace.js` e `facial.js` centralizam as chamadas para `/api/auth`, `/api/workspace` e `/api/facial`.

As chamadas usam a mesma origem e enviam o cookie de sessão do navegador. O backend também serve `index.html` nas rotas da interface, como `/inicio`, `/ponto` e `/relatorios`; essas rotas protegidas exigem autenticação. Rotas de API desconhecidas não são redirecionadas para a interface e continuam respondendo como API.

O reconhecimento facial é executado no navegador com os modelos distribuídos em `wwwroot/models`. O servidor oferece endpoints para os recursos faciais e controla o acesso conforme o perfil. As telas e o fluxo de câmera estão em `src/workspace/Facial.jsx` e `Facial.css`.

O monitoramento temporário da webcam fica na página `src/workspace/CameraMonitor.jsx`, exclusiva de Administrador, e compara descritores no endpoint protegido `POST /api/facial/monitor/match`. Quando não há correspondência, a tela envia uma foto JPEG reduzida ao endpoint de alerta; o backend a protege com Data Protection em `App_Data/CameraAlerts`, e somente o Administrador pode consultá-la. Alertas e fotos são retidos por até 30 dias, com limite de 500 alertas. Os cadastros RTSP/ONVIF são armazenados com conexão protegida por Data Protection e ainda não são reproduzidos. O conector para transmissão de câmeras reais depende do protocolo e modelo do equipamento.

O assistente de dúvidas fica em `src/workspace/HelpChat.jsx` e chama `POST /api/help-chat`. O endpoint exige sessão autenticada. `Services/HelpChatService.cs` envia a pergunta atual, o histórico recente e os documentos `Funcionalidades.md`, `Arch.md`, `README.md` e `Roadmap.md` ao Gemini pela API Interactions. Não há recuperação RAG local. O modelo padrão é Gemini 3.5 Flash-Lite; em caso de limite ou indisponibilidade, o serviço tenta modelos alternativos compatíveis. A chave é lida de `Gemini:ApiKey` (User Secrets) ou `GEMINI_API_KEY` no servidor, nunca pelo navegador. Sem chave, o chat avisa que falta configurar a integração. Os documentos são carregados do repositório durante o desenvolvimento e copiados para a saída e publicação do backend.

## Backend e responsabilidades

- `Program.cs` registra os serviços, configura cookies, autorização, arquivos estáticos e rotas da interface e da API.
- `Controllers/AuthController.cs` trata login, sessão, perfil, senha e autenticação em duas etapas.
- `Controllers/WorkspaceController.cs` reúne operações da área de trabalho, como cadastros, ponto, férias, feriados, relatórios e backup.
- `Controllers/FacialController.cs` expõe operações do fluxo de reconhecimento facial.
- `Services/AuthenticationService.cs` implementa validações de identidade e segurança da sessão. `PasswordResetEmailService.cs` contém o serviço auxiliar de redefinição de senha.
- `Operations/Models.cs` define o modelo serializado do estado local. `Operations/DataStore.cs` lê, valida e persiste esse estado.
- `Operations/TwoFactor.cs` implementa funcionalidades de autenticação em duas etapas; `DTOs/` define os objetos usados nos limites HTTP.
- `Dominios/` agrupa entidades de funcionários, departamentos, ponto, férias, feriados, auditoria e painel.

Os controllers e serviços aplicam as permissões no servidor. A visibilidade de botões e menus na interface não é o mecanismo de autorização.

## Autenticação e segurança

O login emite um cookie HTTP-only chamado `TimeId.Session`, com validade de 30 minutos e política SameSite Strict. A API responde `401` para chamadas sem sessão válida e `403` para operações sem permissão. A validade é conferida com identificador do usuário e marcadores de sessão e segurança; alterações relevantes invalidam sessões anteriores.

As senhas são armazenadas como hashes usando `PasswordHasher`. O ASP.NET Core Data Protection guarda suas chaves locais em `App_Data/keys`; no Windows, as chaves são protegidas com DPAPI. Respostas da API recebem `Cache-Control: no-store`. Segredos locais e dados gerados em `App_Data` não devem ser incluídos no Git.

## Persistência atual

O `TimeIdDbContext` usa SQLite em `Time-ID_backend/App_Data/timeid.sqlite`. Contas, funcionários, departamentos, marcações, perfis faciais, câmeras, férias, feriados, auditoria e configurações são armazenados em tabelas próprias. Listas simples, como dias de trabalho e hashes de recuperação, são serializadas em colunas dessas tabelas.

Na primeira inicialização do caminho padrão, se o banco SQLite estiver vazio, o `DataStore` importa `App_Data/timeid.json` quando esse arquivo existir. A importação inclui todos os dados e só é considerada concluída depois da gravação transacional no SQLite. O JSON antigo é mantido intacto como cópia de origem e não recebe alterações posteriores. Se não houver dados anteriores, são criadas as contas de demonstração.

As operações usam uma cópia de trabalho em memória. Um lock serializa as mutações na instância atual; as diferenças são persistidas em transação SQLite antes de atualizar essa cópia. O desenho continua destinado a uma única instância do servidor: várias instâncias não compartilham o lock de memória. `TIMEID_DATA_PATH` define o caminho do arquivo SQLite (usado também para bancos isolados nos testes); quando esse caminho personalizado for usado com dados antigos, `TIMEID_LEGACY_DATA_PATH` indica explicitamente o JSON a importar.

O backup da interface é exportado em JSON para portabilidade. Ele não deve ser copiado por cima do arquivo SQLite. Para restaurá-lo manualmente, pare o servidor, preserve o arquivo SQLite atual, coloque o JSON de backup em `App_Data/timeid.json` e mova o SQLite atual para outro nome; na próxima inicialização o JSON será importado para um novo banco.

## Build e execução de desenvolvimento

### Alterações no backend C#

Na raiz do repositório, compile ou execute o servidor:

```powershell
dotnet build ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

O segundo comando inicia o backend e serve a interface em `http://localhost:5000`.

### Alterações no frontend React

O frontend requer Node.js 22.12 ou superior para o processo de build. Na raiz do repositório:

```powershell
npm.cmd ci --prefix ./TIME-ID_OFC/Time-ID_frontend
npm.cmd run build --prefix ./TIME-ID_OFC/Time-ID_frontend
```

O build prepara os modelos faciais e gera os arquivos de produção em `Time-ID_backend/wwwroot`. Inclua esses arquivos compilados ao compartilhar alterações da interface. Para usá-las, reinicie o processo `dotnet run` se ele já estiver em execução.

## Testes de integração

Os testes automatizados disponíveis são scripts PowerShell localizados em `scripts/`. Execute-os na raiz:

```powershell
./scripts/Test-Login.ps1
./scripts/Test-Crud.ps1
```

Eles compilam o backend, iniciam servidores temporários nas portas 5099 e 5101 e usam arquivos SQLite isolados em `artifacts`. Cobrem login e sessão, perfil, operações CRUD, permissões, validações, relatórios, importação do backup JSON para SQLite, persistência após reinício, senha e autenticação em duas etapas. Os dados da execução normal não são utilizados pelos testes.
