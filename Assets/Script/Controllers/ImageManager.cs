using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject imageDisplay;       // Panel che contiene tutto
    public RawImage selectedImage;       // L'immagine vera e propria
    
    [Header("Impostazioni")]

    private Texture2D textureCorrente;
    private ImageStorageManager storageManager;
    private SettingsManager settingsManager;

    void Start()
    {
        storageManager = GetComponent<ImageStorageManager>();
        settingsManager = FindObjectOfType<SettingsManager>();
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
    
    if (textureCorrente != null)
    {
        Destroy(textureCorrente);
        textureCorrente = null;
    }
    
    // AGGIUNGI QUESTO: Libera la texture precedente prima di caricarne una nuova
    if (textureCorrente != null)
    {
        Destroy(textureCorrente);
        textureCorrente = null;
    }
    
    // AGGIUNGI ANCHE QUESTO: Pulisci l'eventuale Image component
    Image imageComponent = imageDisplay.GetComponent<Image>();
    if (imageComponent != null)
    {
        imageComponent.sprite = null;
    }
    
    textureCorrente = NativeGallery.LoadImageAtPath(path, 1024, false);
    
    if (textureCorrente != null)
    {
        selectedImage.texture = textureCorrente;
        
        // Calcola e applica l'aspect ratio corretto
        ImpostaAspectRatioCorretto();
        
        imageDisplay.SetActive(true);
        
        Debug.Log("Immagine caricata e mostrata con aspect ratio corretto!");

            float tempoVisualizzazione = 3f; // Default fallback
            if (settingsManager != null)
            {
                tempoVisualizzazione = settingsManager.GetImageDisplayTime();
            }
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
	
	private void ImpostaAspectRatioCorretto()
{
    if (textureCorrente == null || selectedImage == null) return;
    
    // Dimensione target per la dimensione maggiore
    float targetMaxSize = 1000f;
    
    // Ottieni le dimensioni originali dell'immagine
    float imgWidth = textureCorrente.width;
    float imgHeight = textureCorrente.height;
    
    // Trova la dimensione maggiore
    float maxDimension = Mathf.Max(imgWidth, imgHeight);
    
    // Calcola il fattore di scala per portare la dimensione maggiore a 1000
    float scaleFactor = targetMaxSize / maxDimension;
    
    // Applica il fattore di scala a entrambe le dimensioni
    float newWidth = imgWidth * scaleFactor;
    float newHeight = imgHeight * scaleFactor;
    
    // Applica le nuove dimensioni al RawImage
    RectTransform imageRect = selectedImage.GetComponent<RectTransform>();
    imageRect.sizeDelta = new Vector2(newWidth, newHeight);
    
    // Reset UV Rect per essere sicuri
    selectedImage.uvRect = new Rect(0, 0, 1, 1);
    
    Debug.Log($"Dimensioni finali: {newWidth}x{newHeight} (originali: {imgWidth}x{imgHeight}, fattore: {scaleFactor})");
}
}