using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy
    //MovingObjectから継承
	public class Enemy : MovingObject
	{
		public int playerDamage; 							//Playerへのダメージ
		public AudioClip attackSound1;                      //Playeを攻撃する時のオーディオクリッップ１
		public AudioClip attackSound2;                      //Playeを攻撃する時のオーディオクリッップ２


		private Animator animator;                          //Animatorコンポーネントへの参照を格納
		private Transform target;                           //Transform to attempt to move toward each turn.各ターンに向かって移動しようとするために変形する。
		private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.敵がターンをスキップするか、このターンを移動するかどうかを決定するブール値。


        //MovingObjectのStart関数に上乗せ（継承元と同じ（引数の型・数の）メソッドを自分のクラス独自のもので置き換えられる）
		protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			//この敵をGameManagerのインスタンスに登録するには、Enemyオブジェクトのリストに追加します。
			//これにより、GameManagerは移動コマンドを発行できます。
			GameManager.instance.AddEnemyToList (this);

			//Animatorコンポーネントへの参照を取得
			animator = GetComponent<Animator> ();
			
			//Find the Player GameObject using it's tag and store a reference to its transform component.
            //PlayerのGameObjectをそのタグで検索し、その変換コンポーネントへの参照を格納します。
            //Player GameObjectを検索してそのtransformを取得
			target = GameObject.FindGameObjectWithTag ("Player").transform;

            //base = 基本クラスのMovingObject MovingObject.Start();
			base.Start ();
		}


		//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		//MovingObjectのAttemptMove関数をオーバーライドして、Enemyがターンをスキップするために必要な機能を組み込みます。
        //AttemptMoveの基底関数がどのように機能するかについては、MovingObjectのコメントを参照してください。
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Check if skipMove is true, if so set it to false and skip this turn.
            //skipMoveがtrueであるかどうかをチェックし、falseに設定してこのターンをスキップする。
			if(skipMove)
			{
				skipMove = false;
				return;
				
			}
			
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);
			
			//Now that Enemy has moved, set skipMove to true to skip next move.
            //Enemyが移動したので、次の移動をスキップするためにskipMoveをtrueに設定します。
			skipMove = true;
		}


		//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
		//MoveEnemyは各ターンにGameMangerから呼び出され、各敵にプレイヤーに向かって移動しようとします。
		public void MoveEnemy ()
		{
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			//X軸とY軸の移動方向の変数を宣言します。範囲は - 1〜1です。
            //これらの値を使用すると、上、下、左、右のような基本的な方向を選択できます。
			int xDir = 0;
			int yDir = 0;
			
			//If the difference in positions is approximately zero (Epsilon) do the following:
            //位置の差がほぼゼロ（ε）の場合は、次のようにします。
			if(Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
				
				//If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                //ターゲットの（プレイヤー）位置のy座標がこの敵の位置のy座標よりも大きい場合（上に移動するには）y方向1に設定します。 そうでない場合は - 1に設定します（下に移動するには）。
                yDir = target.position.y > transform.position.y ? 1 : -1;

            //if (target.position.y > transform.position.y){
            //    yDir = 1;
            //}else{
            //    yDir = -1;
            //}
			
			//If the difference in positions is not approximately zero (Epsilon) do the following:
            //位置の違いがほぼゼロ（ε）でない場合は、次の操作を行います。
			else
				//Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                //ターゲットxの位置が敵のxの位置よりも大きいかどうかを確認します。x方向を - 1に設定する場合は右に移動し、-1に設定しない場合は左に移動します。
				xDir = target.position.x > transform.position.x ? 1 : -1;
			
			//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            //Attememove関数を呼び出し、汎用パラメータPlayerを渡します。これは、敵が移動している可能性があり、Playerに遭遇する可能性があるためです
			AttemptMove <Player> (xDir, yDir);
		}
		
		
		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        //OnCantMoveは、EnemyがPlayerが占めるスペースに移動しようとすると呼び出され、
        //MovingObjectのOnCantMove関数をオーバーライドし、遭遇すると予想されるコンポーネント（この場合Player）を渡すために使用するジェネリックパラメータTをとります
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
            //hitPlayerを宣言し、遭遇したコンポーネントと等しくなるように設定します
			Player hitPlayer = component as Player;
		
            //hitPlayerのLoseFood関数を呼び出し、playerDamageを渡す
			hitPlayer.LoseFood (playerDamage);
			
            //エネミーの攻撃アニメーションのトリガー
			animator.SetTrigger ("enemyAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
            //SoundManagerのRandomizeSfx関数を呼び出し、ランダムに再生
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
