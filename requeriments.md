# Plataforma Corporativa de Cadastro e Controle de Acessos

> **Especificação de produto e técnica — documento vivo**  
> **Versão:** 2.0.0  
> **Última atualização:** 6 de outubro de 2026

## 1. Objetivo e escopo

Aplicação corporativa para cadastrar empresas e administrar as credenciais de acesso a sistemas externos. Será distribuída como aplicativo desktop para **Windows e macOS**, com dados centralizados e protegidos em uma API.

O MVP permite que usuários autorizados façam login, cadastrem/consultem/editem/excluam empresas, pesquisem por razão social ou CNPJ, administrem quatro credenciais por empresa e consultem ações de auditoria.

O sistema atende múltiplas organizações clientes. Usuários de uma organização nunca podem acessar dados de outra.

## 2. Arquitetura obrigatória

```text
Aplicativo desktop                         Serviços centralizados
React + TypeScript + Tauri   -- HTTPS -->  ASP.NET Core Web API (C#)
                                                |
                                                v
                                           PostgreSQL
```

### Cliente desktop

- React + TypeScript + Vite para interface e tipagem.
- Tauri para instaladores nativos de Windows e macOS.
- Tailwind CSS para design system.
- React Hook Form + Zod para formulários e validação na interface.
- Lucide React para ícones.

O aplicativo desktop não é a fonte de verdade. Ele não pode armazenar a base oficial ou senhas em `localStorage`. Dados temporários e tokens usam apenas armazenamento seguro oferecido pelo sistema operacional/Tauri.

### Backend

- ASP.NET Core Web API com C# para regras de negócio, autenticação, autorização e auditoria.
- Entity Framework Core para persistência e migrations.
- PostgreSQL como banco central.
- Camadas `Domain`, `Application`, `Infrastructure` e `Api`.
- Testes unitários e de integração automatizados.

## 3. Isolamento dos clientes

Na primeira versão, cada empresa cliente terá um banco PostgreSQL próprio. Esse banco pode conter empresas e usuários internos daquele cliente, mas não dados de outros clientes.

Essa decisão simplifica isolamento, backup, restauração e auditoria. Uma estratégia multi-tenant compartilhada só será considerada após revisão técnica e de segurança.

## 4. Modelo de dados

```typescript
export interface AccessCredential {
  id: string;
  moduleKey: 'erp' | 'fiscal' | 'financial' | 'administrative';
  label: string;
  username: string;
  password?: string; // Apenas em trânsito sob HTTPS; nunca persistida em texto puro.
  createdAt: string;
  updatedAt: string;
}

export interface CompanyRecord {
  id: string;
  registrationNumber: string; // Número interno do cadastro, preserva zeros à esquerda.
  companyName: string;
  cnpj: string; // Exibido com máscara; armazenado somente com dígitos.
  stateRegistration?: string;
  credentials: AccessCredential[];
  createdAt: string;
  updatedAt: string;
}

export type UserRole = 'Administrator' | 'Operator';
```

No banco, a senha de uma credencial existe apenas como `encryptedPassword`. Senhas de usuários existem apenas como hash forte, nunca reversível.

## 5. Regras de negócio

### Empresas

1. Número de cadastro obrigatório, com 1 a 20 dígitos, preservando zeros à esquerda e único dentro da organização cliente.
2. Razão social obrigatória, entre 3 e 200 caracteres.
3. CNPJ obrigatório, validado por dígito verificador e único dentro da organização cliente.
4. CNPJ é normalizado para 14 dígitos no banco; a máscara `00.000.000/0000-00` é exclusiva da interface.
5. Inscrição estadual é opcional e aceita texto, para comportar formatos estaduais e a condição de isento.
6. `createdAt` é criado uma única vez; `updatedAt` é atualizado a cada alteração.
7. Exclusão exige confirmação explícita. A definição entre exclusão lógica e definitiva precede a publicação.

### Módulos de acesso

Cada empresa terá quatro módulos predefinidos no MVP:

| Chave            | Rótulo padrão                      |
| ---------------- | ---------------------------------- |
| `erp`            | Sistema principal / ERP            |
| `fiscal`         | Portal fiscal / SEFAZ / prefeitura |
| `financial`      | Internet banking / financeiro      |
| `administrative` | Painel administrativo / diversos   |

Cada módulo possui rótulo, usuário e senha. Os quatro módulos existem na estrutura, mas usuário e senha podem permanecer vazios enquanto o acesso não estiver disponível. Rótulos e quantidade configuráveis são evolução futura.

### Operações

- Criar empresa após validar dados obrigatórios.
- Buscar em tempo real por número de cadastro, razão social ou CNPJ.
- Carregar registro para consulta ou edição.
- Na edição, ignorar o próprio registro ao verificar CNPJ duplicado.
- Ordenar e paginar resultados no servidor.
- Confirmar antes de excluir.

## 6. Segurança

### Credenciais

- Todo tráfego é exclusivamente HTTPS.
- Senhas são criptografadas antes de chegar ao PostgreSQL, usando criptografia autenticada.
- Chaves criptográficas ficam em serviço de segredos, nunca no código, banco ou Git.
- Senhas são mascaradas por padrão.
- Visualizar ou copiar uma senha exige ação explícita, permissão adequada e gera auditoria.
- Senhas não aparecem em tabelas, logs, erros ou telemetria.
- A área de transferência poderá ser limpa automaticamente após intervalo configurável.

### Usuários, sessões e auditoria

- Todo acesso exige login.
- Perfis iniciais: `Administrator` e `Operator`.
- Administrador gerencia usuários e consulta a auditoria; Operador executa somente ações autorizadas.
- Sessões expiram por inatividade.
- Devem ser registrados login/logout/falhas, CRUD de empresas e usuários, criação/alteração de credenciais, e visualização/cópia de senha.
- Cada evento guarda usuário, organização, data/hora, ação, entidade e metadados não sensíveis.

## 7. Interface

```text
App Shell
├── Navegação lateral e perfil do usuário
├── Cabeçalho com busca e notificações
├── Indicadores de empresas, credenciais e segurança
├── CompanyForm
│   ├── Dados da empresa
│   └── CredentialCard (x4)
├── ActionToolbar
├── RecordsTable
└── Modal de confirmação de exclusão
```

- Interface corporativa, limpa e responsiva.
- Paleta: fundo `slate`, superfícies claras, ação primária verde, edição índigo e exclusão rose.
- Ícones Lucide com rótulos ou `aria-label`.
- Navegação por teclado, foco visível, contraste acessível e erros associados aos campos.

## 8. Qualidade e entrega contínua

### Código

- Código limpo, tipado e separado por responsabilidade.
- Componentes, contratos, serviços e regras de domínio em arquivos próprios.
- Comentários explicam regras não óbvias e decisões de segurança, não o código trivial.
- Regras de negócio e segurança não podem depender somente do frontend.

### Testes

- Frontend: utilitários, componentes críticos e formulários.
- Backend: domínio, aplicação, endpoints e integração com banco.
- Isolamento de organização, autorização e criptografia precisam de testes antes da produção.

### CI/CD

Todo pull request e envio à `main` executa instalação de dependências, lint, testes, build do frontend, restore/build/testes do backend. Tags de versão geram artefatos versionados. Deploy de API e distribuição assinada dependem de segredos configurados com segurança.

## 9. Etapas de implementação

1. Fundação do repositório, convenções, CI e banco de desenvolvimento.
2. Domínio de empresas/CNPJ, migrations e validações.
3. Autenticação, usuários, perfis e isolamento de organização.
4. Criptografia e auditoria de ações sensíveis.
5. Interface React/Tauri: login, layout, cadastro e consulta.
6. Gestão de credenciais, permissões de visualização e cópia.
7. Testes de integração, backup/restauração e segurança.
8. Empacotamento, assinatura e distribuição controlada para Windows/macOS.

## 10. Fora do MVP

- Módulos de credenciais configuráveis.
- Autenticação multifator e convites por e-mail.
- Importação/exportação segura.
- Integração com cofres corporativos de senhas.
- Cache offline criptografado.
- Painel central para provisionar novos clientes.
- Separação em microserviços quando houver justificativa operacional.

## 11. Documento complementar

O [plano-desenvolvimento.md](./plano-desenvolvimento.md) detalha decisões de arquitetura e operação. Este arquivo é a fonte principal para escopo e regras de produto. Mudanças em segurança, isolamento de clientes ou retenção de dados exigem a atualização dos dois documentos.
