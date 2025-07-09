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
    
    public void MostraImmagineCasuale()
{
    Debug.Log("Richiesta di mostrare immagine dalla galleria!");
    
    // Apri direttamente la galleria - NativeGallery gestisce automaticamente i permessi
    NativeGallery.GetImageFromGallery(OnImageSelected, "Seleziona un'immagine rilassante");
}
    
    private void OnImageSelected(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Nessuna immagine selezionata");
            return;
        }
        
        Debug.Log($"Immagine selezionata: {path}");
        
        // Carica la texture dall'immagine selezionata
        StartCoroutine(LoadImageCoroutine(path));
    }
    
    private System.Collections.IEnumerator LoadImageCoroutine(string path)
    {
        // Carica l'immagine come texture
        textureCorrente = NativeGallery.LoadImageAtPath(path, 1024); // Max 1024px per performance
        
        if (textureCorrente != null)
        {
            // Mostra l'immagine
            selectedImage.texture = textureCorrente;
            imageDisplay.SetActive(true);
            
            Debug.Log("Immagine caricata e mostrata!");
            
            // Nasconde dopo il tempo impostato
            yield return new WaitForSeconds(tempoVisualizzazione);
            
            NascondiImmagine();
        }
        else
        {
            Debug.Log("Errore nel caricamento dell'immagine");
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