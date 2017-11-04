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
            public int minimum;             //Countクラスの最小値
            public int maximum;             //クラスの最大値


            //Assignment constructor. 代入コンストラクタ。
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }
        public int minint = 1;                                          //乱数の最小値
        public int maxint = 6;                                          //乱数の最大値
        public Count wallCount = new Count(5, 9);                       //1レベルあたりの乱数の乱数の上限と下限。
        public Count foodCount = new Count(1, 5);                       //1レベルあたりの食品の乱数の上限と下限。
        public GameObject exit;                                       //exitプレハブ
        public GameObject clearexit;
        public GameObject[] floorTiles;                                 //床プレハブの配列。
        public GameObject[] wallTiles;                                  //壁プレハブの配列。
        public GameObject[] foodTiles;                                  //foodプレハブの配列。
        public GameObject[] enemyTiles;                                 //enemyExプレハブ。
        public GameObject[] enemyExTiles;                               //Bossプレハブ
        public GameObject[] bossTiles;                                  //プレハブの配列。
        public GameObject[] outerWallTiles;                             //外側のタイルプレハブの配列。

        private int start = 8;                                          //マス目のリセット用
        private int columns = 8;                                        //ゲームボードの列数
        private int rows = 8;                                           //行数
        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object. Boardオブジェクトの変換への参照を格納する変数。
        private List<Vector3> gridPositions = new List<Vector3>();      //A list of possible locations to place tiles. タイルを配置する可能性のある場所のリスト。


        //Clears our list gridPositions and prepares it to generate a new board.リストgridPositionsをクリアし、新しいボードを生成する準備をします。
        void InitialiseList()
        {
            //Clear our list gridPositions.リストのgridPositionsをクリアします。
            gridPositions.Clear();

            //Loop through x axis (columns).X軸（列）をループします。
            for (int x = 1; x < columns - 1; x++)
            {
                //Within each column, loop through y axis (rows).各列内で、y軸（行）をループします
                for (int y = 1; y < rows - 1; y++)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    //各インデックスで、その位置のx座標とy座標を持つ新しいVector3をリストに追加します。
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }


        //Sets up the outer walls and floor (background) of the game board. ゲームボードの外壁と床（背景）を設定します。
        void BoardSetup()
        {
            //Instantiate Board and set boardHolder to its transform. ボードをインスタンス化し、boardHolderをその変換に設定します。
            boardHolder = new GameObject("Board").transform;

            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            //床または外壁のエッジタイルで - 1（コーナーを埋める）から始まるx軸に沿ってループします。
            for (int x = -1; x < columns + 1; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                //1から始まり、床または外壁のタイルを配置するためにy軸に沿ってループします。
                for (int y = -1; y < rows + 1; y++)
                {
                    //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    //私たちのフロアタイルプレハブの配列からランダムなタイルを選択し、それをインスタンス化する準備をします。
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    //私たちの外壁タイルの配列からランダムな外壁プレハブを選択する場合は、現在の位置がボードの端にあるかどうかを確認します。
                    if (x == -1 || x == columns || y == -1 || y == rows)
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    //ループの現在のグリッド位置に対応するVector3でtoInstantiateのために選択されたプレハブを使用してGameObjectインスタンスをインスタンス化し、GameObjectにキャストします。
                    GameObject instance =
                        Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    //新しくインスタンス化されたオブジェクトインスタンスの親をboardHolderに設定します。これは、階層が乱雑にならないように組織化するだけです。
                    instance.transform.SetParent(boardHolder);
                }
            }
        }

        //RandomPositionは、リストgridPositionsからランダムな位置を返します。
        Vector3 RandomPosition()
        {
            //整数randomIndexを宣言し、リストのgridPositions内のアイテムの数と0の間の乱数に値を設定します。
            int randomIndex = Random.Range(0, gridPositions.Count);

            //randomPositionというVector3型の変数を宣言し、ListのgridPositionsからrandomIndexのエントリにその値を設定します。
            Vector3 randomPosition = gridPositions[randomIndex];

            //randomIndexのエントリをリストから削除して、再利用できないようにします。
            gridPositions.RemoveAt(randomIndex);

            //ランダムに選択されたVector3の位置を返します。
            return randomPosition;
        }

        //LayoutObjectAtRandomは、作成するオブジェクトの数の最小範囲と最大範囲から選択するゲームオブジェクトの配列を受け入れます。
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            int objectCount = 101;
            if(GameManager.instance.level < 20)
            {
            //最小値と最大値の範囲内でインスタンス化するオブジェクトの乱数を選択する
            objectCount = Random.Range(minimum, maximum + 1);
            }

            //無作為に選択されたlimitCountに達するまでオブジェクトをインスタンス化する
            for (int i = 0; i < objectCount; i++)
            {
                //gridPositionに格納されている利用可能なVector3のリストからランダムな位置を取得して、randomPositionの位置を選択します
                Vector3 randomPosition = RandomPosition();

                //tileArrayからランダムなタイルを選択してtileChoiceに割り当てます
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //ローテーションを変更せずにRandomPositionから返された位置でtileChoiceをインスタンス化します。
                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }


        //SetupSceneはレベルを初期化し、以前の関数を呼び出してゲームボードをレイアウトします
        //GameManeger.InitGame
        public void SetupScene(int level)
        {
            //面積をランダム
            columns = start;
            rows = start;
            int r = new System.Random().Next(minint, maxint);
            columns *= r;
            rows *= r;

            //外壁と床を作成
            BoardSetup();

            //私たちのグリッドポジションのリストをリセットします。
            InitialiseList();

            //playerの位置をランダムに
            GameObject player = GameObject.Find("Player");
            player.transform.position = RandomPosition();

            //LayoutObjectAtRandom(exit, 1, 1);
            Vector3 randomPosition = RandomPosition(); 
            Instantiate(exit, randomPosition, Quaternion.identity);

            //無作為の位置で最小値と最大値に基づいて壁タイルの乱数をインスタンス化します。
            LayoutObjectAtRandom(wallTiles, wallCount.minimum*r, wallCount.maximum*r);

            //無作為化された位置で最小値と最大値に基づいて無作為数の食品タイルをインスタンス化します。
            LayoutObjectAtRandom(foodTiles, foodCount.minimum*r, foodCount.maximum*r);

            //対数プログレッションに基づいて、現在のレベル番号に基づいて敵の数を決定する
            //Mathf.Log 指定した底で指定した数の対数を返します
            int enemyCount = (int)Mathf.Log(level, 2f);

            //無作為の位置で最小値と最大値に基づいてランダムな数の敵をインスタンス化します。
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

            if(GameManager.instance.level % 5 == 0)
            {
                LayoutObjectAtRandom(enemyExTiles, 1* r, 3* r);
                if (GameManager.instance.level % 10 == 0)
                {
                    LayoutObjectAtRandom(bossTiles, 1* r, 2* r);
                }
            }

        }


        public void SetupLastScene(int level) 
        {
            SoundManager.instance.musicSource.clip = SoundManager.instance.lastStage;
            //SoundManager.instance.musicSource.volume = -40;
            SoundManager.instance.musicSource.Play();

            int lastStage = 10;
            columns = start;
            rows = start;
            columns *= lastStage;
            rows *= lastStage;

            //外壁と床を作成
            BoardSetup();

            //私たちのグリッドポジションのリストをリセットします。
            InitialiseList();

            //playerの位置をランダムに
            GameObject player = GameObject.Find("Player");
            player.transform.position = RandomPosition();

            Vector3 randomPosition = RandomPosition();
            Instantiate(clearexit, randomPosition, Quaternion.identity);

            //無作為の位置で最小値と最大値に基づいて壁タイルの乱数をインスタンス化します。
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            //無作為化された位置で最小値と最大値に基づいて無作為数の食品タイルをインスタンス化します。
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            //対数プログレッションに基づいて、現在のレベル番号に基づいて敵の数を決定する
            //Mathf.Log 指定した底で指定した数の対数を返します
            int enemyCount = (int)Mathf.Log(level, 2f);

            //無作為の位置で最小値と最大値に基づいてランダムな数の敵をインスタンス化します。
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

            if (GameManager.instance.level % 5 == 0)
            {
                LayoutObjectAtRandom(enemyExTiles, 1, 3);
                if (GameManager.instance.level % 10 == 0)
                {
                    LayoutObjectAtRandom(bossTiles, 1, 2);
                }
            }
        }


    }
}