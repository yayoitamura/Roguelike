using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    //Player
	//MovingObjectからの継承
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		//Levelごとの再開の遅延時間
		public int pointsPerFood = 10;				//Foodから得るポイント
		public int pointsPerSoda = 20;				//Sodaから得るポイント
		public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.それをチョッピングするときにプレイヤーが壁に与えるダメージ。
		public Text foodText;						//UI Text to display current player food total.
		public AudioClip moveSound1;				//Playerが動く時のAudio clips１
		public AudioClip moveSound2;				//Playerが動く時のAudio clips２
		public AudioClip eatSound1;					//food収集時のAudio clips１
		public AudioClip eatSound2;                 //food収集時のAudio clips２
		public AudioClip drinkSound1;               //soda収集時のAudio clips１
		public AudioClip drinkSound2;               //soda収集時のAudio clips２
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
		
		private Animator animator;					//animator componentを取得
		private int food;                           //foodの合計point

		//プリプロセッサ ディレクティブ
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
        //プライベートVector2 touchOrigin = -Vector2.one; //モバイルコントロールのスクリーンタッチ原点の場所を格納するために使用されます。
#endif


		//MovingObjectにオーバーライドする
		protected override void Start ()
		{
			//animatorの取得
			animator = GetComponent<Animator>();

			//Get the current food point total stored in GameManager.instance between levels.レベル間でGameManager.instanceに保存されている現在の食品ポイント合計を取得します。
			food = GameManager.instance.playerFoodPoints;

			//Set the foodText to reflect the current player food total.現在のプレーヤーの食べ物の合計を反映するように食物テキストを設定します。
			foodText.text = "Food: " + food;

			//Call the Start function of the MovingObject base class.MovingObject基本クラスのStart関数を呼び出します。
			base.Start ();
		}


		//This function is called when the behaviour becomes disabled or inactive.この関数は、ビヘイビアが無効または無効になったときに呼び出されます。
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
            //Playerオブジェクトが無効になっているときは、現在のローカルフードの合計をGameManagerに格納して、次のレベルに再ロードすることができます。
			GameManager.instance.playerFoodPoints = food;
		}
		
		
		private void Update ()
		{
			//If it's not the player's turn, exit the function.プレイヤーのターンでない場合は、その機能を終了します。
			if(!GameManager.instance.playersTurn) return;
			
			int horizontal = 0;     //Used to store the horizontal move direction.水平移動方向を格納するために使用します。
			int vertical = 0;       //Used to store the vertical move direction.垂直移動方向を格納するために使用します。

			//Check if we are running either in the Unity editor or in a standalone build.
            //Unityエディタまたはスタンドアロンビルドで実行しているかどうかを確認します。
#if UNITY_STANDALONE || UNITY_WEBPLAYER

			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            //入力マネージャから入力を取得し、整数に丸め、水平軸に格納してx軸の移動方向を設定します
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            //入力マネージャから入力を取得し、整数に丸め、垂直軸に格納してy軸の移動方向を設定します
			vertical = (int) (Input.GetAxisRaw ("Vertical"));

			//Check if moving horizontally, if so set vertical to zero.水平方向に移動するかどうかを確認し、垂直方向にゼロに設定する。
			if(horizontal != 0)
			{
				vertical = 0;
			}
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
			//私たちがiOS、Android、Windows Phone 8、またはUnity iPhoneで動作しているか確認してください
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			//Check if Input has registered more than zero touches入力がゼロ以上のタッチを登録しているかどうかを確認する
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.最初に検出されたタッチを保存します。
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Beganそのタッチの位相がBeganに等しいかどうかを確認する
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch そうであれば、touchOriginをそのタッチの位置に設定します
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
                //タッチ位相がBeganでなく、代わりにEndedに等しく、touchOriginのxがゼロ以上である場合：
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touchこのタッチの位置と等しくなるようにtouchEndを設定します
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.x軸のタッチの開始と終了の差を計算します。
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.タッチの開始と終了の差をy軸上で計算します。
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
                    //touchIrigin.xを-1に設定すると、else if文がfalseと評価され、直ちに繰り返されることはありません。
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
                   //x軸に沿った差がy軸に沿った差よりも大きいかどうか確認してください。
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1 xが0より大きい場合、horizontalを1に設定し、そうでない場合は-1に設定します
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1 yが0より大きい場合は、horizontalを1に設定し、そうでない場合は-1に設定します
						vertical = y > 0 ? 1 : -1;
				}
			}
			
#endif //End of mobile platform dependendent compilation section started above with #elif #elifで上で起動したモバイルプラットフォーム依存のコンパイルセクションの終わり
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
                //AttemptMoveはジェネリックパラメータWallを渡します。これはPlayerが攻撃を受けたときに対話することができるためです（
                //水平方向と垂直方向のパラメータを渡してPlayerを移動する方向を指定します）
				AttemptMove<Wall> (horizontal, vertical);
			}
		}
		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        //AttemptMoveは、基底クラスのAttemptMove関数をオーバーライドします。MovingObject AttemptMoveは、PlayerがWall型のジェネリックパラメータTをとります.x方向とy方向の整数も移動します。
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Every time player moves, subtract from food points total.プレイヤーが移動するたびに、合計食糧ポイントを引く。
			food--;

			//Update food text display to reflect current score.現在のスコアを反映するように食べ物のテキスト表示を更新します。
			foodText.text = "Food: " + food;
			
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            //基本クラスのAttemptMoveメソッドを呼び出し、コンポーネントT（この場合はWall）とx方向とy方向を移動して渡します。
			base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
            //ヒットは、我々が移動で行なわれたラインキャストの結果を参照することを可能にする。
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
            //Moveがtrueを返す場合、Playerは空の領域に移動できたことを意味します。
			if (Move (xDir, yDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                //SoundManagerのRandomizeSfxを呼び出して、2つのオーディオクリップを渡して移動音を再生します。
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//Since the player has moved and lost food points, check if the game has ended.
            //プレイヤーは移動して食糧を失ったので、ゲームが終了したかどうかを確認します。
			CheckIfGameOver ();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
            //プレイヤーが終了したら、GameManagerのplayersTurn booleanをfalseに設定します。
			GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        //OnCantMoveは、MovingObjectのOnCantMoveという抽象関数をオーバーライドします。 
        //それはプレーヤーが攻撃で破壊することができる壁である場合、一般的なパラメータTをとります。
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.パラメーターとして渡されたコンポーネントと等しくなるようにhitWallを設定します。
			Wall hitWall = component as Wall;

			//Call the DamageWall function of the Wall we are hitting.私たちが襲っている壁のDamageWall機能を呼び出します。
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            //プレイヤーの攻撃アニメーションを再生するために、プレイヤーのアニメーションコントローラーの攻撃トリガーを設定します。
			animator.SetTrigger ("playerChop");
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        //OnTriggerEnter2Dは、別のオブジェクトがこのオブジェクトに接続されたトリガーコライダーに入ると送信されます（2D物理のみ）。
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.衝突したトリガーのタグがExitかどうかを確認してください。
			if(other.tag == "Exit")
			{
				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                //restartLevelDelayの遅延（デフォルトは1秒）で次のレベルを開始するには、Restart関数を呼び出します。
				Invoke ("Restart", restartLevelDelay);
				
				//Disable the player object since level is over.
				enabled = false;
			}

			//Check if the tag of the trigger collided with is Food.衝突したトリガーのタグがFoodかどうかを確認します。
			else if(other.tag == "Food")
			{
				//Add pointsPerFood to the players current food total.選手の現在の食べ物の合計にpointsPerFoodを追加します。
				food += pointsPerFood;
				
				//Update foodText to represent current total and notify player that they gained points
                //foodTextを更新して現在の合計を表し、ポイントを得たことをプレーヤーに通知します
				foodText.text = "+" + pointsPerFood + " Food: " + food;
				
				//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                //SoundManagerのRandomizeSfx関数を呼び出し、2つの食べ物の音を渡して、食べるサウンドエフェクトを再生するかどうかを選択します。
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);

				//Disable the food object the player collided with.プレイヤーが衝突した食物を無効にする。
				other.gameObject.SetActive (false);
			}

			//Check if the tag of the trigger collided with is Soda.衝突したトリガーのタグがソーダかどうかを確認します。
			else if(other.tag == "Soda")
			{
                Debug.Log("soda");
				//Add pointsPerSoda to players food points total プレイヤーにポイントを追加する
				food += pointsPerSoda;
				
				//Update foodText to represent current total and notify player that they gained points
                //foodTextを更新して現在の合計を表し、ポイントを得たことをプレーヤーに通知します
				foodText.text = "+" + pointsPerSoda + " Food: " + food;
				
				//Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                //SoundManagerのRandomizeSfx関数を呼び出し、2つの飲酒音を渡して飲酒効果を再生するかどうかを選択します
				SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);

				//Disable the soda object the player collided with. プレーヤーが衝突したソーダオブジェクトを無効にする。
				other.gameObject.SetActive (false);
			}


            //enemy atack
			else if (other.tag == "Enemy")
            {
                Debug.Log("OnYrigger");
                Enemy hitEnemy = new Enemy();
				hitEnemy.EnemyDamagee();

            }


		}


		//Restart reloads the scene when called. 再起動すると、呼び出されるとシーンがリロードされます。
		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            //ロードされた最後のシーン、この場合Mainをゲームの唯一のシーンにロードします。 
            //また、 "Single"モードでロードして、既存のシーンオブジェクトを置き換え、現在のシーン内のすべてのシーンオブジェクトをロードしないようにします。
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}


		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		//LoseFoodは、敵がプレイヤーを攻撃するときに呼び出されます。
        //失うポイント数を指定するパラメータの損失が発生します。
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
            //playerHitアニメーションに遷移するように、プレーヤアニメータのトリガを設定します。
			animator.SetTrigger ("playerHit");

			//Subtract lost food points from the players total. プレイヤーの合計から失われた食べ物ポイントを差し引く。
			food -= loss;

			//Update the food display with the new total. フードディスプレイを新しい合計で更新します。
			foodText.text = "-"+ loss + " Food: " + food;

			//Check to see if game has ended. ゲームが終了したかどうかを確認してください。
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        //CheckIfGameOverは、プレイヤーが食糧ポイントを使い果たしているかどうかをチェックし、食べ物ポイントがない場合、ゲームを終了します。
		private void CheckIfGameOver ()
		{
			//Check if food point total is less than or equal to zero. フードポイント合計がゼロ以下であることを確認します。
			if (food <= 0) 
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                //SoundManagerのPlaySingle関数を呼び出し、再生するオーディオクリップとしてgameOverSoundを渡します。
				SoundManager.instance.PlaySingle (gameOverSound);

				//Stop the background music. バックグラウンドミュージックを停止します。
				SoundManager.instance.musicSource.Stop();

				//Call the GameOver function of GameManager. GameManagerのGameOver関数を呼び出します。
				GameManager.instance.GameOver ();
			}
		}
	}
}

