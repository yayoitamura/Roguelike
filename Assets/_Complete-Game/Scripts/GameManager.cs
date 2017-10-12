using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
    //GameManager
	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;                      //レベル開始前の待機時間
		public float turnDelay = 0.1f;                          //Delay between each Player turn.各プレイヤーのターンの間のディレイ。
		public int playerFoodPoints = 5;                      //プレーヤのゲーム開始時のfood points
		public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.他のスクリプトがアクセスできるようにするGameManagerの静的インスタンス。
																//他のスクリプトがアクセスできるようにするGameManagerの静的インスタンス。
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
                                                                //ブール値で、プレイヤーが回っているか、インスペクターではなくパブリックになっているかを調べます。
		
		
		private Text levelText;                                 //.現在のレベル番号を表示するテキスト。
		private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.レベルが設定されているのでレベルをブロックする画像、levelTextの背景。
		private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.レベルを設定するBoardManagerへの参照を保存する
		private int level = 1;                                  //現在のレベル番号　ゲーム中では「Day ○」
		private List<Enemy> enemies;                            //List of all Enemy units, used to issue them move commands.移動コマンドを発行するために使用されるすべての敵ユニットのリスト。
		private bool enemiesMoving;                             //敵が動いているかどうか
		private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.
																//ボードをセットアップしているかどうかを確認するブール値。セットアップ中にPlayerが移動しないようにします。



		//
		void Awake()
		{
			//instanceの有無
			if (instance == null)

				//instanceをthisに設定
				instance = this;

			//thisでないinstanceがある場合
			else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                //その後これを破壊する。 これにより、シングルトンのパターンが強制されます。つまり、GameManagerのインスタンスは1つしか存在できません。
                Destroy(gameObject);

			//scene切り替え時にobjectが破棄されない(シーンが切り替わっても設定が継続）
			DontDestroyOnLoad(gameObject);

			//Assign enemies to a new List of Enemy objects. 敵を新しい敵のリストオブジェクトに割り当てます。
			enemies = new List<Enemy>();

			//BoardManagerスクリプトへのコンポーネント参照を取得
			boardScript = GetComponent<BoardManager>();

			//InitGame関数を呼び出して最初のレベルを初期化する
			InitGame();
		}

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        //これは一度だけ呼び出され、パラメータはシーンのロード後に呼び出されるように指示します
        //（そうでなければ、私たちのScene Loadコールバックは最初のロードと呼ばれ、私たちはそれを望んでいません）
        //Startの前で自動的に呼ばれる
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
			//register the callback to be called everytime the scene is loaded シーンがロードされるたびに呼び出されるコールバックを登録する
			SceneManager.sceneLoaded += OnSceneLoaded;
        }

		//This is called each time a scene is loaded. これはシーンがロードされるたびに呼び出されます。
		static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++; 
            instance.InitGame();
        }


		//Initializes the game for each level. 各レベルのゲームを初期化します。
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
            //プレイ中にSETUPを実行することはできませんが、タイトルカードがアップしている間はプレイヤーが移動するのを防ぎます。
			doingSetup = true;

			//Get a reference to our image LevelImage by finding it by name.私たちの画像LevelImageへの参照を名前で見つけてください。
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            //LevelTextのテキストコンポーネントへの参照を、名前で検索してGetComponentを呼び出すことで取得します。
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
            //levelTextのテキストを文字列「Day」に設定し、現在のレベル番号を追加します。
			levelText.text = "Day " + level;

			//Set levelImage to active blocking player's view of the game board during setup.セットアップ中にlevelImageをゲームボードのアクティブブロッキングプレーヤーのビューに設定します。
			levelImage.SetActive(true);
			
			//levelStartDelay秒後に HideLevelImage関数を呼ぶ
			Invoke("HideLevelImage", levelStartDelay);

			//Clear any Enemy objects in our List to prepare for next level.私たちのリストの敵のオブジェクトをクリアして次のレベルに備える。
			enemies.Clear();

			//BoardManagerスクリプトのSetupScene関数を呼び出し、現在のレベル番号を渡します。
			boardScript.SetupScene(level);
			
		}


		//Hides black image used between levels レベル間で使用される黒い画像を非表示にする
		void HideLevelImage()
		{
			// 黒の背景画像gameObjectを無効にする。
			levelImage.SetActive(false);

			//doingSetupをfalseに設定すると、プレイヤーは再び移動できます。
			doingSetup = false;
		}

		//Update is called every frame. 更新はすべてのフレームと呼ばれます。
		void Update()
		{
			//Check that playersTurn or enemiesMoving or doingSetup are not currently true.プレイヤーがターンまたは敵であることを確認してください。
			if(playersTurn || enemiesMoving || doingSetup)

				//If any of these are true, return and do not start MoveEnemies.これらのいずれかが当てはまる場合、戻って、MoveEnemiesを開始しないでください。
				return;
			
			//Start moving enemies.
			StartCoroutine (MoveEnemies ());
		}
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
        //これを呼び出して、渡された敵を敵のリストオブジェクトに追加します。
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.敵をリストに追加する。
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

		//Coroutine to move enemies in sequence. コルーチンは順番に敵を動かす。
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move. enemiesMovingが真のプレイヤーは移動できませんが、移動することはできません。
			enemiesMoving = true;

			//Wait for turnDelay seconds, defaults to .1 (100 ms). turnDelay秒待機します。デフォルトは0.1（100 ms）です。
			yield return new WaitForSeconds(turnDelay);

			//If there are no enemies spawned (IE in first level): 敵が生まれていない場合（第1レベルのIE）：
			if (enemies.Count == 0) 
			{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                //移動間のターン遅延秒間待機し、敵が移動していない場合に移動する遅延を置き換えます。
				yield return new WaitForSeconds(turnDelay);
			}

			//Loop through List of Enemy objects. Enemyオブジェクトのリストをループします。
			for (int i = 0; i < enemies.Count; i++)
			{
				//Call the MoveEnemy function of Enemy at index i in the enemies List.
                //敵リストのインデックスiの敵のMoveEnemy関数を呼び出します。
				enemies[i].MoveEnemy ();

				//Wait for Enemy's moveTime before moving next Enemy,  次の敵に移動する前に敵のmoveTimeを待ちます。
				yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Once Enemies are done moving, set playersTurn to true so player can move. 敵が移動したら、playersTurnをtrueに設定してプレーヤーが移動できるようにします。
			playersTurn = true;

			//Enemies are done moving, set enemiesMoving to false. 敵は動かされ、敵は偽に変わる。
			enemiesMoving = false;
		}
	}
}

