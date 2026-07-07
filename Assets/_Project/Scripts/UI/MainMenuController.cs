using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BondiSimulator.UI
{
    /// <summary>
    /// Builds the phase 1 main menu and routes the player into the 620 scenario.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string routeSceneName = "Route_620_SanJusto_CarlosCasares";
        [SerializeField] private Font menuFont;

        private void Awake()
        {
            BuildMenu();
        }

        public void StartRoute()
        {
            SceneManager.LoadScene(routeSceneName);
        }

        private void BuildMenu()
        {
            GameObject canvasObject = new("MainMenuCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            menuFont = menuFont != null ? menuFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (menuFont == null)
            {
                menuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            CreateTitle(canvasObject.transform);
            CreateStartButton(canvasObject.transform);
        }

        private void CreateTitle(Transform parent)
        {
            Text title = CreateText("Title", parent, "Bondi Simulator", 72, TextAnchor.MiddleCenter, Color.white);
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(900f, 120f);
        }

        private void CreateStartButton(Transform parent)
        {
            GameObject buttonObject = new("Start620Button");
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.86f, 0.18f, 1f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(StartRoute);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -20f);
            buttonRect.sizeDelta = new Vector2(420f, 96f);

            Text label = CreateText("Label", buttonObject.transform, "Iniciar linea 620", 34, TextAnchor.MiddleCenter, Color.black);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private Text CreateText(string objectName, Transform parent, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = new(objectName);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = menuFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
