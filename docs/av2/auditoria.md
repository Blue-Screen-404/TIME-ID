# Rastreabilidade e auditoria AV2

Estados usados: **evidenciado** = localizado no repositório; **parcial** = existe evidência, mas falta parte do critério; **pendente** = não há comprovação suficiente. A presença de código ou texto isoladamente não prova execução ou correção. A matriz JSON contém o estado por critério e os caminhos de referência.

## Resumo da inspeção

- **Identificação do grupo:** há nomes sob a seção `## Integrantes` no README; o grupo deve confirmar se cada nome corresponde ao nome completo oficial.
- **Modelo:** existe modelo Mermaid textual e uma explicação das discrepâncias; ele registra os identificadores e métodos encontrados, mas não declara associações EF que o código não possui.
- **Regras:** RN01–RN05 estão localizadas em serviços/controladores. As classes ativas não protegem todas essas invariantes.
- **Testes:** há cenários de API em `scripts/Test-Crud.ps1` e `scripts/Test-Login.ps1`; a suíte `tests/Geolocation` cobre geolocalização. Não foram encontrados testes de unidade para as regras de domínio descritas nesta especificação. Esta auditoria não executou testes.
- **Exceções:** `DomainError` é convertido em resposta HTTP central no `Program.cs`; ainda falta uma matriz com três falhas distintas e testes automatizados dos resultados e do estado.
- **Histórico Git:** o repositório contém alterações funcionais, mas ainda não foi produzido aqui um parecer de diff com inspeção de impacto e ligação a um requisito da AV2.
- **Depuração:** não há evidência registrada de um defeito reproduzível, hipótese, técnica aplicada e comparação antes/depois. Não inventar esse registro; selecionar um defeito real e reproduzi-lo.
- **Especificação para agente:** não há especificação de alteração funcional para DsCode acompanhada de inspeção do resultado.
- **Clone limpo:** o README fornece comando de execução e o Arch.md descreve build/testes; a execução limpa precisa ser confirmada antes da entrega.

## Próximos passos verificáveis

1. Confirmar os nomes completos dos integrantes.
2. Escolher cinco regras (RN01–RN05) e criar para cada uma cenários positivos e negativos automatizados; acrescentar um teste que tente violar uma invariante e confirme estado válido após a rejeição.
3. Adicionar uma operação de integração pela API que confira também a persistência.
4. Selecionar um diff funcional do histórico, registrar arquivos e impactos, comparar propriedades de POO e emitir parecer fundamentado.
5. Reproduzir um defeito real, registrar esperado/observado, hipótese, logs ou inspeção, causa raiz, correção mínima e verificação antes/depois.
6. Atualizar `evidencias.json` e esta auditoria com resultados observados, sem converter pendências em atendidas por documentação.
7. Seguir as instruções de clone limpo em `Arch.md` e registrar os comandos e resultados da compilação e testes.

## Índice

Consulte [`evidencias.json`](evidencias.json) para a relação estruturada dos 40 critérios, estados e evidências localizadas.
