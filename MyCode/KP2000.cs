using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace masic3.MyCode
{
    internal class KP2000 : IDisposable
    {
        SerialPort port = new SerialPort();
        ModbusAscii frame = new ModbusAscii();
        private bool disposedValue;

        public bool IsOpen => port.IsOpen;

        public void Close()
        {
            if (port.IsOpen == true)
            {
                port.Close();
            }
        }

        public void Init()
        {
            //port.PortName = Properties.Settings.Default.SerialPort;
            port.PortName = Preferences.Default.Get("SerialPort", "COM1");
            port.BaudRate = 38400;
            port.Parity = Parity.Even;
            port.DataBits = 7;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.ReadTimeout = 100;
            port.WriteTimeout = 100;
        }

        string Communicate(string wFrame)
        {
            //if (Properties.Settings.Default.EnabledKP != true || wFrame == "") { return ""; }
            if (Preferences.Default.Get("EnabledKP",false) != true || wFrame == "") { return ""; }

            string copyFrame = wFrame, readFrame = "";
            char[] buff = new char[32];
            int readCnt = 0, limitLoop = 0, limitRetry = 0;
            bool loopFlag = true;
            while (loopFlag == true)
            {
                if (IsOpen)
                {
                    if (copyFrame != "")
                    {
                        port.Write(copyFrame);
                        copyFrame = "";
                        readFrame = "";
                    }
                    else
                    {
                        if (port.BytesToRead > 0)
                        {
                            try
                            {
                                readCnt = port.Read(buff, 0, port.BytesToRead % (buff.Length - 1));
                                readFrame += new string(buff.Take(readCnt).ToArray());
                                if (readFrame.Contains("\n") == true)
                                {
                                    // 改行が来たら終わりだが、先頭が":"ではない場合はやり直す！
                                    if (readFrame[0] == ':')
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        copyFrame = wFrame;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                        }
                    }
                }
                else
                {
                    Init();
                    port.Open();
                }

                if (limitLoop++ == 10)
                {
                    if (limitRetry++ < 3)
                    {
                        limitLoop = 0;
                        copyFrame = wFrame;
                    }
                    else
                    {
                        loopFlag = false;
                        readFrame = "";
                    }
                }

                Thread.Sleep(50);
            }

            return readFrame;
        }

        public (bool, int) GetSingleData(int sa, int fc, int rn, int dn)
        {
            string retStr = Communicate(frame.MakeWriteFrame(sa, fc, rn, dn));
            if (frame.IsError(retStr) != true)
            {
                return (true, Convert.ToInt32(retStr.Substring(7, 4), 16));
            }
            else
            {
                return (false, -1);
            }
        }

        public bool SetSingleData(int sa, int fc, int rn, int dn)
        {
            bool retBool = true;
            string retStr = Communicate(frame.MakeWriteFrame(sa, fc, rn, dn));
            if (frame.IsError(retStr) != true)
            {
                retBool = false;
            }

            return retBool;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                if (port.IsOpen == true) { port.Close(); }

                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~KP2000()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
