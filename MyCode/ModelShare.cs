namespace masic3.MyCode
{
    /// <summary>
    /// グローバル変数的に使う
    /// アプリ終了時にワーカスレッドを終了させたかったが、良い方法がわからなかったので苦肉の策です(-_-;)
    /// </summary>
    public static class ModelShare
    {
        public static bool EnableThreadLoop { get; set; } = true;   // ワーカスレッドのループ保持フラグ
        public static bool Working { get; set; } = false;           // ワーカスレッドの生存フラグ
    }
}
