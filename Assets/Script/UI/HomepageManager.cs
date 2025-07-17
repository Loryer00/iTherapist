using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomepageManager : MonoBehaviour
{
    [Header("UI Homepage")]
    public GameObject homepagePanel;
    public GameObject gamePanel;
    public Button respiroGuidatoButton;
    public Button galleryButton;

    [Header("Menu Impostazioni Homepage")]
    public GameObject menuImpostazioniHomepage;
    public Button gestioneImmaginiButton;
    public Button chiudiGalleryButton;
	
	[Header("Accesso Segreto Statistiche")]
    public RectTransform logoArea; // Area del logo in alto a destra
    public float tempoAccessoSegreto = 3f; // 3 secondi di pressione

    [Header("Panel References")]
    public GameObject panelGestioneImmagini;

    private bool accessoSegretoAttivo = false;
    private float tempoInizioAccessoSegreto;
    private CalendarUI calendarUI;
    
    private ArrowController arrowController;

    void Start()
    {
        // Trova i componenti necessari
        arrowController = FindObjectOfType<ArrowController>();
        calendarUI = FindObjectOfType<CalendarUI>();

        // Configura i pulsanti
        if (respiroGuidatoButton != null)
            respiroGuidatoButton.onClick.AddListener(IniziRespiroGuidato);

        if (galleryButton != null)
            galleryButton.onClick.AddListener(ToggleMenuHomepage);

        if (gestioneImmaginiButton != null)
            gestioneImmaginiButton.onClick.AddListener(ApriGestioneImmagini);

        // Inizia dalla homepage
        MostraHomepage();
    }


    public void IniziRespiroGuidato()
{
    Debug.Log("Avvio Respiro Guidato");
    
    // Inizia tracciamento sessione
    StatisticsManager statsManager = FindObjectOfType<StatisticsManager>();
    if (statsManager != null)
    {
        statsManager.StartSession();
    }
    
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
    
    // Termina tracciamento sessione
    StatisticsManager statsManager = FindObjectOfType<StatisticsManager>();
    if (statsManager != null)
    {
        statsManager.EndSession();
    }
    
    // Nascondi interfaccia gioco
    if (gamePanel != null)
        gamePanel.SetActive(false);
            
        // Mostra homepage
        if (homepagePanel != null)
            homepagePanel.SetActive(true);
    }


    public void ToggleMenuHomepage()
    {
        bool menuAperto = menuImpostazioniHomepage.activeInHierarchy;
        menuImpostazioniHomepage.SetActive(!menuAperto);
        Debug.Log($"Menu homepage: {(!menuAperto ? "Aperto" : "Chiuso")}");
    }

    public void ChiudiGestioneImmagini()
    {
        if (panelGestioneImmagini != null)
            panelGestioneImmagini.SetActive(false);

        Debug.Log("Chiusa gestione immagini");
    }

    public void ApriGestioneImmagini()
    {
        // Chiudi il menu homepage
        if (menuImpostazioniHomepage != null)
            menuImpostazioniHomepage.SetActive(false);

        // Usa il riferimento diretto
        if (panelGestioneImmagini != null)
        {
            panelGestioneImmagini.SetActive(true);
            Debug.Log("Aperta schermata gestione immagini");
        }
        else
        {
            Debug.LogError("Panel gestione immagini non assegnato nell'Inspector!");
        }
    }


    private void MostraHomepage()
    {
        if (homepagePanel != null)
            homepagePanel.SetActive(true);
            
        if (gamePanel != null)
            gamePanel.SetActive(false);            
    }

	
	void Update()
{
    GestisciAccessoSegreto();
}


    private void GestisciAccessoSegreto()
{
    // Solo se siamo nella homepage
    if (homepagePanel == null || !homepagePanel.activeInHierarchy) return;
    
    // Touch/Mouse iniziato
    if (Input.GetMouseButtonDown(0))
    {
        Vector2 clickPosition = Input.mousePosition;
        
        // Verifica se il click è nell'area del logo (in alto a destra)
        if (IsClickInLogoArea(clickPosition))
        {
            accessoSegretoAttivo = true;
            tempoInizioAccessoSegreto = Time.time;
        }
    }
    
    // Durante la pressione
    if (accessoSegretoAttivo && Input.GetMouseButton(0))
    {
        float tempoTrascorso = Time.time - tempoInizioAccessoSegreto;
        
        if (tempoTrascorso >= tempoAccessoSegreto)
        {
            // Accesso segreto completato
            ApriStatistiche();
            accessoSegretoAttivo = false;
        }
    }
    
    // Touch/Mouse rilasciato
    if (Input.GetMouseButtonUp(0))
    {
        accessoSegretoAttivo = false;
    }
}


    private bool IsClickInLogoArea(Vector2 clickPosition)
{
    if (logoArea == null) return false;
    
    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        logoArea, clickPosition, null, out localPoint);
    
    return logoArea.rect.Contains(localPoint);
}

    private void ApriStatistiche()
    {
        Debug.Log("Accesso segreto attivato - Apertura statistiche");

        if (calendarUI != null)
        {
            calendarUI.ShowCalendar();
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toastInstance = toast.CallStatic<AndroidJavaObject>("makeText", activity, "CalendarUI non trovato!", 0);
                toastInstance.Call("show");
            }
        }
    }


    public void ApriImpostazioni()
    {
        SettingsManager settingsManager = FindObjectOfType<SettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.OpenSettings();
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toastInstance = toast.CallStatic<AndroidJavaObject>("makeText", activity, "SettingsManager non trovato!", 0);
                toastInstance.Call("show");
            }
        }
    }

}