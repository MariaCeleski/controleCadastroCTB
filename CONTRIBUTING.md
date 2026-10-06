# Guia de contribuição

## Fluxo de branches

- `main`: código protegido e pronto para produção. Nunca recebe commits diretos.
- `develop`: integração contínua das funcionalidades aprovadas. Nunca recebe commits diretos.
- `feature/<id>-descricao-curta`: nova funcionalidade.
- `fix/<id>-descricao-curta`: correção sem urgência.
- `hotfix/<id>-descricao-curta`: correção urgente criada a partir de `main`.
- `chore/<descricao-curta>`: manutenção técnica, documentação ou pipeline.

Cada alteração começa com uma Issue. A branch deve citar o número da Issue e o pull request deve usar `Closes #<numero>` na descrição.

## Ciclo de trabalho

1. Criar Issue com critério de aceite e prioridade.
2. Criar branch a partir de `develop`.
3. Implementar mudanças pequenas, testadas e com commits objetivos.
4. Abrir Pull Request para `develop` usando o template.
5. Corrigir comentários e aguardar CI verde + aprovação.
6. Mesclar por **squash merge**.
7. Abrir PR de `develop` para `main` somente em uma release validada.

## Qualidade obrigatória

Antes do PR, execute:

```bash
npm run lint
npm run test
npm run build
dotnet test apps/backend/AccessControl.sln --configuration Release
```

Não inclua segredos, dados reais de clientes, arquivos `.env`, chaves de criptografia ou senhas em commits, Issues ou Pull Requests.
