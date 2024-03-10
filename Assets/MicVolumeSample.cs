using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicVolumeSample : MonoBehaviour
{
    private readonly int SampleNum = (2 << 9); // サンプリング数は2のN乗(N=5-12)
    [SerializeField, Range(0f, 1000f)] float m_gain = 200f; // 倍率
    AudioSource m_source;
    float[] currentValues;

    void Start () {
        m_source = GetComponent<AudioSource>();
        currentValues = new float[SampleNum];
        if ((m_source != null) && (Microphone.devices.Length > 0)) // オーディオソースとマイクがある
        {
            string devName = Microphone.devices[0]; // 複数見つかってもとりあえず0番目のマイクを使用
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(devName, out minFreq, out maxFreq); // 最大最小サンプリング数を得る
            int ms = minFreq / SampleNum; // サンプリング時間を適切に取る
            m_source.loop = true; // ループにする
            m_source.clip = Microphone.Start(devName, true, ms, minFreq); // clipをマイクに設定
            while (!(Microphone.GetPosition(devName) > 0)) { } // きちんと値をとるために待つ
            Microphone.GetPosition(null);
            m_source.Play();
        }
    }

    // Update is called once per frame
    void Update () {
        m_source.GetSpectrumData(currentValues, 0, FFTWindow.Hamming);
        float sum = 0f;
        for (int i = 0; i < currentValues.Length; ++i)
        {
            sum += currentValues[i]; // データ（周波数帯ごとのパワー）を足す
        }
        // データ数で割ったものに倍率をかけて音量とする
        float volumeRate = Mathf.Clamp01(sum * m_gain / (float)currentValues.Length);
        //Debug.Log(volumeRate); //マイクのボリュームをコンソールに表示0~1
    }
}
