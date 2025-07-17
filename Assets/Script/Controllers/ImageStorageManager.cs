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
	
    [Header("Modalità Cancellazione")]
    public Button buttonModificaImmagini;         // Pulsante per entrare/uscire da modalità cancellazione
    public GameObject prefabPulsanteAggiungi;    // Prefab del pulsante + per aggiungere immagini
    public TextMeshProUGUI testoModifica;        // Testo "MODIFICA"
    public TextMeshProUGUI testoFine;            // Testo "FINE"

    private bool modalitaCancellazione = false;
    private float ultimoToggle = 0f;
    
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
		
        // Configura il pulsante modalità modifica
        if (buttonModificaImmagini != null)
            buttonModificaImmagini.onClick.AddListener(ToggleModalitaCancellazione);
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
        Debug.Log("=== INIZIO AggiornaUI ===");    
           
        // Mostra/nascondi testo placeholder
        if (testoPlaceholder != null)
        {
            testoPlaceholder.gameObject.SetActive(immaginiSalvate.Count == 0);
        }
        
        // Pulisci le anteprime esistenti
        if (areaImmagini != null)
        {
            foreach (Transform child in areaImmagini)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.Log("ERRORE: areaImmagini è NULL!");
            return;
        }
        
        // Crea anteprime delle immagini esistenti
        if (prefabAnteprima != null && areaImmagini != null)
        {
            foreach (string percorsoImmagine in immaginiSalvate)
            {
                GameObject nuovaAnteprima = Instantiate(prefabAnteprima, areaImmagini);
                
                // Configura l'immagine
                Image imageComponent = nuovaAnteprima.GetComponentInChildren<Image>();
                if (imageComponent != null)
                {
                    Texture2D textureOriginale = NativeGallery.LoadImageAtPath(percorsoImmagine, 512, false);
                    if (textureOriginale != null)
                    {
                        Sprite anteprimaSprite = CreaAnteprimaQuadrata(textureOriginale, 150);
                        if (anteprimaSprite != null)
                        {
                            imageComponent.sprite = anteprimaSprite;
                        }
                    }
                }
                
                // Configura il comportamento del pulsante
                Button buttonAnteprima = nuovaAnteprima.GetComponentInChildren<Button>();
                if (buttonAnteprima != null)
                {
                    string percorsoPerButton = percorsoImmagine;
                    
                    if (modalitaCancellazione)
                    {
                        // Modalità cancellazione: mostra overlay e cancella al click dell'overlay
                        AggiungiIconaCancellazione(nuovaAnteprima);
                        
                        // Trova il pulsante dell'overlay appena creato
                        Button[] buttons = nuovaAnteprima.GetComponentsInChildren<Button>();
                        Button overlayButton = null;
                        
                        foreach (Button btn in buttons)
                        {
                            if (btn.name != buttonAnteprima.name)
                            {
                                overlayButton = btn;
                                break;
                            }
                        }
                        
                        if (overlayButton != null)
                        {
                            overlayButton.onClick.AddListener(() => {
                                if (Application.platform == RuntimePlatform.Android)
                                {
                                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                                    AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
                                    AndroidJavaObject toastInstance = toast.CallStatic<AndroidJavaObject>("makeText", activity, "Immagine cancellata", 0);
                                    toastInstance.Call("show");
                                }
                                RimuoviImmagine(percorsoPerButton);
                            });
                        }
                        
                        // Il pulsante dell'immagine originale non fa nulla in modalità cancellazione
                        buttonAnteprima.onClick.RemoveAllListeners();
                    }
                    else
                    {
                        // Modalità normale: mostra immagine piena al click
                        buttonAnteprima.onClick.RemoveAllListeners();
                        buttonAnteprima.onClick.AddListener(() => {
                            MostraImmaginePienaSchermata(percorsoPerButton);
                        });
                    }
                }
            }
        }
        
        // Aggiungi il pulsante "+" per aggiungere nuove immagini (solo se non in modalità cancellazione)
        if (!modalitaCancellazione && prefabPulsanteAggiungi != null && areaImmagini != null)
        {
            GameObject pulsanteAggiungi = Instantiate(prefabPulsanteAggiungi, areaImmagini);
            Button buttonAggiungi = pulsanteAggiungi.GetComponentInChildren<Button>();
            if (buttonAggiungi != null)
            {
                buttonAggiungi.onClick.AddListener(AggiungiNuovaImmagine);
            }
        }
        
        Debug.Log($"=== FINE AggiornaUI - {immaginiSalvate.Count} immagini caricate ===");
    }

    private void AggiungiIconaCancellazione(GameObject anteprima)
    {
        // Crea un overlay nero semi-trasparente che copre tutta l'immagine
        GameObject overlayNero = new GameObject("OverlayCancellazione");
        overlayNero.transform.SetParent(anteprima.transform, false);
        
        // Posiziona l'overlay per coprire tutta l'anteprima
        RectTransform rectOverlay = overlayNero.AddComponent<RectTransform>();
        rectOverlay.anchorMin = Vector2.zero;
        rectOverlay.anchorMax = Vector2.one;
        rectOverlay.sizeDelta = Vector2.zero;
        rectOverlay.anchoredPosition = Vector2.zero;
        
        // Aggiungi sfondo nero semi-trasparente
        Image imageOverlay = overlayNero.AddComponent<Image>();
        imageOverlay.color = new Color(0, 0, 0, 0.7f); // Nero con 70% di opacità
        
        // Crea la X bianca al centro
        GameObject testoX = new GameObject("TestoX");
        testoX.transform.SetParent(overlayNero.transform, false);
        
        TextMeshProUGUI textX = testoX.AddComponent<TextMeshProUGUI>();
        textX.text = "X";
        textX.fontSize = 40; // X grande e ben visibile
        textX.color = Color.white;
        textX.alignment = TextAlignmentOptions.Center;
        textX.fontStyle = FontStyles.Bold;
        
        // Posiziona la X al centro dell'overlay
        RectTransform rectTestoX = testoX.GetComponent<RectTransform>();
        rectTestoX.anchorMin = Vector2.zero;
        rectTestoX.anchorMax = Vector2.one;
        rectTestoX.sizeDelta = Vector2.zero;
        rectTestoX.anchoredPosition = Vector2.zero;
        
        // Rendi tutto l'overlay cliccabile per cancellare
        Button buttonOverlay = overlayNero.AddComponent<Button>();
        buttonOverlay.targetGraphic = imageOverlay;
    }
    		
    public int NumeroImmaginiCaricate()
    {
        return immaginiSalvate.Count;
    }
	
    public void ToggleModalitaCancellazione()
    {
        // Debounce: evita chiamate multiple troppo rapide
        if (Time.time - ultimoToggle < 0.5f)
        {
            Debug.Log("Toggle troppo rapido - ignorato");
            return;
        }
        ultimoToggle = Time.time;
        
        modalitaCancellazione = !modalitaCancellazione;
        
        Debug.Log($"METODO CHIAMATO! Modalità: {modalitaCancellazione}");
        
        // PRIMA chiama AggiornaUI per gestire le X di cancellazione
        AggiornaUI();
        
        // POI usa una Coroutine per cambiare il testo dopo un piccolo delay
        StartCoroutine(CambiaTestoPulsanteDopoDelay());
    }

    private System.Collections.IEnumerator CambiaTestoPulsanteDopoDelay()
    {
        // Aspetta un frame per essere sicuri che AggiornaUI abbia finito
        yield return null;
        
        TextMeshProUGUI[] tuttiITesti = FindObjectsOfType<TextMeshProUGUI>();
        
        // Cerca specificatamente il pulsante MODIFICA
        for (int i = 0; i < tuttiITesti.Length; i++)
        {
            if (tuttiITesti[i].text == "MODIFICA" || tuttiITesti[i].text == "FINE")
            {
                string nuovoTesto = modalitaCancellazione ? "FINE" : "MODIFICA";
                tuttiITesti[i].text = nuovoTesto;
                Debug.Log($"Cambiato pulsante '{tuttiITesti[i].text}' in: '{nuovoTesto}' al numero {i}");
                break; // Ferma dopo aver trovato il primo
            }
        }
    }
    
    private void MostraImmaginePienaSchermata(string percorsoImmagine)
    {
        Debug.Log($"Mostrando immagine a schermo pieno: {percorsoImmagine}");
        
        // Qui userai l'ImageManager esistente per mostrare l'immagine
        ImageManager imageManager = GetComponent<ImageManager>();
        if (imageManager != null)
        {
            // Usa il sistema esistente per mostrare l'immagine
            StartCoroutine(MostraImmagineTempCoroutine(percorsoImmagine));
        }
    }

    private System.Collections.IEnumerator MostraImmagineTempCoroutine(string percorso)
    {
        ImageManager imageManager = GetComponent<ImageManager>();
        if (imageManager != null)
        {
            // Carica la texture
            Texture2D texture = NativeGallery.LoadImageAtPath(percorso, 1024, false);
            if (texture != null)
            {
                imageManager.selectedImage.texture = texture;
                // Calcola l'aspect ratio corretto
                float aspectRatio = (float)texture.width / texture.height;
                RectTransform rectTransform = imageManager.selectedImage.GetComponent<RectTransform>();
                if (aspectRatio > 1) // Immagine landscape
                {
                    rectTransform.sizeDelta = new Vector2(800, 800 / aspectRatio);
                }
                else // Immagine portrait
                {
                    rectTransform.sizeDelta = new Vector2(600 * aspectRatio, 600);
                }
                imageManager.imageDisplay.SetActive(true);
                
                // Aspetta 5 secondi o fino a quando l'utente tocca
                float tempoInizio = Time.time;
                while (Time.time - tempoInizio < 5f && !Input.GetMouseButtonDown(0))
                {
                    yield return null;
                }
                
                // Nascondi l'immagine
                imageManager.imageDisplay.SetActive(false);
                if (texture != null)
                {
                    Destroy(texture);
                }
            }
        }
    }
	
    private Sprite CreaAnteprimaQuadrata(Texture2D textureOriginale, int dimensioneQuadrata)
    {
        if (textureOriginale == null) return null;
        
        int larghezzaOriginale = textureOriginale.width;
        int altezzaOriginale = textureOriginale.height;
        
        // Trova la dimensione più piccola per determinare il lato del quadrato di crop
        int latoCrop = Mathf.Min(larghezzaOriginale, altezzaOriginale);
        
        // Calcola l'offset per centrare il crop
        int offsetX = (larghezzaOriginale - latoCrop) / 2;
        int offsetY = (altezzaOriginale - latoCrop) / 2;
        
        Debug.Log($"Crop: {larghezzaOriginale}x{altezzaOriginale} -> quadrato {latoCrop}x{latoCrop}, offset: ({offsetX},{offsetY})");
        
        // Crea direttamente uno Sprite con il crop centrato
        Rect cropRect = new Rect(offsetX, offsetY, latoCrop, latoCrop);
        Sprite spriteQuadrato = Sprite.Create(textureOriginale, cropRect, new Vector2(0.5f, 0.5f));
        
        return spriteQuadrato;
    }
}