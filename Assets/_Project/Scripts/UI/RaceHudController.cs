using BondiSimulator.Core;
using BondiSimulator.Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace BondiSimulator.UI
{
    /// <summary>
    /// Builds and updates the minimum race HUD: speed, timer, countdown and results.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceHudController : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private Font hudFont;
        [SerializeField] private float goldTimeSeconds = 90f;
        [SerializeField] private float silverTimeSeconds = 130f;
        [SerializeField] private float bronzeTimeSeconds = 180f;

        private GameManager gameManager;
        private Text speedText;
        private Text timerText;
        private Text countdownText;
        private Text resultsText;
        private Transform hudRoot;
        private float countdownClearTime;
        private float raceStartTime = -1f;

        public void SetVehicle(VehicleController playerVehicle)
        {
            vehicle = playerVehicle;
        }

        private void Awake()
        {
            ServiceLocator.TryGet(out gameManager);
            BuildHud();
        }

        private void OnEnable()
        {
            GameEvents.CountdownStarted += OnCountdownStarted;
            GameEvents.CountdownTicked += OnCountdownTicked;
            GameEvents.CountdownFinished += OnCountdownFinished;
            GameEvents.RaceStarted += OnRaceStarted;
            GameEvents.RaceFinished += OnRaceFinished;
        }

        private void OnDisable()
        {
            GameEvents.CountdownStarted -= OnCountdownStarted;
            GameEvents.CountdownTicked -= OnCountdownTicked;
            GameEvents.CountdownFinished -= OnCountdownFinished;
            GameEvents.RaceStarted -= OnRaceStarted;
            GameEvents.RaceFinished -= OnRaceFinished;
        }

        private void Update()
        {
            if (vehicle == null)
            {
                vehicle = FindFirstObjectByType<VehicleController>();
            }

            if (gameManager == null)
            {
                ServiceLocator.TryGet(out gameManager);
            }

            speedText.text = $"{(vehicle != null ? vehicle.CurrentSpeedKph : 0f):0} km/h";
            if (gameManager != null && gameManager.IsRacing && raceStartTime >= 0f)
            {
                timerText.text = FormatTime(Time.time - raceStartTime);
            }

            if (countdownText.gameObject.activeSelf && countdownClearTime > 0f && Time.time >= countdownClearTime)
            {
                countdownText.gameObject.SetActive(false);
                countdownClearTime = 0f;
            }
        }

        private void BuildHud()
        {
            GameObject canvasObject = new("HudCanvas");
            canvasObject.transform.SetParent(transform, false);
            hudRoot = canvasObject.transform;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            hudFont = hudFont != null ? hudFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (hudFont == null)
            {
                hudFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            speedText = CreateText("SpeedText", new Vector2(24f, -24f), TextAnchor.UpperLeft, 30);
            timerText = CreateText("TimerText", new Vector2(0f, -24f), TextAnchor.UpperCenter, 30);
            countdownText = CreateText("CountdownText", Vector2.zero, TextAnchor.MiddleCenter, 72);
            resultsText = CreateText("ResultsText", Vector2.zero, TextAnchor.MiddleCenter, 42);

            RectTransform timerRect = timerText.rectTransform;
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);

            RectTransform countdownRect = countdownText.rectTransform;
            countdownRect.anchorMin = Vector2.zero;
            countdownRect.anchorMax = Vector2.one;
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;

            RectTransform resultsRect = resultsText.rectTransform;
            resultsRect.anchorMin = Vector2.zero;
            resultsRect.anchorMax = Vector2.one;
            resultsRect.offsetMin = Vector2.zero;
            resultsRect.offsetMax = Vector2.zero;

            countdownText.gameObject.SetActive(false);
            resultsText.gameObject.SetActive(false);
            timerText.text = "00:00.000";
        }

        private Text CreateText(string objectName, Vector2 anchoredPosition, TextAnchor alignment, int fontSize)
        {
            GameObject textObject = new(objectName);
            textObject.transform.SetParent(hudRoot, false);

            Text text = textObject.AddComponent<Text>();
            text.font = hudFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;

            RectTransform rectTransform = text.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(420f, 120f);
            return text;
        }

        private void OnCountdownStarted(int seconds)
        {
            resultsText.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(true);
            countdownText.text = seconds.ToString();
            raceStartTime = -1f;
        }

        private void OnCountdownTicked(int remainingSeconds)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = remainingSeconds.ToString();
        }

        private void OnCountdownFinished()
        {
            countdownText.text = "GO";
            countdownClearTime = Time.time + 0.75f;
        }

        private void OnRaceStarted()
        {
            resultsText.gameObject.SetActive(false);
            raceStartTime = Time.time;
            timerText.text = "00:00.000";
        }

        private void OnRaceFinished(float elapsedSeconds)
        {
            countdownText.gameObject.SetActive(false);
            resultsText.gameObject.SetActive(true);
            resultsText.text = $"Tiempo final\n{FormatTime(elapsedSeconds)}\n{GetMedal(elapsedSeconds)}";
            raceStartTime = -1f;
        }

        private string GetMedal(float elapsedSeconds)
        {
            if (elapsedSeconds <= goldTimeSeconds)
            {
                return "Oro";
            }

            if (elapsedSeconds <= silverTimeSeconds)
            {
                return "Plata";
            }

            return elapsedSeconds <= bronzeTimeSeconds ? "Bronce" : "Sin medalla";
        }

        private static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remainingSeconds = seconds - minutes * 60f;
            return $"{minutes:00}:{remainingSeconds:00.000}";
        }
    }
}
