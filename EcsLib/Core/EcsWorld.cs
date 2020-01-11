using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EcsLib.Core {
    public sealed class EcsWorld
    {
        private uint LastId;
        private Queue<Entity> _pooledEntities = new Queue<Entity>();
        private List<AbstractSystem> _systems = new List<AbstractSystem>();
        private Dictionary<Type, AbstractSystem> _systemsMap = new Dictionary<Type, AbstractSystem>();
        private Dictionary<uint, Entity> _entities;
        public IEnumerable<Type> ComponentTypes => _systems.Select(e => e.ComponentType);

        public EcsWorld AddSystem<T>(System<T> abstractSystem) where T:struct{
            _systems.Add(abstractSystem);
            abstractSystem.SetWorld(this);
            return this;
        }
        
        public void Update() {
            for (int i = 0; i < _systems.Count; i++) {
                _systems[i].Update();
            }
        }

        // TODO: remove boxing
        public int AddComponent<T>(Entity entity, T p) where T : struct {
            if (_systemsMap.TryGetValue(typeof(T), out var system)) {
                var id = system.ReserveComponent(p);
                entity.RegisterComponent<T>(id);
                return id;
            }
            else {
                throw new Exception($"Unregistered component type {typeof(T).Name}");
            }
        }

        public void RemoveComponent<T>(Entity entity, int id) where T: struct {
            if (_systemsMap.TryGetValue(typeof(T), out var system)) {
                entity.UnregisterComponent<T>(id);
                system.ReleaseComponent(id);
            }
            else {
                throw new Exception($"Unregistered component type {typeof(T).Name}");
            }
        }

        internal void RemoveComponent(Type cmpType, int id)
        {
            if (_systemsMap.TryGetValue(cmpType, out var system)) {
                system.ReleaseComponent(id);
            }
            else {
                throw new Exception($"Unregistered component type {cmpType.Name}");
            }
        }

        public EntityBuilder NewEntity()
        {
            if (_pooledEntities.Any())
            {
                return new EntityBuilder(this, _pooledEntities.Dequeue());
            }
            LastId++;
            return new EntityBuilder(this, LastId);
        }

        public Entity GetEntity(uint entityId)
        {
            if (_entities.TryGetValue(entityId, out var entity))
            {
                return entity;
            }
            return null;
        }

        private void RegisterEntity(Entity entity)
        {
            _entities.Add(entity.Id, entity);
        }

        private void UnregisterEntity(Entity entity)
        {
            _entities.Remove(entity.Id);
            entity.Reset();
        }

        public class EntityBuilder
        {
            private Entity _entity;
            private EcsWorld _world;

            internal EntityBuilder(EcsWorld world, Entity entity)
            {
                _entity = entity;
                _world = world;
            }
            
            internal EntityBuilder(EcsWorld world, uint id)
            {
                _world = world;
                _entity = new Entity(id);
            }

            public EntityBuilder With<T>() where T : struct
            {
                _entity.AddComponent<T>();
                return this;
            }

            public EntityBuilder With<T>(T cmp) where T : struct
            {
                var id = _entity.AddComponent<T>();
                _entity.AddComponent<T>(cmp);
                return this;
            }

            public Entity Create()
            {
                _world.RegisterEntity(_entity);
                return _entity;
            }
        }

    }
}