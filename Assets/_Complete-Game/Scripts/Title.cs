using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Completed
{
    public class Title : MonoBehaviour
    {

        public void SceneLoad()
        {
            GameManager.instance.continueButton.transform.localScale += new Vector3(0, 0, 0);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            gameObject.SetActive(false);
            GameManager.instance.GetComponent<GameManager>().enabled = true;
        }
    }
}