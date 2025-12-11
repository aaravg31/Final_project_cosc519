using UnityEngine;

/// <summary>
/// AR Sanity UI Bar - Displays sanity as a visual bar on screen
/// Uses OnGUI for maximum compatibility across all platforms
/// Changes color from green (calm) to red (stressed)
/// </summary>
public class ARSanityUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the SanitySystem. Will auto-find if null.")]
    public SanitySystem sanitySystem;

    [Header("UI Settings")]
    [Tooltip("Width of the sanity bar in pixels.")]
    public float barWidth = 400f;
    [Tooltip("Height of the sanity bar in pixels.")]
    public float barHeight = 40f;
    [Tooltip("Margin from screen edge.")]
    public float margin = 30f;
    [Tooltip("Position: 0=Top Left, 1=Top Center, 2=Top Right")]
    public int position = 1;
    [Tooltip("Show label text.")]
    public bool showLabel = true;
    [Tooltip("Font size for label.")]
    public int fontSize = 32;

    [Header("Colors")]
    [Tooltip("Color when sanity is full (calm).")]
    public Color calmColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    [Tooltip("Color when sanity is low (stressed).")]
    public Color stressedColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [Tooltip("Background bar color.")]
    public Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    [Tooltip("Border color.")]
    public Color borderColor = new Color(1f, 1f, 1f, 0.6f);

    // Textures for drawing
    private Texture2D whiteTexture;
    private GUIStyle labelStyle;
    private bool initialized = false;

    void Start()
    {
        // Find SanitySystem
        if (sanitySystem == null)
        {
            sanitySystem = FindObjectOfType<SanitySystem>();
        }

        if (sanitySystem == null)
        {
            Debug.LogWarning("ARSanityUI: SanitySystem not found!");
        }

        InitializeTextures();
        Debug.Log("ARSanityUI: Initialized with OnGUI method");
    }

    void InitializeTextures()
    {
        // Create a white texture for drawing colored rectangles
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        // Create label style
        labelStyle = new GUIStyle();
        labelStyle.fontSize = fontSize;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;

        initialized = true;
    }

    void OnGUI()
    {
        if (!initialized || sanitySystem == null)
            return;

        // Calculate fill amount
        float fillAmount = sanitySystem.currentSanity / sanitySystem.maxSanity;
        float stress = 1f - fillAmount;

        // FIXED: Use smaller, more conservative scaling
        float scale = 1f;  // No scaling - use fixed sizes
        
        float scaledWidth = 300f;   // Fixed width
        float scaledHeight = 30f;   // Fixed height
        float scaledMargin = 20f;
        float scaledFontSize = 18f;
        float borderWidth = 2f;

        // Update label style font size
        labelStyle.fontSize = Mathf.RoundToInt(scaledFontSize);

        // Calculate position
        float x;
        switch (position)
        {
            case 0: // Top Left
                x = scaledMargin;
                break;
            case 2: // Top Right
                x = Screen.width - scaledWidth - scaledMargin;
                break;
            default: // Top Center (1)
                x = (Screen.width - scaledWidth) / 2f;
                break;
        }
        float y = scaledMargin;

        // Draw label if enabled
        float labelHeight = showLabel ? scaledFontSize + 10f : 0f;
        if (showLabel)
        {
            string labelText;
            Color labelColor;
            
            // Show actual values
            int sanityValue = Mathf.RoundToInt(sanitySystem.currentSanity);
            int maxSanity = Mathf.RoundToInt(sanitySystem.maxSanity);
            int stressPercent = Mathf.RoundToInt(stress * 100);
            
            if (stress > 0.7f)
            {
                labelText = $"⚠ CRITICAL! Sanity: {sanityValue}/{maxSanity} | Stress: {stressPercent}%";
                labelColor = stressedColor;
            }
            else if (stress > 0.4f)
            {
                labelText = $"Sanity: {sanityValue}/{maxSanity} | Stress: {stressPercent}%";
                labelColor = Color.Lerp(Color.white, stressedColor, stress);
            }
            else
            {
                labelText = $"Sanity: {sanityValue}/{maxSanity} | Stress: {stressPercent}%";
                labelColor = Color.white;
            }

            labelStyle.normal.textColor = labelColor;
            GUI.Label(new Rect(x, y, scaledWidth, labelHeight), labelText, labelStyle);
            y += labelHeight;
        }

        // Draw border
        DrawRect(new Rect(x, y, scaledWidth, scaledHeight), borderColor);

        // Draw background
        Rect bgRect = new Rect(x + borderWidth, y + borderWidth, 
                               scaledWidth - borderWidth * 2, scaledHeight - borderWidth * 2);
        DrawRect(bgRect, backgroundColor);

        // Draw fill bar
        float fillWidth = (bgRect.width - borderWidth * 2) * fillAmount;
        Rect fillRect = new Rect(bgRect.x + borderWidth, bgRect.y + borderWidth,
                                 fillWidth, bgRect.height - borderWidth * 2);
        Color fillColor = Color.Lerp(stressedColor, calmColor, fillAmount);
        DrawRect(fillRect, fillColor);

        // Draw value text on bar
        string valueText = $"{Mathf.RoundToInt(sanitySystem.currentSanity)}";
        GUIStyle valueStyle = new GUIStyle(labelStyle);
        valueStyle.fontSize = Mathf.RoundToInt(scaledFontSize * 0.8f);
        valueStyle.normal.textColor = Color.white;
        
        // Add shadow effect for better readability
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(x + 2, y + 2, scaledWidth, scaledHeight), valueText, valueStyle);
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(x, y, scaledWidth, scaledHeight), valueText, valueStyle);
    }

    void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, whiteTexture);
        GUI.color = Color.white;
    }

    void OnDestroy()
    {
        if (whiteTexture != null)
        {
            Destroy(whiteTexture);
        }
    }
}

