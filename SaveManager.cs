using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// --- KLASY POMOCNICZE DO ZAPISU ---

[System.Serializable]
public class SavedInventoryItem
{
    public string fileName; // Nazwa pliku w folderze Resources
    public int amount;

    public SavedInventoryItem(string _name, int _amount)
    {
        fileName = _name;
        amount = _amount;
    }
}

[System.Serializable]
public class SaveData
{
    // DANE GRACZA I ŒWIAT
    public int gold;
    public float[] position;
    public string currentScene;
    public List<string> defeatedEnemies;
    public List<string> collectedItemIDs;
    public List<string> collectedItems; // <--- DODANE (ID zebranych rzeczy z mapy)

    // --- NOWOŒÆ: Lista Twojej dru¿yny ---
    public List<Pokezaur> team;

    // EKWIPUNEK
    public List<string> itemNames;
    public List<int> itemCounts;

    public float movementSpeed;

    // EKWIPUNEK
    public List<SavedInventoryItem> inventory; // <--- ZMIENIONE na listê obiektów
    public string equippedItemFileName; // <--- DODANE (co mamy za³o¿one)
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [Header("Referencje")]
    public PlayerData playerData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. Statystyki
        data.team = new List<Pokezaur>(playerData.team);
        data.collectedItemIDs = new List<string>(playerData.collectedItemIDs);
        data.gold = playerData.currentGold; // Zapisujemy z³oto
        data.movementSpeed = playerData.moveSpeed;

        // 2. Pozycja gracza
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.position = new float[3];
            data.position[0] = player.transform.position.x;
            data.position[1] = player.transform.position.y;
            data.position[2] = player.transform.position.z;
        }
        data.currentScene = SceneManager.GetActiveScene().name;

        // 3. Listy ID (Wrogowie i Przedmioty na mapie)
        data.defeatedEnemies = new List<string>(BattleTransitionManager.defeatedEnemies);
        data.collectedItems = new List<string>(playerData.collectedItemIDs); // Zapisujemy co zebraliœmy

        // 4. Ekwipunek (Zapisujemy nazwy PLIKÓW, a nie nazwy wyœwietlane)
        data.inventory = new List<SavedInventoryItem>();
        foreach (var slot in playerData.inventory)
        {
            // slot.item.name to nazwa pliku w projekcie (np. "Potion_Small")
            // slot.item.itemName to nazwa w grze (np. "Ma³a Mikstura")
            // Do wczytywania potrzebujemy nazwy pliku!
            data.inventory.Add(new SavedInventoryItem(slot.item.name, slot.amount));
        }

        // 5. Za³o¿ony przedmiot
        if (playerData.currentEquippedItem != null)
        {
            data.equippedItemFileName = playerData.currentEquippedItem.name;
        }
        else
        {
            data.equippedItemFileName = "";
        }

        // 6. Zapis do pliku
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        GameNotifications.instance.ShowMessage($"Gra zapisana", 3f);
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // 1. Przywracanie statystyk
            playerData.team = new List<Pokezaur>(data.team);
            playerData.collectedItemIDs = data.collectedItemIDs;
            playerData.currentGold = data.gold; // Wczytujemy z³oto
            playerData.moveSpeed = data.movementSpeed;

            // 2. Przywracanie œwiata
            BattleTransitionManager.defeatedEnemies = data.defeatedEnemies;
            playerData.collectedItemIDs = data.collectedItems; // ¯eby zebrane itemy nie wróci³y

            // 3. Ustawienie flagi teleportacji
            if (data.position != null && data.position.Length == 3)
            {
                BattleTransitionManager.targetSpawnPosition = new Vector3(data.position[0], data.position[1], data.position[2]);
                BattleTransitionManager.spawnPositionActive = true;
            }

            // 4. INWENTARZ - WCZYTYWANIE Z RESOURCES
            playerData.inventory.Clear(); // Czyœcimy obecny
            playerData.currentEquippedItem = null; // Resetujemy EQ

            foreach (SavedInventoryItem savedSlot in data.inventory)
            {
                // Szukamy pliku w Assets/Resources/Items/
                ItemData itemAsset = Resources.Load<ItemData>("Items/" + savedSlot.fileName);

                if (itemAsset != null)
                {
                    playerData.inventory.Add(new InventorySlot(itemAsset, savedSlot.amount));
                }
                else
                {
                    Debug.LogWarning($"Nie znaleziono pliku przedmiotu: {savedSlot.fileName}. Upewnij siê, ¿e jest w folderze Resources/Items!");
                }
            }

            // 5. PRZYWRACANIE ZA£O¯ONEGO EKWIPUNKU
            if (!string.IsNullOrEmpty(data.equippedItemFileName))
            {
                ItemData equippedItem = Resources.Load<ItemData>("Items/" + data.equippedItemFileName);
                if (equippedItem != null)
                {
                    // Ustawiamy tylko referencjê (statystyki wczytaliœmy ju¿ w punkcie 1)
                    playerData.currentEquippedItem = equippedItem;
                }
            }

            // 6. Za³adowanie sceny
            SceneManager.LoadScene(data.currentScene);
            GameNotifications.instance.ShowMessage($"Gra wczytana", 3f);
        }
        else
        {
            GameNotifications.instance.ShowMessage($"Brak zapisanej gry", 3f);
        }
    }
}