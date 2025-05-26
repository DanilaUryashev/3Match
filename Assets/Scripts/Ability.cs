using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [Header("Ability Icons")]
    [SerializeField] private GameObject abilityLighting;
    [SerializeField] private int howMuchDestroy=3;
    [SerializeField] private GameObject abilityHeavy;
    [SerializeField] private GameObject abilityTwin;
    [SerializeField] private GameObject abilityFrozen;
    [SerializeField] private GameObject spawnObjects;

    [Header("Ability enable")]
    public bool EnableAbility; //�������� �� ������� ����� ����������� 
    public bool enableAbilityLighting;
    public bool enableAbilityHeavy;
    public bool enableAbilityTwin;
    public bool enableAbilityFrozen;

    [Header("Frozen settings")]
    [SerializeField] private Material frozenMaterial;
    private int dropsDefrosting = 1;
    private SpriteRenderer[] spriteRenderer =  new SpriteRenderer[2]; // ������� Color � Animal
    public static event Action defrozenTrigger;
    [SerializeField] private TextMeshProUGUI frozen�ounter;
    [SerializeField] private Material mainMaterial;

    [Header("Lighting prefab")]
    public GameObject lightningPrefab;

    private Rigidbody2D rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnObjects = GameObject.Find("SpawnObject");
        SearchIconAbility();
        ActivateAbility();

        defrozenTrigger += AbilityFrozen;
    }

    // ����� ��� ������ �������
    public static void TriggerGlobalAbility()
    {
        defrozenTrigger?.Invoke();
    }
    
    public void ActivateAbility()
    {
        if (enableAbilityLighting)
        {
            abilityLighting.SetActive(true);
        }
        if (enableAbilityHeavy)
        {
            abilityHeavy.SetActive(true);
            AbilityHeavy();
        }
        if (enableAbilityTwin)
        {
            abilityTwin.SetActive(true);
        }
        if (enableAbilityFrozen)
        {
            abilityFrozen.SetActive(true);
            frozen�ounter.gameObject.SetActive(true);
            dropsDefrosting = UnityEngine.Random.Range(1, 4);
            frozen�ounter.text = dropsDefrosting.ToString();

            Transform colorChild = transform.Find("Color");
            if (colorChild != null)
            {
                // �������� ������ ��������
                spriteRenderer[0] = colorChild.GetComponent<SpriteRenderer>();

                // �������� ������ �������� � ��������� ������� ��������� �������
                if (colorChild.childCount > 0)
                {
                    spriteRenderer[1] = colorChild.GetChild(0).GetComponent<SpriteRenderer>();
                }

                // ��������� ��� ��� ��������� �������
                if (spriteRenderer[0] != null && spriteRenderer[1] != null)
                {
                    mainMaterial = spriteRenderer[0].material;
                    spriteRenderer[0].material = frozenMaterial;
                    spriteRenderer[1].material = frozenMaterial;
                }
                else
                {
                    Debug.LogError("�� ������� ����� SpriteRenderer ����������");
                }
            }
            else
            {
                Debug.LogError("�� ������ �������� ������ 'Color'");
            }
        }
    }

    public void UseAbility()
    {
        if (enableAbilityLighting)
        {
            Debug.Log($"����������� AbilityLighting");
            AbilityLighting();
        }
        if(enableAbilityHeavy)
        {
            Debug.Log($"����������� AbilityHeavy");
            AbilityHeavy();
        }
        if (enableAbilityTwin)
        {
            Debug.Log($"����������� AbilityTwin");
            AbilityTwin();
        }
        if (enableAbilityFrozen)
        {
            Debug.Log($"����������� AbilityFrozen");
            AbilityFrozen();
        }
    }

    private void SearchIconAbility()
    {
        // �������� ������ ���������������� �������� �������
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
            {
                switch (child.name)
                {
                    case "iconAbilityLighting":
                        abilityLighting = child.gameObject;
                        break;
                    case "iconAbilityFrozen":
                        abilityFrozen = child.gameObject;
                        break;
                    case "iconAbilityHeavy":
                        abilityHeavy = child.gameObject;
                        break;
                    case "iconAbilityTwin":
                        abilityTwin = child.gameObject;
                        break;
                }
            }
        }
    }
    #region AbilityHeavy
    private void AbilityHeavy()
    {
        rb.mass = 100f;
    }
    #endregion

    #region AbilityFrozen
    private void AbilityFrozen()
    {

        dropsDefrosting--;
        frozen�ounter.text = dropsDefrosting.ToString();

        if (dropsDefrosting == 0)
        {
            // ��������� ��� ��������� ��� ��� ����������
            if (spriteRenderer[0] != null)
                spriteRenderer[0].material = mainMaterial;

            if (spriteRenderer[1] != null)
                spriteRenderer[1].material = mainMaterial;

            frozen�ounter.gameObject.SetActive(false);
            defrozenTrigger -= AbilityFrozen;
            enableAbilityFrozen = false;
        }
    }
    #endregion

    #region AbilityTwin
    private void AbilityTwin()
    {
        FindAndDestroyALLMatches(transform.GetComponent<ObjectScript>());
    }
    private void FindAndDestroyALLMatches(ObjectScript target)
    {
        if (target == null) return;
        var matches = spawnObjects.GetComponentsInChildren<ObjectScript>()
            .Where(x => x != null && x != target && IsMatch(target, x))
            .ToList();

        // ���������� ����������
        foreach (var match in matches)
        {
            match.DestroyFigure();
        }
        GetComponent<ObjectScript>()?.DestroyFigure();
    }
    #endregion

    #region lightingAbility

    private void AbilityLighting()
    {
        if (spawnObjects == null || spawnObjects.transform.childCount == 0)
            return;

        // �������� ��������� �������
        var targets = GetRandomChildren(spawnObjects.transform, howMuchDestroy);

        // ������������ ������ ��������� ������
        foreach (var target in targets.Where(t => t != null))
        {
            CreateLightningEffect(transform, target);

            var targetScript = target.GetComponent<ObjectScript>();
            if (targetScript != null)
            {
                FindAndDestroyMatches(targetScript);
            }
        }

        // ���������� �������� ������
        GetComponent<ObjectScript>()?.DestroyFigure();
    }

    private void CreateLightningEffect(Transform source, Transform target)
    {
        if (lightningPrefab == null || source == null || target == null)
            return;

        // ������������ ������� � ������ ��������
        var startPos = source.position + Vector3.up * 1.3f;
        var endPos = target.position + Vector3.up * 1.3f;

        // ������� � ����������� ������
        var lightning = Instantiate(lightningPrefab, startPos, Quaternion.identity);
        var lineRenderer = lightning.GetComponent<LineRenderer>();

        // ���������� ��������������� ������
        GenerateLightningPath(lineRenderer, startPos, endPos, segments: 5, jitter: 0.3f);

        // ������������� ���������� ������ ����� 0.5 �������
        Destroy(lightning, 0.5f);
    }

    private void GenerateLightningPath(LineRenderer line, Vector3 start, Vector3 end, int segments, float jitter)
    {
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // ��������� ����������� � ������� ������
            if (i > 0 && i < segments - 1)
            {
                point += UnityEngine.Random.insideUnitSphere * jitter;
            }

            line.SetPosition(i, point);
        }
    }

    private void FindAndDestroyMatches(ObjectScript target)
    {
        if (target == null) return;

        // ������� 2 ����������� �������
        var matches = spawnObjects.GetComponentsInChildren<ObjectScript>()
            .Where(x => x != null && x != target && IsMatch(target, x))
            .Take(2)
            .ToList();

        // ���������� ����������
        foreach (var match in matches)
        {
            match.DestroyFigure();
        }
    }

    private Transform[] GetRandomChildren(Transform parent, int count)
    {
        if (parent == null || parent.childCount == 0)
            return Array.Empty<Transform>();

        // ������� � ������������ ������ �����
        var children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child != parent)
                children.Add(child);
        }

        // ������������ � ���������� ������ ����������
        return children.OrderBy(x => UnityEngine.Random.value)
                     .Take(Mathf.Min(count, children.Count))
                     .ToArray();
    }

    private bool IsMatch(ObjectScript a, ObjectScript b)
    {
        return a != null && b != null &&
               a.shapeName == b.shapeName &&
               a.colorName == b.colorName &&
               a.animalName == b.animalName;
    }
    #endregion
}
