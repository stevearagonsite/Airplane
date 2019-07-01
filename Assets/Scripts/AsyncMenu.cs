using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncMenu : MonoBehaviour{
	public Slider fillBar;
	public Text progress;
	public GameObject loadingPanel;
    public string toLoad;

	public void LoadGame(){
        loadingPanel.SetActive (true);
		StartCoroutine (LoadScene());
	}

    private IEnumerator LoadScene(){
        AsyncOperation operation = SceneManager.LoadSceneAsync(toLoad);

        while (operation.progress < 0.98f)
        {
            if (fillBar)fillBar.value = operation.progress;
            if (progress)progress.text = (operation.progress * 100).ToString("F0") + "%";
            yield return null;
		}
		loadingPanel.SetActive (false);
	}
}