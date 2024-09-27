using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

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
            ModelShare.Working = true;

            DateTime dateTimeNow;
            TimeSpan timeSpan;      // 制御全体の経過時間
            TimeSpan timeSpanSum;   // 現プロセスまでの設定時間積み上げ
            int currentProcessIndex;
            bool gotoNext = false;

            do
            {
                //MyMessage = $"Welcome at {DateTime.Now}";

                if (IsRunning == true)
                {
                    if (ProcessItemsCopy.Count > 0)
                    {
                        dateTimeNow = DateTime.Now;
                        if (dateTimeNow >= dateTimeStartProcess)
                        {
                            // 制御経過時間を計算する
                            timeSpan = dateTimeNow - dateTimeStartProcess;
                            PastProcessDateTimeText = TimeSpanToString(timeSpan);

                            currentProcessIndex = 0;
                            timeSpanSum = ProcessItemsCopy[currentProcessIndex].ProcessTime;

                            // プロセス経過処理
                            foreach (ModelProcessItem item in ProcessItemsCopy)
                            {
                                if (timeSpan < timeSpanSum)
                                {
                                    gotoNext = Processing(currentProcessIndex, dateTimeNow - dateTimeStartItem);
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

                            // プロセスを次へ進める処理（Advance）
                            if (IsAdvance == true || gotoNext == true)
                            {
                                ProcessItemsCopy[currentProcessIndex].ProcessTime = dateTimeNow - dateTimeStartItem;
                                IsAdvance = false;
                            }

                            //ModelProcessItem mi = ProcessItemsCopy[currentProcessIndex];
                            //Debug.WriteLine($"P:{mi.ProcessId:0}, T: {(dateTimeNow - dateTimeStartItem).TotalSeconds:0} / {mi.ProcessTime.TotalSeconds:0}");

                            // コントロール表示更新
                            if (currentProcessIndex < ProcessItemsCopy.Count)
                            {
                                CurrentProcessIndexText = ProcessItemsCopy[currentProcessIndex].ProcessId.ToString();
                            }
                            StartItemDateTimeText = dateTimeStartItem.ToString("HH:mm:ss");
                            PastItemDateTimeText = TimeSpanToString(dateTimeNow - dateTimeStartItem);

                            if (currentProcessIndex >= ProcessItemsCopy.Count)
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
            } while (ModelShare.EnableThreadLoop == true);

            ModelShare.Working = false;
            Debug.WriteLine("スレッド終了！");
        }

        bool Processing(int index, TimeSpan timeSpan)
        {
            //Debug.WriteLine($"P-ID:{ProcessItemsCopy[index].ProcessId}, P-Time:{timeSpan.TotalSeconds} passed of the {ProcessItemsCopy[index].ProcessTime.TotalSeconds}");

            bool ret = false;
            switch (ProcessItemsCopy[index].ProcessCommand.ToUpper())
            {
                case "KP-OUT":
                    double StartValue = double.Parse((index > 0) ? ProcessItemsCopy[index - 1].ProcessParam : "0.0");
                    double TargetValue = double.Parse(ProcessItemsCopy[index].ProcessParam);
                    double ChangeValue = TargetValue - StartValue;
                    double CurrentValue = StartValue + ChangeValue * (timeSpan.TotalSeconds / ProcessItemsCopy[index].ProcessTime.TotalSeconds);
                    Debug.WriteLine($"KP-OUT:{CurrentValue:0.0}");
                    break;
                case "KP-WAIT-EVENT":
                    if (IsCheckedTest == true)
                    {
                        ret = true;
                    }
                    Debug.WriteLine("KP-WAIT-EVENT");
                    break;
            }

            return ret;
        }
    }
}
