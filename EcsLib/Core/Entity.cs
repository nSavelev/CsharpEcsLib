using System;
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

        public uint Id { get; }

        internal void Init(EcsWorld world)
        {
            _world = world;
            _components = world.ComponentTypes.ToDictionary(k => k, v => new HashSet<int>());
        }

        public int GetComponent<T>() where T : struct
        {
            if (_components.TryGetValue(typeof(T), out var cmps)) return cmps.First();
            throw new Exception($"Entity {Id} dont have any components of type {typeof(T).Name}");
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

        internal void RegisterComponent<T>(int id) where T : struct
        {
            _components[typeof(T)].Add(id);
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