using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomepageManager : MonoBehaviour
{
    [Header("UI Homepage")]
    public GameObject homepagePanel;
    public GameObject gamePanel;           // Panel con le frecce
    public Button respiroGuidatoButton;
    public Button menuButton;
    
    [Header("Menu Impostazioni Homepage")]
    public GameObject menuImpostazioniHomepage;
    public Button gestioneImmaginiButton;
    public Button chiudiMenuButton;
    
    private UIManager uiManager;
    private ArrowController arrowController;
    
    void Start()
    {
        // Trova i componenti necessari
        uiManager = FindObjectOfType<UIManager>();
        arrowController = FindObjectOfType<ArrowController>();
        
        // Configura i pulsanti
        if (respiroGuidatoButton != null)
            respiroGuidatoButton.onClick.AddListener(IniziRespiroGuidato);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(ToggleMenuHomepage);
            
        if (gestioneImmaginiButton != null)
            gestioneImmaginiButton.onClick.AddListener(ApriGestioneImmagini);         
        
        // Inizia dalla homepage
        MostraHomepage();
    }
    
    public void IniziRespiroGuidato()
    {
        Debug.Log("Avvio Respiro Guidato");
        
        // Nascondi homepage
        if (homepagePanel != null)
            homepagePanel.SetActive(false);
            
        // Mostra interfaccia gioco
        if (gamePanel != null)
            gamePanel.SetActive(true);
            
        // Avvia il controller delle frecce
        if (arrowController != null)
        {
            arrowController.MostraNuovaFreccia();
        }
    }
    
    public void TornaAllaHomepage()
    {
        Debug.Log("Ritorno alla Homepage");
        
        // Nascondi interfaccia gioco
        if (gamePanel != null)
            gamePanel.SetActive(false);
            
        // Mostra homepage
        if (homepagePanel != null)
            homepagePanel.SetActive(true);
    }
    
    public void ToggleMenuHomepage()
    {
        /*
		menuAperto = !menuAperto;
        if (menuImpostazioniHomepage != null)
            menuImpostazioniHomepage.SetActive(menuAperto);
        Debug.Log($"Menu homepage: {(menuAperto ? "Aperto" : "Chiuso")}");
		*/
		
		ApriGestioneImmagini();
    }
        
    public void ApriGestioneImmagini()
    {
        if (uiManager != null)
        {
            uiManager.ApriGestioneImmagini();
        }
    }
    
    private void MostraHomepage()
    {
        if (homepagePanel != null)
            homepagePanel.SetActive(true);
            
        if (gamePanel != null)
            gamePanel.SetActive(false);            
    }
}