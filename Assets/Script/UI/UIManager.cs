using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Menu Impostazioni")]
    public GameObject menuImpostazioni;
    public GameObject panelGestioneImmagini;
    
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
    
    public void ApriGestioneImmagini()
    {
        ChiudiMenu(); // Chiudi il menu impostazioni
        panelGestioneImmagini.SetActive(true);
        Debug.Log("Aperta schermata gestione immagini");
    }
    
    public void ChiudiGestioneImmagini()
    {
        panelGestioneImmagini.SetActive(false);
        Debug.Log("Chiusa schermata gestione immagini");
    }
}