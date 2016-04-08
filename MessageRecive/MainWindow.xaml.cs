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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MessageRecive
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //创建 1个客户端套接字 和1个负责监听服务端请求的线程  
        Socket socketClient = null;
        Thread threadClient = null;
        private NotifyIcon m_notifyIcon;
        
        public MainWindow()
        {
            InitializeComponent();

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            m_notifyIcon.BalloonTipTitle = "The App";
            m_notifyIcon.Text = "The App";
            //m_notifyIcon.Icon = this.Icon;
            m_notifyIcon.Click += m_notifyIcon_Click;
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //通过 clientSocket 发送数据
            ClientSendMsg(this.txt.Text);
        }

        private void StartLs(object sender, RoutedEventArgs e)
        {
            try
            {
                //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //需要获取文本框中的IP地址
                IPAddress ipaddress = IPAddress.Parse(txtIP.Text.Trim());
                //将获取的ip地址和端口号绑定到网络节点endpoint上
                IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(txtPort.Text));
                //这里客户端套接字连接到网络节点(服务端)用的方法是Connect 而不是Bind
                socketClient.Connect(endpoint);
                //创建一个线程 用于监听服务端发来的消息
                threadClient = new Thread(RecMsg);
                //将窗体线程设置为与后台同步
                threadClient.IsBackground = true;
                //启动线程
                threadClient.Start();
            }
            catch (Exception exp)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    this.txtRecord.AppendText("\r\n无法连接服务器\r\n");
                    txtRecord.ScrollToEnd();
                });
                return;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                this.txtRecord.AppendText("\r\n已连接上服务器\r\n");
                txtRecord.ScrollToEnd();
            });
        }

        /// <summary>
        /// 接收服务端发来信息的方法
        /// </summary>
        private void RecMsg()
        {
            while (true) //持续监听服务端发来的消息
            {
                try
                {
                    //定义一个1M的内存缓冲区 用于临时性存储接收到的信息
                    byte[] arrRecMsg = new byte[1024 * 1024];
                    //将客户端套接字接收到的数据存入内存缓冲区, 并获取其长度
                    int length = socketClient.Receive(arrRecMsg);
                    //将套接字获取到的字节数组转换为人可以看懂的字符串
                    string strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                    //将发送的信息追加到聊天内容文本框中
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        this.txtRecord.AppendText("对方:" + DateTime.Now.ToString() + "\r\n" + strRecMsg + "\r\n");
                        txtRecord.ScrollToEnd();
                    });
                }
                catch (Exception exp)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        this.txtRecord.AppendText("\r\nError:" + exp.Message + "\r\n");
                        txtRecord.ScrollToEnd();
                    });
                    return;
                }
            }
        }

        /// <summary>
        /// 发送字符串信息到服务端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        private void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组
            socketClient.Send(arrClientSendMsg);
            //将发送的信息追加到聊天内容文本框中
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                this.txtRecord.AppendText("我:" + DateTime.Now.ToString() + "\r\n" + sendMsg + "\r\n");
                txtRecord.ScrollToEnd();
                this.txt.Text = "";
            });
        }

        private void txt_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Enter)
            {
                //通过 clientSocket 发送数据
                ClientSendMsg(this.txt.Text);
            }
        }
    }
}
