using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour {
    //ブール値系
    bool isCounting = true;  //計測が開始しているか
    bool isPopup = false;  //ポップアップが表示されているか

    //表示用テキスト
    [SerializeField] Text timerTextHour;  //時
    [SerializeField] Text timerTextMinute;  //分
    [SerializeField] Text timerTextSecond;  //秒

    //ポップアップ時の表示用テキスト
    [SerializeField] Text popupTextHour;  //時
    [SerializeField] Text popupTextMinute;  //分
    [SerializeField] Text popupTextSecond;  //秒

    //時間カウント用
    int hours;  //時
    int minutes;  //分
    int seconds;  //秒
    float time;  //現在のカウント時間（秒）

    //ゲームオブジェクト系
    [SerializeField] GameObject popup;  //ポップアップオブジェクト

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
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

        //ポップアップを表示する
        isPopup = true;
    }

    //キャンセルボタンを押した
    public void PressCansel() {

    }

    //OKボタンを押した
    public void PressOK() {

    }
}