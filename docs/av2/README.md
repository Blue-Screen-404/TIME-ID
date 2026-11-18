# AV2 — Design e Programação Orientados a Objetos

Esta pasta reúne requisitos verificáveis, modelo textual, decisões de modelagem, rastreabilidade e auditoria. O conteúdo descreve o código que está no repositório; itens sem evidência suficiente permanecem como pendências.

## Artefatos

- [Especificação e critérios de aceitação](especificacao.md)
- [Modelo de classes observado](modelo-classes.md)
- [Análise e decisões de modelagem](decisoes-modelagem.md)
- [Rastreabilidade e auditoria](auditoria.md)
- [Índice de evidências em JSON](evidencias.json)

## Leitura importante

O arquivo `time-id.md` é uma avaliação anterior e registra observações sobre uma versão do projeto. O código atual já usa `Dominios/Employee/Colaborador.cs` (`Employee`) dentro de `EmployeeService`, portanto a afirmação de que todo o diretório `Dominios/` está sem referências não descreve mais o estado atual. Ainda há uma separação entre entidades persistidas em `Operations/Models.cs` e o objeto `Employee`, que é reconstruído durante a gravação e não é a entidade persistida pelo EF Core.

O PDF da AV2 define os critérios de avaliação. Este conjunto de documentos não substitui código, testes ou evidências de execução. Nenhum requisito é marcado como atendido apenas por estar descrito aqui.

## Pendências que dependem do grupo

- Confirmar se os nomes em `## Integrantes` no README são os nomes completos oficiais exigidos pela disciplina.
- Selecionar e registrar um defeito real reproduzível com evidências antes/depois; este repositório ainda não contém um registro verificável dessa investigação.
- Executar os comandos de build e os testes em clone limpo antes da entrega. Esta organização documental não executou testes.
- Acrescentar cenários automatizados de domínio para regras e invariantes; os testes atuais não cobrem essas regras no nível dos objetos.
- Atualizar os estados da auditoria somente após verificar as evidências correspondentes.
