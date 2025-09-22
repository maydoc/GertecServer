using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GertecServer.Views
{
    /// <summary>
    /// Interação lógica para Terminal.xaml
    /// </summary>
    public partial class Terminal : Page
    {
        [DllImport("sc501ger.dll")]
        private static extern void vInitialize();

        private Socket server;
        private IPEndPoint IPServer;
        private Socket cliente;

        public Terminal()
        {
            InitializeComponent();
            vInitialize();
        }
    }
}
