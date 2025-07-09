using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{
    [Header("Impostazioni Frecce")]
    public GameObject frecciaPrefab;  // Prefab della freccia
    public Transform containerFrecce; // Dove mettere le frecce
    
    [Header("Direzioni")]
    private Vector2[] direzioni = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private string[] nomiDirezioni = { "Su", "Giù", "Sinistra", "Destra" };
    
    private int frecceMostrate = 0;
    private int direzioneCorrente;
    
    void Start()
    {
        Debug.Log("ArrowController avviato!");
        MostraNuovaFreccia();
    }
    
    void MostraNuovaFreccia()
    {
        // Sceglie direzione casuale
        direzioneCorrente = Random.Range(0, 4);
        Debug.Log($"Mostra freccia: {nomiDirezioni[direzioneCorrente]}");
        
        // TODO: Qui creeremo la freccia visualmente
    }
    
    public void GestoCorretto()
    {
        Debug.Log("Gesto corretto!");
        
        // Vibrazione
        Handheld.Vibrate();
        
        frecceMostrate++;
        
        // Ogni 10 frecce mostra immagine
        if (frecceMostrate % 10 == 0)
        {
            Debug.Log("Tempo di mostrare un'immagine!");
            // TODO: Implementare caricamento immagine
        }
        
        // Mostra prossima freccia
        MostraNuovaFreccia();
    }
}