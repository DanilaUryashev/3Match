using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textButton;
    [SerializeField] private GameObject mainText;
    [SerializeField] private GameObject board;

    [SerializeField] private GameObject HelpBoard;
    private bool openHelp; // обозначение открыта ли подсказка

    public void ViewBord(bool win)
    {
        TextGameOver textGameOver = mainText.GetComponent<TextGameOver>();
        board.SetActive(true);
        if (win)
        {
            textButton.text = "WINNNNN";
        }else
        {
            textButton.text = "LOOOOOOOOOSE";
        }
        textGameOver.SelectText(win);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenCloseHelp()
    {
        if (openHelp)
        {
            HelpBoard.SetActive(false);
            openHelp = false;
        }
        else
        {
            HelpBoard.SetActive(true);
            openHelp = true;
        }
    }
}
