# Sistema de Agendamento RR INFRA

Sistema de agendamento desenvolvido para a RR INFRA, permitindo o gerenciamento de ordens de serviço e técnicos.

## 🚀 Funcionalidades

- **Gestão de Técnicos**
  - Seleção de técnicos disponíveis
  - Acompanhamento de O.S. por técnico
  - Visualização de O.S. do dia por técnico

- **Gestão de O.S.**
  - Visualização de O.S. agendadas
  - Download e armazenamento local de O.S.
  - Controle de O.S. baixadas
  - Períodos de atendimento (Manhã/Tarde)

- **Integração com Google Sheets**
  - Sincronização automática com planilhas
  - Atualização em tempo real
  - Backup na nuvem

- **Notificações**
  - Alertas de novos agendamentos
  - Notificações de O.S. atribuídas
  - Lembretes de atendimentos

## 📱 Tecnologias Utilizadas

- .NET MAUI (Multi-platform App UI)
- Google Sheets API v4
- Google Drive API v3
- iText7 para manipulação de PDFs
- EPPlus para Excel
- Plugin.LocalNotification para notificações
- CommunityToolkit.Maui

## 🔧 Configuração do Ambiente

### Pré-requisitos

- Visual Studio 2022 ou posterior
- .NET 9.0 SDK
- Conta Google Cloud Platform

### Configuração das Credenciais do Google Sheets

Para que o sistema funcione corretamente, você precisa configurar as credenciais do Google Sheets. Siga os passos abaixo:

1. Acesse o [Google Cloud Console](https://console.cloud.google.com)
2. Crie um novo projeto ou selecione um existente
3. Ative as seguintes APIs:
   - Google Sheets API
   - Google Drive API
4. Crie uma credencial do tipo "Service Account"
5. Faça o download do arquivo JSON de credenciais

### Configurando as Credenciais no Projeto

1. Crie uma pasta chamada `credentials` no seu computador (fora do diretório do projeto)
2. Copie o arquivo JSON de credenciais baixado para esta pasta
3. Renomeie o arquivo para `credencial.json`
4. Copie o arquivo também para a pasta `credentials/agendamento.Resources.credencial.json`

### Compartilhando as Planilhas

1. Abra suas planilhas do Google Sheets:
   - Planilha de Clientes
   - Planilha de Técnicos
   - Planilha de O.S. Finalizadas
2. Clique no botão "Compartilhar"
3. Adicione o email do Service Account (encontrado no arquivo de credenciais) com permissão de "Editor"

### ⚠️ Importante: Segurança das Credenciais

- NUNCA compartilhe ou comite o arquivo de credenciais no Git
- NUNCA inclua o arquivo de credenciais em backups públicos
- Mantenha o arquivo de credenciais em um local seguro
- Se as credenciais forem comprometidas, revogue-as imediatamente no Google Cloud Console e gere novas

## 📦 Instalação

1. Clone este repositório:
```bash
git clone https://github.com/Mestre20/agendamento-mixtel.git
```

2. Configure as credenciais conforme as instruções acima
3. Abra o projeto no Visual Studio
4. Restaure os pacotes NuGet
5. Execute o projeto

## 📋 IDs das Planilhas

O sistema utiliza as seguintes planilhas:

- Clientes: `11hayRzhS4-VRhc9m6drZ42yRNPC14SLJnwtIew36WeQ`
- Técnicos: `1u7V13H6o-mzC-b3rOTOJzxxeuahxXGjcm0pIntNwKrk`
- O.S. Finalizadas: `1fRKzS5sAfYuWd5QMm2uwzRjL_xtTajfAdTzWIWRYWOs`

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, leia o arquivo CONTRIBUTING.md para detalhes sobre nosso código de conduta e o processo para enviar pull requests.

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE.md](LICENSE.md) para detalhes.

## 👥 Autores

- **RR INFRA** - *Desenvolvimento inicial*

## 📞 Suporte

Para suporte, entre em contato com a equipe de desenvolvimento da RR INFRA.