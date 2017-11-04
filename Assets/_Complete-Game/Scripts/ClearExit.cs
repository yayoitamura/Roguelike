using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public class ClearExit : MonoBehaviour
    {
        private Animator animator;
        public GameObject lightParticle;

        // Use this for initialization
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ExitClear()
        {
            animator.SetTrigger("exitTrigger");
        }

        public void Ending()
        {
            GameManager.instance.GameClear();
        }
    }
}