using UnityEngine;
using System.Collections;

namespace Completed
{
    //The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
    //abstractキーワードを使用すると、不完全で派生クラスで実装する必要があるクラスおよびクラスメンバーを作成できます。
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 0.1f;           //移動するまでの時間
        public LayerMask blockingLayer;         //衝突がチェックされるレイヤー。
        public bool canMoving;

        private BoxCollider2D boxCollider;      //このオブジェクトに添付されたBoxCollider2Dコンポーネント。
        private Rigidbody2D rb2D;               //このオブジェクトに付けられたRigidbody2Dコンポーネント。
        private float inverseMoveTime;          //動きをより効率的にするために使用されます。


        //保護された仮想関数は、継承するクラスによってオーバーライドできます。
        protected virtual void Start()
        {
            //このオブジェクトのBoxCollider2Dへのコンポーネント参照を取得します。
            boxCollider = GetComponent<BoxCollider2D>();

            //このオブジェクトのRigidbody2Dへのコンポーネント参照を取得します。
            rb2D = GetComponent<Rigidbody2D>();

            //移動時間の逆数を保存することで、分割する代わりに掛けることで使用できます。これはより効率的です。
            inverseMoveTime = 1f / moveTime;
        }

        //移動が可能な場合はtrue、そうでない場合はfalseを返します。
        //Moveは、x方向、y方向、RaycastHit2Dのパラメータを取り、衝突をチェックします。
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            //オブジェクトの現在の変換位置に基づいて、移動先の開始位置を格納します。
            Vector2 start = transform.position;

            //Moveを呼び出すときに渡された方向パラメータに基づいて終了位置を計算します。
            Vector2 end = start + new Vector2(xDir, yDir);

            //ラインキャストがこのオブジェクト自身のコライダーにヒットしないように、boxColliderを無効にします。
            boxCollider.enabled = false;

            //blockingLayerの開始点から終了点の衝突をチェックする行をキャストします。
            hit = Physics2D.Linecast(start, end, blockingLayer);

            //ラインキャスト後にboxColliderを再度有効にする
            boxCollider.enabled = true;

            //何かヒットしたかどうかを確認する
            if (hit.transform == null)
            {
                //何もヒットしなかった場合は、SmoothMovementのコルーチンをVector2の終点に目的地として渡します
                StartCoroutine(SmoothMovement(end));
                //Moveが成功したと言うと真
                return true;
            }

            //何かヒットした場合はfalseを返し、Moveは失敗しました。
            return false;
        }

        //ユニットをあるスペースから次のスペースに移動させるコルーチンは、どこに移動するかを指定するためにパラメータの終わりをとります。
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            //現在の位置と終了パラメータの差の2乗の大きさに基づいて移動する残りの距離を計算します。
            //計算上安価であるため、大きさの代わりに方形の大きさが使用されます。
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //その距離は非常に小さい（Epsilon、ほぼゼロ）よりも大きいです：
            while (sqrRemainingDistance > float.Epsilon)
            {
                //moveTimeに基づいて、最後に比例して新しい位置を見つける
                Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

                //接続されているRigidbody2DのMovePositionを呼び出し、計算された位置に移動します。
                rb2D.MovePosition(newPostion);

                //移動後の残りの距離を再計算します。
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;

                //関数を終了するためにsqrRemainingDistanceが十分に近くなるまで戻り、ループする
                yield return null;
            }
        }

        //仮想キーワードは、overrideキーワードを使用してクラスを継承することによって、AttemptMoveをオーバーライドできることを意味します。
        //AttemptMoveは、ユニットがブロックされた場合に私たちのユニットが対話すると期待しているコンポーネントのタイプを指定するための汎用パラメータTをとります（Player for Enemies、Playerの壁）。
        protected virtual void AttemptMove<T>(int xDir, int yDir)
            where T : Component
        {
            //ヒットは、Moveが呼び出されたときに私たちのラインカードが命中したものを保存します
            RaycastHit2D hit;

            //Moveが成功した場合はcanMoveをtrueに設定し、失敗した場合はfalseに設定します。
            bool canMove = Move(xDir, yDir, out hit);

            //Check if nothing was hit by linecast ラインキャストによって何もヒットしなかったかどうかを確認する
            if (hit.transform == null)
                //何もヒットしなかった場合は、戻りコードを実行しないでください。
                return;

            //ヒットしたオブジェクトに接続されているT型のコンポーネントへのコンポーネント参照を取得します。
            T hitComponent = hit.transform.GetComponent<T>();

            //canMoveがfalseでhitComponentがnullでない場合は、MovingObjectがブロックされ、相互作用できるものがヒットしたことを意味します。
            if (!canMove && hitComponent != null){
                //OnCantMove関数を呼び出して、hitComponentをパラメータとして渡します。
                OnCantMove(hitComponent);
                canMoving = false;
            }
        }

        //抽象修飾子は、変更されるものが欠落しているか不完全な実装であることを示します。
        //OnCantMoveは、継承するクラスの関数によってオーバーライドされます。
        protected abstract void OnCantMove<T>(T component)
            where T : Component;
    }
}
