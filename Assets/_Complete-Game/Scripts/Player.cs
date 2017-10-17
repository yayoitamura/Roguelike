using UnityEngine;
using System.Collections;
using UnityEngine.UI;   //Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    //Player
    //MovingObjectからの継承
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;        //Levelごとの再開の遅延時間
        public int pointsPerFood = 10;              //Foodから得るポイント
        public int pointsPerSoda = 20;              //Sodaから得るポイント
        public int wallDamage = 1;                  //プレイヤーが壁に与えるダメージ。
        public Text foodText;                       //UI Text to display current player food total.
        public AudioClip moveSound1;                //Playerが動く時のAudio clips１
        public AudioClip moveSound2;                //Playerが動く時のAudio clips２
        public AudioClip eatSound1;                 //food収集時のAudio clips１
        public AudioClip eatSound2;                 //food収集時のAudio clips２
        public AudioClip drinkSound1;               //soda収集時のAudio clips１
        public AudioClip drinkSound2;               //soda収集時のAudio clips２
        public AudioClip gameOverSound;             //Audio clip to play when player dies.

        private Animator animator;                  //animator componentを取得
        private int food;                           //foodの合計point

        //プリプロセッサ ディレクティブ
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one; //Used to store location of screen touch origin for mobile controls.
        //プライベートVector2 touchOrigin = -Vector2.one; //モバイルコントロールのスクリーンタッチ原点の場所を格納するために使用されます。
#endif


        //MovingObjectにオーバーライドする
        protected override void Start()
        {
            //animatorの取得
            animator = GetComponent<Animator>();

            //レベル間でGameManager.instanceに保存されている現在の食品ポイント合計を取得します。
            food = GameManager.instance.playerFoodPoints;

            //現在のプレーヤーの食べ物の合計を反映するように食物テキストを設定します。
            foodText.text = "Food: " + food;

            //MovingObject基本クラスのStart関数を呼び出します。
            base.Start();
        }


        //この関数は、ビヘイビアが無効または無効になったときに呼び出されます。
        private void OnDisable()
        {
            //Playerオブジェクトが無効になっているときは、現在のローカルフードの合計をGameManagerに格納して、次のレベルに再ロードすることができます。
            GameManager.instance.playerFoodPoints = food;
        }


        private void Update()
        {
            //プレイヤーのターンでない場合は、その機能を終了します。
            if (!GameManager.instance.playersTurn) return;

            int horizontal = 0;     //水平移動方向を格納するために使用します。
            int vertical = 0;       //垂直移動方向を格納するために使用します。

            //Unityエディタまたはスタンドアロンビルドで実行しているかどうかを確認します。
#if UNITY_STANDALONE || UNITY_WEBPLAYER

            //入力マネージャから入力を取得し、整数に丸め、水平軸に格納してx軸の移動方向を設定します
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            //入力マネージャから入力を取得し、整数に丸め、垂直軸に格納してy軸の移動方向を設定します
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            //水平方向に移動するかどうかを確認し、垂直方向にゼロに設定する。
            if (horizontal != 0)
            {
                vertical = 0;
            }
            //私たちがiOS、Android、Windows Phone 8、またはUnity iPhoneで動作しているか確認してください
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            
            //入力がゼロ以上のタッチを登録しているかどうかを確認する
            if (Input.touchCount > 0)
            {
                //Store the first touch detected.最初に検出されたタッチを保存します。
                Touch myTouch = Input.touches[0];
                
                //そのタッチの位相がBeganに等しいかどうかを確認する
                if (myTouch.phase == TouchPhase.Began)
                {
                    //そうであれば、touchOriginをそのタッチの位置に設定します
                    touchOrigin = myTouch.position;
                }
                
                //タッチ位相がBeganでなく、代わりにEndedに等しく、touchOriginのxがゼロ以上である場合：
                else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                {
                    //このタッチの位置と等しくなるようにtouchEndを設定します
                    Vector2 touchEnd = myTouch.position;
                    
                    //x軸のタッチの開始と終了の差を計算します。
                    float x = touchEnd.x - touchOrigin.x;
                    
                    //タッチの開始と終了の差をy軸上で計算します。
                    float y = touchEnd.y - touchOrigin.y;
                    
                    //touchIrigin.xを-1に設定すると、else if文がfalseと評価され、直ちに繰り返されることはありません。
                    touchOrigin.x = -1;
                    
                   //x軸に沿った差がy軸に沿った差よりも大きいかどうか確認してください。
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        //xが0より大きい場合、horizontalを1に設定し、そうでない場合は-1に設定します
                        horizontal = x > 0 ? 1 : -1;
                    else
                        //yが0より大きい場合は、horizontalを1に設定し、そうでない場合は-1に設定します
                        vertical = y > 0 ? 1 : -1;
                }
            }
            
#endif //#elifで上で起動したモバイルプラットフォーム依存のコンパイルセクションの終わり
            //Check if we have a non-zero value for horizontal or vertical
            if (horizontal != 0 || vertical != 0)
            {
                //AttemptMoveはジェネリックパラメータWallを渡します。これはPlayerが攻撃を受けたときに対話することができるためです（
                //水平方向と垂直方向のパラメータを渡してPlayerを移動する方向を指定します）
                //AttemptMove<Wall>(horizontal, vertical);
                AttemptMove<MonoBehaviour>(horizontal, vertical);
            }
        }

        //AttemptMoveは、基底クラスのAttemptMove関数をオーバーライドします。MovingObject AttemptMoveは、PlayerがWall型のジェネリックパラメータTをとります.x方向とy方向の整数も移動します。
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //プレイヤーが移動するたびに、合計食糧ポイントを引く。
            food--;

            //現在のスコアを反映するように食べ物のテキスト表示を更新します。
            foodText.text = "Food: " + food;

            //基本クラスのAttemptMoveメソッドを呼び出し、コンポーネントT（この場合はWall）とx方向とy方向を移動して渡します。
            base.AttemptMove<T>(xDir, yDir);

            //ヒットは、我々が移動で行なわれたラインキャストの結果を参照することを可能にする。
            RaycastHit2D hit;

            //Moveがtrueを返す場合、Playerは空の領域に移動できたことを意味します。
            if (Move(xDir, yDir, out hit))
            {
                //SoundManagerのRandomizeSfxを呼び出して、2つのオーディオクリップを渡して移動音を再生します。
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            //プレイヤーは移動して食糧を失ったので、ゲームが終了したかどうかを確認します。
            CheckIfGameOver();

            //プレイヤーが終了したら、GameManagerのplayersTurn booleanをfalseに設定します。
            GameManager.instance.playersTurn = false;
        }


        //OnCantMoveは、MovingObjectのOnCantMoveという抽象関数をオーバーライドします。 
        //それはプレーヤーが攻撃で破壊することができる壁である場合、一般的なパラメータTをとります。
        //MovingObject.AttemptMove でhitしたcomponentを取得
        protected override void OnCantMove<T>(T component)
        {
            if (component.tag == "Wall")
            {
                //引数のcomponentをhitWallに代入
                Wall hitWall = component as Wall;

                //襲っている壁のDamageWall機能を呼び出します。
                hitWall.DamageWall(wallDamage);

                //プレイヤーの攻撃アニメーションを再生するために、プレイヤーのアニメーションコントローラーの攻撃トリガーを設定します。
                animator.SetTrigger("playerChop");
            }
            else if (component.tag == "Enemy")
            {
                //引数のcomponentをhitEnemyに代入
                Enemy hitEnemy = component as Enemy;

                //DamageEnemy機能を呼び出します。
                hitEnemy.DamageEnemy(wallDamage);

                //プレイヤーの攻撃アニメーションを再生するために、プレイヤーのアニメーションコントローラーの攻撃トリガーを設定します。
                animator.SetTrigger("playerChop");
            }
        }


        //OnTriggerEnter2Dは、別のオブジェクトがこのオブジェクトに接続されたトリガーコライダーに入ると送信されます（2D物理のみ）。
        private void OnTriggerEnter2D(Collider2D other)
        {
            //衝突したトリガーのタグがExitかどうかを確認してください。
            if (other.tag == "Exit")
            {
                //restartLevelDelayの遅延（デフォルトは1秒）で次のレベルを開始するには、Restart関数を呼び出します。
                Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //衝突したトリガーのタグがFoodかどうかを確認します。
            else if (other.tag == "Food")
            {
                //選手の現在の食べ物の合計にpointsPerFoodを追加します。
                food += pointsPerFood;

                //foodTextを更新して現在の合計を表し、ポイントを得たことをプレーヤーに通知します
                foodText.text = "+" + pointsPerFood + " Food: " + food;

                //SoundManagerのRandomizeSfx関数を呼び出し、2つの食べ物の音を渡して、食べるサウンドエフェクトを再生するかどうかを選択します。
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //プレイヤーが衝突した食物を無効にする。
                other.gameObject.SetActive(false);
            }

            //衝突したトリガーのタグがソーダかどうかを確認します。
            else if (other.tag == "Soda")
            {
                //プレイヤーにポイントを追加する
                food += pointsPerSoda;

                //foodTextを更新して現在の合計を表し、ポイントを得たことをプレーヤーに通知します
                foodText.text = "+" + pointsPerSoda + " Food: " + food;

                //SoundManagerのRandomizeSfx関数を呼び出し、2つの飲酒音を渡して飲酒効果を再生するかどうかを選択します
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                //プレーヤーが衝突したソーダオブジェクトを無効にする。
                other.gameObject.SetActive(false);
            }
        }


        //再起動すると、呼び出されるとシーンがリロードされます。
        private void Restart()
        {
            //ロードされた最後のシーン、この場合Mainをゲームの唯一のシーンにロードします。 
            //また、 "Single"モードでロードして、既存のシーンオブジェクトを置き換え、現在のシーン内のすべてのシーンオブジェクトをロードしないようにします。
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }


        //LoseFoodは、敵がプレイヤーを攻撃するときに呼び出されます。
        //失うポイント数を指定するパラメータの損失が発生します。
        public void LoseFood(int loss)
        {
            //playerHitアニメーションに遷移するように、プレーヤアニメータのトリガを設定します。
            animator.SetTrigger("playerHit");

            //プレイヤーの合計から失われた食べ物ポイントを差し引く。
            food -= loss;

            //フードディスプレイを新しい合計で更新します。
            foodText.text = "-" + loss + " Food: " + food;

            //ゲームが終了したかどうかを確認してください。
            CheckIfGameOver();
        }


        //CheckIfGameOverは、プレイヤーが食糧ポイントを使い果たしているかどうかをチェックし、食べ物ポイントがない場合、ゲームを終了します。
        private void CheckIfGameOver()
        {
            //フードポイント合計がゼロ以下であることを確認します。
            if (food <= 0)
            {
                //SoundManagerのPlaySingle関数を呼び出し、再生するオーディオクリップとしてgameOverSoundを渡します。
                SoundManager.instance.PlaySingle(gameOverSound);

                //バックグラウンドミュージックを停止します。
                SoundManager.instance.musicSource.Stop();

                //GameManagerのGameOver関数を呼び出します。
                GameManager.instance.GameOver();
            }
        }
    }
}

