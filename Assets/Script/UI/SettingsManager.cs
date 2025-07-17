using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class GameSettings
{
    public int standardArrowCount = 3; // Numero base di frecce
    public float imageDisplayTime = 3f; // Tempo visualizzazione immagini
    public bool advancedSequenceEnabled = false; // Sequenza avanzata abilitata
    public List<int> advancedSequence = new List<int>(); // Sequenza personalizzata
    public int currentSequenceIndex = 0; // Indice corrente nella sequenza

    public GameSettings()
    {
        // Valori di default
        standardArrowCount = 3;
        imageDisplayTime = 3f;
        advancedSequenceEnabled = false;
        advancedSequence = new List<int> { 3, 5, 7 }; // Esempio di sequenza
        currentSequenceIndex = 0;
    }
}

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Button closeSettingsButton;

    [Header("Standard Settings")]
    public Button minusArrowButton;
    public Button plusArrowButton;
    public TextMeshProUGUI arrowCountText;

    [Header("Image Display Time")]
    public Button minusTimeButton;
    public Button plusTimeButton;
    public TextMeshProUGUI displayTimeText;

    [Header("Advanced Sequence")]
    public Toggle advancedSequenceToggle;
    public GameObject advancedSequencePanel;
    public Transform sequenceContainer;
    public GameObject sequenceStepPrefab;
    public Button addStepButton;

    [Header("Current Step Display")]
    public TextMeshProUGUI currentStepText;

    private GameSettings settings;
    private string settingsFilePath;
    private List<GameObject> sequenceStepObjects = new List<GameObject>();

    // References to other managers
    private ArrowController arrowController;
    private ImageManager imageManager;

    void Start()
    {
        PlayerPrefs.DeleteAll(); // Oppure elimina il file settings.json

        // Trova i manager necessari
        arrowController = FindObjectOfType<ArrowController>();
        imageManager = FindObjectOfType<ImageManager>();

        // Percorso file impostazioni
        string dataFolder = Path.Combine(Application.persistentDataPath, "ImmaginiRilassanti");
        settingsFilePath = Path.Combine(dataFolder, "settings.json");

        // Carica impostazioni
        LoadSettings();

        // Configura UI
        SetupUI();

        // Applica impostazioni ai manager
        ApplySettingsToManagers();

        // Nascondi panel all'inizio
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetupUI()
    {
        // Pulsanti standard arrows
        if (minusArrowButton != null)
            minusArrowButton.onClick.AddListener(() => ChangeStandardArrowCount(-1));
        if (plusArrowButton != null)
            plusArrowButton.onClick.AddListener(() => ChangeStandardArrowCount(1));

        // Pulsanti tempo visualizzazione
        if (minusTimeButton != null)
            minusTimeButton.onClick.AddListener(() => ChangeDisplayTime(-0.5f));
        if (plusTimeButton != null)
            plusTimeButton.onClick.AddListener(() => ChangeDisplayTime(0.5f));

        // Toggle sequenza avanzata
        if (advancedSequenceToggle != null)
            advancedSequenceToggle.onValueChanged.AddListener(OnAdvancedSequenceToggle);

        // Pulsante aggiungi step
        if (addStepButton != null)
            addStepButton.onClick.AddListener(AddSequenceStep);

        // Pulsante chiudi
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // Aggiorna UI iniziale
        UpdateUI();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            UpdateUI();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            SaveSettings();
            ApplySettingsToManagers();
        }
    }

    private void ChangeStandardArrowCount(int delta)
    {
        settings.standardArrowCount = Mathf.Clamp(settings.standardArrowCount + delta, 1, 20);
        UpdateUI();
    }

    private void ChangeDisplayTime(float delta)
    {
        settings.imageDisplayTime = Mathf.Clamp(settings.imageDisplayTime + delta, 0.5f, 10f);
        UpdateUI();
    }

    private void OnAdvancedSequenceToggle(bool enabled)
    {
        settings.advancedSequenceEnabled = enabled;
        UpdateUI();
    }

    private void AddSequenceStep()
    {
        settings.advancedSequence.Add(3); // Valore di default
        UpdateAdvancedSequenceUI();
    }

    private void RemoveSequenceStep(int index)
    {
        if (index >= 0 && index < settings.advancedSequence.Count && settings.advancedSequence.Count > 1)
        {
            settings.advancedSequence.RemoveAt(index);

            // Ajusta currentSequenceIndex se necessario
            if (settings.currentSequenceIndex >= settings.advancedSequence.Count)
            {
                settings.currentSequenceIndex = 0;
            }

            UpdateAdvancedSequenceUI();
        }
    }

    private void UpdateSequenceStepValue(int index, int newValue)
    {
        if (index >= 0 && index < settings.advancedSequence.Count)
        {
            settings.advancedSequence[index] = Mathf.Clamp(newValue, 1, 50);
            UpdateAdvancedSequenceUI();
        }
    }

    private void UpdateUI()
    {
        // Aggiorna testo conteggio frecce standard
        if (arrowCountText != null)
            arrowCountText.text = settings.standardArrowCount.ToString();

        // Aggiorna testo tempo visualizzazione
        if (displayTimeText != null)
            displayTimeText.text = $"{settings.imageDisplayTime:F1}s";

        // Aggiorna toggle sequenza avanzata
        if (advancedSequenceToggle != null)
            advancedSequenceToggle.isOn = settings.advancedSequenceEnabled;

        // Mostra/nascondi panel sequenza avanzata
        if (advancedSequencePanel != null)
            advancedSequencePanel.SetActive(settings.advancedSequenceEnabled);

        // Aggiorna UI sequenza avanzata
        if (settings.advancedSequenceEnabled)
        {
            UpdateAdvancedSequenceUI();
        }

        // Aggiorna display step corrente
        UpdateCurrentStepDisplay();
    }

    private void UpdateAdvancedSequenceUI()
    {
        // Pulisci step esistenti
        foreach (GameObject stepObj in sequenceStepObjects)
        {
            if (stepObj != null)
                Destroy(stepObj);
        }
        sequenceStepObjects.Clear();

        // Crea nuovi step UI
        for (int i = 0; i < settings.advancedSequence.Count; i++)
        {
            CreateSequenceStepUI(i);
        }
    }

    private void CreateSequenceStepUI(int index)
    {
        if (sequenceStepPrefab == null || sequenceContainer == null) return;

        GameObject stepObj = Instantiate(sequenceStepPrefab, sequenceContainer);

        RectTransform stepRect = stepObj.GetComponent<RectTransform>();
        if (stepRect != null)
        {
            stepRect.sizeDelta = new Vector2(500, 60); // Forza dimensioni corrette
        }

        sequenceStepObjects.Add(stepObj);

        // Trova componenti del prefab
        TextMeshProUGUI stepLabel = stepObj.transform.Find("StepLabel")?.GetComponent<TextMeshProUGUI>();
        TMP_InputField stepInput = stepObj.transform.Find("StepInput")?.GetComponent<TMP_InputField>();
        Button removeButton = stepObj.transform.Find("RemoveButton")?.GetComponent<Button>();

        // Configura label
        if (stepLabel != null)
            stepLabel.text = $"Passo {index + 1}:";

        // Configura input field
        if (stepInput != null)
        {
            stepInput.text = settings.advancedSequence[index].ToString();
            int currentIndex = index; // Cattura l'indice per il listener
            stepInput.onEndEdit.AddListener((value) => {
                if (int.TryParse(value, out int newValue))
                {
                    UpdateSequenceStepValue(currentIndex, newValue);
                }
            });
        }

        // Configura pulsante rimozione (solo se ci sono più di 1 step)
        if (removeButton != null)
        {
            removeButton.gameObject.SetActive(settings.advancedSequence.Count > 1);
            int currentIndex = index; // Cattura l'indice per il listener
            removeButton.onClick.AddListener(() => RemoveSequenceStep(currentIndex));
        }

        // Evidenzia step corrente
        Image stepBackground = stepObj.GetComponent<Image>();
        if (stepBackground != null)
        {
            if (index == settings.currentSequenceIndex)
            {
                stepBackground.color = new Color(0.8f, 1f, 0.8f, 1f); // Verde chiaro
            }
            else
            {
                stepBackground.color = Color.white;
            }
        }
    }

    private void UpdateCurrentStepDisplay()
    {
        if (currentStepText != null)
        {
            if (settings.advancedSequenceEnabled && settings.advancedSequence.Count > 0)
            {
                int currentStep = settings.currentSequenceIndex + 1;
                int totalSteps = settings.advancedSequence.Count;
                int currentArrows = settings.advancedSequence[settings.currentSequenceIndex];
                currentStepText.text = $"Step corrente: {currentStep}/{totalSteps}";
            }
            else
            {
                currentStepText.text = $"Modalità standard: {settings.standardArrowCount} frecce";
            }
        }
    }

    public int GetCurrentArrowCount()
    {
        if (settings.advancedSequenceEnabled && settings.advancedSequence.Count > 0)
        {
            return settings.advancedSequence[settings.currentSequenceIndex];
        }
        else
        {
            return settings.standardArrowCount;
        }
    }

    public void AdvanceSequence()
    {
        if (settings.advancedSequenceEnabled && settings.advancedSequence.Count > 0)
        {
            settings.currentSequenceIndex = (settings.currentSequenceIndex + 1) % settings.advancedSequence.Count;
            SaveSettings(); // Salva la progressione
        }
    }

    public float GetImageDisplayTime()
    {
        return settings.imageDisplayTime;
    }

    private void ApplySettingsToManagers()
    {
        // Applica impostazioni all'ArrowController
        if (arrowController != null)
        {
            // Dovremo modificare ArrowController per usare GetCurrentArrowCount()
            Debug.Log($"Applicando impostazioni frecce: {GetCurrentArrowCount()}");
        }

        // Applica impostazioni all'ImageManager
        if (imageManager != null)
        {
            // Dovremo modificare ImageManager per usare GetImageDisplayTime()
            Debug.Log($"Applicando tempo visualizzazione: {GetImageDisplayTime()}s");
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                settings = JsonUtility.FromJson<GameSettings>(json);
                Debug.Log("Impostazioni caricate");
            }
            else
            {
                settings = new GameSettings();
                Debug.Log("Nuove impostazioni create con valori di default");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore caricamento impostazioni: {e.Message}");
            settings = new GameSettings();
        }
    }

    private void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(settingsFilePath, json);
            Debug.Log("Impostazioni salvate");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore salvataggio impostazioni: {e.Message}");
        }
    }

    // Metodo pubblico per ottenere le impostazioni
    public GameSettings GetSettings()
    {
        return settings;
    }
}