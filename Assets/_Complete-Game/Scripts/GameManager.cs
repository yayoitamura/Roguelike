using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;       //Allows us to use Lists. 
    using UnityEngine.UI;                   //Allows us to use UI.

    //GameManager
    public class GameManager : MonoBehaviour
    {
        

        public float levelStartDelay = 2f;                      //レベル開始前の待機時間
        public float turnDelay = 0.1f;                          //各プレイヤーのターンの間のディレイ。
        public int playerFoodPoints = 5;                        //プレーヤのゲーム開始時のfood points
        public static GameManager instance = null;              //他のスクリプトがアクセスできるようにするGameManagerの静的インスタンス。
                                                                //他のスクリプトがアクセスできるようにするGameManagerの静的インスタンス。
        [HideInInspector] public bool playersTurn = true;       //プレイヤーのターンかどうか、インスペクターではなくパブリックになっているかを調べます。


        private Text levelText;                                 //現在のレベル番号を表示するテキスト。
        private GameObject levelImage;                          //レベルが設定されているのでレベルをブロックする画像、levelTextの背景。
        private BoardManager boardScript;                       //レベルを設定するBoardManagerへの参照を保存する
        [System.NonSerialized] public int level = 1;            //現在のレベル番号　ゲーム中では「Day ○」
        private List<Enemy> enemies;                            //移動コマンドを発行するために使用されるすべての敵ユニットのリスト。
        private bool enemiesMoving;                             //敵が動いているかどうか
        private bool doingSetup = true;                         //ボードをセットアップしているかどうかを確認するブール値。セットアップ中にPlayerが移動しないようにします

        //
        void Awake()
        {
            //instanceの有無
            if (instance == null)

                //instanceをthisに設定
                instance = this;

            //thisでないinstanceがある場合
            else if (instance != this)

                //その後これを破壊する。 これにより、シングルトンのパターンが強制されます。つまり、GameManagerのインスタンスは1つしか存在できません。
                Destroy(gameObject);

            //scene切り替え時にobjectが破棄されない(シーンが切り替わっても設定が継続）
            DontDestroyOnLoad(gameObject);

            //敵を新しい敵のリストオブジェクトに割り当てます。
            enemies = new List<Enemy>();

            //BoardManagerスクリプトへのコンポーネント参照を取得
            boardScript = GetComponent<BoardManager>();

            //InitGame関数を呼び出して最初のレベルを初期化する
            InitGame();
        }

        //これは一度だけ呼び出され、パラメータはシーンのロード後に呼び出されるように指示します
        //（そうでなければ、私たちのScene Loadコールバックは最初のロードと呼ばれ、私たちはそれを望んでいません）
        //Startの前で自動的に呼ばれる
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //シーンがロードされるたびに呼び出されるコールバックを登録する
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //これはシーンがロードされるたびに呼び出されます。
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }


        //各レベルのゲームを初期化します。
        void InitGame()
        {
            //プレイ中にSETUPを実行することはできませんが、タイトルカードがアップしている間はプレイヤーが移動するのを防ぎます。
            doingSetup = true;

            //私たちの画像LevelImageへの参照を名前で見つけてください。
            levelImage = GameObject.Find("LevelImage");

            //LevelTextのテキストコンポーネントへの参照を、名前で検索してGetComponentを呼び出すことで取得します。
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //levelTextのテキストを文字列「Day」に設定し、現在のレベル番号を追加します。
            levelText.text = "Day " + level;

            //セットアップ中にlevelImageをゲームボードのアクティブブロッキングプレーヤーのビューに設定します。
            levelImage.SetActive(true);

            //levelStartDelay秒後に HideLevelImage関数を呼ぶ
            Invoke("HideLevelImage", levelStartDelay);

            //私たちのリストの敵のオブジェクトをクリアして次のレベルに備える。
            enemies.Clear();

            //BoardManagerスクリプトのSetupScene関数を呼び出し、現在のレベル番号を渡します。
            boardScript.SetupScene(level);

        }


        //レベル間で使用される黒い画像を非表示にする
        void HideLevelImage()
        {
            // 黒の背景画像gameObjectを無効にする。
            levelImage.SetActive(false);

            //doingSetupをfalseに設定すると、プレイヤーは再び移動できます。
            doingSetup = false;
        }

        void Update()
        {
            //プレイヤーがターンまたは敵であることを確認してください。
            if (playersTurn || enemiesMoving || doingSetup)
            {
                //これらのいずれかが当てはまる場合、戻って、MoveEnemiesを開始しないでください。
                return;
            }

            //Start moving enemies.
            StartCoroutine("MoveEnemies");
        }

        //これを呼び出して、渡された敵を敵のリストオブジェクトに追加します。
        public void AddEnemyToList(Enemy script)
        {
            //敵をリストに追加する。
            enemies.Add(script);
        }


        //foodpointが0になるとPlayerのCheckIfGameOverで呼ばれる
        public void GameOver()
        {
            //GameOver時のMessage
            levelText.text = "Game Over " + level + " days";

            //黒の背景画像gameObjectを有効にする。
            levelImage.SetActive(true);

            //Disable this GameManager.このGameManagerを無効にする
            enabled = false;
        }

        //コルーチンは順番に敵を動かす。
        IEnumerator MoveEnemies()
        {
            //enemiesMovingが真のプレイヤーは移動できませんが、移動することはできません。
            enemiesMoving = true;

            //turnDelay秒待機します。デフォルトは0.1（100 ms）です。
            yield return new WaitForSeconds(turnDelay);

            //敵が生まれていない場合（IE in first level)(第1レベルのIE）：
            if (enemies.Count == 0)
            {
                //移動間のターン遅延秒間待機し、敵が移動していない場合に移動する遅延を置き換えます。
                yield return new WaitForSeconds(turnDelay);
            }

            //Enemyオブジェクトのリストをループします。
            for (int i = 0; i < enemies.Count; i++)
            {
                //敵リストのインデックスiの敵のMoveEnemy関数を呼び出します。
                enemies[i].MoveEnemy();

                //次の敵に移動する前に敵のmoveTimeを待ちます。
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            //敵が移動したら、playersTurnをtrueに設定してプレーヤーが移動できるようにします。
            playersTurn = true;

            //敵は動かされ、敵は偽に変わる。
            enemiesMoving = false;

        }
    }
}

