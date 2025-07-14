using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class CalendarUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject calendarPanel;
    public Transform daysContainer;
    public GameObject dayButtonPrefab;
    public TextMeshProUGUI monthYearText;
    public Button prevMonthButton;
    public Button nextMonthButton;

    [Header("Statistics Display")]
    public TextMeshProUGUI totalSwipesText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI totalDaysText;

    [Header("Day Details Area")]
    public GameObject dayDetailsArea;
    public TextMeshProUGUI selectedDayTitle;
    public TextMeshProUGUI selectedDayContent;

    [Header("Reset Statistics")]
    public Button resetStatisticsButton;

    private DateTime currentMonth;
    private StatisticsManager statsManager;
    private List<Button> dayButtons = new List<Button>();
    private Button selectedButton = null;

    private float tempoAperturaCalendario = 0f;
    private float delayChiusura = 0.5f;

    void Start()
    {
        statsManager = FindObjectOfType<StatisticsManager>();
        currentMonth = DateTime.Now;

        // TROVA dayDetailsArea correttamente nella gerarchia
        if (dayDetailsArea == null && calendarPanel != null)
        {
            Debug.Log("Cercando DayDetailsArea nel CalendarContainer...");

            // Cerca dentro CalendarContainer
            Transform calendarContainer = calendarPanel.transform.Find("CalendarContainer");
            if (calendarContainer != null)
            {
                Transform dayDetailsTransform = calendarContainer.Find("DayDetailsArea");
                if (dayDetailsTransform != null)
                {
                    dayDetailsArea = dayDetailsTransform.gameObject;
                    Debug.Log("DayDetailsArea trovato!");
                }
                else
                {
                    Debug.LogError("DayDetailsArea NON trovato in CalendarContainer!");
                }
            }
            else
            {
                Debug.LogError("CalendarContainer NON trovato!");
            }
        }

        // TROVA i componenti di testo all'interno di DayDetailsArea
        if (dayDetailsArea != null)
        {
            if (selectedDayTitle == null)
            {
                selectedDayTitle = dayDetailsArea.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"selectedDayTitle trovato: {selectedDayTitle != null}");
            }

            if (selectedDayContent == null)
            {
                // Trova il secondo TextMeshProUGUI (quello del contenuto)
                TextMeshProUGUI[] allTexts = dayDetailsArea.GetComponentsInChildren<TextMeshProUGUI>();
                if (allTexts.Length > 1)
                {
                    selectedDayContent = allTexts[1]; // Il secondo è il contenuto
                }
                Debug.Log($"selectedDayContent trovato: {selectedDayContent != null}");
            }
        }

        // VERIFICA FINALE
        Debug.Log($"=== VERIFICA FINALE ===");
        Debug.Log($"calendarPanel: {calendarPanel != null}");
        Debug.Log($"dayDetailsArea: {dayDetailsArea != null}");
        Debug.Log($"selectedDayTitle: {selectedDayTitle != null}");
        Debug.Log($"selectedDayContent: {selectedDayContent != null}");
        Debug.Log($"statsManager: {statsManager != null}");

        if (resetStatisticsButton != null)
            resetStatisticsButton.onClick.AddListener(ShowResetConfirmation);

        // Configura pulsanti
        if (prevMonthButton != null)
            prevMonthButton.onClick.AddListener(() => ChangeMonth(-1));

        if (nextMonthButton != null)
            nextMonthButton.onClick.AddListener(() => ChangeMonth(1));

        // Inizializza calendario nascosto
        if (calendarPanel != null)
            calendarPanel.SetActive(false);

        if (dayDetailsArea != null)
            dayDetailsArea.SetActive(false);
    }

    public void ShowCalendar()
    {
        if (calendarPanel != null)
        {
            calendarPanel.SetActive(true);
            tempoAperturaCalendario = Time.time; // Segna il momento di apertura
            UpdateGlobalStatistics();
            GenerateCalendar();

            // Nascondi area dettagli all'apertura
            if (dayDetailsArea != null)
                dayDetailsArea.SetActive(false);

            // Reset testo di default
            if (selectedDayTitle != null)
                selectedDayTitle.text = "Seleziona un giorno";
            if (selectedDayContent != null)
                selectedDayContent.text = "";
        }
    }

    void Update()
    {
        // Gestisci chiusura calendario toccando fuori
        if (calendarPanel != null && calendarPanel.activeInHierarchy)
        {
            // Non permettere chiusura immediata dopo apertura
            if (Time.time - tempoAperturaCalendario < delayChiusura)
            {
                return; // Esci senza fare nulla
            }

            if (Input.GetMouseButtonUp(0)) // Solo al rilascio del tocco
            {
                Vector2 touchPosition = Input.mousePosition;

                // Controlla se il tocco è fuori dal calendario
                if (!IsPointInsideCalendar(touchPosition))
                {
                    CloseCalendar();
                }
            }
        }
    }

    private bool IsPointInsideCalendar(Vector2 screenPoint)
    {
        if (calendarPanel == null) return false;

        // Prova prima con il CalendarContainer
        Transform calendarContainer = calendarPanel.transform.Find("CalendarContainer");
        RectTransform targetRect = null;

        if (calendarContainer != null)
        {
            targetRect = calendarContainer.GetComponent<RectTransform>();
        }
        else
        {
            // Fallback al calendarPanel stesso
            targetRect = calendarPanel.GetComponent<RectTransform>();
        }

        if (targetRect == null) return false;

        // Converti il punto dello schermo in coordinate locali
        Vector2 localPoint;
        bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect, screenPoint, null, out localPoint);

        if (!result) return false;

        return targetRect.rect.Contains(localPoint);
    }

    public void CloseCalendar()
    {
        if (calendarPanel != null)
            calendarPanel.SetActive(false);

        if (dayDetailsArea != null)
            dayDetailsArea.SetActive(false);
    }

    private void ChangeMonth(int direction)
    {
        currentMonth = currentMonth.AddMonths(direction);
        GenerateCalendar();

        // Nascondi dettagli quando cambi mese
        if (dayDetailsArea != null)
            dayDetailsArea.SetActive(false);
        selectedButton = null;

        if (selectedDayTitle != null)
            selectedDayTitle.text = "Seleziona un giorno";
        if (selectedDayContent != null)
            selectedDayContent.text = "";
    }

    private void GenerateCalendar()
    {
        // Aggiorna titolo mese/anno
        if (monthYearText != null)
        {
            monthYearText.text = currentMonth.ToString("MMMM yyyy");
        }

        // Pulisci giorni esistenti
        ClearDayButtons();

        // Calcola primo giorno del mese e numero di giorni
        DateTime firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

        // Aggiungi spazi vuoti per i giorni prima del primo del mese
        for (int i = 0; i < startDayOfWeek; i++)
        {
            CreateEmptyDayButton();
        }

        // Crea pulsanti per ogni giorno del mese
        for (int day = 1; day <= daysInMonth; day++)
        {
            CreateDayButton(day);
        }
    }

    private void CreateEmptyDayButton()
    {
        // NON CREARE NULLA - rimuoviamo i giorni vuoti completamente
        // Questo metodo ora non fa niente, i giorni vuoti semplicemente non esistono
    }

    private void CreateDayButton(int day)
    {
        if (dayButtonPrefab != null && daysContainer != null)
        {
            GameObject newDayButton = Instantiate(dayButtonPrefab, daysContainer);

            // Imposta testo del giorno
            TextMeshProUGUI dayText = newDayButton.GetComponentInChildren<TextMeshProUGUI>();
            if (dayText != null)
                dayText.text = day.ToString();

            // Configura pulsante
            Button dayButton = newDayButton.GetComponent<Button>();
            if (dayButton != null)
            {
                DateTime dayDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
                string dateString = dayDate.ToString("yyyy-MM-dd");

                // Controlla se ci sono statistiche per questo giorno
                DayStatistics dayStats = null;
                if (statsManager != null)
                {
                    dayStats = statsManager.GetDayStatistics(dateString);
                }

                // IMPOSTA COLORE INIZIALE CORRETTO
                Image buttonImage = dayButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (dayStats != null && dayStats.swipeCount > 0)
                    {
                        // Ha dati: blu chiaro
                        buttonImage.color = new Color(0.573f, 0.722f, 0.945f, 1f);
                    }
                    else
                    {
                        // Non ha dati: grigio
                        buttonImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                }

                // Aggiunge listener per click
                dayButton.onClick.AddListener(() => ShowDayDetails(dateString, dayButton));

                dayButtons.Add(dayButton);
            }
        }
    }

    private void ClearDayButtons()
    {
        dayButtons.Clear();
        selectedButton = null;

        if (daysContainer != null)
        {
            foreach (Transform child in daysContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ShowDayDetails(string dateString, Button clickedButton)
    {
        if (dayDetailsArea == null || statsManager == null)
        {
            return;
        }

        // Reset colore del pulsante precedentemente selezionato
        if (selectedButton != null && selectedButton != clickedButton)
        {
            Image prevButtonImage = selectedButton.GetComponent<Image>();
            if (prevButtonImage != null)
            {
                // Ottieni la data del pulsante precedente
                string prevDateString = GetDateFromButton(selectedButton);

                if (!string.IsNullOrEmpty(prevDateString))
                {
                    DayStatistics prevStats = statsManager.GetDayStatistics(prevDateString);

                    if (prevStats != null && prevStats.swipeCount > 0)
                    {
                        // Aveva dati: torna al blu chiaro
                        prevButtonImage.color = new Color(0.573f, 0.722f, 0.945f, 1f);
                    }
                    else
                    {
                        // Non aveva dati: torna al grigio
                        prevButtonImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                }
            }
        }

        // Imposta il nuovo pulsante selezionato come bianco
        selectedButton = clickedButton;
        Image currentButtonImage = clickedButton.GetComponent<Image>();
        if (currentButtonImage != null)
        {
            currentButtonImage.color = Color.white; // Bianco per selezione
        }

        // Resto del codice per mostrare i dettagli rimane uguale
        DayStatistics dayStats = statsManager.GetDayStatistics(dateString);

        DateTime date = DateTime.Parse(dateString);
        string formattedDate = date.ToString("dd MMMM yyyy");

        if (selectedDayTitle != null)
            selectedDayTitle.text = formattedDate;

        string content = "";

        if (dayStats != null && dayStats.swipeCount > 0)
        {
            content += $"Swipe eseguiti: {dayStats.swipeCount}\n\n";

            int totalMinutes = Mathf.RoundToInt(dayStats.sessionTime / 60f);
            if (totalMinutes > 0)
            {
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;

                if (hours > 0)
                    content += $"Tempo totale: {hours}h {minutes}m\n\n";
                else
                    content += $"Tempo totale: {minutes}m\n\n";
            }
            else
            {
                content += $"Tempo totale: {Mathf.RoundToInt(dayStats.sessionTime)}s\n\n";
            }

            content += $"Sessioni: {dayStats.sessionDurations.Count}\n\n";

            if (dayStats.sessionDurations.Count > 0)
            {
                float avgDuration = 0f;
                foreach (float duration in dayStats.sessionDurations)
                {
                    avgDuration += duration;
                }
                avgDuration /= dayStats.sessionDurations.Count;

                int avgMinutes = Mathf.RoundToInt(avgDuration / 60f);
                if (avgMinutes > 0)
                    content += $"Durata media: {avgMinutes}m";
                else
                    content += $"Durata media: {Mathf.RoundToInt(avgDuration)}s";
            }
        }
        else
        {
            content = "Nessuna attività registrata per questo giorno.";
        }

        if (selectedDayContent != null)
            selectedDayContent.text = content;

        dayDetailsArea.SetActive(true);
    }

    private string GetDateFromButton(Button button)
    {
        if (button == null) return "";

        // Trova il componente TextMeshProUGUI del pulsante
        TextMeshProUGUI dayText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (dayText != null && int.TryParse(dayText.text, out int day))
        {
            // Costruisci la data usando il mese corrente
            DateTime dayDate = new DateTime(currentMonth.Year, currentMonth.Month, day);
            return dayDate.ToString("yyyy-MM-dd");
        }
        return "";
    }

    private void UpdateGlobalStatistics()
    {
        if (statsManager == null) return;

        StatisticsData stats = statsManager.GetStatistics();

        if (totalSwipesText != null)
            totalSwipesText.text = $"Swipe totali: {stats.totalSwipes}";

        if (totalTimeText != null)
        {
            StatisticsData statisticsData = statsManager.GetStatistics();
            int totalMinutes = Mathf.RoundToInt(statisticsData.totalTime / 60f);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            int seconds = Mathf.RoundToInt(statisticsData.totalTime % 60f);

            if (hours > 0)
                totalTimeText.text = $"Tempo totale: {hours}h {minutes}m {seconds}s";
            else if (minutes > 0)
                totalTimeText.text = $"Tempo totale: {minutes}m {seconds}s";
            else
                totalTimeText.text = $"Tempo totale: {seconds}s";
        }

        if (totalDaysText != null)
            totalDaysText.text = $"Giorni attivi: {stats.totalDays}";

        Debug.Log($"Statistiche globali aggiornate: {stats.totalSwipes} swipe, {stats.totalDays} giorni");
    }

     private void ShowResetConfirmation()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastInstance = toast.CallStatic<AndroidJavaObject>("makeText", activity, "Tieni premuto per 3 secondi per resettare le statistiche", 1);
            toastInstance.Call("show");
        }

        StartCoroutine(CheckLongPress());
    }

    private System.Collections.IEnumerator CheckLongPress()
    {
        float pressTime = 0f;

        while (Input.GetMouseButton(0) && pressTime < 3f)
        {
            pressTime += Time.deltaTime;
            yield return null;
        }

        if (pressTime >= 3f)
        {
            ResetAllStatistics();
        }
    }

    private void ResetAllStatistics()
    {
        if (statsManager != null)
        {
            statsManager.ResetAllStatistics();

            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass toast = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toastInstance = toast.CallStatic<AndroidJavaObject>("makeText", activity, "Statistiche resettate!", 0);
                toastInstance.Call("show");
            }

            // Aggiorna l'UI
            UpdateGlobalStatistics();
            GenerateCalendar();

            // Nascondi area dettagli
            if (dayDetailsArea != null)
                dayDetailsArea.SetActive(false);
        }
    }
}