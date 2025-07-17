using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{
    [Header("Impostazioni Frecce")]
    public GameObject frecciaPrefab;  
    public Transform containerFrecce; 
    
    [Header("Direzioni")]
    private Vector2[] direzioni = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private string[] nomiDirezioni = { "Su", "Giù", "Sinistra", "Destra" };
    
    [Header("Tocco Prolungato")]
    public float tempoRitornoHomepage = 3f;
    public float raggioCentrale = 0.3f; // 30% del centro schermo
    public float ritardoAttivazione = 0.8f; // Secondi prima di attivare tocco prolungato
    public float intervalloVibrazioniTocco = 0.5f; // Ogni quanto vibrare durante tocco prolungato
    
    [Header("Swipe Settings")]
    public float distanzaMinima = 50f;
    public float tempoMassimoSwipe = 1f;
    
    [Header("Gioco")]
    public int freccePerImmagine = 3; // Ogni N frecce mostra immagine
    
    [Header("Animazione Freccia")]
    public float ampiezzaMovimento = 80f; // Pixel di movimento
    public float velocitaAnimazione = 6f; // Velocità animazione

    [Header("Referencias UI")]
    public GameObject calendarPanel; // Trascinaci il CalendarPanel dall'Inspector

    private int frecceMostrate = 0;
    private int direzioneCorrente;
    private Coroutine animazioneCorrente;
    private ImageManager imageManager;
    private HomepageManager homepageManager;
    private SettingsManager settingsManager;

    // Variabili per gestione tocco unificata
    private bool touchAttivo = false;
    private Vector2 puntoInizio;
    private float tempoInizioTouch;
    private Vector2 centroSchermo;
    
    // Variabili per tocco prolungato
    private bool toccoProlungatoAttivo = false;
    private float tempoInizioPressione;
    private Coroutine coroutineVibrazioni;

    void Start()
    {
        Debug.Log("ArrowController avviato!");

        // Trova i componenti necessari
        imageManager = GetComponent<ImageManager>();
        homepageManager = FindObjectOfType<HomepageManager>();
        settingsManager = FindObjectOfType<SettingsManager>();

        // Calcola centro schermo
        centroSchermo = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        MostraNuovaFreccia();
    }

    void Update()
    {
        GestisciInputUnificato();
    }

    private void GestisciInputUnificato()
    {
        // Non gestire input se il calendario è aperto
        if (calendarPanel != null && calendarPanel.activeInHierarchy)
        {
            return;
        }

        // Touch/Mouse iniziato
        if (Input.GetMouseButtonDown(0))
        {
            puntoInizio = Input.mousePosition;
            tempoInizioTouch = Time.time;
            touchAttivo = true;
            toccoProlungatoAttivo = false; // Reset tocco prolungato
            
            Debug.Log("Touch iniziato");
        }
        
        // Durante il tocco - controlla se attivare tocco prolungato
        if (touchAttivo && !toccoProlungatoAttivo)
        {
            float tempoTrascorso = Time.time - tempoInizioTouch;
            Vector2 posizioneCorrente = Input.mousePosition;
            float movimentoTotale = Vector2.Distance(puntoInizio, posizioneCorrente);
            
            // Attiva tocco prolungato solo dopo il ritardo impostato senza movimento significativo
            if (tempoTrascorso >= ritardoAttivazione && movimentoTotale < 30f)
            {
                // Verifica se il tocco iniziale era nel centro
                float distanzaDalCentro = Vector2.Distance(puntoInizio, centroSchermo);
                float raggioMassimoPixel = Mathf.Min(Screen.width, Screen.height) * raggioCentrale;
                
                if (distanzaDalCentro <= raggioMassimoPixel)
                {
                    toccoProlungatoAttivo = true;
                    tempoInizioPressione = Time.time;
                    Debug.Log("Attivato tocco prolungato al centro");
                    
                    // Avvia vibrazioni ripetute
                    if (coroutineVibrazioni != null)
                        StopCoroutine(coroutineVibrazioni);
                    coroutineVibrazioni = StartCoroutine(VibrazioniRipetute());
                }
            }
        }
        
        // Controlla progresso tocco prolungato
        if (touchAttivo && toccoProlungatoAttivo)
        {
            float tempoTrascorso = Time.time - tempoInizioPressione;
            
            if (tempoTrascorso >= tempoRitornoHomepage)
            {
                // Tocco prolungato completato - torna alla homepage
                TornaAllaHomepage();
                return; // Esci dalla funzione
            }
        }
        
        // Touch/Mouse finito
        if (Input.GetMouseButtonUp(0) && touchAttivo)
        {
            // Se era un tocco prolungato, fermalo
            if (toccoProlungatoAttivo)
            {
                toccoProlungatoAttivo = false;
                
                // Ferma vibrazioni
                if (coroutineVibrazioni != null)
                {
                    StopCoroutine(coroutineVibrazioni);
                }
                Debug.Log("Tocco prolungato interrotto");
            }
            else
            {
                // Era un tocco normale - verifica se è uno swipe
                Vector2 puntoFine = Input.mousePosition;
                float tempoTrascorso = Time.time - tempoInizioTouch;
                
                if (tempoTrascorso <= tempoMassimoSwipe)
                {
                    Vector2 direzione = puntoFine - puntoInizio;
                    float distanza = direzione.magnitude;
                    
                    if (distanza >= distanzaMinima)
                    {
                        // È uno swipe valido
                        direzione.Normalize();
                        
                        int direzioneSwipe = -1;
                        
                        // Determina la direzione principale
                        if (Mathf.Abs(direzione.x) > Mathf.Abs(direzione.y))
                        {
                            // Swipe orizzontale
                            if (direzione.x > 0)
                            {
                                Debug.Log("Swipe Destra");
                                direzioneSwipe = 3; // Destra
                            }
                            else
                            {
                                Debug.Log("Swipe Sinistra");
                                direzioneSwipe = 2; // Sinistra
                            }
                        }
                        else
                        {
                            // Swipe verticale
                            if (direzione.y > 0)
                            {
                                Debug.Log("Swipe Su");
                                direzioneSwipe = 0; // Su
                            }
                            else
                            {
                                Debug.Log("Swipe Giù");
                                direzioneSwipe = 1; // Giù
                            }
                        }
                        
                        // Verifica se lo swipe è corretto
                        if (direzioneSwipe != -1)
                        {
                            VerificaGesto(direzioneSwipe);
                        }
                    }
                }
            }
            
            touchAttivo = false;
        }
    }
    
    private System.Collections.IEnumerator VibrazioniRipetute()
    {
        while (toccoProlungatoAttivo)
        {
            Handheld.Vibrate();
            Debug.Log("Vibrazione tocco prolungato");
            yield return new WaitForSeconds(intervalloVibrazioniTocco);
        }
    }
    
    private void TornaAllaHomepage()
    {
        toccoProlungatoAttivo = false;
        touchAttivo = false;
        
        // Ferma vibrazioni
        if (coroutineVibrazioni != null)
        {
            StopCoroutine(coroutineVibrazioni);
        }
        
        Debug.Log("Tornando alla homepage tramite tocco prolungato");
        
        // Nascondi eventuali immagini
        if (imageManager != null)
        {
            imageManager.NascondiImmagine();
        }
        
        // Torna alla homepage
        if (homepageManager != null)
        {
            homepageManager.TornaAllaHomepage();
        }
    }
    
    public void MostraNuovaFreccia()
    {
        // Ferma l'animazione precedente se esiste
        if (animazioneCorrente != null)
        {
            StopCoroutine(animazioneCorrente);
        }
        
        int nuovaDirezione;
        
        // Assicurati che la nuova direzione sia diversa dalla precedente
        do
        {
            nuovaDirezione = Random.Range(0, 4);
        }
        while (nuovaDirezione == direzioneCorrente && frecceMostrate > 0);
        
        direzioneCorrente = nuovaDirezione;
        Debug.Log($"Mostra freccia: {nomiDirezioni[direzioneCorrente]}");
        
        // RESET POSIZIONE AL CENTRO
        if (frecciaPrefab != null)
        {
            frecciaPrefab.transform.localPosition = Vector3.zero;
            // Angoli di rotazione: Su=0°, Giù=180°, Sinistra=90°, Destra=270°
            float[] angoli = { 0f, 180f, 90f, 270f };
            frecciaPrefab.transform.rotation = Quaternion.Euler(0, 0, angoli[direzioneCorrente]);
            
            // Avvia l'animazione di movimento
            animazioneCorrente = StartCoroutine(AnimazioneFreccia());
        }
    }
    
    public void NascondiFreccia()
    {
        if (frecciaPrefab != null)
        {
            frecciaPrefab.SetActive(false);
        }
        
        // Ferma l'animazione
        if (animazioneCorrente != null)
        {
            StopCoroutine(animazioneCorrente);
        }
    }
    
    public void MostraFreccia()
    {
        if (frecciaPrefab != null)
        {
            frecciaPrefab.SetActive(true);
        }
    }
    
    void VerificaGesto(int direzioneGesto)
    {
        Debug.Log($"Gesto rilevato: {nomiDirezioni[direzioneGesto]}, Richiesto: {nomiDirezioni[direzioneCorrente]}");
        
        if (direzioneGesto == direzioneCorrente)
        {
            GestoCorretto();
        }
        else
        {
            Debug.Log("Gesto sbagliato! Riprova.");
        }
    }

    void GestoCorretto()
    {
        Debug.Log("Gesto corretto!");

        // Registra lo swipe nelle statistiche
        StatisticsManager statsManager = FindObjectOfType<StatisticsManager>();
        if (statsManager != null)
        {
            statsManager.RecordSwipe();
        }

        // Ferma l'animazione precedente
        if (animazioneCorrente != null)
        {
            StopCoroutine(animazioneCorrente);
        }

        // Vibrazione
        Handheld.Vibrate();

        frecceMostrate++;

        // Ottieni il numero di frecce corrente dalle impostazioni
        int freccePerImmagineCorrente = 3; // Default fallback
        if (settingsManager != null)
        {
            freccePerImmagineCorrente = settingsManager.GetCurrentArrowCount();
        }

        // Controlla se è il momento di mostrare un'immagine
        if (frecceMostrate >= freccePerImmagineCorrente)
        {
            Debug.Log($"Completate {frecceMostrate} frecce! Tempo di mostrare un'immagine!");
            Debug.Log($"Frecce richieste per questo step: {freccePerImmagineCorrente}");

            // Reset contatore frecce per il prossimo step
            frecceMostrate = 0;

            // Avanza la sequenza se siamo in modalità avanzata
            if (settingsManager != null)
            {
                settingsManager.AdvanceSequence();
            }

            Debug.Log("Chiamando imageManager.MostraImmagineCasuale()...");
            imageManager.MostraImmagineCasuale();
        }
        else
        {
            // Mostra prossima freccia
            MostraNuovaFreccia();
        }
    }

    private System.Collections.IEnumerator AnimazioneFreccia()
    {
        if (frecciaPrefab == null) yield break;
        
        Vector3 posizioneOriginale = frecciaPrefab.transform.localPosition;
        
        // Calcola direzione del movimento in base alla rotazione
        Vector2[] direzioniMovimento = { 
            Vector2.up,      // Su
            Vector2.down,    // Giù  
            Vector2.left,    // Sinistra
            Vector2.right    // Destra
        };
        
        Vector2 direzioneAnimazione = direzioniMovimento[direzioneCorrente];
        
        while (true)
        {
            // Movimento solo in avanti usando (seno + 1) / 2 per avere valori 0-1
            float progresso = (Mathf.Sin(Time.time * velocitaAnimazione) + 1f) / 2f;
            float offset = progresso * ampiezzaMovimento;
            
            Vector3 nuovaPosizione = posizioneOriginale + (Vector3)(direzioneAnimazione * offset);
            
            frecciaPrefab.transform.localPosition = nuovaPosizione;
            
            yield return null; // Aspetta il prossimo frame
        }
    }
}