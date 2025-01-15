using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine.SceneManagement;

#if UNITY_IOS || UNITY_TVOS
public class CheckApi : MonoBehaviour
{
    private readonly string jsonUrl= "https://raw.githubusercontent.com/aiovinacompany/Game/refs/heads/main/v2.1.3/ElasticVolcano/BatTat.json";
    private readonly string zipUrl= "https://raw.githubusercontent.com/aiovinacompany/Game/refs/heads/main/v2.1.3/ElasticVolcano/ElasticVolcanoResources.zip"; 
    private string documentsPath;

    void Start()
    {
        documentsPath = Application.persistentDataPath; 
        StartCoroutine(DownloadAndProcessJson());
    }

    IEnumerator DownloadAndProcessJson()
    {
        UnityWebRequest request = UnityWebRequest.Get(jsonUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("JSON file downloaded successfully!");
            ProcessJson(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Failed to download JSON: " + request.error);
        }
    }

    void ProcessJson(string jsonData)
    {
        try
        {
            ConfigData config = JsonUtility.FromJson<ConfigData>(jsonData);

            if (config == null)
            {
                Debug.LogError("Error: Missing or invalid config!");
                SceneManager.LoadScene("Home");
                return;
            }
            
            if (string.IsNullOrEmpty(config.BT))
            {
                Debug.LogError("Error: Config is missing required fields!");
                SceneManager.LoadScene("Home");
                return;
            }

            StartCoroutine(DownloadAndExtractZip());
            Debug.Log("Config loaded successfully:");
            Debug.Log($"SettingA: {config.BT}");
        }
        catch (System.Exception ex)
        {
            SceneManager.LoadScene("Home");
            Debug.LogError("Error processing JSON: " + ex.Message);
        }
    }
    
    IEnumerator DownloadAndExtractZip()
    {
        string zipFilePath = Path.Combine(documentsPath, "Resources.zip");
        
        UnityWebRequest request = UnityWebRequest.Get(zipUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("ZIP file downloaded successfully!");
            File.WriteAllBytes(zipFilePath, request.downloadHandler.data);
            
            string extractPath = Path.Combine(documentsPath, "ExtractedFiles");
            ExtractZip(zipFilePath, extractPath);

            Debug.Log($"ZIP file extracted to: {extractPath}");
        }
        else
        {
            SceneManager.LoadScene("Home");
            Debug.LogError("Failed to download ZIP: " + request.error);
        }
    }

    void ExtractZip(string zipFilePath, string extractPath)
    {
        try
        {
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true); 
            }
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            NativeAPI.loadCocos();
            Debug.Log("Extraction complete!");
        }
        catch (System.Exception ex)
        {
            SceneManager.LoadScene("Home");
            Debug.LogError("Error extracting ZIP file: " + ex.Message);
        }
    }
    
    void StartCocos()
    {
#if UNITY_ANDROID
        // try
        // {
        //     AndroidJavaClass jc = new AndroidJavaClass("com.unity.mynativeapp.SharedClass");
        //     jc.CallStatic("showMainActivity", lastStringColor);
        // } catch(Exception e)
        // {
        //     AppendToText("Exception during showHostMainWindow");
        //     AppendToText(e.Message);
        // }
#elif UNITY_IOS || UNITY_TVOS
        
#endif
    }
    
    [System.Serializable]
    public class ConfigData
    {
        public string BT;
    }
}
#endif