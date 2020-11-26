using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Signals;

public class PlayerInventoryManager : MonoBehaviour
{
    private List<Item> m_PlayerInventory;
    
    private GameManager m_GameManager;
    private SignalBus m_SignalBus;
    private int m_CurrentSlot;

    public bool isHoldingItem => currentItem != null;

    public bool IsInInventory(Item.ItemType itemType) => 
        m_PlayerInventory.Exists(i => i.itemType == itemType);

    public Item currentItem => 
        m_PlayerInventory.Count > 0 ? 
        m_PlayerInventory[m_CurrentSlot] : 
        null;

    public Item nextItem => 
        m_CurrentSlot + 1 < m_PlayerInventory.Count ? 
        m_PlayerInventory[m_CurrentSlot + 1] : 
        null;

    public Item previousItem => 
        m_CurrentSlot - 1 >= 0 ? 
        m_PlayerInventory[m_CurrentSlot - 1] : 
        null;

    [Inject]
    public void Construct(GameManager gameManager, SignalBus signalBus)
    {
        m_GameManager = gameManager;
        m_SignalBus = signalBus;
    }

    public void OnStageLoaded(StageLoadedSignal signal)
    {
        if(signal.isRestartingProgress)
        {
            m_PlayerInventory = new List<Item>();
            foreach(var item in GetComponentsInChildren(typeof(Item), true))
            {
                Object.Destroy(item.gameObject);
            }
        }
    }

    private void Start()
    {
        m_PlayerInventory = new List<Item>();
    }

    private void Update()
    {
        if(m_GameManager.CurrentGameState == GameState.RUNNING
            && isHoldingItem)
        {
            if(PlayerInputHandler.Instance.GetFireInputDown()
                && isHoldingItem)
            {
                currentItem.Use();
            }
            else if(PlayerInputHandler.Instance.GetSwitchWeaponInput() != 0
                && isHoldingItem)
            {
                int input = PlayerInputHandler.Instance.GetSwitchWeaponInput();
                Debug.Log(input);
                m_CurrentSlot = UnityEngine.Mathf.Clamp(m_CurrentSlot + input, m_PlayerInventory.Count - 1, 0);

                m_SignalBus.Fire(new ItemSwitchedSignal 
                    { 
                        previousItem = previousItem,
                        currentItem = currentItem,
                        nextItem = nextItem
                    });
            }
        }
    }

    public void OnItemThrown(ItemThrownSignal signal)
    {
        DiscardFromInventory(signal.throwableItem);

        // remove parenting and set scene
        signal.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(signal.transform.gameObject, SceneManager.GetActiveScene());
    }

    public void DiscardFromInventory(Item item)
    {
        m_PlayerInventory.Remove(item);
    }

    public void OnItemCollected(ItemCollectedSignal signal)
    {
        m_PlayerInventory.Add(signal.item);
        m_CurrentSlot = m_PlayerInventory.Count - 1;

        // each item is represented by a gameobject and is a child of inventory manager when player has collected it
        signal.item.transform.SetParent(this.transform);
    }
}
