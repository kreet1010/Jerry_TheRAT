using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets; // for Socket Programing
using System.IO; //for Streams
using System.Threading; //to run commands concurrently using threads
using System.Net; //for IPEndPoint

namespace Client
{
    public partial class Form1 : Form
    {
        TcpListener tcpListener;
        Socket socketForServer;
        NetworkStream networkStream;
        StreamWriter streamWriter;
        StreamReader streamReader;
        StringBuilder strInput;
        Thread th_StartListen, th_RunClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            th_StartListen = new Thread(new ThreadStart(StartListen)); //When the exe file is executed, start listening for connection requests on port 9876
            th_StartListen.Start();
            textBox2.Focus();
        }

        // Open Port 9876 and accept connection
        private void StartListen()
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 9876);
            tcpListener.Start();
            toolStripStatusLabel1.Text = "Listening on port 9876 ...";
            for (; ; )
            {
                socketForServer = tcpListener.AcceptSocket();
                IPEndPoint ipend = (IPEndPoint)socketForServer.RemoteEndPoint;
                toolStripStatusLabel1.Text = "Connected to " + IPAddress.Parse(ipend.Address.ToString());
                th_RunClient = new Thread(new ThreadStart(RunClient));
                th_RunClient.Start();
            }
        }


        // Delivers the communication functionality
        private void RunClient()
        {
            networkStream = new NetworkStream(socketForServer);
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);
            strInput = new StringBuilder();

            while (true)
            {
                try
                {
                    strInput.Append(streamReader.ReadLine());
                    strInput.Append("\r\n");
                }
                catch (Exception err)
                {
                    Cleanup();
                    break;
                }
                Application.DoEvents();
                DisplayMessage(strInput.ToString()); //Recieved Data will be presented in textBox1
                strInput.Remove(0, strInput.Length); 
            }
        }

        private void Cleanup()      //clear the streams and socket
        {
            try
            {
                streamReader.Close();
                streamWriter.Close();
                networkStream.Close();
                socketForServer.Close();
            }
            catch (Exception err) { }
            toolStripStatusLabel1.Text = "Connection Lost";
        }

        private delegate void DisplayDelegate(string message);
        private void DisplayMessage(string message)
        {
            if (textBox1.InvokeRequired)
            {
                Invoke(new DisplayDelegate(DisplayMessage), new object[] { message });
            }
            else
            {
                textBox1.AppendText(message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cleanup();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    strInput.Append(textBox2.Text.ToString());
                    streamWriter.WriteLine(strInput);
                    streamWriter.Flush();
                    strInput.Remove(0, strInput.Length);
                    if (textBox2.Text == "exit") Cleanup();
                    if (textBox2.Text == "cls") textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
            catch (Exception err) { }
        }


        //helpbox
        private void button1_Click(object sender, EventArgs e)
        {
            streamWriter.WriteLine("1");
            streamWriter.Flush();
        }

        //messagebox
        private void button2_Click(object sender, EventArgs e)
        {
            streamWriter.WriteLine("2");
            streamWriter.Flush();
        }

        //beep
        private void button3_Click(object sender, EventArgs e)
        {
            streamWriter.WriteLine("3");
            streamWriter.Flush();
        }

        //playsound
        private void button4_Click(object sender, EventArgs e)
        {
            streamWriter.WriteLine("4");
            streamWriter.Flush();
        }

        //shutdown server
        private void button5_Click(object sender, EventArgs e)
        {
            streamWriter.WriteLine("5");
            streamWriter.Flush();
            toolStripStatusLabel1.Text = "Server Disconnected.";
        }
    }
}
