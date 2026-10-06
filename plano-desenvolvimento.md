# Plataforma de Cadastro e Controle de Acessos

> Documento vivo de planejamento técnico e funcional.  
> Versão inicial: 0.1.0  
> Última atualização: 6 de outubro de 2026

## 1. Objetivo

Construir uma plataforma corporativa para cadastrar empresas e administrar credenciais de acesso aos seus sistemas. O produto deverá funcionar em computadores Windows e macOS, permitir múltiplos usuários e manter os dados de cada empresa cliente isolados dos demais.

O foco inicial é entregar um MVP seguro e utilizável, que possa evoluir de forma incremental sem reescrever a base.

## 2. Premissas

- O aplicativo será utilizado por clientes diferentes, em mais de uma máquina.
- Usuários de uma empresa não podem visualizar ou alterar dados de outra empresa.
- Credenciais são dados sensíveis e nunca devem ser persistidas em texto puro.
- O serviço deve funcionar com dados centralizados e exigir conexão para sincronização. Um modo offline poderá ser avaliado depois.
- A primeira versão terá quatro módulos de credenciais por empresa, com possibilidade de tornar os módulos configuráveis em uma versão posterior.

## 3. Arquitetura proposta

```text
Aplicativo desktop (Windows/macOS)
React + TypeScript + Tauri
                |
              HTTPS
                v
API central
ASP.NET Core (C#)
                |
                v
PostgreSQL + armazenamento seguro de chaves
```

### 3.1 Cliente desktop

- **React + TypeScript + Vite:** interface, componentes e regras de apresentação.
- **Tailwind CSS:** identidade visual e layout responsivo.
- **React Hook Form + Zod:** formulários e validações de interface.
- **Tauri:** empacotamento do aplicativo para Windows e macOS, distribuição de atualizações e integração pontual com recursos do sistema.

O cliente não será a fonte de verdade dos dados. Ele se comunicará com a API por HTTPS e manterá apenas dados temporários estritamente necessários, sem salvar senhas em `localStorage`.

### 3.2 API e domínio

- **ASP.NET Core Web API (C#):** endpoints, autenticação, autorização, regras de negócio e auditoria.
- **Entity Framework Core:** persistência e migrations.
- **PostgreSQL:** banco transacional centralizado.
- **Arquitetura em camadas:** `Domain`, `Application`, `Infrastructure` e `Api`.

### 3.3 Isolamento entre clientes

Na primeira versão, cada empresa cliente terá banco de dados próprio. Essa decisão facilita isolamento, backup, restauração, auditoria e resposta a incidentes.

Cada banco poderá conter várias empresas cadastradas pelo cliente, seus usuários e seus acessos. Se a escala futura justificar, a solução poderá migrar para um modelo multi-tenant com isolamento por organização.

## 4. Segurança

### 4.1 Credenciais cadastradas

- Senhas serão criptografadas antes de serem gravadas no banco.
- A chave de criptografia não será armazenada junto com os dados criptografados.
- Senhas serão reveladas somente mediante ação explícita e permissão do usuário.
- A cópia para a área de transferência terá confirmação visual e poderá ser limpa automaticamente após período configurável.
- A tabela de registros nunca exibirá senhas.

### 4.2 Acesso ao sistema

- Login por usuário e senha, com senha armazenada por hash forte no servidor.
- Perfis iniciais: `Administrador` e `Operador`.
- Sessão expirada após inatividade e renovação segura de tokens.
- Todo acesso, inclusão, alteração, exclusão, visualização e cópia de credenciais deverá gerar evento de auditoria.
- Comunicação exclusivamente por HTTPS.

### 4.3 Operação

- Backups criptografados e testados regularmente.
- Segredos, chaves e strings de conexão guardados fora do código-fonte.
- Atualizações do aplicativo assinadas e distribuídas por canal controlado.
- Política de retenção e exclusão de dados definida com cada cliente.

## 5. Funcionalidades do MVP

### 5.1 Usuários e acesso

- Criar o primeiro administrador da organização.
- Login, logout e recuperação/alteração de senha.
- Controle de permissão por perfil.

### 5.2 Cadastro de empresas

- Criar, consultar, editar e excluir empresas.
- Campos: razão social, CNPJ e inscrição estadual.
- Razão social obrigatória, com mínimo de três caracteres.
- CNPJ com máscara na interface, validação dos dígitos e unicidade no banco.
- IE opcional, com suporte a empresa isenta; a validação estadual específica fica para evolução futura.
- Registro automático de data de criação e última alteração.

### 5.3 Módulos de acesso

Cada empresa cadastrada terá inicialmente quatro módulos:

1. Sistema principal / ERP
2. Portal fiscal / SEFAZ / prefeitura
3. Internet banking / financeiro
4. Painel administrativo / diversos

Cada módulo possui rótulo, usuário e senha. A senha será mascarada por padrão e poderá ser visualizada ou copiada por usuários autorizados.

### 5.4 Pesquisa e operações

- Busca em tempo real por razão social ou CNPJ.
- Carregamento de um registro para consulta ou edição.
- Exclusão com confirmação explícita.
- Paginação e ordenação dos resultados.
- Indicadores de empresas cadastradas e credenciais configuradas.

### 5.5 Auditoria

Eventos mínimos:

- login e logout;
- cadastro, edição e exclusão de empresa;
- visualização e cópia de credencial;
- criação, edição e bloqueio de usuário.

## 6. Modelo inicial de dados

```text
Organization
  id, name, status, createdAt

User
  id, organizationId, name, email, passwordHash, role, status, createdAt, updatedAt

Company
  id, organizationId, companyName, cnpjDigits, stateRegistration, createdAt, updatedAt

AccessCredential
  id, companyId, moduleKey, label, username, encryptedPassword, createdAt, updatedAt

AuditEvent
  id, organizationId, userId, action, entityType, entityId, metadata, createdAt
```

`cnpjDigits` será salvo apenas com números; a máscara `00.000.000/0000-00` será aplicada na interface.

## 7. Organização sugerida dos repositórios

```text
controle-acessos/
  apps/
    desktop/                 # React + TypeScript + Tauri
    api/                     # ASP.NET Core Web API
  packages/
    contracts/               # contratos de API e tipos compartilhados, se necessário
  docs/
    arquitetura.md
    seguranca.md
    operacao.md
```

No início, também é aceitável manter os projetos em repositórios separados, desde que os contratos da API sejam versionados e documentados.

## 8. Etapas de desenvolvimento

### Etapa 0 — Decisões e preparação

- Validar este documento com o produto.
- Definir hospedagem, domínio, ambiente de testes e política de backup.
- Definir o processo de criação de bancos para novos clientes.
- Criar repositórios, padrão de branches, CI e gestão de variáveis de ambiente.

### Etapa 1 — Fundação da API

- Criar solução ASP.NET Core e banco PostgreSQL.
- Implementar migrations, health check e tratamento padronizado de erros.
- Modelar organização, usuários, empresas, credenciais e auditoria.
- Implementar autenticação e autorização inicial.

### Etapa 2 — Segurança de credenciais

- Implementar serviço de criptografia e gestão segura de chaves.
- Persistir credenciais criptografadas.
- Criar auditoria para ações sensíveis.
- Cobrir o fluxo com testes unitários e de integração.

### Etapa 3 — Aplicativo desktop

- Criar React + TypeScript + Tailwind + Tauri.
- Implementar login e armazenamento seguro da sessão.
- Construir layout, navegação e componentes base.

### Etapa 4 — Cadastro e consulta

- Implementar formulário de empresas e validação de CNPJ.
- Implementar os quatro cards de credenciais.
- Implementar CRUD, busca, tabela, paginação e confirmação de exclusão.

### Etapa 5 — Administração e auditoria

- Implementar gestão de usuários e perfis.
- Exibir consulta de eventos de auditoria para administradores.
- Ajustar bloqueio por inatividade e regras de sessão.

### Etapa 6 — Distribuição e operação

- Gerar instaladores para Windows e macOS.
- Definir atualização controlada do aplicativo.
- Implementar backup, restauração e monitoramento da API.
- Executar testes de instalação em máquinas limpas dos dois sistemas operacionais.

## 9. Critérios de aceite do primeiro MVP

- Um administrador consegue criar usuários e controlar seus perfis.
- Um operador autorizado consegue cadastrar, buscar, editar e excluir empresas.
- O CNPJ inválido ou duplicado não é salvo.
- As quatro credenciais são salvas criptografadas e não aparecem em texto puro no banco.
- A visualização e a cópia de senha ficam restritas a usuários autorizados e deixam rastro de auditoria.
- Usuários de clientes diferentes não conseguem acessar os dados um do outro.
- O aplicativo instala e atualiza corretamente em Windows e macOS.
- Existe procedimento testado de backup e restauração.

## 10. Evoluções planejadas, fora do MVP

- Rótulos e quantidade de módulos de acesso configuráveis.
- Convites de usuários por e-mail e autenticação em dois fatores.
- Importação e exportação segura de dados.
- Painel administrativo central para provisionar novos clientes.
- Integrações com gerenciadores de senha corporativos.
- Cache offline criptografado e sincronização controlada.
- Relatórios de auditoria e alertas de atividades sensíveis.

## 11. Decisões pendentes

- Onde a API e os bancos serão hospedados?
- O cliente poderá administrar usuários ou isso será responsabilidade interna?
- Quais perfis, além de Administrador e Operador, são necessários?
- Quem poderá visualizar e copiar senhas?
- Haverá requisito de autenticação em dois fatores no lançamento?
- Qual será a política de backup, retenção e recuperação de dados?
- A primeira versão deve oferecer português apenas ou múltiplos idiomas?

---

Este documento deve ser atualizado sempre que uma decisão de produto, segurança ou arquitetura for tomada. Mudanças que alterem isolamento de clientes, autenticação, criptografia ou retenção de dados exigem revisão técnica antes da implementação.
