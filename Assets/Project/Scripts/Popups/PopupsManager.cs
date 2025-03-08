using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PopupsManager : BaseMono
{
    public event Action OnPopupLoadingStarted;
    public event Action OnPopupLoadingFinished;

    [SerializeField] private Transform spawnAtContent;
    [SerializeField] private List<AssetReference> allPossiblePopupsRefs = new List<AssetReference>();

    // Queue for popups waiting to be displayed
    private readonly Queue<PopupType> _popupQueue = new Queue<PopupType>();

    private readonly Dictionary<PopupType, AsyncOperationHandle<GameObject>> _addressableHandles =
        new Dictionary<PopupType, AsyncOperationHandle<GameObject>>();

    private PopupBase _currentPopup;

    public enum PopupType
    {
        None,
        Start_Popup,
        Lose_Popup,
        Win_Popup,
        Settings_Popup
    }

    public override async UniTask Init(object data = null)
    {
        await base.Init(data);
    }

    public async UniTask<PopupBase> ShowPopup(PopupType popupType)
    {
        // If a popup is currently displayed, queue the new popup
        if (_currentPopup != null)
        {
            _popupQueue.Enqueue(popupType);
            return null; // Return null if queued
        }

        // Notify loading has started
        OnPopupLoadingStarted?.Invoke();
    
        // Load and show the popup
        var popup = await LoadAndShowPopup(popupType);
    
        // Notify loading has finished
        OnPopupLoadingFinished?.Invoke();
    
        return popup;
    }

    private async UniTask<PopupBase> LoadAndShowPopup(PopupType popupType)
    {
        // Construct the popup path using the enum name
        string popupPath = $"Assets/Project/Prefabs/Popups/{popupType}.prefab";

        try
        {
            // Load the specific asset by its path
            var popupHandle = Addressables.LoadAssetAsync<GameObject>(popupPath);
            var popupPrefab = await popupHandle.Task;

            // Store the handle for proper release later
            _addressableHandles[popupType] = popupHandle;

            if (popupPrefab == null)
            {
                Debug.LogError($"No popup found for type {popupType}");
                ProcessNextQueuedPopup();
                return null;
            }

            // Instantiate the popup
            var popupInstance = Instantiate(popupPrefab, spawnAtContent);
            var popup = popupInstance.GetComponent<PopupBase>();

            if (popup == null)
            {
                Debug.LogError($"Popup prefab does not have PopupBase component: {popupType}");
                Destroy(popupInstance);
                ProcessNextQueuedPopup();
                return null;
            }

            // Show the popup
            await popup.Init();
            await popup.ShowPopup();

            _currentPopup = popup;

            // Subscribe to close event
            popup.OnPopupClosed += HandlePopupClosed;
            
            return popup;
        }
        catch (InvalidKeyException)
        {
            Debug.LogError($"Invalid Addressable key for popup: {popupType}");
            ProcessNextQueuedPopup();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading popup {popupType}: {e.Message}");
            ProcessNextQueuedPopup();
        }
        
        return null;
    }

    private void HandlePopupClosed(PopupBase popupBase)
    {
        // Hide current popup
        if (_currentPopup != null)
        {
            PopupType popupType = _currentPopup.PopupType;

            // Unsubscribe event
            _currentPopup.OnPopupClosed -= HandlePopupClosed;

            // Destroy GameObject
            Destroy(_currentPopup.gameObject);
            _currentPopup = null;

            // Release the Addressables handle if we have it
            if (_addressableHandles.TryGetValue(popupType, out AsyncOperationHandle<GameObject> handle))
            {
                Addressables.Release(handle);
                _addressableHandles.Remove(popupType);
            }
        }

        // Process next queued popup
        ProcessNextQueuedPopup();
    }


    private void ProcessNextQueuedPopup()
    {
        // Check if there are queued popups
        if (_popupQueue.Count > 0)
        {
            var nextPopupType = _popupQueue.Dequeue();
            ShowPopup(nextPopupType).Forget();
        }
    }

    private void OnDestroy()
    {
        // Release all remaining Addressables handles
        foreach (var handle in _addressableHandles.Values)
        {
            Addressables.Release(handle);
        }

        _addressableHandles.Clear();
    }
}