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
    [SerializeField] private AudioClip burstSoundClip; // ���� ������ �������
    [SerializeField] private AudioSource audioSource; // �������� �����
    [Header("---Visual")]
    [SerializeField] private ParticleSystem BurstEffect; // ������ �����

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

    // ���������� ������� �� ��������� �����������
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
    //    // ������� ���������� � �������
    //    Debug.Log($"�������������� �������:\n" +
    //             $"�����: {shapeName}\n" +
    //             $"����: {colorName}\n" +
    //             $"��������: {animalName}");
    //}

    private bool FindMatches() // ���� ���������� �������� ������� � ���� ��� ��� ���� � ����� ����
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
                    Debug.Log("��� �������");
                    matchCount++;
                    if (firstMatch == null)
                        firstMatch = child;
                    else if (secondMatch == null)
                        secondMatch = child;
                }
            }
        }

        if (matchCount >= 2) // ��� ���������� ������� ���������� ��� ������� ������� ������� � �������� ����� ���
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

    private bool IsMatch(ObjectScript other) // ��������� ���������� ���� ��� ������� �� ���� ���� ���� ����������
    {
        return shapeName == other.shapeName &&
               colorName == other.colorName &&
               animalName == other.animalName;
    }

    private void MoveToActionBar()// ���� ����� ��� �� ������ ���������� ������ � ������ ��������� ������
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

    private void RemovePhysicsComponents()// ������� ������ � �������� ���
    {
        var rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null) Destroy(rb2D);

        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders) Destroy(col);
    }

    private IEnumerator MoveToCellAnimation(Transform targetCell)
    {
        // ���������� ��������� ���������
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
       

        // ��������� ������ � ������ ������ �������������� �� ����� ��������
        GetComponent<Collider2D>().enabled = false;

        float duration = 0.5f; // ������������ ��������
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // ������� �����������
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // �����������

            transform.position = Vector3.Lerp(startPosition, targetCell.position, t);
            transform.rotation = Quaternion.Lerp(startRotation, Quaternion.identity, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ������������ �������
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // �������� ��������� �������
        GetComponent<Collider2D>().enabled = true;
    }
    private void CheckingRemainingFigures()
    {
        Debug.Log($"��������� {transform.parent.transform.childCount}");
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

            // ������� ������
            Instantiate(BurstEffect, transform.position, Quaternion.identity);

            // ��������� ���������� ����� �������
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
                var canvas = GetComponentInChildren<Canvas>();
                if (canvas != null)
                    canvas.enabled = false;
            }

            // ��������� ����������
            foreach (var collider in GetComponentsInChildren<Collider2D>())
            {
                collider.enabled = false;
            }

            // ���� ������������ �����
            yield return new WaitForSeconds(burstSoundClip.length);
        }

        // ���������� ������
        Destroy(gameObject);
    }
}
