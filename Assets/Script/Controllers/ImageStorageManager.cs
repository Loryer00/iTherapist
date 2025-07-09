using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class ImageStorageManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform areaImmagini;           // Area dove mostrare le anteprime
    public TextMeshProUGUI testoPlaceholder; // "Nessuna immagine caricata"
    public GameObject prefabAnteprima;       // Prefab per anteprime immagini
    
    [Header("Impostazioni")]
    public int maxImmagini = 20;             // Massimo numero immagini
    
    private List<string> immaginiSalvate = new List<string>();
    private List<string> immaginiUsate = new List<string>();
    private string cartellaPersistente;
    
    void Start()
    {
        // Cartella persistente dell'app
        cartellaPersistente = Path.Combine(Application.persistentDataPath, "ImmaginiRilassanti");
        
        // Crea la cartella se non esiste
        if (!Directory.Exists(cartellaPersistente))
        {
            Directory.CreateDirectory(cartellaPersistente);
            Debug.Log($"Creata cartella: {cartellaPersistente}");
        }
        
        CaricaListaImmagini();
        AggiornaUI();
    }
    
    public void AggiungiNuovaImmagine()
    {
        Debug.Log("Richiesta aggiunta nuova immagine");
        
        if (immaginiSalvate.Count >= maxImmagini)
        {
            Debug.Log($"Raggiunto limite massimo di {maxImmagini} immagini");
            return;
        }
        
        // Apri galleria per selezione
        NativeGallery.GetImageFromGallery(OnImmagineSelezionata, "Seleziona immagine rilassante");
    }
    
    private void OnImmagineSelezionata(string percorso)
    {
        if (string.IsNullOrEmpty(percorso))
        {
            Debug.Log("Nessuna immagine selezionata");
            return;
        }
        
        Debug.Log($"Immagine selezionata: {percorso}");
        SalvaImmagine(percorso);
    }
    
    private void SalvaImmagine(string percorsoOriginale)
{
    try
    {
        // Genera nome file unico
        string nomeFile = $"img_{System.DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        string percorsoDestinazione = Path.Combine(cartellaPersistente, nomeFile);
        
        // Leggi i byte direttamente dal file originale
        byte[] bytesOriginali = File.ReadAllBytes(percorsoOriginale);
        
        // Salva direttamente nella cartella dell'app
        File.WriteAllBytes(percorsoDestinazione, bytesOriginali);
        
        // Aggiungi alla lista
        immaginiSalvate.Add(percorsoDestinazione);
        
        Debug.Log($"Immagine salvata: {nomeFile}");
        AggiornaUI();
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Errore nel salvare l'immagine: {e.Message}");
    }
}
    
    private void CaricaListaImmagini()
    {
        immaginiSalvate.Clear();
        
        if (Directory.Exists(cartellaPersistente))
        {
            string[] files = Directory.GetFiles(cartellaPersistente, "*.jpg");
            foreach (string file in files)
            {
                immaginiSalvate.Add(file);
            }
            
            Debug.Log($"Caricate {immaginiSalvate.Count} immagini dalla cartella persistente");
        }
    }
    
    public string OttieniImmagineCasuale()
    {
        if (immaginiSalvate.Count == 0)
        {
            Debug.Log("Nessuna immagine disponibile");
            return null;
        }
        
        // Se abbiamo usato tutte le immagini, resettiamo la lista
        if (immaginiUsate.Count >= immaginiSalvate.Count)
        {
            immaginiUsate.Clear();
            Debug.Log("Reset lista immagini usate - tutte le immagini sono nuovamente disponibili");
        }
        
        // Trova immagini non ancora usate
        List<string> immaginiDisponibili = new List<string>();
        foreach (string img in immaginiSalvate)
        {
            if (!immaginiUsate.Contains(img))
            {
                immaginiDisponibili.Add(img);
            }
        }
        
        if (immaginiDisponibili.Count == 0)
        {
            Debug.Log("Tutte le immagini sono state usate");
            return null;
        }
        
        // Scegli casualmente
        int indice = Random.Range(0, immaginiDisponibili.Count);
        string immaginescelta = immaginiDisponibili[indice];
        
        // Aggiungi alla lista delle usate
        immaginiUsate.Add(immaginescelta);
        
        Debug.Log($"Immagine casuale scelta: {Path.GetFileName(immaginescelta)} ({immaginiUsate.Count}/{immaginiSalvate.Count} usate)");
        return immaginescelta;
    }
    
    public void RimuoviImmagine(string percorso)
    {
        try
        {
            if (File.Exists(percorso))
            {
                File.Delete(percorso);
                immaginiSalvate.Remove(percorso);
                immaginiUsate.Remove(percorso);
                
                Debug.Log($"Immagine rimossa: {Path.GetFileName(percorso)}");
                AggiornaUI();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Errore nella rimozione: {e.Message}");
        }
    }
    
    private void AggiornaUI()
    {
        // Mostra/nascondi testo placeholder
        if (testoPlaceholder != null)
        {
            testoPlaceholder.gameObject.SetActive(immaginiSalvate.Count == 0);
        }
        
        // TODO: Aggiornare le anteprime nell'area
        Debug.Log($"UI aggiornata - {immaginiSalvate.Count} immagini caricate");
    }
    
    public int NumeroImmaginiCaricate()
    {
        return immaginiSalvate.Count;
    }
}