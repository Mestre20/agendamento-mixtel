namespace agendamento
{
    public partial class PeriodoPage : ContentPage
    {
        private string _tecnico;

        public PeriodoPage(string tecnico)
        {
            InitializeComponent();
            _tecnico = tecnico; // Atribuindo o técnico ao campo _tecnico
        }

        private async void OnPeriodoSelected(object sender, EventArgs e)
        {
            // Adicionar animação ao clicar no botão
            var button = (Button)sender;
            await button.ScaleTo(0.80, 100); // Reduz o botão para 80% do tamanho em 100 ms
            await button.ScaleTo(1, 100);    // Restaura o tamanho original em 100 ms

            string periodo = button.Text;

            // Navegar para a página dos clientes
            await Navigation.PushAsync(new ClientesPage(_tecnico, periodo));
        }
    }
}