using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public bool IsStarted { get; private set; }

    [SerializeField]
    private Button[] buttons;

    [SerializeField]
    private Text resultTxt;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var btn in buttons)
            btn.interactable = false;

        resultTxt.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        IsStarted = true;

        foreach (var btn in buttons)
            btn.interactable = true;
    }

    public void SendChoose(int choose)
    {
        FirebaseController.Instance.SendChoose(choose);

        foreach (var btn in buttons)
            btn.interactable = false;
    }

    public void SetResult(bool win)
    {
        IsStarted = false;
        resultTxt.gameObject.SetActive(true);
        resultTxt.text = win ? "You won!" : "You lost!";
    }
}
