# Agendamento Mixtel

Sistema de agendamento desenvolvido para a Mixtel.

## Configuração das Credenciais do Google Sheets

Para que o sistema funcione corretamente, você precisa configurar as credenciais do Google Sheets. Siga os passos abaixo:

1. Acesse o [Google Cloud Console](https://console.cloud.google.com)
2. Crie um novo projeto ou selecione um existente
3. Ative a API do Google Sheets para o projeto
4. Crie uma credencial do tipo "Service Account"
5. Faça o download do arquivo JSON de credenciais

### Configurando as Credenciais no Projeto

1. Crie uma pasta chamada `credentials` no seu computador (fora do diretório do projeto)
2. Copie o arquivo JSON de credenciais baixado para esta pasta
3. Renomeie o arquivo para `credencial.json`
4. Copie o arquivo também para a pasta `credentials/agendamento.Resources.credencial.json`

### Compartilhando a Planilha

1. Abra sua planilha do Google Sheets
2. Clique no botão "Compartilhar"
3. Adicione o email do Service Account (encontrado no arquivo de credenciais) com permissão de "Editor"

### ⚠️ Importante

- NUNCA compartilhe ou comite o arquivo de credenciais no Git
- NUNCA inclua o arquivo de credenciais em backups públicos
- Mantenha o arquivo de credenciais em um local seguro
- Se as credenciais forem comprometidas, revogue-as imediatamente no Google Cloud Console e gere novas

## Executando o Projeto

1. Clone este repositório
2. Configure as credenciais conforme as instruções acima
3. Abra o projeto no Visual Studio
4. Execute o projeto