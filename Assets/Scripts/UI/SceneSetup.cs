using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Osnovna nastavitev scene - nebo, trava, podlaga.
/// To je privremena verzija 
/// </summary>
public class SceneSetup : MonoBehaviour
{
    private void Start()
    {
        // Nastavi svetlobo
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.8f);

        Debug.Log("GameScene je pripravljena! Čakamo na sprite-ove in grafiko...");
    }
}
