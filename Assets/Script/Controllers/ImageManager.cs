using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject imageDisplay;       // Panel che contiene tutto
    public RawImage selectedImage;       // L'immagine vera e propria
    
    [Header("Impostazioni")]
    public float tempoVisualizzazione = 3f; // Secondi di visualizzazione
    
    private Texture2D textureCorrente;
    private ImageStorageManager storageManager;
    
    void Start()
    {
        storageManager = GetComponent<ImageStorageManager>();
    }
    
    public void MostraImmagineCasuale()
    {
        Debug.Log("Richiesta di mostrare immagine dalla raccolta!");
        
        if (storageManager == null)
        {
            Debug.LogError("ImageStorageManager non trovato!");
            return;
        }
        
        string percorsoImmagine = storageManager.OttieniImmagineCasuale();
        
        if (!string.IsNullOrEmpty(percorsoImmagine))
        {
            StartCoroutine(LoadImageCoroutine(percorsoImmagine));
        }
        else
        {
            Debug.Log("Nessuna immagine disponibile nella raccolta - continua con le frecce");
            // Continua con la prossima freccia se non ci sono immagini
            ArrowController arrowController = GetComponent<ArrowController>();
            if (arrowController != null)
            {
                arrowController.MostraNuovaFreccia();
            }
        }
    }
    
    private System.Collections.IEnumerator LoadImageCoroutine(string path)
{
    // Nascondi le frecce prima di mostrare l'immagine
    ArrowController arrowController = GetComponent<ArrowController>();
    if (arrowController != null)
    {
        arrowController.NascondiFreccia();
    }
    
    textureCorrente = NativeGallery.LoadImageAtPath(path, 1024, false);
    
    if (textureCorrente != null)
    {
        selectedImage.texture = textureCorrente;
        imageDisplay.SetActive(true);
        
        Debug.Log("Immagine caricata e mostrata!");
        
        yield return new WaitForSeconds(tempoVisualizzazione);
        
        NascondiImmagine();
        
        // Rimostra le frecce e continua il gioco
        if (arrowController != null)
        {
            arrowController.MostraFreccia();
            arrowController.MostraNuovaFreccia();
        }
    }
    else
    {
        Debug.Log("Errore nel caricamento dell'immagine");
        // In caso di errore, rimostra comunque le frecce
        if (arrowController != null)
        {
            arrowController.MostraFreccia();
            arrowController.MostraNuovaFreccia();
        }
    }
}
    
    public void NascondiImmagine()
    {
        imageDisplay.SetActive(false);
        
        // Libera memoria
        if (textureCorrente != null)
        {
            Destroy(textureCorrente);
            textureCorrente = null;
        }
        
        Debug.Log("Immagine nascosta");
    }
}