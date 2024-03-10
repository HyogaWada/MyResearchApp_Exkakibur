using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;
using TMPro;
[RequireComponent(typeof(LineRenderer))]
public class Stroke : MonoBehaviour{
    //線を消すためにpublicとして設定
    public GameObject lineObj;
    //線の材質
    [SerializeField] Material lineMaterial;
    //線の色
    [SerializeField] Color lineColor;
    //線の太さ
    [Range(0.01f, 0.50f)]
    [SerializeField] float lineWidth;
    //追加　LineRdenerer型のリスト宣言List<LineRenderer> lineRenderers;
    public List<LineRenderer> lineRenderers;
    //総移動距離の元になる微小距離を入れる配列の追加:型はVector2
    List<Vector2> listMinDis = new List<Vector2>(); //Vector2型のListを定義
    float AAA;//確認用
    //List<float> AAA = new List<float>(); //float型のListを定義
    Vector2 start_position;
    Vector2 finish_position;
    Vector2 change_position;// finish_position - start_position
    Vector2 middle_position;//中央座標
    float directDistance;//絶対値化した直線距離
    float totalDistance;//1ストロークでの総筆記距離
    float changeAngle;//角度変化
    float pastAngle;
    int listNum = 0;//配列番号を格納する変数
    
    AudioSource audioSource;//効果音に必要な定義
    public AudioClip sound_point;
    public AudioClip sound_straight;
    public AudioClip sound_curve;

    //11月10日に追加
    //点エフェクトに必要
    private Vector3 mpHit;
    private Vector3 opHit;
    [SerializeField] GameObject particle;

    //曲線エフェクトに必要
    private Vector3 mpCurve;
    private Vector3 opCurve;
    [SerializeField] GameObject curveparticle;

    //直線エフェクトに必要
    private Vector3 mpStraight;
    private Vector3 opStraight;
    [SerializeField] GameObject straightparticle;
    //GameObject straightparticle;
    float straightAngle;//直線の角度を格納する変数
    
    [SerializeField] private Gradient gradient;
    public int numCapVertices = 0;
    private Vector3 curvePosition;

    //ボタン押した際の時間とファイル名
    DateTime dt;
    String filename;

    //時間計測用
    private int playMinute;//計測時間(分)
    private float playSecond;//計測時間(秒)
    private float oldSecond;//前のUpdate時の秒数
    DateTime dtSS;
    String filenameSS;
    int buttonCount = 0;//ボタン押した回数

    //1011
    List<LineRenderer> blueLineRenderers = new List<LineRenderer>();
    Queue<Vector3> blueLinePositions = new Queue<Vector3>();//新しい位置データのキュー
    //float delayTime = 0.07f;//遅延時間（秒）
    //曲線の材質
    [SerializeField] Material curveMaterial;
    //曲線エフェクトの太さカーブを宣言
    [SerializeField] private AnimationCurve curve;
    public Color curveColor = Color.white;//曲線の色
    //1013
    int countOnOff=0;
    public TextMeshProUGUI buttonOnOffText;
    
    //STARRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRT
    void Start(){//スタート関数
        //Debug.Log("START");
        buttonCount = 0;//ボタン押した回数
        totalDistance = 0;//一番最初に初期化
        audioSource = GetComponent<AudioSource>();//効果音のためのComponentを取得
        middle_position.x = 1920;
        middle_position.y = 540;
        //追加　Listの初期化 VERY IMPORTANT
        lineRenderers = new List<LineRenderer>();
        //経過時間のための変数
        playMinute = 0;
        playSecond = 0f;
        oldSecond = 0f;
    }

    void Update(){
        playSecond += Time.deltaTime;
        if(playSecond >= 60f){
            playMinute++;
            playSecond = playSecond - 60;
            dtSS = DateTime.Now;
            filenameSS = playMinute.ToString() + dtSS.ToString("yyyyMMddHHmmss") + ".png";
        }
        oldSecond = playSecond;
        if (Input.GetMouseButtonDown(0)){//クリックをしたタイミング
            //lineObjを生成し、初期化する
            _addLineObject();
            start_position = Input.mousePosition;//始点：マウス座標の取得
        }
        //クリック中（ストローク中）
         if (Input.GetMouseButton(0)){
            listMinDis.Add((Input.mousePosition));//配列listMinDisにクリック中のマウス座標をリアルタイムで入れている
            _addPositionDataToLineRendererList();
            listNum = listMinDis.Count-1;
            //Debug.Log(listMinDis[listMinDis.Count - 1]);//1フレームごとの座標：入っていました！！
           // Debug.Log(listMinDis.Count);//リストの総フレーム数を1から数えている：できました
            if(listNum % 4 == 0 && listNum  != 0){//条件：配列番号が0以外かつ、4の倍数の時
                if(listNum>=40 && Mathf.Abs(listMinDis[listNum].x-listMinDis[listNum-8].x) < 10 && Mathf.Abs(listMinDis[listNum].y-listMinDis[listNum-8].y) < 10
                && Mathf.Abs(listMinDis[listNum].x-listMinDis[listNum-8].x) != 0 && Mathf.Abs(listMinDis[listNum].y-listMinDis[listNum-8].y) != 0){
                    //Debug.Log("ストローク分解ぽよ");
                    finish_position = Input.mousePosition;//ストローク終点：マウス座標の取得
                    change_position = finish_position-start_position;//始点から終点までの座標変遷
                    directDistance = change_position.magnitude;
                    
                    //ストローク総筆記距離
                    for(int i = 0;i < listMinDis.Count - 1;i++){
                    totalDistance = totalDistance + (listMinDis[i + 1] - listMinDis[i]).magnitude;
                    }
                    //Debug.Log("このストロークの始点から終点までの直線距離：" + directDistance);
                    //Debug.Log("このストロークの総筆記距離：" + totalDistance);
                    
                    if(totalDistance > 0 && totalDistance <= 100){//いったん総筆記距離が100以下の時に点と判断する
                        //Debug.Log("2以降の打点");

                    }else{
                        if(totalDistance / directDistance >= 1.08){//直線と曲線の判定
                        if(countOnOff%2 == 0){
                            //Debug.Log("曲線");
                            //StopCoroutine(DrawDelayedBlueLine());
                            if (DrawDelayedBlueLine() != null){
                                StopCoroutine(DrawDelayedBlueLine());
                            }
                            audioSource.PlayOneShot(sound_curve);
                            StartCoroutine(DrawDelayedBlueLine());
                        }
                        }else{
                            if(countOnOff%2 == 0){
                            //Debug.Log("直線");
                            //1012
                            Quaternion rotation;
                            audioSource.PlayOneShot(sound_straight);
                            //11月10日に追記
                            mpStraight = Input.mousePosition;
                            opStraight = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            if(change_position.y > 0){
                                rotation = Quaternion.Euler(0f, 0f, Vector2.Angle(Vector2.right, change_position));
                            }else{
                                rotation = Quaternion.Euler(0f, 0f, 360f - Vector2.Angle(Vector2.right, change_position));
                            }
                            GameObject sP = Instantiate(straightparticle, opStraight, rotation);
                            //スケールをtotalDistanceに設定
                            Vector3 scale = sP.transform.localScale;
                            scale.x = totalDistance;  // 回転方向
                            scale.y = totalDistance/5;//エフェクトの長さ
                            sP.transform.localScale = scale/100;
                            }
                        }
                    }
                    listMinDis.Clear();
                    //次のストロークのために、再度初期化を
                    start_position = Input.mousePosition;//ストローク終点：マウス座標の取得
                    listMinDis.Add((Input.mousePosition));//配列listMinDisにクリック中のマウス座標をリアルタイムで入れている
                    totalDistance = 0;//一番最後に総筆記距離を初期化
                }
            }
        }
        //追加 クリックを離したタイミング
        if (Input.GetMouseButtonUp(0)){
            finish_position = Input.mousePosition;//終点：マウス座標の取得
            //Debug.Log("Last of list" + listMinDis[listMinDis.Count - 1]);//弘大先生ありがとうございます
            change_position = finish_position-start_position;//始点から終点までの座標変遷
            directDistance = change_position.magnitude;//.magniitudeで(ベクトルの)直線距離がわかる            
            //総筆記距離
            for(int i = 0;i < listMinDis.Count - 1;i++){
                totalDistance = totalDistance + (listMinDis[i + 1] - listMinDis[i]).magnitude;
            }
            //お試しすぐ消す右92~96をコメントアウト外す
            for(int j = 0;j < (listMinDis.Count - 19) / 20;j++){
                AAA = Vector2.Angle(listMinDis[j * 20], listMinDis[j * 20 + 19]);
            }
            if(directDistance != 0){
            }
            if(totalDistance <= 100){//いったん総筆記距離が100以下の時に点と判断する
            if(countOnOff%2 == 0){
                //Debug.Log("点");
                //Debug.Log("FIRST POINT!!!");
                audioSource.PlayOneShot(sound_point);
                        mpHit = Input.mousePosition;
                        //ここにz座標を設定
                        mpHit.z = 3.0f;
                        opHit = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Instantiate(particle, opHit, Quaternion.identity);
            }       
            }else{
                if(totalDistance / directDistance >= 1.08){//直線と曲線の判定
                if(countOnOff%2 == 0){
                //Debug.Log("曲線");
                //StopCoroutine(DrawDelayedBlueLine());
                if (DrawDelayedBlueLine() != null){
                    StopCoroutine(DrawDelayedBlueLine());
                }
                audioSource.PlayOneShot(sound_curve);
                StartCoroutine(DrawDelayedBlueLine());
                }    
                }else{
                    if(countOnOff%2 == 0){
                    //Debug.Log("直線");
                    //1012
                    Quaternion rotation;
                    audioSource.PlayOneShot(sound_straight);
                    //11月10日に追記
                    mpStraight = Input.mousePosition;
                    opStraight = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if(change_position.y > 0){
                        rotation = Quaternion.Euler(0f, 0f, Vector2.Angle(Vector2.right, change_position));
                    }else{
                        rotation = Quaternion.Euler(0f, 0f, 360f - Vector2.Angle(Vector2.right, change_position));
                    }
                    GameObject sP = Instantiate(straightparticle, opStraight, rotation);
                    // スケールをtotalDistanceに設定
                    Vector3 scale = sP.transform.localScale;
                    scale.x = totalDistance;// 回転方向
                    scale.y = totalDistance/5;//エフェクトの長さ
                    sP.transform.localScale = scale/100;
                    }
                }
            }
            //クリックを話した瞬間にリストを初期化
            listMinDis.Clear();
            totalDistance = 0;//一番最後に総筆記距離を初期化
            }
        //総距離の長さ(192ppi, 56pixel)で点か線かを判別・二つの方向ベクトルの角度が51度以上の時にストロークを分解        
    }

    public void OnClick(){//ボタン押された際の処理
        dt = DateTime.Now;
        filename = dt.ToString("yyyyMMddHHmmss") + ".png";
        //Debug.Log(filename + "が保存されました。");
        buttonCount++;
        //これでストロークと5つの文字gameObjectを消せる
        foreach(Transform child in gameObject.transform){
            Destroy(child.gameObject);
        }
        lineRenderers.Clear();
    }

    public void OnOffChanger(){//切り替えボタン押された際の処理
        countOnOff++;
        if (countOnOff % 2　== 1){
            //Debug.Log("OFF");
            buttonOnOffText.text = "OFF";
        }else{
            //Debug.Log("ON");
            buttonOnOffText.text = "ON";
        }
    }

    public void CaptureScreenShot(string filePath){
        ScreenCapture.CaptureScreenshot(filePath);
    }

    //追加　クリックしたら発動
    void _addLineObject(){
        //空のゲームオブジェクト作成
        GameObject lineObj = new GameObject();
        //オブジェクトの名前をStrokeに変更
        lineObj.name = "Stroke";
        //lineObjにLineRendereコンポーネント追加
        lineObj.AddComponent<LineRenderer>();
        //lineRendererリストにlineObjを追加
        lineRenderers.Add(lineObj.GetComponent<LineRenderer>());
        //lineObjを自身の子要素に設定
        lineObj.transform.SetParent(transform);
        //lineObj初期化処理
        _initRenderers();
    }

    //lineObj初期化処理
    void _initRenderers(){
        //線をつなぐ点を0に初期化
        lineRenderers.Last().positionCount = 0;
        //マテリアルを初期化
        lineRenderers.Last().material = lineMaterial;
        //色の初期化
        lineRenderers.Last().material.color = lineColor;
        //太さの初期化
        lineRenderers.Last().startWidth = lineWidth;
        lineRenderers.Last().endWidth = lineWidth;
    }

    void _addPositionDataToLineRendererList(){
        //マウスポインタがあるスクリーン座標を取得
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f );
        //スクリーン座標をワールド座標に変換
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint( mousePosition);        
        //ワールド座標をローカル座標に変換
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition.x, worldPosition.y, -1.0f);
        //lineRenderersの最後のlineObjのローカルポジションを上記のローカルポジションに設定
        lineRenderers.Last().transform.localPosition = localPosition;        
        //lineObjの線と線をつなぐ点の数を更新
        lineRenderers.Last().positionCount += 1;
        //LineRendererコンポーネントリストを更新
        lineRenderers.Last().SetPosition(lineRenderers.Last().positionCount - 1, worldPosition);
        //あとから描いた線が上に来るように調整
        lineRenderers.Last().sortingOrder = lineRenderers.Count;
    }

    IEnumerator DrawDelayedBlueLine(){
        //遅延
        yield return new WaitForSeconds(0.07f);
        //新たな水色のLineRendererオブジェクトを作成
        GameObject blueLineObj = new GameObject();
        blueLineObj.name = "CurveEffect";
        blueLineObj.AddComponent<LineRenderer>();
        LineRenderer blueLineRenderer = blueLineObj.GetComponent<LineRenderer>();
        blueLineRenderers.Add(blueLineRenderer);

        blueLineObj.transform.SetParent(transform);
        //初期設定
        blueLineRenderer.positionCount = 0;
        blueLineRenderer.material = curveMaterial;//曲線の素材
        blueLineRenderer.material.color = curveColor; //色
        blueLineRenderer.startWidth = lineWidth;
        blueLineRenderer.widthCurve = curve;
        blueLineRenderer.endWidth = lineWidth;
        //黒い線のポジションデータをコピー
        LineRenderer lastBlackLine = lineRenderers.Last();
        for(int i = 0; i < lastBlackLine.positionCount; i++) {
            blueLineRenderer.positionCount++;
            blueLineRenderer.SetPosition(i, lastBlackLine.GetPosition(i));
            //追記
            yield return new WaitForSeconds(0.0005f);
            //Destroy(blueLineRenderer.gameObject);
        }
        // 再び短い待機時間（float x秒）
        yield return new WaitForSeconds(0.05f);
        // カーブエフェクトを破棄
        Destroy(blueLineRenderer.gameObject);
    }
}
