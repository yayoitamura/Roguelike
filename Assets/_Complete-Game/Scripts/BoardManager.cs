using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.リストを使用できるようにします。
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.ランダムにUnity Engineの乱数ジェネレータを使用するように指示します。

namespace Completed
	
{
	//GameManager
	public class BoardManager : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector. Serializableを使用すると、インスペクタにサブプロパティを持つクラスを埋め込むことができます。
		 [Serializable]
		public class Count
		{
			public int minimum;             //Minimum value for our Count class. Countクラスの最小値
			public int maximum;             //Maximum value for our Count class. Countクラスの最大値


			//Assignment constructor. 代入コンストラクタ。
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		public int minint = 1;                                          //乱数の最小値
		public int maxint = 5;                                          //乱数の最大値
        public int start = 8;                                           //マス目のリセット用
		public int columns = 8;                                         //ゲームボードの列数
		public int rows = 8;                                            //行数
		public Count wallCount = new Count (5, 9);                      //Lower and upper limit for our random number of walls per level. 1レベルあたりの乱数の乱数の上限と下限。
		public Count foodCount = new Count (1, 5);                      //Lower and upper limit for our random number of food items per level. 1レベルあたりの食品の乱数の上限と下限。
		public GameObject exit;                                         //Prefab to spawn for exit.プレハブを実行して終了します。
		public GameObject[] floorTiles;                                 //床プレハブの配列。
		public GameObject[] wallTiles;                                  //壁プレハブの配列。
		public GameObject[] foodTiles;                                  //foodプレハブの配列。
		public GameObject[] enemyTiles;                                 //enemyプレハブの配列。
		public GameObject[] outerWallTiles;                             //Array of outer tile prefabs. 外側のタイルプレアブの配列。

		private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object. Boardオブジェクトの変換への参照を格納する変数。
		private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles. タイルを配置する可能性のある場所のリスト。


		//Clears our list gridPositions and prepares it to generate a new board.リストgridPositionsをクリアし、新しいボードを生成する準備をします。
		void InitialiseList ()
		{
			//Clear our list gridPositions.リストのgridPositionsをクリアします。
			gridPositions.Clear ();

			//Loop through x axis (columns).X軸（列）をループします。
			for(int x = 1; x < columns-1; x++)
			{
				//Within each column, loop through y axis (rows).各列内で、y軸（行）をループします
				for(int y = 1; y < rows-1; y++)
				{
					//At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    //各インデックスで、その位置のx座標とy座標を持つ新しいVector3をリストに追加します。
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}


		//Sets up the outer walls and floor (background) of the game board. ゲームボードの外壁と床（背景）を設定します。
		void BoardSetup ()
		{
			//Instantiate Board and set boardHolder to its transform. ボードをインスタンス化し、boardHolderをその変換に設定します。
			boardHolder = new GameObject ("Board").transform;
			
			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            //床または外壁のエッジタイルで - 1（コーナーを埋める）から始まるx軸に沿ってループします。
			for(int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
                //1から始まり、床または外壁のタイルを配置するためにy軸に沿ってループします。
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    //私たちのフロアタイルプレハブの配列からランダムなタイルを選択し、それをインスタンス化する準備をします。
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
					
					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    //私たちの外壁タイルの配列からランダムな外壁プレハブを選択する場合は、現在の位置がボードの端にあるかどうかを確認します。
					if(x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
					
					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    //ループの現在のグリッド位置に対応するVector3でtoInstantiateのために選択されたプレハブを使用してGameObjectインスタンスをインスタンス化し、GameObjectにキャストします。
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    //新しくインスタンス化されたオブジェクトインスタンスの親をboardHolderに設定します。これは、階層が乱雑にならないように組織化するだけです。
					instance.transform.SetParent (boardHolder);
				}
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}
		
		
		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum+1);
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
				Vector3 randomPosition = RandomPosition();
				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}


		//SetupScene initializes our level and calls the previous functions to lay out the game board 
        //SetupSceneはレベルを初期化し、以前の関数を呼び出してゲームボードをレイアウトします
		//GameManeger.InitGame
		public void SetupScene (int level)
		{
            //面積をランダム
            //columns = start;
            //int r = new System.Random().Next(minint, maxint);
            //columns *= r;
            //rows *= r;

			//Creates the outer walls and floor.外壁と床を作成します。
			BoardSetup ();
			
			//Reset our list of gridpositions.
			InitialiseList ();
			
			//Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
			
			//Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);
			
			//Determine number of enemies based on current level number, based on a logarithmic progression
			int enemyCount = (int)Mathf.Log(level, 2f);
			
			//Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);
			
			//Instantiate the exit tile in the upper right hand corner of our game board
			Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
			//exitをランダム
			//int r1 = new System.Random().Next(8);
			//int r2 = new System.Random().Next(8);
			//Instantiate (exit, new Vector3 (r1, r2, 0f), Quaternion.identity);
		}
	}
}