using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{

	public class CameraControl : MonoBehaviour
	{

		public GameObject player;
        public int y = 5;
        public int z = -10;

		void Start()
		{

		}

		void Update()
		{
			transform.position = new Vector3(player.transform.position.x, y, z);
		}

	}
}