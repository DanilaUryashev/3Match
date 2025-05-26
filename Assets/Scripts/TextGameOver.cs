using TMPro;
using UnityEngine;

public class TextGameOver : MonoBehaviour
{
    TextMeshProUGUI text;

    public string textWin;
    public string textLose;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void SelectText(bool win) 
    {
        if (win)
        {
            text.text = textWin;
        }
        else 
        {
        text.text = textLose;
        }
    }
}
