using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalOrientationController : MonoBehaviour
{
    #region Campos e Propriedades
    public static GlobalOrientationController Instance { get; private set; }

    [Header("Configurações de Orientação")]
    [SerializeField] private string gameplaySceneName = "GamePlay";
    [SerializeField] private ScreenOrientation gameplayOrientation = ScreenOrientation.LandscapeLeft;
    [SerializeField] private ScreenOrientation defaultOrientation = ScreenOrientation.Portrait;
    #endregion

    #region Métodos do Ciclo de Vida
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Manipulação de Cena
    // Ajusta a orientação da tela com base na cena carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.name.Equals(gameplaySceneName, System.StringComparison.OrdinalIgnoreCase);
        Screen.orientation = isGameplayScene ? gameplayOrientation : defaultOrientation;

        Screen.autorotateToPortrait = !isGameplayScene;
        Screen.autorotateToPortraitUpsideDown = !isGameplayScene;
        Screen.autorotateToLandscapeLeft = isGameplayScene;
        Screen.autorotateToLandscapeRight = isGameplayScene;
    }
    #endregion
}