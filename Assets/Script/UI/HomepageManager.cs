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
	
	[Header("Accesso Segreto Statistiche")]
public RectTransform logoArea; // Area del logo in alto a destra
public float tempoAccessoSegreto = 3f; // 3 secondi di pressione

private bool accessoSegretoAttivo = false;
private float tempoInizioAccessoSegreto;
private CalendarUI calendarUI;
    
    private UIManager uiManager;
    private ArrowController arrowController;
    
    void Start()
    {
        // Trova i componenti necessari
        uiManager = FindObjectOfType<UIManager>();
        arrowController = FindObjectOfType<ArrowController>();
		calendarUI = FindObjectOfType<CalendarUI>();
        
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
}