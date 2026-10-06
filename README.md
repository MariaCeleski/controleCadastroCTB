# Controle de Acessos

Monorepo para a plataforma desktop de cadastro de empresas e controle seguro de credenciais.

## Estrutura

- `apps/frontend`: interface React, preparada para empacotamento posterior com Tauri.
- `apps/backend`: API ASP.NET Core e camadas de domínio, aplicação e infraestrutura.
- `.github/workflows`: validação contínua e publicação de artefatos.

## Desenvolvimento local

### Frontend

```bash
npm install
npm run dev
```

### Aplicativo desktop

Com Rust instalado, execute o cliente desktop em desenvolvimento com:

```bash
npm run tauri:dev --workspace=@controle-acessos/frontend
```

O comando `npm run tauri:build --workspace=@controle-acessos/frontend` gera os instaladores nativos. A release por tag executa esse build em runners separados de macOS e Windows; certificados de assinatura devem ser configurados como segredos do GitHub Actions antes da distribuição ao cliente.

### Backend

Instale o .NET SDK 10 e execute em uma máquina com PostgreSQL configurado:

```bash
dotnet restore apps/backend/AccessControl.sln
dotnet test apps/backend/AccessControl.sln --configuration Release
dotnet run --project apps/backend/src/AccessControl.Api
```

Copie `apps/backend/src/AccessControl.Api/appsettings.Development.example.json` para `appsettings.Development.json` e informe a conexão local. Esse arquivo não deve ser enviado ao Git.

Para evitar credenciais em arquivos locais, prefira os User Secrets do .NET:

```bash
dotnet user-secrets set "ConnectionStrings:AccessControl" "Host=localhost;..." --project apps/backend/src/AccessControl.Api
dotnet user-secrets set "Security:CredentialEncryptionKey" "<chave-base64-32-bytes>" --project apps/backend/src/AccessControl.Api
dotnet user-secrets set "Security:Jwt:SigningKey" "<segredo-com-32-ou-mais-caracteres>" --project apps/backend/src/AccessControl.Api
```

## Qualidade e entrega

O workflow de CI executa lint, testes e build a cada pull request. O workflow de release só publica artefatos após uma tag `v*` e deve receber os segredos do ambiente antes de ser habilitado em produção.
