﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
/// <summary>
/// Every item's cell must contain this script
/// </summary>
[RequireComponent(typeof(Image))]
public class DragAndDropCell : MonoBehaviour, IDropHandler
{
    private static int dropCount = 0;
    private static int score = 0;
    public enum CellType
    {
        Swap,                                                               // Items will be swapped between cells
        DropOnly,                                                           // Item will be dropped into cell
        DragOnly,                                                           // Item will be dragged from this cell
        UnlimitedSource                                                     // Item will be cloned and dragged from this cell
    }
    public CellType cellType = CellType.Swap;                               // Special type of this cell

    public struct DropDescriptor                                            // Struct with info about item's drop event
    {
        public DragAndDropCell sourceCell;                                  // From this cell item was dragged
        public DragAndDropCell destinationCell;                             // Into this cell item was dropped
        public DragAndDropItem item;                                        // dropped item
    }

    public Color empty = new Color();                                       // Sprite color for empty cell
    public Color full = new Color();                                        // Sprite color for filled cell

    void OnEnable()
    {
        DragAndDropItem.OnItemDragStartEvent += OnAnyItemDragStart;         // Handle any item drag start
        DragAndDropItem.OnItemDragEndEvent += OnAnyItemDragEnd;             // Handle any item drag end
    }

    void OnDisable()
    {
        DragAndDropItem.OnItemDragStartEvent -= OnAnyItemDragStart;
        DragAndDropItem.OnItemDragEndEvent -= OnAnyItemDragEnd;
    }

    void Start()
    {
        SetBackgroundState(GetComponentInChildren<DragAndDropItem>() == null ? false : true);
        
    }

    /// <summary>
    /// On any item drag start need to disable all items raycast for correct drop operation
    /// </summary>
    /// <param name="item"> dragged item </param>
    private void OnAnyItemDragStart(DragAndDropItem item)
    {
        DragAndDropItem myItem = GetComponentInChildren<DragAndDropItem>(); // Get item from current cell
        if (myItem != null)
        {
            myItem.MakeRaycast(false);                                      // Disable item's raycast for correct drop handling
            if (myItem == item)                                             // If item dragged from this cell
            {
                // Check cell's type
                switch (cellType)
                {
                    case CellType.DropOnly:
                        DragAndDropItem.icon.SetActive(false);              // Item will not be dropped
                        break;
                    case CellType.UnlimitedSource:
                        // Nothing to do
                        break;
                    default:
                        item.MakeVisible(false);                            // Hide item in cell till dragging
                        SetBackgroundState(false);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// On any item drag end enable all items raycast
    /// </summary>
    /// <param name="item"> dragged item </param>
    private void OnAnyItemDragEnd(DragAndDropItem item)
    {
        DragAndDropItem myItem = GetComponentInChildren<DragAndDropItem>(); // Get item from current cell
        if (myItem != null)
        {
            if (myItem == item)
            {
                SetBackgroundState(true);
            }
            myItem.MakeRaycast(true);                                       // Enable item's raycast
        }
        else
        {
            SetBackgroundState(false);
        }
    }

    /// <summary>
    /// Item is dropped in this cell
    /// </summary>
    /// <param name="data"></param>
    public void OnDrop(PointerEventData data)
    {
        DragAndDropCell sourceCell = DragAndDropItem.sourceCell;
        if (DragAndDropItem.icon != null)
        {
            if (DragAndDropItem.icon.activeSelf == true)                    // If icon inactive do not need to drop item in cell
            {
                DragAndDropItem item = DragAndDropItem.draggedItem;
                try
                {
                    UInt16 selectedItem = Convert.ToUInt16(item.GetComponent<Image>().sprite.name.ToString());
                    UInt16 sourceItem = Convert.ToUInt16(gameObject.GetComponent<Image>().sprite.name.ToString());
                    Debug.Log("childCount: " + gameObject.transform.childCount.ToString());
                    if (selectedItem != sourceItem || gameObject.transform.childCount >= 1)
                    {
                        sourceCell.GetComponent<AudioSource>().clip = item.GetComponent<number>().FalseSound;
                        sourceCell.GetComponent<AudioSource>().enabled = true;
                        sourceCell.GetComponent<AudioSource>().Play();
                        score -= 10;
                        GameObject.Find("cark").GetComponent<gamePlayManager>().printToText(score);
                       //( StartCoroutine();
                        return;
                    }
                } catch (System.Exception e)
                {
                    Debug.Log("Hata : " + e.Message);
                }
                DropDescriptor desc = new DropDescriptor();
                if ((item != null) && (sourceCell != this))
                {
                    switch (sourceCell.cellType)                            // Check source cell's type
                    {
                        /*
                        case CellType.UnlimitedSource:
                            string itemName = item.name;
                            item = Instantiate(item);                       // Clone item from source cell
                            item.name = itemName;
                            break;
                            */
                        default:
                            // Nothing to do
                            break;
                    }
                    switch (cellType)                                       // Check this cell's type
                    {
                        case CellType.Swap:
                            DragAndDropItem currentItem = GetComponentInChildren<DragAndDropItem>();
                            switch (sourceCell.cellType)
                            {
                                case CellType.Swap:
                                    SwapItems(sourceCell, this);            // Swap items between cells
                                    // Fill event descriptor
                                    desc.item = item;
                                    desc.sourceCell = sourceCell;
                                    desc.destinationCell = this;
                                    // Send message with DragAndDrop info to parents GameObjects
                                    StartCoroutine(NotifyOnDragEnd(desc));
                                    if (currentItem != null)
                                    {
                                        // Fill event descriptor
                                        desc.item = currentItem;
                                        desc.sourceCell = this;
                                        desc.destinationCell = sourceCell;
                                        // Send message with DragAndDrop info to parents GameObjects
                                        StartCoroutine(NotifyOnDragEnd(desc));
                                    }
                                    break;
                                default:
                                    PlaceItem(item.gameObject);             // Place dropped item in this cell
                                    // Fill event descriptor
                                    desc.item = item;
                                    desc.sourceCell = sourceCell;
                                    desc.destinationCell = this;
                                    // Send message with DragAndDrop info to parents GameObjects
                                    StartCoroutine(NotifyOnDragEnd(desc));
                                    break;
                            }
                            break;
                        case CellType.DropOnly:
                    
                            PlaceItem(item.gameObject);                     // Place dropped item in this cell
                            // Fill event descriptor
                            desc.item = item;
                            desc.sourceCell = sourceCell;
                            sourceCell.GetComponent<AudioSource>().clip = item.GetComponent<number>().TrueSound;
                            sourceCell.GetComponent<AudioSource>().enabled = true;
                            sourceCell.GetComponent<AudioSource>().Play();
                           
                            Debug.Log("sourceCell: " + sourceCell.GetComponent<Image>().sprite.name.ToString());

                            //refreshKuyruk(sourceCell, item.gameObject);
                           
                            StartCoroutine(GameObject.Find("cark").GetComponent<gamePlayManager>().reloadKuyruk());
                            //StartCoroutine(GameObject.Find("GameManager").GetComponent<gamePlayManager>().endOfWheel());
                            desc.destinationCell = this;
                            dropCount++;
                            score += 10;
                            GameObject.Find("cark").GetComponent<gamePlayManager>().printToText(score);
                           // StartCoroutine(GameObject.Find("cark").GetComponent<gamePlayManager>().printToText(score));
                            if (dropCount == 8)
                            {
                                StartCoroutine(GameObject.Find("cark").GetComponent<gamePlayManager>().successMethod());
                                dropCount = 0;
                            }
                            Debug.Log("Drop Count: " + dropCount.ToString());
                            // Send message with DragAndDrop info to parents GameObjects
                           /// StartCoroutine(NotifyOnDragEnd(desc));
                            break;
                        default:
                            // Nothing to do
                            break;
                    }
                }
                if (item.GetComponentInParent<DragAndDropCell>() == null)   // If item have no cell after drop
                {
                    Destroy(item.gameObject);                               // Destroy it
                }
            }
        }
    }
    private void refreshKuyruk(DragAndDropCell cell, GameObject item)
    {
        Debug.Log("refresKuyruk soruce cell: " + cell.GetComponent<Image>().sprite.name.ToString());
        string name = item.name;
        item = Instantiate(item);
        item.name = name;
        PlaceItemForCell(cell, item);
        //StartCoroutine(GameObject.Find("GameManager").GetComponent<wheelManager>().refKuyruk(item));
    }
    /// <summary>
    /// Change cell's sprite color on item put/remove
    /// </summary>
    /// <param name="condition"> true - filled, false - empty </param>
    private void SetBackgroundState(bool condition)
    {
        GetComponent<Image>().color = condition ? full : empty;
    }

    /// <summary>
    /// Delete item from this cell
    /// </summary>
    public void RemoveItem()
    {
        foreach (DragAndDropItem item in GetComponentsInChildren<DragAndDropItem>())
        {
            Destroy(item.gameObject);
        }
        SetBackgroundState(false);
    }

    /// <summary>
    /// Put new item in this cell
    /// </summary>
    /// <param name="itemObj"> New item's object with DragAndDropItem script </param>
    ///
    public void PlaceItemForCell(DragAndDropCell cell, GameObject itemObj)
    {
        cell.RemoveItem();                                                       // Remove current item from this cell
        if (itemObj != null)
        {
            itemObj.transform.SetParent(cell.transform, false);
            itemObj.transform.localPosition = Vector3.zero;
            DragAndDropItem item = itemObj.GetComponent<DragAndDropItem>();
            if (item != null)
            {
                item.MakeRaycast(true);
            }
            cell.SetBackgroundState(true);
        }
    }
    public void PlaceItem(GameObject itemObj)
    {
        RemoveItem();                                                       // Remove current item from this cell
        if (itemObj != null)
        {
            itemObj.transform.SetParent(transform, false);
            itemObj.transform.localPosition = Vector3.zero;
            DragAndDropItem item = itemObj.GetComponent<DragAndDropItem>();
            if (item != null)
            {
                item.MakeRaycast(true);
            }
            SetBackgroundState(true);
        }
    }

    /// <summary>
    /// Get item from this cell
    /// </summary>
    /// <returns> Item </returns>
    public DragAndDropItem GetItem()
    {
        return GetComponentInChildren<DragAndDropItem>();
    }

    /// <summary>
    /// Swap items between to cells
    /// </summary>
    /// <param name="firstCell"> Cell </param>
    /// <param name="secondCell"> Cell </param>
    public void SwapItems(DragAndDropCell firstCell, DragAndDropCell secondCell)
    {
        if ((firstCell != null) && (secondCell != null))
        {
            DragAndDropItem firstItem = firstCell.GetItem();                // Get item from first cell
            DragAndDropItem secondItem = secondCell.GetItem();              // Get item from second cell
            if (firstItem != null)
            {
                // Place first item into second cell
                firstItem.transform.SetParent(secondCell.transform, false);
                firstItem.transform.localPosition = Vector3.zero;
                secondCell.SetBackgroundState(true);
            }
            if (secondItem != null)
            {
                // Place second item into first cell
                secondItem.transform.SetParent(firstCell.transform, false);
                secondItem.transform.localPosition = Vector3.zero;
                firstCell.SetBackgroundState(true);
            }
        }
    }

    private IEnumerator NotifyOnDragEnd(DropDescriptor desc)
    {
        // Wait end of drag operation
        while (DragAndDropItem.draggedItem != null)
        {
            yield return new WaitForEndOfFrame();
        }
        // Send message with DragAndDrop info to parents GameObjects
        gameObject.SendMessageUpwards("OnItemPlace", desc, SendMessageOptions.DontRequireReceiver);
    }
    private void Update()
    {
        if (!gameObject.name.Contains("kuyruk"))
        {
            transform.Rotate(new Vector3(0, 0, -1) * Time.deltaTime * 10, Space.World);
        }
        if (gameObject.name.Contains("cell"))
        {
            transform.localRotation = Quaternion.identity;
        }
    }
   
}