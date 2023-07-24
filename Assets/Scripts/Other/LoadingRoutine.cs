using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Other
{
    public class LoadingRoutine : MonoBehaviour
    {
        // If for any reason we need a second or third FMOD bank, add to this list!!!!!
        [BankRef] private readonly List<string> _banks = new() { "Master" };
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private TMP_Text debugLoadingText;
        
        private void Start()
        {
            if (Debug.isDebugBuild || Application.isEditor)
                debugLoadingText.gameObject.SetActive(true);
            StartCoroutine(LoadGameAsync());
        }

        private void Update()
        {
            loadingIndicator.transform.Rotate(Vector3.forward, 140 * Time.deltaTime);
        }
        
        private IEnumerator LoadGameAsync()
        {
            // Load Main Menu scene but don't allow it to activate yet
            var asyncScene = SceneManager.LoadSceneAsync("Scenes/MainMenu");
            asyncScene.allowSceneActivation = false;

            debugLoadingText.text += $"\nNeed to load {_banks.Count} FMOD banks";
            
            // Load all FMOD banks
            foreach (var bank in _banks)
            {
                debugLoadingText.text += $"\nStart loading bank {bank}...";
                RuntimeManager.LoadBank(bank, true);
            }

            // Continue yielding until all banks have loaded
            while (!RuntimeManager.HaveAllBanksLoaded)
            {
                yield return null;
            }

            debugLoadingText.text += "\nAll FMOD banks have loaded!\nStart loading FMOD sample data...";
            
            // Continue yielding until all sample data has loaded
            while (RuntimeManager.AnySampleDataLoading())
            {
                yield return null;
            }

            debugLoadingText.text += "\nAll FMOD sample data loaded!\nFinishing scene load if necessary...";
            
            // Allow the scene to now be activated once it's loaded.
            // Now all scenes will have guaranteed that FMOD Studio
            // loading is finished and there will be no delayed events.
            asyncScene.allowSceneActivation = true;
            
            // Continue yielding until the actual scene has finished loading
            while (!asyncScene.isDone)
            {
                yield return null;
            }

            debugLoadingText.text += "\nScene load finished! Ready to rock!!";
        }
    }
}