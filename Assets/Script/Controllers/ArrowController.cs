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
    
    private int frecceMostrate = 0;
    private int direzioneCorrente;
    private SwipeDetector swipeDetector;
	private Coroutine animazioneCorrente;
	private ImageManager imageManager;
	private HomepageManager homepageManager;
    
    void Start()
{
    Debug.Log("ArrowController avviato!");
    
    // Trova il componente SwipeDetector
    swipeDetector = GetComponent<SwipeDetector>();
    
    // Trova il componente ImageManager
    imageManager = GetComponent<ImageManager>();
	
	// Trova il componente HomepageManager
homepageManager = FindObjectOfType<HomepageManager>();
    
    // Collega gli eventi swipe
    swipeDetector.OnSwipeUp += () => VerificaGesto(0);
    swipeDetector.OnSwipeDown += () => VerificaGesto(1);
    swipeDetector.OnSwipeLeft += () => VerificaGesto(2);
    swipeDetector.OnSwipeRight += () => VerificaGesto(3);
    
    MostraNuovaFreccia();
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
    
    // Ferma l'animazione precedente
    if (animazioneCorrente != null)
    {
        StopCoroutine(animazioneCorrente);
    }
    
    // Vibrazione
    Handheld.Vibrate();
    
    frecceMostrate++;
    
    // Ogni 3 frecce mostra immagine
    if (frecceMostrate % 3 == 0)
    {
        Debug.Log($"Completate {frecceMostrate} frecce! Tempo di mostrare un'immagine!");
    Debug.Log("Chiamando imageManager.MostraImmagineCasuale()...");
    imageManager.MostraImmagineCasuale();
}
    
    // Mostra prossima freccia
    MostraNuovaFreccia();
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
    float ampiezzaMovimento = 80f; // Pixel di movimento
    float velocita = 6f;           // Velocità più alta
    
    while (true)
    {
        // Movimento solo in avanti usando (seno + 1) / 2 per avere valori 0-1
        float progresso = (Mathf.Sin(Time.time * velocita) + 1f) / 2f;
        float offset = progresso * ampiezzaMovimento;
        
        Vector3 nuovaPosizione = posizioneOriginale + (Vector3)(direzioneAnimazione * offset);
        
        frecciaPrefab.transform.localPosition = nuovaPosizione;
        
        yield return null; // Aspetta il prossimo frame
    }
}
}