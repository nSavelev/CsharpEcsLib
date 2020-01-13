using System;

namespace EcsLib.Core
{
    public abstract class AbstractSystem
    {
        protected EcsWorld _world;

        internal abstract Type ComponentType { get; }

        internal void SetWorld(EcsWorld world)
        {
            _world = world;
        }

        internal abstract void Update();
        internal abstract int ReserveComponent(uint owner, object component);
        internal abstract void ReleaseComponent(int id);
    }
}