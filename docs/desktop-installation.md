# Instalação do aplicativo desktop

Cada release aprovada publica artefatos para Windows e macOS. A equipe de implantação deve entregar ao cliente somente o instalador correspondente à versão e à API dele.

## Antes de distribuir

1. Defina o endereço HTTPS da API do cliente em `VITE_API_URL` no build do desktop.
2. Atualize a diretiva `connect-src` de [tauri.conf.json](../apps/frontend/src-tauri/tauri.conf.json) para permitir exatamente esse endereço HTTPS. Não libere origens genéricas em produção. A origem do aplicativo no Windows é `http://tauri.localhost`; no macOS é `tauri://localhost`, e ambas devem constar em `Client:AllowedOrigins` da API.
3. Assine os instaladores com os certificados corporativos configurados como segredos do GitHub Actions.
4. Execute a CI, crie uma tag `vX.Y.Z` e valide os artefatos gerados pela release.

O instalador não contém a base de dados nem senhas de empresas. Os dados permanecem na API central protegida.

## Windows

1. Confirme que o computador é Windows 10 ou 11 de 64 bits e possui acesso HTTPS à API do cliente.
2. Baixe o instalador assinado (`.msi` ou `.exe`) recebido pela equipe responsável.
3. Confira o editor exibido pelo Windows e execute o instalador.
4. Aceite a pasta padrão, salvo orientação da TI local, e conclua a instalação.
5. Abra **Controle de Acessos**, faça login e confirme que a lista de empresas é carregada.

Se o Windows alertar que o editor é desconhecido, interrompa a instalação e valide a assinatura com a TI. Não contorne alertas de segurança nem instale artefatos recebidos por fontes não oficiais.

## macOS

1. Confirme que o macOS é compatível com o artefato distribuído e possui acesso HTTPS à API.
2. Abra o arquivo `.dmg` assinado recebido da equipe responsável.
3. Arraste **Controle de Acessos** para a pasta **Aplicativos**.
4. Abra o aplicativo pela pasta **Aplicativos**, faça login e confirme o carregamento da lista de empresas.

Caso o Gatekeeper bloqueie a abertura, valide primeiro a assinatura e a origem com a TI. Não remova atributos de segurança do arquivo para forçar a execução.

## Atualização e suporte

- Feche o aplicativo antes de instalar uma versão mais recente.
- Instale somente versões aprovadas e assinadas. A atualização não exige copiar dados locais, pois os dados são centralizados.
- Registre versão instalada, computador, data e responsável no processo de suporte do cliente.
- Em caso de falha de login, valide conectividade HTTPS e status da API em `/health`; não solicite senhas do usuário por canais informais.
