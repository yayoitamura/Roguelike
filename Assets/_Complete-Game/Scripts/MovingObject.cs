using UnityEngine;
using System.Collections;

namespace Completed
{
	//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
    //abstractキーワードを使用すると、不完全で派生クラスで実装する必要があるクラスおよびクラスメンバーを作成できます。
	public abstract class MovingObject : MonoBehaviour
	{
		public float moveTime = 0.1f;           //Time it will take object to move, in seconds.数秒で移動する時間がかかります。
		public LayerMask blockingLayer;         //Layer on which collision will be checked.衝突がチェックされるレイヤー。


		private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.このオブジェクトに添付されたBoxCollider2Dコンポーネント。
		private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.このオブジェクトに付けられたRigidbody2Dコンポーネント。
		private float inverseMoveTime;          //Used to make movement more efficient.動きをより効率的にするために使用されます。


		//Protected, virtual functions can be overridden by inheriting classes.保護された仮想関数は、継承するクラスによってオーバーライドできます。
		protected virtual void Start ()
		{
			//Get a component reference to this object's BoxCollider2D このオブジェクトのBoxCollider2Dへのコンポーネント参照を取得します。
			boxCollider = GetComponent <BoxCollider2D> ();

			//Get a component reference to this object's Rigidbody2D このオブジェクトのRigidbody2Dへのコンポーネント参照を取得します。
			rb2D = GetComponent <Rigidbody2D> ();
			
			//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
            //移動時間の逆数を保存することで、分割する代わりに掛けることで使用できます。これはより効率的です。
			inverseMoveTime = 1f / moveTime;
		}


		//Move returns true if it is able to move and false if not. 
		//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
		//移動が可能な場合はtrue、そうでない場合はfalseを返します。
        //Moveは、x方向、y方向、RaycastHit2Dのパラメータを取り、衝突をチェックします。
		protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
		{
			//Store start position to move from, based on objects current transform position.
            //オブジェクトの現在の変換位置に基づいて、移動先の開始位置を格納します。
			Vector2 start = transform.position;
			
			// Calculate end position based on the direction parameters passed in when calling Move.
            //Moveを呼び出すときに渡された方向パラメータに基づいて終了位置を計算します。
			Vector2 end = start + new Vector2 (xDir, yDir);
			
			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
            //ラインキャストがこのオブジェクト自身のコライダーにヒットしないように、boxColliderを無効にします。
			boxCollider.enabled = false;

			//Cast a line from start point to end point checking collision on blockingLayer.
			//blockingLayerの開始点から終了点の衝突をチェックする行をキャストします。
			hit = Physics2D.Linecast (start, end, blockingLayer);

			//Re-enable boxCollider after linecast ラインキャスト後にboxColliderを再度有効にする
			boxCollider.enabled = true;

			//Check if anything was hit 何かヒットしたかどうかを確認する
			if(hit.transform == null)
			{
				//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
                //何もヒットしなかった場合は、SmoothMovementのコルーチンをVector2の終点に目的地として渡します
				StartCoroutine (SmoothMovement (end));

				//Return true to say that Move was successful Moveが成功したと言うと真
				return true;
			}
			
			//If something was hit, return false, Move was unsuccesful.
            //何かヒットした場合はfalseを返し、Moveは失敗しました。
			return false;
		}
		
		
		//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
        //ユニットをあるスペースから次のスペースに移動させるコルーチンは、どこに移動するかを指定するためにパラメータの終わりをとります。
		protected IEnumerator SmoothMovement (Vector3 end)
		{
			//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
			//Square magnitude is used instead of magnitude because it's computationally cheaper.
			//現在の位置と終了パラメータの差の2乗の大きさに基づいて移動する残りの距離を計算します。
            //計算上安価であるため、大きさの代わりに方形の大きさが使用されます。
			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			//While that distance is greater than a very small amount (Epsilon, almost zero):
            //その距離は非常に小さい（Epsilon、ほぼゼロ）よりも大きいです：
			while(sqrRemainingDistance > float.Epsilon)
			{
				//Find a new position proportionally closer to the end, based on the moveTime
                //moveTimeに基づいて、最後に比例して新しい位置を見つける
				Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
				
				//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
                //接続されているRigidbody2DのMovePositionを呼び出し、計算された位置に移動します。
				rb2D.MovePosition (newPostion);

				//Recalculate the remaining distance after moving. 移動後の残りの距離を再計算します。
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				//Return and loop until sqrRemainingDistance is close enough to zero to end the function
                //関数を終了するためにsqrRemainingDistanceが十分に近くなるまで戻り、ループする
				yield return null;
			}
		}


		//The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
		//AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
		//仮想キーワードは、overrideキーワードを使用してクラスを継承することによって、AttemptMoveをオーバーライドできることを意味します。
        //AttemptMoveは、ユニットがブロックされた場合に私たちのユニットが対話すると期待しているコンポーネントのタイプを指定するための汎用パラメータTをとります（Player for Enemies、Playerの壁）。
		protected virtual void AttemptMove <T> (int xDir, int yDir)
			where T : Component
		{
			//Hit will store whatever our linecast hits when Move is called.ヒットは、Moveが呼び出されたときに私たちのラインカードが命中したものを保存します
			RaycastHit2D hit;
			
			//Set canMove to true if Move was successful, false if failed.
            //Moveが成功した場合はcanMoveをtrueに設定し、失敗した場合はfalseに設定します。
			bool canMove = Move (xDir, yDir, out hit);

			//Check if nothing was hit by linecast ラインキャストによって何もヒットしなかったかどうかを確認する
			if(hit.transform == null)
				//If nothing was hit, return and don't execute further code.
                //何もヒットしなかった場合は、戻りコードを実行しないでください。
				return;
			
			//Get a component reference to the component of type T attached to the object that was hit
            //ヒットしたオブジェクトに接続されているT型のコンポーネントへのコンポーネント参照を取得します。
			T hitComponent = hit.transform.GetComponent <T> ();
			
			//If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
            //canMoveがfalseでhitComponentがnullでない場合は、MovingObjectがブロックされ、相互作用できるものがヒットしたことを意味します。
			if(!canMove && hitComponent != null)
				
				//Call the OnCantMove function and pass it hitComponent as a parameter.
                //OnCantMove関数を呼び出して、hitComponentをパラメータとして渡します。
				OnCantMove (hitComponent);
		}


		//The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
		//OnCantMove will be overriden by functions in the inheriting classes.
		//抽象修飾子は、変更されるものが欠落しているか不完全な実装であることを示します。
        //OnCantMoveは、継承するクラスの関数によってオーバーライドされます。
		protected abstract void OnCantMove <T> (T component)
			where T : Component;
	}
}
