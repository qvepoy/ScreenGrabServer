using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenGrabServer {
    public partial class MainWindow : Window {

        int i = 0;

        public MainWindow() {
            InitializeComponent();
            
        }

        public class ClientObject {
            public TcpClient client;
            public ClientObject(TcpClient tcpClient) {
                client = tcpClient;
            }

            public void Process() {
                NetworkStream stream = null;
                try {
                    stream = client.GetStream();
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    while (true) {
                        // получаем сообщение
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        string message = builder.ToString();

                        Console.WriteLine(message);
                        // отправляем обратно сообщение в верхнем регистре
                        message = message.Substring(message.IndexOf(':') + 1).Trim().ToUpper();
                        data = Encoding.Unicode.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                finally {
                    if (stream != null)
                        stream.Close();
                    if (client != null)
                        client.Close();
                }
            }
        }

        const int port = 8888;
        static TcpListener listener;

        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Wait connection...");

                while (true) {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            finally {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
