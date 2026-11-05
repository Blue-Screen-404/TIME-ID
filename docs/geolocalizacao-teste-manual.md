# Teste manual da geolocalização

O Administrador já possui acesso integral à geolocalização. Não é necessário alterar o perfil nem promover outro usuário. O ambiente abaixo é separado da execução normal e usa dados fictícios; as permissões do administrador são as existentes, sem exceção temporária na segurança.

## Iniciar

Na pasta principal do projeto:

```powershell
./scripts/Start-GeolocationManual.ps1
```

O script compila e inicia a API em http://localhost:5104, prepara um departamento, um colaborador remoto e um local fictício em latitude 0 e longitude 0, com raio de 100 metros. Não cria batidas. Arquivos, chaves e logs ficam em artifacts/geolocation-manual; cadastros normais não são alterados. Cada inicialização preserva os testes anteriores. Se a porta estiver ocupada, encerre a instância anterior ou use -Port com outra porta; nesse caso importe novamente o ambiente atualizado.

Conta do ambiente de teste:

- E-mail: teste@timeid.local
- Senha: TimeId@123
- Perfil: Administrador

## Testar no Postman

1. Importe docs/Geolocation.postman_collection.json.
2. Importe artifacts/geolocation-manual/Manual.postman_environment.json e selecione o ambiente TIMEID - teste manual local.
3. Abra a coleção e execute as requisições manualmente, na ordem dos números, usando Send. O login salva o cookie no Postman; não é necessário token Bearer. Não desative os cookies.
4. Nas etapas 04 e 05, confira DentroDaArea e ForaDaArea. São simulações e não criam marcações.
5. Na etapa 06, confira o erro 400 por ausência de justificativa.
6. Na etapa 07, confira o ponto salvo como Pendente. O script da requisição preenche punchId automaticamente.
7. Na etapa 08, veja a pendência; na 09, aceite a justificativa; na 10, confira o histórico.
8. Aguarde 30 segundos após a etapa 07 antes de executar a etapa 11, que registra outra batida, agora dentro da área.

{{$isoTimestamp}} é preenchido pelo Postman com o horário atual em cada envio. As coordenadas são simuladas; estes testes manuais não capturam a posição física do computador. Para testar Recusada, altere decision na etapa 09 antes de enviá-la. Depois da análise, a mesma batida não pode ser reanalisada.

A API permite quatro marcações por colaborador/dia e exige intervalo mínimo de 30 segundos. Repetir as requisições de gravação pode atingir esse limite. Não é necessário repetir as etapas que gravam batida para testar consultas e simulações.

A etapa 12 mostra que o provedor de bairro/cidade ainda não está configurado no ambiente de teste. Isso não impede a validação das coordenadas. Este roteiro não habilita geocodificação externa nem limpeza por retenção.

## Encerrar

O script informa o PID do processo iniciado. Use o comando informado, por exemplo:

```powershell
Stop-Process -Id NUMERO_DO_PID
```

Isso encerra apenas a instância iniciada para o teste. Os dados fictícios permanecem para uma próxima execução. Não é preciso revogar permissões: nenhum perfil do ambiente normal foi alterado.

## Limite desta entrega

O frontend não foi modificado. Abrir uma tela do sistema não apresenta novos campos de geolocalização. O teste manual desta etapa acontece pela API com a coleção do Postman.

## Testar pela interface

O frontend agora inclui a área Geolocalização para Administrador e captura de localização na tela Ponto. Para iniciar em outro momento, na raiz do projeto:

```powershell
./scripts/Start-GeolocationManual.ps1 -Port 5105
```

Abra http://localhost:5105/geolocalizacao e entre com teste@timeid.local / TimeId@123. O script usa dados fictícios separados em artifacts/geolocation-manual-5105 e serve o frontend já compilado do repositório. Não é necessário instalar Postman ou Node para essa execução.

Na aba Validar e bater ponto, selecione o colaborador de teste e marque Usar coordenadas simuladas para teste. Latitude 0, longitude 0 e precisão 5 ficam dentro do local fictício. Longitude 1 fica fora. Valide sem registrar primeiro; depois informe justificativa e registre, se desejar. A aba Analisar justificativas permite concluir a análise. A tela Ponto mostra a situação geográfica no histórico e permite capturar a localização real antes da batida.

A simulação grava batidas reais dentro do ambiente de teste quando Registrar ponto for pressionado. Ela está disponível apenas na área administrativa. A captura real depende da permissão do navegador; a ausência de captura pode ser justificada conforme a política configurada. Não foram aceitas permissões de localização automaticamente.

As abas Locais permitidos e Configurações permitem testar os cadastros e as regras. A área administrativa de Geolocalização desta interface é exclusiva do Administrador; a delegação do Gestor continua disponível pelos endpoints do backend.

A compilação do frontend e do backend foi verificada. A abertura automática da interface para inspeção visual foi recusada pelo controle de acesso do navegador nesta sessão, portanto a interface ainda precisa da conferência manual do usuário.
