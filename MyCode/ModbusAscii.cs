using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace masic3.MyCode
{
    internal class ModbusAscii
    {
        public string ReadFrame { get; set; } = "";
        public string WriteFrame { get; set; } = "";
        byte[] MsgBuffer = new byte[6];

        /// <summary>
        /// 伝送データを作る（バイナリ）
        /// </summary>
        /// <param name="sa">スレーブアドレス</param>
        /// <param name="fc">ファンクションコード</param>
        /// <param name="rn">開始番号</param>
        /// <param name="dn">データ個数</param>
        /// <returns>メッセージ本体</returns>
        public byte[] MakeTransData(int sa, int fc, int rn, int dn)
        {
            MsgBuffer[0] = (byte)sa;
            MsgBuffer[1] = (byte)fc;
            MsgBuffer[2] = (byte)((rn & 0xff00) >> 8);
            MsgBuffer[3] = (byte)(rn & 0x00ff);
            MsgBuffer[4] = (byte)((dn & 0xff00) >> 8);
            MsgBuffer[5] = (byte)(dn & 0x00ff);

            return MsgBuffer;
        }

        /// <summary>
        /// チェックデータを作る(LRC)
        /// </summary>
        /// <param name="data">伝送データ</param>
        /// <returns>チェックデータ</returns>
        byte MakeLRC(byte[] data)
        {
            byte lrc = 0;
            foreach (byte b in data)
            {
                lrc += b;
            }
            lrc ^= 0xff;

            return ++lrc;
        }

        /// <summary>
        /// メッセージフレーム作成
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="fc"></param>
        /// <param name="rn"></param>
        /// <param name="dn"></param>
        /// <returns></returns>
        public string MakeWriteFrame(int sa, int fc, int rn, int dn)
        {
            byte[] msg = MakeTransData(sa, fc, rn, dn);   // 伝送データ作成

            // ASCII変換
            WriteFrame = ":";
            foreach (byte b in MakeTransData(sa, fc, rn, dn))
            {
                WriteFrame += $"{b:X2}";
            }

            WriteFrame += $"{MakeLRC(msg):X2}\r\n"; // エラーチェック付加

            return WriteFrame;
        }

        public byte[] GetBinaryMSG(string str)
        {
            int headPos = str.LastIndexOf(":") + 1;
            string sub = str.Substring(headPos, str.Length - (headPos + ("\r\n").Length));    // 伝送データ部分（先頭[:]と末尾[CR][LF]以外）を抜き出す

            // 伝送データを２文字ずつに区切ってバイナリ配列に変換
            byte[] decode = new byte[(sub.Length) / 2];
            int i = 0;
            foreach (Match m in Regex.Matches(sub, "(..)"))
            {
                decode[i++] = Convert.ToByte(m.Value, 16);
            }

            return decode;
        }

        /// <summary>
        /// 受信フレームのエラーチェック
        /// </summary>
        /// <param name="str"></param>
        /// <returns>エラーのときはtrue</returns>
        public bool IsError(string str)
        {
            bool judge = true;
            if (str != "")
            {
                byte[] decode = GetBinaryMSG(str);
                if (decode[1] >= 0x80)
                {
                    Debug.Print($"エラー応答！コード：{decode[2]:X2}");
                }

                // エラーチェック照合
                byte[] msg = new byte[(decode.Length) - 1];
                Array.Copy(decode, msg, (decode.Length) - 1);
                if (decode[decode.Length - 1] == MakeLRC(msg))
                {
                    judge = false;
                }
                else
                {
                    Debug.Print($"受信文字列エラー：{str}");
                }
            }

            return judge;
        }

        /// <summary>
        /// メッセージフレームを読みやすい文字列に変換
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public string ParseFrame(string buff)
        {
            char[] cary = buff.ToCharArray();
            string str = "";
            int code;
            for (int i = 0; i < cary.Length; i++)
            {
                if (i == 0)
                {
                    str = $"[{cary[i]}]";
                }
                else if (i >= cary.Length - 2)
                {
                    code = cary[i];
                    str += $"[{code:X2}]";
                }
                else
                {
                    str += cary[i];
                }
            }

            return str;
        }
    }
}
