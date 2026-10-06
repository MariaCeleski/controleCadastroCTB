# Operação no GitHub

## Configuração única do repositório

1. Definir `main` como branch padrão no GitHub.
2. Criar a branch `develop` a partir de `main`.
3. Em **Settings → Branches**, criar regras de proteção para `main` e `develop`:
   - exigir pull request antes do merge;
   - exigir pelo menos uma aprovação;
   - exigir CI aprovado;
   - exigir que a branch esteja atualizada antes do merge;
   - bloquear push direto e force push;
   - exigir conversas resolvidas;
   - incluir administradores na regra.
4. Em **Settings → General**, habilitar somente *squash merging* e apagar automaticamente branches mescladas.
5. Em **Settings → Secrets and variables → Actions**, cadastrar segredos de deploy e assinatura. Nunca cadastrar esses valores em arquivos do projeto.
6. Criar o projeto GitHub Projects "Controle de Acessos" com colunas: `Backlog`, `Ready`, `In progress`, `In review`, `Done`.

## Convenções de Issues

- Uma Issue representa uma unidade de trabalho verificável.
- Use labels de tipo, área e prioridade.
- Descreva objetivo, contexto, critério de aceite e cuidados de segurança.
- Associe toda Issue ao projeto Kanban.

## Releases

1. Criar PR de `develop` para `main` com todas as Issues da versão.
2. Após aprovação e merge, criar tag semântica, por exemplo `v0.1.0`.
3. O workflow de release cria os artefatos. Distribuição e deploy só ocorrem com segredos de produção configurados.

## Comandos locais

```bash
git checkout develop
git pull --ff-only origin develop
git checkout -b feature/123-cadastro-de-usuarios
git push -u origin feature/123-cadastro-de-usuarios
```
