using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class GameManager : MonoBehaviour {
    //ブール値系
    bool isCounting = false;  //計測が開始しているか
    bool isPopup = false;  //ポップアップが表示されているか
    bool isPopup_end = false;  //（一日が終わった時の）ポップアップが表示されているか

    //表示用テキスト
    [SerializeField] Text timerTextHour;  //時
    [SerializeField] Text timerTextMinute;  //分
    [SerializeField] Text timerTextSecond;  //秒

    //ポップアップ時の表示用テキスト
    [SerializeField] Text popupTextHour;  //時
    [SerializeField] Text popupTextMinute;  //分
    [SerializeField] Text popupTextSecond;  //秒

    //一日の合計時間表示用テキスト
    [SerializeField] Text dayTextHour;  //時
    [SerializeField] Text dayTextMinute;  //分
    [SerializeField] Text dayTextSecond;  //秒

    //時間カウント用
    int hours;  //時
    int minutes;  //分
    int seconds;  //秒
    float time = 0.0f;  //現在のカウント時間（秒）

    //ゲームオブジェクト系
    [SerializeField] GameObject popup;  //ポップアップオブジェクト
    [SerializeField] GameObject popup_end;  //一日の終わりがきたときにポップアップするオブジェクト
    [SerializeField] GameObject panels;  //モードごとに画面を切り替えるための、全画面をまとめたパネル

    //パス系
    string basePath;  //ベースとなるパス
    string savePath;  //保存先のパス
    DirectoryInfo info_unity;

    //今の日付と前フレームの日付
    DateTime date_prev;
    DateTime date_now;

    // Start is called before the first frame update
    void Start() {
        //エディタ上、アンドロイド、iOSそれぞれについて保存パスを取る
#if UNITY_EDITOR
        basePath = Application.dataPath;
        //FocusTimerフォルダがなければ作る
        if (!Directory.Exists(basePath + "/FocusTimer")) {
            Directory.CreateDirectory(basePath + "/../FocusTimer");
        }
        //DirectoryInfoを取る
        info_unity = new DirectoryInfo(basePath + "/../FocusTimer");
        savePath = info_unity.FullName + "/TimeHistory.txt";
#elif UNITY_ANDROID
        basePath = Application.persistentDataPath;
        savePath = basePath + "/../TimeHistory.txt";
#elif UNITY_IOS
        basePath = Application.persistentDataPath;
        if (!Directory.Exists(basePath + "/FocusTimer")){
            info_unity = Directory.CreateDirectory(basePath + "/FocusTimer");
        } else {
            info_unity = new DirectoryInfo(basePath + "/FocusTimer");
        }
        savePath = info_unity.FullName + "/TimeHistory.txt";
#endif

        //savePathが存在しないとき
        if (!File.Exists(savePath)) {
            StreamWriter writer = File.CreateText(savePath);
            writer.Close();
        }

        //1000行分の最初分の行を記入する
        FileInfo info = new FileInfo(savePath);
        if (info.Length == 0) {
            using (StreamWriter writer = new StreamWriter(savePath)) {
                for (int i = 0; i < 10; i++) {
                    writer.Write(DateTime.Today.AddDays(i).Date.ToString("yyyy/MM/dd"));
                    writer.WriteLine(" 00:00:00");
                }
                writer.Close();
            }
        }

        //今日の合計勉強時間を表示する
        //テキストファイルから、これまでの時間を取得する
        string total_time = GetCurrentSumTime();
        //★合計時間を表示する
        dayTextSecond.text = String.Format("{0:D2}", int.Parse(total_time.Split(':')[2]));  //秒
        dayTextMinute.text = String.Format("{0:D2}", int.Parse(total_time.Split(':')[1]));  //分
        dayTextHour.text = String.Format("{0:D2}", int.Parse(total_time.Split(':')[0]));  //時
    }

    // Update is called once per frame
    void Update() {
        //もし途中で日付をまたいだ時
        date_now = DateTime.Today.Date;
        if (date_now != date_prev) {
            //日付をまたぐ処理
            EndOfTheDay();
        }
        date_prev = date_now;

        //カウントフラグがオンの時のみカウントを有効にする
        if (isCounting) {
            time += Time.deltaTime;

            //秒の計算
            seconds = (int)Mathf.Floor(time) % 60;  //現在のカウント時間を60で割った余りが秒である

            //分の計算
            minutes = (int)Mathf.Floor(time) / 60;  //ここで分数を求める
            minutes %= 60;  //それを60で割った余りが分となる

            //時の計算
            hours = (int)Mathf.Floor(time) / 3600;  //3600で割った商が時間である
        }

        //時間の表示
        timerTextSecond.text = String.Format("{0:D2}", seconds);  //秒
        timerTextMinute.text = String.Format("{0:D2}", minutes);  //分
        timerTextHour.text = String.Format("{0:D2}", hours);  //時
        //ポップアップ用の時間
        popupTextSecond.text = String.Format("{0:D2}", seconds);  //秒
        popupTextMinute.text = String.Format("{0:D2}", minutes);  //分
        popupTextHour.text = String.Format("{0:D2}", hours);  //時

        //ポップアップの表示
        popup.SetActive(isPopup);
        popup_end.SetActive(isPopup_end);
    }

    //再生ボタンを押した
    public void PressStart() {
        //カウント停止中の時
        if (!isCounting) {
            //カウント再開する
            isCounting = true;
        }
    }

    //一時停止ボタンを押した
    public void PressTemp() {
        //カウント中の時
        if (isCounting) {
            //一時停止する
            isCounting = false;
        }
    }

    //停止ボタンを押した
    public void PressStop() {
        //カウント中か否かにかかわらず、カウントを停止する
        isCounting = false;

        //もし時間がゼロなら、リターン
        if (time == 0) {
            return;
        }

        //ポップアップを表示する
        isPopup = true;
    }

    //キャンセルボタンを押した
    public void PressCansel() {
        //ポップアップ関連の操作
        PressPopUp();
    }

    //OKボタンを押した
    public void PressOK() {
        //★これまでの時間を取得する
        string total_time = GetCurrentSumTime();
        //すべてのテキストデータを取得
        string all = GetAllText();

        //取得したこれまでの時間を秒に変換する
        string[] time_splits = total_time.Split(':');
        int time_passed = 3600 * int.Parse(time_splits[0]) + 60 * int.Parse(time_splits[1]) + int.Parse(time_splits[2]);

        //★合計時間をテキストファイルに記録する
        int time_sum = time_passed + (int)Mathf.Floor(time);
        //合計時間を、00:00:00の形で表記する
        //秒の計算
        int seconds_sum = (int)Mathf.Floor(time_sum) % 60;  //現在のカウント時間を60で割った余りが秒である
        //分の計算
        int minutes_sum = (int)Mathf.Floor(time_sum) / 60;  //ここで分数を求める
        minutes %= 60;  //それを60で割った余りが分となる
        //時の計算
        int hours_sum = (int)Mathf.Floor(time_sum) / 3600;  //3600で割った商が時間である
        //文字列置換
        all = all.Replace(DateTime.Today.ToString().Split(' ')[0] + ' ' + total_time, DateTime.Today.ToString().Split(' ')[0] + ' ' + hours_sum.ToString() + ':' + minutes_sum.ToString() + ':' + seconds_sum.ToString());

        StreamWriter writer = new StreamWriter(savePath, false);
        writer.Write(all);
        writer.Close();

        //ポップアップ関連の操作
        PressPopUp();

        //★合計時間を表示する
        dayTextSecond.text = String.Format("{0:D2}", seconds_sum);  //秒
        dayTextMinute.text = String.Format("{0:D2}", minutes_sum);  //分
        dayTextHour.text = String.Format("{0:D2}", hours_sum);  //時
    }

    void PressPopUp() {
        //ポップアップを隠す
        isPopup = false;
        isPopup_end = false;

        //時間をゼロにリセットする
        time = 0;
        hours = 0;
        minutes = 0;
        seconds = 0;
    }

    //日付が変わった時の処理
    void EndOfTheDay() {
        //ポップアップが出ていたらスルー
        if (popup) {
            return;
        }
        //カウント停止中ならスルー
        if (!isCounting) {
            return;
        }

        //カウントを停止する
        isCounting = false;
        //日付変更を知らせるポップアップを出す
        isPopup_end = true;
    }

    //これまでの合計時間を求める
    string GetCurrentSumTime() {
        FileStream fs_read = File.OpenRead(savePath);
        StreamReader reader = new StreamReader(fs_read);
        string total_time = null;
        while (true) {
            string read = reader.ReadLine();
            if (read == null) {
                break;
            }
            string[] splits = read.Split(' ');
            if (splits[0] == DateTime.Today.ToString().Split(' ')[0]) {
                total_time = splits[1];
            }
        }
        fs_read.Seek(0, SeekOrigin.Begin);  //読み込み位置初期化
        reader.Close();

        return total_time;
    }

    //テキストファイルの全文を求める
    string GetAllText() {
        FileStream fs_read = File.OpenRead(savePath);
        StreamReader reader = new StreamReader(fs_read);
        fs_read.Seek(0, SeekOrigin.Begin);  //読み込み位置初期化
        string all;
        all = reader.ReadToEnd();
        reader.Close();

        return all;
    }

    //Historyボタンを押した
    public void PressHistory() {
        //パネルを移動
        panels.transform.localPosition = new Vector3(1200.0f, 0.0f, 0.0f);
    }

    //Settingsボタンを押した
    public void PressSettings() {
        //パネルを移動
        panels.transform.localPosition = new Vector3(-1200.0f, 0.0f, 0.0f);
    }

    //ComingSoonボタンを押した
    public void PressComingSoon() {

    }

    //Backボタンを押した
    public void PressBack() {
        //パネルを移動
        panels.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }
}