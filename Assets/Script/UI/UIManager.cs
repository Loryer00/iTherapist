using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Menu Impostazioni")]
    public GameObject menuImpostazioni;
    
    private bool menuAperto = false;
    
    public void ToggleMenuImpostazioni()
    {
        menuAperto = !menuAperto;
        menuImpostazioni.SetActive(menuAperto);
        Debug.Log($"Menu impostazioni: {(menuAperto ? "Aperto" : "Chiuso")}");
    }
    
    public void ChiudiMenu()
    {
        menuAperto = false;
        menuImpostazioni.SetActive(false);
    }
}