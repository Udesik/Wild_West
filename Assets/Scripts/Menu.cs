using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject _menu;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private Health _player;

    private string _winText = "You Win!";
    private string _gameOverText = "Game Over";

    private void OnEnable()
    {
        _player.Died += GameOver;
        _enemySpawner.Win += Win;
    }

    private void OnDisable()
    {
        _player.Died -= GameOver;
        _enemySpawner.Win -= Win;
    }

    private void EndGame()
    {
        _menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Win()
    {
        EndGame();
        _text.text = _winText;
    }

    private void GameOver()
    {
        EndGame();
        _text.text = _gameOverText;
    }

    public void Click()
    {
        _menu.SetActive(false);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
