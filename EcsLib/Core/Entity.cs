﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsLib.Core
{
    public sealed class Entity
    {
        private Dictionary<Type, HashSet<int>> _components;
        private EcsWorld _world;

        internal Entity(uint id)
        {
            Id = id;
        }

        public Entity(Dictionary<Type, HashSet<int>> components, EcsWorld world)
        {
            _components = components;
            _world = world;
        }

        public uint Id { get; }

        internal void Init(EcsWorld world)
        {
            _world = world;
            _components = world.ComponentTypes.ToDictionary(k => k, v => new HashSet<int>());
        }

        public int GetFirstComponent<T>() where T : struct
        {
            if (_components.TryGetValue(typeof(T), out var cmps)) return cmps.First();
            throw new Exception($"Entity {Id} dont have any components of type {typeof(T).Name}");
        }

        public T GetComponent<T>(int id) where T : struct
        {
            if (_world.TryGetComponent<T>(id, out var cmp))
            {
                return cmp;
            }
            return default(T);
        }
        
        public IEnumerable<int> GetComponents<T>() where T : struct
        {
            if (_components.TryGetValue(typeof(T), out var cmps)) return cmps;
            return new int[0];
        }

        public int AddComponent<T>() where T : struct
        {
            return _world.AddComponent(this, default(T));
        }

        public int AddComponent<T>(T cmp) where T : struct
        {
            return _world.AddComponent(this, cmp);
        }

        internal void UnregisterComponent<T>(int id) where T : struct
        {
            _components[typeof(T)].Add(id);
        }

        public void UpdateComponent<T>(int id, T component) where T : struct
        {
            _world.TryUpdateComponent<T>(id, component);
        }
        
        internal void RegisterComponent<T>(int id) where T : struct
        {
            _components[typeof(T)].Add(id);
        }

        public void RemoveAllComponents()
        {
            foreach (var cmp in _components)
            {
                foreach (var id in cmp.Value)
                {
                    _world.RemoveComponent(cmp.Key, id);
                }
            }
        }
        
        public void Reset()
        {
            foreach (var cmpData in _components)
            foreach (var id in cmpData.Value)
                _world.RemoveComponent(cmpData.Key, id);
            _components.Clear();
        }
    }
}