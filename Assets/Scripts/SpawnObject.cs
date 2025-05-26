using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnObject : MonoBehaviour
{
    [Header("Shape Prefabs")]
    public List<NamedPrefab> shapePrefabs;

    [Header("Animal Sprites")]
    public List<NamedSprite> animalSprites;

    [Header("Border Colors")]
    public List<NamedColor> borderColors;

    [Range(1,60)]
    public int numberObjectsSpawn = 3;
    public float spawnDelay = 0.5f;
    public float maxOffset = 1.5f;
    public float offsetStep = 0.6f;

    [SerializeField] private int howMuchSpawnAbilityFigure=4;

    

    private void Start()
    {
        StartCoroutine(GenerateObjectsWithDelay());
    }

    private struct TileData
    {
        public GameObject shapePrefab;
        public Sprite animalSprite;
        public Color borderColor;
        public string shapeName;
        public string animalName;
        public string colorName;
    }

    private IEnumerator GenerateObjectsWithDelay()
    {
        List<TileData> tilesToSpawn = GenerateTileDataList(numberObjectsSpawn);
        float currentOffset = -maxOffset;
        bool movingRight = true;
        int[] randomNumbers = new int[howMuchSpawnAbilityFigure];

        for (int i = 0; i < howMuchSpawnAbilityFigure; i++)
        {
            randomNumbers[i] = Random.Range(1, tilesToSpawn.Count); // генерируем позиции фигур со способностью
        }
        for (int i = 0; i < tilesToSpawn.Count; i++)
        {
            Vector2 spawnPosition = new Vector2(
                transform.position.x + currentOffset,
                transform.position.y
            );
            bool abilEnable = false;
            for (int j = 0; j < howMuchSpawnAbilityFigure; j++)
            {
                if (i == randomNumbers[j])
                {
                    abilEnable = true;
                }
            }
            SpawnTile(tilesToSpawn[i], spawnPosition, abilEnable);
            if (movingRight)
            {
                currentOffset += offsetStep;
                if (currentOffset >= maxOffset) movingRight = false;
            }
            else
            {
                currentOffset -= offsetStep;
                if (currentOffset <= -maxOffset) movingRight = true;
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private List<TileData> GenerateTileDataList(int count)
    {
        count = Mathf.Max(3, Mathf.CeilToInt(count / 3f) * 3);
        List<TileData> tileDataList = new List<TileData>();
        int uniqueTilesCount = count / 3;

        for (int i = 0; i < uniqueTilesCount; i++)
        {
            int shapeIndex = Random.Range(0, shapePrefabs.Count);
            int animalIndex = Random.Range(0, animalSprites.Count);
            int colorIndex = Random.Range(0, borderColors.Count);

            TileData newData = new TileData()
            {
                shapePrefab = shapePrefabs[shapeIndex].prefab,
                animalSprite = animalSprites[animalIndex].sprite,
                borderColor = borderColors[colorIndex].color,
                shapeName = shapePrefabs[shapeIndex].name,
                animalName = animalSprites[animalIndex].name,
                colorName = borderColors[colorIndex].name
            };

            for (int j = 0; j < 3; j++)
            {
                tileDataList.Add(newData);
            }
        }

        return ShuffleList(tileDataList);
    }

    private void SpawnTile(TileData data, Vector2 position, bool enableAbility)
    {
        if (data.shapePrefab == null) return;

        GameObject spawnedObject = Instantiate(data.shapePrefab, position, Quaternion.identity,transform);

        ObjectScript characteristics = spawnedObject.GetComponent<ObjectScript>();
        if (characteristics == null)
            characteristics = spawnedObject.AddComponent<ObjectScript>();

        characteristics.shapeName = data.shapeName;
        characteristics.animalName = data.animalName;
        characteristics.colorName = data.colorName;
        data.borderColor.a = 1;
        Transform border = FindDeepChild(spawnedObject.transform, "BorderColor");
        Transform mainColor = FindDeepChild(spawnedObject.transform, "Color");
        Transform animal = FindDeepChild(spawnedObject.transform, "Animal");

        if (border != null)
        {
            SpriteRenderer borderRenderer = border.GetComponent<SpriteRenderer>();
            if (borderRenderer != null) borderRenderer.color = data.borderColor * 0.7f;
        }

        if (mainColor != null)
        {
            SpriteRenderer mainColorRenderer = mainColor.GetComponent<SpriteRenderer>();
            if (mainColorRenderer != null) mainColorRenderer.color = data.borderColor;
        }

        if (animal != null)
        {
            SpriteRenderer animalRenderer = animal.GetComponent<SpriteRenderer>();
            if (animalRenderer != null) animalRenderer.sprite = data.animalSprite;
        }
        if (enableAbility)
        {
            ActivateAbiliry(spawnedObject);
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        numberObjectsSpawn = Mathf.Max(3, numberObjectsSpawn);
    }
#endif

    public void ResetFigures()
    {
        numberObjectsSpawn = transform.childCount;
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        StartCoroutine(GenerateObjectsWithDelay());
    }

    private void ActivateAbiliry(GameObject figure)
    {
        Ability ability = figure.GetComponent<Ability>();
        int abilityRandom = Random.Range(0, 4);
        switch (abilityRandom)
        {
            case 0:
                ability.enableAbilityFrozen = true;
                break;
            case 1:
                ability.enableAbilityTwin = true;
                break;
            case 2:
                ability.enableAbilityHeavy = true;
                break;
            case 3:
                ability.enableAbilityLighting = true;
                break;

        }
        ability.EnableAbility = true;
    }

}


[System.Serializable]
public class NamedPrefab
{
    public string name;
    public GameObject prefab;
}

[System.Serializable]
public class NamedSprite
{
    public string name;
    public Sprite sprite;
}

[System.Serializable]
public class NamedColor
{
    public string name;
    public Color color;
}
