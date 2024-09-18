using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;

namespace masic3.MyCode
{
    internal partial class ViewModelProcess : ObservableObject
    {

        /// <summary>
        /// ワーカスレッド
        /// </summary>
        /// <param name="state"></param>
        private void ProcessThread(object? state)
        {
            Debug.WriteLine("スレッド開始！");
            masic3.MyCode.ModelShare.Working = true;

            DateTime dateTimeNow;
            TimeSpan timeSpan;
            TimeSpan timeSpanSum;
            int currentProcessIndex;

            do
            {
                //MyMessage = $"Welcome at {DateTime.Now}";

                if (IsRunning == true)
                {
                    if (ProcessItems.Count > 0)
                    {
                        dateTimeNow = DateTime.Now;
                        if (dateTimeNow >= dateTimeStartProcess)
                        {
                            // 制御経過時間を計算する
                            timeSpan = dateTimeNow - dateTimeStartProcess;
                            PastProcessDateTimeText = TimeSpanToString(timeSpan);

                            currentProcessIndex = 0;
                            timeSpanSum = ProcessItems[currentProcessIndex].ProcessTime;

                            // プロセス経過処理
                            foreach (ModelProcessItem item in ProcessItems)
                            {
                                if (timeSpan < timeSpanSum)
                                {
                                    currentProcessIndex += Processing(currentProcessIndex, dateTimeNow - dateTimeStartItem);
                                    break;
                                }
                                else
                                {
                                    // プロセスが終了と判定

                                    // 次のプロセスに進む準備
                                    dateTimeStartItem = dateTimeStartProcess + timeSpanSum;
                                    timeSpanSum += item.ProcessTime;
                                }

                                currentProcessIndex++;
                            }

                            // コントロール表示更新
                            if (currentProcessIndex < ProcessItems.Count)
                            {
                                CurrentProcessIndexText = ProcessItems[currentProcessIndex].ProcessId.ToString();
                            }
                            StartItemDateTimeText = dateTimeStartItem.ToString("HH:mm:ss");
                            PastItemDateTimeText = TimeSpanToString(dateTimeNow - dateTimeStartItem);

                            if (currentProcessIndex >= ProcessItems.Count)
                            {
                                // 制御終了と判定
                                Debug.WriteLine("制御完了");

                                IsRunning = false;
                                continue;
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            } while (masic3.MyCode.ModelShare.EnableThreadLoop == true);

            masic3.MyCode.ModelShare.Working = false;
            Debug.WriteLine("スレッド終了！");
        }

        int Processing(int index, TimeSpan timeSpan)
        {
            int ret = 0;
            switch (ProcessItems[index].ProcessCommand.ToUpper())
            {
                case "KP-OUT":
                    double StartValue = double.Parse((index > 0) ? ProcessItems[index - 1].ProcessParam : "0.0");
                    double TargetValue = double.Parse(ProcessItems[index].ProcessParam);
                    double ChangeValue = TargetValue - StartValue;
                    double CurrentValue = StartValue + ChangeValue * (timeSpan.TotalSeconds / ProcessItems[index].ProcessTime.TotalSeconds);
                    Debug.WriteLine($"KP-OUT:{CurrentValue:0.0}");
                    break;
                case "KP-WAIT-EVENT":
                    if (IsCheckedTest == true)
                    {
                        ret = 1;
                    }
                    Debug.WriteLine("KP-WAIT-EVENT");
                    break;
            }

            return ret;
        }
    }
}
