# Implantação da API e do banco

Este guia estabelece o procedimento inicial para cada organização cliente. A arquitetura atual usa uma API ASP.NET Core e um PostgreSQL próprios por cliente. Não reutilize banco, chaves criptográficas ou segredos entre clientes.

## Pré-requisitos

- Um servidor com .NET Runtime 10, acesso ao PostgreSQL 17+ e HTTPS configurado em um proxy reverso.
- Um DNS definitivo para a API, por exemplo `https://api.cliente.exemplo`.
- Uma conta de serviço sem privilégios administrativos para executar a API.
- Um cofre de segredos ou mecanismo equivalente do ambiente. Arquivos de configuração com senhas não devem ser enviados ao Git.

## Variáveis obrigatórias

Configure os valores no serviço de hospedagem, utilizando `__` para representar a hierarquia do .NET. Os exemplos abaixo são nomes, não valores reais.

| Variável                            | Finalidade                                                                                  |
| ----------------------------------- | ------------------------------------------------------------------------------------------- |
| `ConnectionStrings__AccessControl`  | Conexão exclusiva com o PostgreSQL do cliente.                                              |
| `Security__CredentialEncryptionKey` | Chave Base64 de 32 bytes para AES-GCM das credenciais cadastradas.                          |
| `Security__Jwt__Issuer`             | Emissor fixo dos tokens JWT.                                                                |
| `Security__Jwt__Audience`           | Público aceito pelo aplicativo desktop.                                                     |
| `Security__Jwt__SigningKey`         | Segredo de pelo menos 32 caracteres para assinatura JWT.                                    |
| `Client__AllowedOrigins__0`         | Origem do WebView Tauri no Windows: `http://tauri.localhost`.                               |
| `Client__AllowedOrigins__1`         | Origem do WebView Tauri no macOS: `tauri://localhost`.                                      |
| `Database__ApplyMigrations`         | `true` apenas no processo único que aplica migrations.                                      |
| `Database__ExitAfterMigrations`     | `true` no processo de migration; encerra a aplicação depois que o banco estiver atualizado. |

Gere a chave de criptografia uma única vez por cliente e guarde-a no cofre. Perder essa chave impede a leitura das credenciais já armazenadas. Uma forma segura de geração é `openssl rand -base64 32` em macOS/Linux ou `[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))` no PowerShell.

O arquivo [appsettings.Production.example.json](../apps/backend/src/AccessControl.Api/appsettings.Production.example.json) documenta a estrutura sem incluir segredos.

## Publicação e migrations

1. Baixe o artefato `api-vX.Y.Z` da release aprovada e extraia-o em uma pasta de versão no servidor.
2. Configure as variáveis de ambiente na conta de serviço.
3. Pare as instâncias da API ou garanta que somente o processo de migration terá acesso ao banco durante a alteração do esquema.
4. Faça um backup validável antes de cada migration.
5. Execute uma única vez o artefato publicado com `Database__ApplyMigrations=true` e `Database__ExitAfterMigrations=true`.
6. Confirme que o processo terminou com sucesso e que a tabela `__EFMigrationsHistory` contém a migration esperada.
7. Inicie a API normalmente, sem as duas variáveis de migration. Verifique `GET /health` pelo proxy HTTPS.

Em PowerShell, a execução controlada fica assim, após as demais variáveis já estarem configuradas no serviço:

```powershell
$env:Database__ApplyMigrations = 'true'
$env:Database__ExitAfterMigrations = 'true'
dotnet .\AccessControl.Api.dll
```

Em seguida, remova essas duas variáveis da sessão ou serviço antes de iniciar a API em modo normal. Nunca habilite migrations em múltiplas réplicas simultaneamente.

## Backup e restauração

Faça backup diário com retenção definida em contrato e teste uma restauração periodicamente em ambiente isolado. O backup deve ser criptografado e armazenado fora do servidor de produção.

Exemplo de operação, usando uma variável de ambiente temporária para a senha do PostgreSQL:

```bash
pg_dump --format=custom --file=access-control-AAAA-MM-DD.dump "$ConnectionStrings__AccessControl"
pg_restore --clean --if-exists --dbname="$RESTORE_CONNECTION_STRING" access-control-AAAA-MM-DD.dump
```

Após restaurar, valide a migration mais recente, um login de administrador e o acesso a um registro de teste. A restauração não deve ocorrer sobre o banco em produção sem aprovação formal e janela de manutenção.

## Checklist de aceite

- API acessível somente via HTTPS e `GET /health` responde com sucesso.
- Banco, chave AES-GCM e chave JWT pertencem exclusivamente ao cliente.
- Migration aplicada uma vez e API normal iniciada sem `Database__ApplyMigrations`.
- Backup recente e restauração de teste registrados.
- DNS da API foi incluído na configuração de CSP e `VITE_API_URL` do instalador desktop daquele cliente.
