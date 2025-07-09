using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    [Header("Impostazioni Swipe")]
    public float distanzaMinima = 50f;  // Distanza minima per considerare uno swipe
    public float tempoMassimo = 1f;     // Tempo massimo per lo swipe
    
    private Vector2 puntoInizio;
    private float tempoInizio;
    private bool touchAttivo = false;
    
    // Eventi per le direzioni
    public System.Action OnSwipeUp;
    public System.Action OnSwipeDown;
    public System.Action OnSwipeLeft;
    public System.Action OnSwipeRight;
    
    void Update()
    {
        RilevaSwipe();
    }
    
    void RilevaSwipe()
    {
        // Mouse/Touch iniziato
        if (Input.GetMouseButtonDown(0))
        {
            puntoInizio = Input.mousePosition;
            tempoInizio = Time.time;
            touchAttivo = true;
            Debug.Log("Touch iniziato");
        }
        
        // Mouse/Touch finito
        if (Input.GetMouseButtonUp(0) && touchAttivo)
        {
            Vector2 puntoFine = Input.mousePosition;
            float tempoTrascorso = Time.time - tempoInizio;
            
            if (tempoTrascorso <= tempoMassimo)
            {
                Vector2 direzione = puntoFine - puntoInizio;
                float distanza = direzione.magnitude;
                
                if (distanza >= distanzaMinima)
                {
                    // Normalizza la direzione
                    direzione.Normalize();
                    
                    // Determina la direzione principale
                    if (Mathf.Abs(direzione.x) > Mathf.Abs(direzione.y))
                    {
                        // Swipe orizzontale
                        if (direzione.x > 0)
                        {
                            Debug.Log("Swipe Destra");
                            OnSwipeRight?.Invoke();
                        }
                        else
                        {
                            Debug.Log("Swipe Sinistra");
                            OnSwipeLeft?.Invoke();
                        }
                    }
                    else
                    {
                        // Swipe verticale
                        if (direzione.y > 0)
                        {
                            Debug.Log("Swipe Su");
                            OnSwipeUp?.Invoke();
                        }
                        else
                        {
                            Debug.Log("Swipe Giù");
                            OnSwipeDown?.Invoke();
                        }
                    }
                }
            }
            
            touchAttivo = false;
        }
    }
}