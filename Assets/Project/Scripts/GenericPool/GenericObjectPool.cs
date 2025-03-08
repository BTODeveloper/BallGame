using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

  /// Async generic object pool for efficient object reuse 
    public class GenericObjectPool<T> where T : Component
    {
        
        /// <summary>
        /// Get the current total count of objects in the pool
        /// </summary>
        public int TotalCount => _inactiveObjects.Count + _activeObjects.Count;

        /// <summary>
        /// Get the number of available inactive objects
        /// </summary>
        public int AvailableCount => _inactiveObjects.Count;
        
        
        private readonly AssetReference _prefabReference;
        private readonly Transform _parent;
        private readonly List<T> _inactiveObjects;
        private readonly HashSet<T> _activeObjects;
        private readonly int _initialCapacity;
        private readonly bool _expandable;
        private readonly int _maxPoolSize;
        
        private AsyncOperationHandle<GameObject> _prefabHandle;
        private bool _isInitialized = false;

        public GenericObjectPool(AssetReference prefabReference, int initialCapacity, Transform parent = null, bool expandable = true, int maxPoolSize = 60)
        {
            _prefabReference = prefabReference ?? throw new ArgumentNullException(nameof(prefabReference));
            _initialCapacity = Mathf.Max(1, initialCapacity);
            _parent = parent;
            _expandable = expandable;
            _maxPoolSize = Mathf.Max(_initialCapacity, maxPoolSize); // Ensure maxPoolSize is never lower than initialCapacity
            
            _inactiveObjects = new List<T>(initialCapacity);
            _activeObjects = new HashSet<T>(); // Track active objects separately
        }

        /// <summary>
        /// Initialize the pool asynchronously
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                _prefabHandle = Addressables.LoadAssetAsync<GameObject>(_prefabReference);

                await PrewarmPoolAsync();
                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize pool: {e.Message}");
                if (_prefabHandle.IsValid()) Addressables.Release(_prefabHandle);
                throw;
            }
        }

        /// <summary>
        /// Create initial pool objects asynchronously
        /// </summary>
        private async UniTask PrewarmPoolAsync()
        {
            List<UniTask> instantiateTasks = new List<UniTask>();
            for (int i = 0; i < _initialCapacity; i++)
            {
                instantiateTasks.Add(InstantiateToPoolAsync());
            }
            await UniTask.WhenAll(instantiateTasks);
        }

        /// <summary>
        /// Instantiate a single object to the pool
        /// </summary>
        private async UniTask<T> InstantiateToPoolAsync()
        {
            if (_inactiveObjects.Count + _activeObjects.Count >= _maxPoolSize)
            {
                Debug.LogWarning($"Pool for {_prefabReference.AssetGUID} has reached max capacity ({_maxPoolSize}). No more objects will be spawned.");
                return null;
            }

            try
            {
                AsyncOperationHandle<GameObject> instanceHandle = _prefabReference.InstantiateAsync(_parent);
                GameObject instance = await instanceHandle.Task;
                
                T component = instance.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogError($"Prefab does not contain component of type {typeof(T).Name}");
                    Addressables.ReleaseInstance(instance);
                    return null;
                }

                instance.SetActive(false);
                _inactiveObjects.Add(component);
                return component;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to instantiate object to pool: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get an object from the pool asynchronously
        /// </summary>
        public async UniTask<T> GetAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            T obj = null;

            if (_inactiveObjects.Count > 0)
            {
                int lastIndex = _inactiveObjects.Count - 1;
                obj = _inactiveObjects[lastIndex];
                _inactiveObjects.RemoveAt(lastIndex);
            }
            else if (_expandable && (_inactiveObjects.Count + _activeObjects.Count < _maxPoolSize))
            {
                obj = await InstantiateToPoolAsync();
            }
            else
            {
                Debug.LogWarning($"Pool for {_prefabReference.AssetGUID} is empty and cannot expand further.");
                return null;
            }

            if (obj == null) return null;

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            
            if (obj is IPoolable poolable)
            {
                poolable.OnGetFromPool();
            }

            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null || !_activeObjects.Contains(obj))
            {
                Debug.LogWarning("Trying to return a null or unmanaged object to the pool");
                return;
            }

            if (obj is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }

            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _inactiveObjects.Add(obj);
        }

        /// <summary>
        /// Clear the pool and destroy all objects
        /// </summary>
        public void Clear()
        {
            foreach (T obj in _inactiveObjects)
            {
                if (obj != null) Addressables.ReleaseInstance(obj.gameObject);
            }

            foreach (T obj in _activeObjects)
            {
                if (obj != null) Addressables.ReleaseInstance(obj.gameObject);
            }

            _inactiveObjects.Clear();
            _activeObjects.Clear();

            if (_prefabHandle.IsValid())
            {
                Addressables.Release(_prefabHandle);
            }

            _isInitialized = false;
        }
    }