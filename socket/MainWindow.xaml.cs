using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net;
using System.Net.Sockets;

namespace socket
{
    public partial class MainWindow : Window
    {
        Socket mySocket;
        Socket client;

        bool isServer { get => rbServer.IsChecked == true; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string message = tbMessage.Text;
            DisplayMessage(message);

            byte[] byteMessage = Encoding.ASCII.GetBytes(message);

            if (isServer)
            {
                client.Send(byteMessage);
            }
            else
            {
                mySocket.Send(byteMessage);
            }
        }

        private void tbCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            if (mySocket != null)
            {
                mySocket.Close();
            }
            if (client != null)
            {
                client.Close();
            }

            Dispatcher.Invoke(() =>
            {
                btnSend.IsEnabled = false;
                tbMessage.IsEnabled = false;
                btnCloseConnection.IsEnabled = false;
                btnListenConnect.IsEnabled = true;
                rbClient.IsEnabled = true;
                rbServer.IsEnabled = true;
            });
        }

        private void tbListen_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ipAddress;
            if (tbAddress.Text == "loopback")
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHostInfo.AddressList[0];
            }
            else
            {
                ipAddress = IPAddress.Parse(tbAddress.Text);
            }

            IPEndPoint endPoint = new IPEndPoint(ipAddress, Int32.Parse(tbPort.Text));

            mySocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (rbServer.IsChecked == true)
            {
                mySocket.Bind(endPoint);
                mySocket.Listen(100);

                client = mySocket.Accept();
                btnCloseConnection.IsEnabled = true;

                var thread = new Thread(new ParameterizedThreadStart(IncomingListener));
                thread.IsBackground = true;
                thread.Start(client);
            }
            else if (rbClient.IsChecked == true)
            {
                mySocket.Connect(endPoint);

                var thread = new Thread(new ParameterizedThreadStart(IncomingListener));
                thread.IsBackground = true;
                thread.Start(mySocket);
            }

            btnSend.IsEnabled = true;
            tbMessage.IsEnabled = true;
            btnCloseConnection.IsEnabled = true;
            btnListenConnect.IsEnabled = false;
            rbClient.IsEnabled = false;
            rbServer.IsEnabled = false;
        }

        private void IncomingListener(object objClient)
        {
            try
            {
                var client = objClient as Socket;
                while (true)
                {
                    string data = String.Empty;
                    var bytes = new byte[1024];

                    int bytesRec = client.Receive(bytes);  
                    data += Encoding.ASCII.GetString(bytes,0,bytesRec);

                    DisplayMessage(data);
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    tbAllMessages.Text = String.Empty;
                });
                tbCloseConnection_Click(null, null);
            }
        }

        private void rbServer_Click(object sender, RoutedEventArgs e)
        {
            btnListenConnect.Content = "Listen";
        }

        private void rbClient_Click(object sender, RoutedEventArgs e)
        {
            btnListenConnect.Content = "Connect";
        }

        private void DisplayMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                tbAllMessages.Text += message + "\n\n";
            });
        }
    }
}
