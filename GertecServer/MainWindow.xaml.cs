using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GertecServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Produto> Produtos { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            CarregarProdutos();
        }

        private void CarregarProdutos()
        {
            DataContext = this;
            string connectionString = "Data Source=BDGertec.db"; // Caminho do arquivo SQLite
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = "SELECT Codigo, CodigoBarras, Descricao, Preco1, Preco2, COB FROM Produtos"; // Consulta SQL para selecionar todos os produtos
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            Produtos.Clear();
            while (reader.Read())
            {
                bool cob = false;
                if (reader.GetInt16(5) == 1) { cob = true; }
                else { cob = false; }

                Produtos.Add(new Produto
                {
                    Codigo = reader.GetString(0),
                    CodBarra = reader.GetString(1),
                    Descricao = reader.GetString(2),
                    Preco1 = reader.GetDecimal(3),
                    Preco2 = reader.GetDecimal(4),
                    COB = cob
                });
            }
        }
        
        bool addOrEdit = true; //true = add, false = edit
        private void Button_Cadastro_Click(object sender, RoutedEventArgs e)
        {
            addOrEdit = true;
            Dialog_AddEdit.IsOpen = true;
            TextBox_DescricaoAOE.Text = "";
            TextBox_CodigoAOE.Text = "";
            TextBox_CodigoAOE.IsEnabled = true;
            TextBox_CodigoBarrasAOE.Text = "";
            TextBox_Preco1AOE.Text = "0,00";
            TextBox_Preco2AOE.Text = "0,00";
            Radio_CodigoBarras.IsChecked = true;
            Text_DialogAddOrEdit.Text = "Adicionar Produto";
        }

        private void Button_Consulta_Click(object sender, RoutedEventArgs e)
        {
            Dialog_Filtrar.IsOpen = true;
            TextBox_CodigoFiltro.Text = "";
            TextBox_CodigoBFiltro.Text = "";
            TextBox_DescricaoFiltro.Text = "";
            TextBox_Preco1Filtro.Text = "";
            TextBox_Preco2Filtro.Text = "";
            Chip_Codigo.IsChecked = false;
            Chip_CodigoBarras.IsChecked = false;
            Chip_Descricao.IsChecked = false;
            Chip_Preco1.IsChecked = false;
            Cihp_Preco2.IsChecked = false;
            Chip_COB.IsChecked = false;
        }

        private void Button_Editar_Click(object sender, RoutedEventArgs e)
        {
            addOrEdit = false;
            var produtoSelecionado = (Produto)DataGridProdutos.SelectedItem;
            if (produtoSelecionado != null)
            {
                TextBox_DescricaoAOE.Text = produtoSelecionado.Descricao;
                TextBox_CodigoAOE.Text = produtoSelecionado.Codigo;
                TextBox_CodigoAOE.IsEnabled = false;
                TextBox_CodigoBarrasAOE.Text = produtoSelecionado.CodBarra;
                TextBox_Preco1AOE.Text = produtoSelecionado.Preco1.ToString("F2");
                TextBox_Preco2AOE.Text = produtoSelecionado.Preco2.ToString("F2");
                switch (produtoSelecionado.COB)
                {
                    case true:
                        Radio_Codigo.IsChecked = true;
                        break;
                    case false:
                        Radio_CodigoBarras.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }
            else { return; }

            Dialog_AddEdit.IsOpen = true;
            Text_DialogAddOrEdit.Text = "Editar Produto";
        }

        private void Button_Sair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Config_Click(object sender, RoutedEventArgs e)
        {
            Dialog_Config.IsOpen = true;
        }

        private void Button_DialogSalvar_Click(object sender, RoutedEventArgs e)
        {
            // Validação simples para garantir que os campos não estejam vazios
            if (TextBox_DescricaoAOE.Text == "" || TextBox_CodigoAOE.Text == "" || TextBox_CodigoBarrasAOE.Text == "" 
                || TextBox_Preco1AOE.Text == "" || TextBox_Preco2AOE.Text == "")  
            {
                MessageBox.Show("Por favor, preencha todos os campos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {

                switch (addOrEdit)
                {
                    case true:
                        // Adicionar novo produto
                        string connectionString = "Data Source=BDGertec.db"; // Caminho do arquivo SQLite
                        var connection = new SQLiteConnection(connectionString);
                        connection.Open();

                        string query = @"INSERT INTO Produtos(Codigo, CodigoBarras, Descricao, Preco1, Preco2) 
                                VALUES(@Codigo, @CodigoBarras, @Descricao, @Preco1, @Preco2)";
                        var command = new SQLiteCommand(query, connection);
                        command.Parameters.AddWithValue("@Codigo", TextBox_CodigoAOE.Text);
                        command.Parameters.AddWithValue("@CodigoBarras", TextBox_CodigoBarrasAOE.Text);
                        command.Parameters.AddWithValue("@Descricao", TextBox_DescricaoAOE.Text);
                        command.Parameters.AddWithValue("@Preco1", decimal.Parse(TextBox_Preco1AOE.Text));
                        command.Parameters.AddWithValue("@Preco2", decimal.Parse(TextBox_Preco2AOE.Text));
                        command.Parameters.AddWithValue("@COB", Obter_BoolCOBInt(false));
                        command.ExecuteNonQuery();

                        CarregarProdutos();
                        Dialog_AddEdit.IsOpen = false;
                        break;
                    case false:
                        // Editar produto existente
                        string connectionString1 = "Data Source=BDGertec.db"; // Caminho do arquivo SQLite
                        var connection1 = new SQLiteConnection(connectionString1);
                        connection1.Open();

                        string query1 = @$"UPDATE Produtos SET CodigoBarras = @CodigoBarras, Descricao = @Descricao, Preco1 = @Preco1, Preco2 = @Preco2 WHERE Codigo = {TextBox_CodigoAOE.Text}";
                        var command1 = new SQLiteCommand(query1, connection1);
                        command1.Parameters.AddWithValue("@Codigo", TextBox_CodigoAOE.Text);
                        command1.Parameters.AddWithValue("@CodigoBarras", TextBox_CodigoBarrasAOE.Text);
                        command1.Parameters.AddWithValue("@Descricao", TextBox_DescricaoAOE.Text);
                        command1.Parameters.AddWithValue("@Preco1", decimal.Parse(TextBox_Preco1AOE.Text));
                        command1.Parameters.AddWithValue("@Preco2", decimal.Parse(TextBox_Preco2AOE.Text));
                        command1.Parameters.AddWithValue("@COB", Obter_BoolCOBInt(false));
                        command1.ExecuteNonQuery();

                        CarregarProdutos();
                        Dialog_AddEdit.IsOpen = false;
                        break;
                    default:
                }
            }
        }

        private void DataGridProdutos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            addOrEdit = false;
            var produtoSelecionado = (Produto)DataGridProdutos.SelectedItem;
            if (produtoSelecionado != null)
            {
                TextBox_DescricaoAOE.Text = produtoSelecionado.Descricao;
                TextBox_CodigoAOE.Text = produtoSelecionado.Codigo;
                TextBox_CodigoAOE.IsEnabled = false;
                TextBox_CodigoBarrasAOE.Text = produtoSelecionado.CodBarra;
                TextBox_Preco1AOE.Text = produtoSelecionado.Preco1.ToString("F2");
                TextBox_Preco2AOE.Text = produtoSelecionado.Preco2.ToString("F2");
            }
            else { return; }

            Dialog_AddEdit.IsOpen = true;
            Text_DialogAddOrEdit.Text = "Editar Produto";
        }

        private void Button_FolderDB_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecione o banco de dados",
                Filter = "Arquivo SQLite (*.db)|*.db|Todos os arquivos (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                TextBox_PathDB.Text = dialog.FileName;
                TextBox_PathDB.ToolTip = dialog.FileName;
                Properties.Settings.Default.PathDBExport = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void Button_Excluir_Click(object sender, RoutedEventArgs e)
        {
            var produtoSelecionado = (Produto)DataGridProdutos.SelectedItem;
            if (produtoSelecionado != null)
            {
                Text_DialogExcluir.Text = $"Você deseja realmente excluir o produto de código {produtoSelecionado.Codigo} ?";
            }
            else { return; }

            Dialog_Excluir.IsOpen = true;
        }

        private void Button_ExcluirDialog_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=BDGertec.db"; // Caminho do arquivo SQLite
            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM Produtos WHERE Codigo = {((Produto)DataGridProdutos.SelectedItem).Codigo}";
            var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            CarregarProdutos();
            Dialog_Excluir.IsOpen = false;
        }

        #region Filtros Chips
        private void Chip_Codigo_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_CodigoFiltro.IsEnabled = true;
        }

        private void Chip_Codigo_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox_CodigoFiltro.IsEnabled = false;
        }

        private void Chip_CodigoBarras_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_CodigoBFiltro.IsEnabled = true;
        }

        private void Chip_CodigoBarras_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox_CodigoBFiltro.IsEnabled = false;
        }

        private void Chip_Descricao_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_DescricaoFiltro.IsEnabled = true;
        }

        private void Chip_Descricao_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox_DescricaoFiltro.IsEnabled = false;
        }

        private void Chip_Preco1_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Preco1Filtro.IsEnabled = true;
        }

        private void Chip_Preco1_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox_Preco1Filtro.IsEnabled = false;
        }

        private void Cihp_Preco2_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Preco2Filtro.IsEnabled = true;
        }

        private void Cihp_Preco2_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox_Preco2Filtro.IsEnabled = false;
        }
        #endregion
        private void Button_Filtrar_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder filter = new();
            if (Chip_Codigo.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"Codigo = '{TextBox_CodigoFiltro.Text}'");
            }
            if (Chip_CodigoBarras.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"CodigoBarras = '{TextBox_CodigoBFiltro.Text}'");
            }
            if (Chip_Descricao.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"Descricao LIKE '%{TextBox_DescricaoFiltro.Text}%'");
            }
            if (Chip_Preco1.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"Preco1 = {TextBox_Preco1Filtro.Text}");
            }
            if (Cihp_Preco2.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"Preco2 = {TextBox_Preco2Filtro.Text}");
            }
            if (Chip_COB.IsChecked == true)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                filter.Append($"COB = {Obter_BoolCOBInt(true)}");
            }
            if (filter.Length > 0)
            {
                string connectionString = "Data Source=BDGertec.db"; // Caminho do arquivo SQLite
                var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = $"SELECT * FROM Produtos WHERE {filter}";
                var command = new SQLiteCommand(query, connection);
                using var reader = command.ExecuteReader();
                
                Produtos.Clear();
                while (reader.Read())
                {
                    Produtos.Add(new Produto
                    {
                        Codigo = reader.GetString(0),
                        CodBarra = reader.GetString(1),
                        Descricao = reader.GetString(2),
                        Preco1 = reader.GetDecimal(3),
                        Preco2 = reader.GetDecimal(4)
                    });
                }
            }
            else
            {
                return;
            }
            Dialog_Filtrar.IsOpen = false;
        }

        private void Context_Filtrar_Click(object sender, RoutedEventArgs e)
        {
            Dialog_Filtrar.IsOpen = true;
            TextBox_CodigoFiltro.Text = "";
            TextBox_CodigoBFiltro.Text = "";
            TextBox_DescricaoFiltro.Text = "";
            TextBox_Preco1Filtro.Text = "";
            TextBox_Preco2Filtro.Text = "";
            Chip_Codigo.IsChecked = false;
            Chip_CodigoBarras.IsChecked = false;
            Chip_Descricao.IsChecked = false;
            Chip_Preco1.IsChecked = false;
            Cihp_Preco2.IsChecked = false;
        }

        private void Context_Excluir_Click(object sender, RoutedEventArgs e)
        {
            var produtoSelecionado = (Produto)DataGridProdutos.SelectedItem;
            if (produtoSelecionado != null)
            {
                Text_DialogExcluir.Text = $"Você deseja realmente excluir o produto de código {produtoSelecionado.Codigo} ?";
            }
            else { return; }

            Dialog_Excluir.IsOpen = true;
        }

        private void Context_Edit_Click(object sender, RoutedEventArgs e)
        {
            addOrEdit = false;
            var produtoSelecionado = (Produto)DataGridProdutos.SelectedItem;
            if (produtoSelecionado != null)
            {
                TextBox_DescricaoAOE.Text = produtoSelecionado.Descricao;
                TextBox_CodigoAOE.Text = produtoSelecionado.Codigo;
                TextBox_CodigoAOE.IsEnabled = false;
                TextBox_CodigoBarrasAOE.Text = produtoSelecionado.CodBarra;
                TextBox_Preco1AOE.Text = produtoSelecionado.Preco1.ToString("F2");
                TextBox_Preco2AOE.Text = produtoSelecionado.Preco2.ToString("F2");
            }
            else { return; }

            Dialog_AddEdit.IsOpen = true;
            Text_DialogAddOrEdit.Text = "Editar Produto";
        }

        private void Context_Add_Click(object sender, RoutedEventArgs e)
        {
            addOrEdit = true;
            Dialog_AddEdit.IsOpen = true;
            TextBox_DescricaoAOE.Text = "";
            TextBox_CodigoAOE.Text = "";
            TextBox_CodigoAOE.IsEnabled = true;
            TextBox_CodigoBarrasAOE.Text = "";
            TextBox_Preco1AOE.Text = "0,00";
            TextBox_Preco2AOE.Text = "0,00";
            Text_DialogAddOrEdit.Text = "Adicionar Produto";
        }

        private int Obter_BoolCOBInt(bool pesquisa)
        {
            switch (pesquisa)
            {
                case false:
                    if (Radio_Codigo.IsChecked == true)
                    {
                        return 1;
                    }
                    else if (Radio_CodigoBarras.IsChecked == true)
                    {
                        return 0;
                    }
                    break;
                case true:
                    if (Radio_CodigoFiltro.IsChecked == true)
                    {
                        return 1;
                    }
                    else if (Radio_CodigoBarrasFiltro.IsChecked == true)
                    {
                        return 0;
                    }
                    break;
                default:
            }
            return int.MinValue;
        }
        private void Button_CMD_Click(object sender, RoutedEventArgs e)
        {
            switch (TransitionerCMD.SelectedIndex)
            {
                case 0:
                    TransitionerCMD.SelectedIndex = 1;
                    TransitionerMain.SelectedIndex = 1;
                    break;
                case 1:
                    TransitionerCMD.SelectedIndex = 0;
                    TransitionerMain.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void Chip_COB_Checked(object sender, RoutedEventArgs e)
        {
            COBFilter.IsEnabled = true;
        }

        private void Chip_COB_Unchecked(object sender, RoutedEventArgs e)
        {
            COBFilter.IsEnabled = false;
        }
    }
}