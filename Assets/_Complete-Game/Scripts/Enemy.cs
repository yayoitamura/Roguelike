using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy
    //MovingObjectから継承
    public class Enemy : MovingObject
    {
        public int playerDamage;                            //Playerへのダメージ
        public AudioClip attackSound1;                      //Playeを攻撃する時のオーディオクリッップ１
        public AudioClip attackSound2;                      //Playeを攻撃する時のオーディオクリッップ２
        public int hp;
        public bool enemyDead = false;

        private Animator animator;                          //Animatorコンポーネントへの参照を格納
        private Transform target;                           //各ターンに向かって移動しようとするために変形する。
        private bool skipMove;                              //敵がターンをスキップするか、このターンを移動するかどうかを決定するブール値。

        //MovingObjectのStart関数に上乗せ（継承元と同じ（引数の型・数の）メソッドを自分のクラス独自のもので置き換えられる）
        protected override void Start()
        {
            //この敵をGameManagerのインスタンスに登録するには、Enemyオブジェクトのリストに追加します。
            //これにより、GameManagerは移動コマンドを発行できます。
            GameManager.instance.AddEnemyToList(this);

            //Animatorコンポーネントへの参照を取得
            animator = GetComponent<Animator>();

            //PlayerのGameObjectをそのタグで検索し、その変換コンポーネントへの参照を格納します。
            //Player GameObjectを検索してそのtransformを取得
            target = GameObject.FindGameObjectWithTag("Player").transform;

            //base = 基本クラスのMovingObject MovingObject.Start();
            base.Start();
        }

        //MovingObjectのAttemptMove関数をオーバーライドして、Enemyがターンをスキップするために必要な機能を組み込みます。
        //AttemptMoveの基底関数がどのように機能するかについては、MovingObjectのコメントを参照してください。
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //skipMoveがtrueであるかどうかをチェックし、falseに設定してこのターンをスキップする。
            if (skipMove)
            {
                skipMove = false;
                return;

            }

            //Call the AttemptMove function from MovingObject.
            base.AttemptMove<T>(xDir, yDir);

            //Enemyが移動したので、次の移動をスキップするためにskipMoveをtrueに設定します。
            skipMove = true;
        }

        //MoveEnemyは各ターンにGameMangerから呼び出され、各敵にプレイヤーに向かって移動しようとします。
        public void MoveEnemy()
        {
            if (enemyDead) return;
            //X軸とY軸の移動方向の変数を宣言します。範囲は - 1〜1です。
            //これらの値を使用すると、上、下、左、右のような基本的な方向を選択できます。
            int xDir = 0;
            int yDir = 0;

            //位置の差がほぼゼロ（ε）の場合は、次のようにします。
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
                
                //ターゲットの（プレイヤー）位置のy座標がこの敵の位置のy座標よりも大きい場合（上に移動するには）y方向1に設定します。 そうでない場合は - 1に設定します（下に移動するには）。
                yDir = target.position.y > transform.position.y ? 1 : -1;

            //if (target.position.y > transform.position.y){
            //    yDir = 1;
            //}else{
            //    yDir = -1;
            //}

            //位置の違いがほぼゼロ（ε）でない場合は、次の操作を行います。
            else
                //ターゲットxの位置が敵のxの位置よりも大きいかどうかを確認します。x方向を - 1に設定する場合は右に移動し、-1に設定しない場合は左に移動します。
                xDir = target.position.x > transform.position.x ? 1 : -1;

            //Attememove関数を呼び出し、汎用パラメータPlayerを渡します。これは、敵が移動している可能性があり、Playerに遭遇する可能性があるためです
            AttemptMove<Player>(xDir, yDir);
        }

        //OnCantMoveは、EnemyがPlayerが占めるスペースに移動しようとすると呼び出され、
        //MovingObjectのOnCantMove関数をオーバーライドし、遭遇すると予想されるコンポーネント（この場合Player）を渡すために使用するジェネリックパラメータTをとります
        protected override void OnCantMove<T>(T component) //Playerへのダメージ
        {
            //hitPlayerを宣言し、遭遇したコンポーネントと等しくなるように設定します
            Player hitPlayer = component as Player;

            //hitPlayerのLoseFood関数を呼び出し、playerDamageを渡す
            hitPlayer.LoseFood(playerDamage);

            //エネミーの攻撃アニメーションのトリガー
            animator.SetTrigger("enemyAttack");

            //SoundManagerのRandomizeSfx関数を呼び出し、ランダムに再生
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }

        public void DamageEnemy(int loss)
        {
            hp -= loss;
            if (hp <= 0)
            {
                Destroy(gameObject);
                enemyDead = true;
            }
        }
    }
}
