using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class DayStatistics
{
    public string date; // formato yyyy-MM-dd
    public int swipeCount;
    public float sessionTime; // in secondi
    public List<float> sessionDurations; // durate delle singole sessioni
    
    public DayStatistics()
    {
        sessionDurations = new List<float>();
    }
}

[System.Serializable]
public class StatisticsData
{
    public List<DayStatistics> dailyStats;
    public int totalSwipes;
    public float totalTime;
    public int totalDays;
    
    public StatisticsData()
    {
        dailyStats = new List<DayStatistics>();
    }
}

public class StatisticsManager : MonoBehaviour
{
    private StatisticsData stats;
    private string dataFilePath;
    private bool sessionActive = false;
    private float sessionStartTime;
    private int sessionSwipes = 0;
    private string currentDate;
    
    void Start()
    {
        // Percorso file statistiche
        string dataFolder = Path.Combine(Application.persistentDataPath, "ImmaginiRilassanti");
        dataFilePath = Path.Combine(dataFolder, "statistics.json");
        
        LoadStatistics();
        currentDate = DateTime.Now.ToString("yyyy-MM-dd");
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && sessionActive)
        {
            EndSession();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && sessionActive)
        {
            EndSession();
        }
    }
    
    public void StartSession()
    {
        if (!sessionActive)
        {
            sessionActive = true;
            sessionStartTime = Time.time;
            sessionSwipes = 0;
            currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            
            Debug.Log($"Sessione iniziata: {currentDate}");
        }
    }

    public void EndSession()
    {
        if (sessionActive)
        {
            float sessionDuration = Time.time - sessionStartTime;

            // Salva solo se la sessione è durata almeno 5 secondi
            if (sessionDuration >= 5f)
            {
                SaveSessionData(sessionDuration);

                // RIMOSSO IL TOAST "Sessione completata"
            }

            sessionActive = false;
            Debug.Log($"Sessione terminata: {sessionSwipes} swipe, {sessionDuration:F1} secondi");
        }
    }

    public void RecordSwipe()
    {
        if (sessionActive)
        {
            sessionSwipes++;
            Debug.Log($"Swipe registrato: {sessionSwipes} nella sessione corrente");
        }
    }
    
    private void SaveSessionData(float duration)
    {
        // Trova o crea statistiche per oggi
        DayStatistics todayStats = GetOrCreateDayStats(currentDate);
        
        todayStats.swipeCount += sessionSwipes;
        todayStats.sessionTime += duration;
        todayStats.sessionDurations.Add(duration);
        
        // Aggiorna totali
        stats.totalSwipes += sessionSwipes;
        stats.totalTime += duration;
        
        // Conta giorni unici
        HashSet<string> uniqueDays = new HashSet<string>();
        foreach (var day in stats.dailyStats)
        {
            uniqueDays.Add(day.date);
        }
        stats.totalDays = uniqueDays.Count;
        
        SaveStatistics();
    }
    
    private DayStatistics GetOrCreateDayStats(string date)
    {
        // Cerca se esiste già
        foreach (var day in stats.dailyStats)
        {
            if (day.date == date)
                return day;
        }
        
        // Crea nuovo
        DayStatistics newDay = new DayStatistics();
        newDay.date = date;
        stats.dailyStats.Add(newDay);
        return newDay;
    }
    
    private void LoadStatistics()
    {
        try
        {
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                stats = JsonUtility.FromJson<StatisticsData>(json);
                Debug.Log($"Statistiche caricate: {stats.dailyStats.Count} giorni");
            }
            else
            {
                stats = new StatisticsData();
                Debug.Log("Nuovo file statistiche creato");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore caricamento statistiche: {e.Message}");
            stats = new StatisticsData();
        }
    }
    
    private void SaveStatistics()
    {
        try
        {
            string json = JsonUtility.ToJson(stats, true);
            File.WriteAllText(dataFilePath, json);
            Debug.Log("Statistiche salvate");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore salvataggio statistiche: {e.Message}");
        }
    }
    
    // Metodi pubblici per accesso ai dati
    public StatisticsData GetStatistics()
    {
        return stats;
    }

    public DayStatistics GetDayStatistics(string date)
    {
        foreach (var day in stats.dailyStats)
        {
            if (day.date == date)
            {
                Debug.Log($"Trovati dati per {date}: {day.swipeCount} swipe");
                return day;
            }
        }
        Debug.Log($"Nessun dato trovato per {date}");
        return null; // Nessun dato per questo giorno
    }

    public string GetFormattedTotalTime()
    {
        int totalMinutes = Mathf.RoundToInt(stats.totalTime / 60f);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        
        if (hours > 0)
            return $"{hours}h {minutes}m";
        else
            return $"{minutes}m";
    }

    public void ResetAllStatistics()
    {
        // Reset tutti i dati
        stats = new StatisticsData();

        // Salva il file vuoto
        SaveStatistics();

        Debug.Log("Tutte le statistiche sono state resettate");
    }
}