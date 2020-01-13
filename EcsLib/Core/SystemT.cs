using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsLib.Core
{
    public abstract class System<T> : AbstractSystem where T : struct
    {
        private readonly Wrap[] _components;
        protected readonly bool _isOneTick;
        protected readonly HashSet<int> _ocupied = new HashSet<int>();

        public System(int maxSize)
        {
            _components = new Wrap[maxSize];
        }

        public IEnumerable<Tuple<uint, T>> Components => _components.Select(e => new Tuple<uint, T>(e.Owner, e.Value));
        internal override Type ComponentType => typeof(T);

        public void UpdateComponent(int id, T component)
        {
            _components[id].Value = component;
        }

        public bool TryGetComponent(int id, ref T component)
        {
            if (!_ocupied.Contains(id)) return false;
            component = _components[id].Value;
            return true;
        }

        internal override int ReserveComponent(uint entity, object component)
        {
            if (component is T)
                return ReserveComponent(entity, (T) component);
            throw new Exception($"Component Should be {typeof(T).Name}, but recieved {component.GetType().Name}");
        }

        public int ReserveComponent(uint entity, T component)
        {
            var id = -1;
            for (var i = 0; i < _components.Length; i++)
                if (!_ocupied.Contains(i)) {
                    id = i;
                    break;
                }

            if (id == -1)
                throw new Exception("All components are occupied, try create system with more available components");
            _ocupied.Add(id);
            _components[id].Owner = entity;
            _components[id].Value = component;
            return id;
        }

        internal override void ReleaseComponent(int id)
        {
            if (!_ocupied.Contains(id)) throw new ArgumentException($"Component with {id} already released!");
            _ocupied.Remove(id);
            _components[id] = default;
        }

        internal override void Update()
        {
            // TODO: escape allocations
            foreach (var id in _ocupied) {
                var cmp = _components[id].Value;
                var entity = _world.GetEntity(_components[id].Owner);
                Iterate(entity, ref cmp);
                _components[id].Value = cmp;
            }

            if (_isOneTick) _ocupied.Clear();
        }

        public abstract void Iterate(Entity owner, ref T c);

        private struct Wrap
        {
            public uint Owner;
            public T Value;
        }
    }
}