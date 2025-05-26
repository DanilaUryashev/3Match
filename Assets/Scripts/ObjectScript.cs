using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectScript : MonoBehaviour
{
    [Header("Object Characteristics")]
    public string shapeName;
    public string colorName;
    public string animalName;
    [Header("Object inf")]
    public GameObject actionBar;
    public GameObject gameOverBoard;

    private Ability ability;
    [Header("Effect")]
    [Header("---Sound")]
    [SerializeField] private AudioClip burstSoundClip; // звук взрыва фигупки
    [SerializeField] private AudioSource audioSource; // Источник звука
    [Header("---Visual")]
    [SerializeField] private ParticleSystem BurstEffect; // эффект бабах

    private void Start()
    {
        actionBar = GameObject.Find("ActionBarItems");
        gameOverBoard = GameObject.Find("WinLoseCanvas");
        ability = gameObject.GetComponent<Ability>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnMouseDown()
    {
        //ShowObjectInfo();
        if (ability.enableAbilityFrozen) return;
        
        if (gameObject.GetComponent<Ability>().EnableAbility)
        {
            gameObject.GetComponent<Ability>().UseAbility();
        }
        CheckingRemainingFigures();
        if (ability.enableAbilityLighting) return;
        if (FindMatches())
            return;
        MoveToActionBar();
    }

    // Обработчик касания на мобильных устройствах
    private void OnTouch()
    {
        //ShowObjectInfo();
        if (ability.enableAbilityFrozen) return;
        if (gameObject.GetComponent<Ability>().EnableAbility)
        {
            gameObject.GetComponent<Ability>().UseAbility();
        }
        CheckingRemainingFigures();
        if (ability.enableAbilityLighting) return;
        if (FindMatches())
            return;
        MoveToActionBar();
    }
    //private void ShowObjectInfo()
    //{
    //    // Выводим информацию в консоль
    //    Debug.Log($"Характеристики объекта:\n" +
    //             $"Форма: {shapeName}\n" +
    //             $"Цвет: {colorName}\n" +
    //             $"Животное: {animalName}");
    //}

    private bool FindMatches() // ищем совпадения нажатого объекта с теми что уже есть в экшен баре
    {
        int matchCount = 0;
        Transform firstMatch = null;
        Transform secondMatch = null;

        for (int i = 0; i < actionBar.transform.childCount; i++)
        {
            var child = actionBar.transform.GetChild(i);
            if (child.childCount > 1)
            {
                var objScript = child.GetChild(1).GetComponent<ObjectScript>();

                if (objScript != null && IsMatch(objScript))
                {
                    Debug.Log("чет совпало");
                    matchCount++;
                    if (firstMatch == null)
                        firstMatch = child;
                    else if (secondMatch == null)
                        secondMatch = child;
                }
            }
        }

        if (matchCount >= 2) // два совпадения значитт уничтожаем три объекта которые совпали и очищщаем экшен бар
        {
            firstMatch.GetChild(0).gameObject.SetActive(true);
            secondMatch.GetChild(0).gameObject.SetActive(true);
            firstMatch.GetChild(1).gameObject.GetComponent<ObjectScript>().DestroyFigure();
            secondMatch.GetChild(1).gameObject.GetComponent<ObjectScript>().DestroyFigure();
            DestroyFigure();
            Ability.TriggerGlobalAbility();
            return true;
        }
        return false;
    }

    private bool IsMatch(ObjectScript other) // Проверяем совпадения если все совпало то есть одно типо совпадение
    {
        return shapeName == other.shapeName &&
               colorName == other.colorName &&
               animalName == other.animalName;
    }

    private void MoveToActionBar()// если метча нет то просто перемещаем объект в первую свободную ячейку
    {
        bool foundEmptyCell = false;
        for (int i = 0; i < actionBar.transform.childCount; i++)
        {
            var cell = actionBar.transform.GetChild(i);
            if (cell.childCount == 1)
            {
                cell.GetChild(0).gameObject.SetActive(false);
                RemovePhysicsComponents();
                transform.SetParent(cell);
                //transform.localPosition = Vector3.zero;
                //transform.localRotation = Quaternion.identity;
                StartCoroutine(MoveToCellAnimation(cell));
                foundEmptyCell = true;
                if(i== actionBar.transform.childCount) foundEmptyCell = false;
                break;
            }
        }
        if (!foundEmptyCell)
        {
            ViewGameOverBoard(false);
        }
    }

    private void RemovePhysicsComponents()// удаляем физику у объектов всю
    {
        var rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null) Destroy(rb2D);

        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders) Destroy(col);
    }

    private IEnumerator MoveToCellAnimation(Transform targetCell)
    {
        // Запоминаем начальные параметры
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
       

        // Отключаем физику и делаем объект некликабельным на время анимации
        GetComponent<Collider2D>().enabled = false;

        float duration = 0.5f; // Длительность анимации
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Плавное перемещение
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // Сглаживание

            transform.position = Vector3.Lerp(startPosition, targetCell.position, t);
            transform.rotation = Quaternion.Lerp(startRotation, Quaternion.identity, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Финализируем позицию
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Включаем коллайдер обратно
        GetComponent<Collider2D>().enabled = true;
    }
    private void CheckingRemainingFigures()
    {
        Debug.Log($"Оставлось {transform.parent.transform.childCount}");
        if (transform.parent.transform.childCount == 1) ViewGameOverBoard(true);
        }
    private void ViewGameOverBoard(bool win)
    {
        GameOver gameOver = gameOverBoard.GetComponent<GameOver>();
        gameOver.ViewBord(win);
    }


    public void DestroyFigure()
    {
        StartCoroutine(DestroyWithSound());

    }
    private IEnumerator DestroyWithSound()
    {
        if (burstSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(burstSoundClip);

            // Создаем эффект
            Instantiate(BurstEffect, transform.position, Quaternion.identity);

            // Отключаем визуальную часть объекта
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
                var canvas = GetComponentInChildren<Canvas>();
                if (canvas != null)
                    canvas.enabled = false;
            }

            // Отключаем коллайдеры
            foreach (var collider in GetComponentsInChildren<Collider2D>())
            {
                collider.enabled = false;
            }

            // Ждем длительность звука
            yield return new WaitForSeconds(burstSoundClip.length);
        }

        // Уничтожаем объект
        Destroy(gameObject);
    }
}
