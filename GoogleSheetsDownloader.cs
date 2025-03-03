namespace agendamento
{
    public class GoogleSheetsDownloader
    {
        private static readonly string DownloadUrl = "https://docs.google.com/spreadsheets/d/1kGz2EHBuTgGymIFRMuDcs_Zxn3GAtKSu/export?format=xlsx";

        public static async Task<string> DownloadSheetAsync()
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "clientes.xlsx");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(DownloadUrl);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync();

                    File.WriteAllBytes(filePath, content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao baixar a planilha: {ex.Message}");
                }
            }

            return filePath;
        }
    }
}
