using UnityEngine;
using System.Collections;

namespace Completed
{
    //MainCamera
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;          //GameManager prefab to instantiate.
        public GameObject soundManager;         //SoundManager prefab to instantiate.


        void Awake()
        {
            //GameManager instanceの有無
            if (GameManager.instance == null)

                //Instantiate gameManager prefab
                Instantiate(gameManager);

            //Check if a SoundManager has already been assigned to static variable GameManager.instance or if it's still null
            if (SoundManager.instance == null)

                //Instantiate SoundManager prefab
                Instantiate(soundManager);
        }
    }
}


/*
instance = Instantiate(soundManager);//prefab -> instance
instance = new SoundManager();//class -> instance
インスペクター場でアタッチ




soundManager.instance

*/
