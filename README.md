CsharpEcsLib
=====================
this is a simple ECS library for C#. No Engine dependence, structures as components.

How to use
-----------------------------------

### Create you own system and component

```C#
    public struct MoveComponent
    {
        public Vector3 Direction;
        public float Speed;
    }
    
    public class MoveSystem : System<MoveComponent>
    {
        [EcsInject] 
        private PositionSystem _positionSystem;

        private ITimeProvider _time;

        public MoveSystem(ITimeProvider timeProvider, int maxSize) : base(maxSize)
        {
            _time = timeProvider;
        }

        public override void Iterate(Entity owner, ref MoveComponent c)
        {
            var posId = owner.GetComponent<PositionComponent>();
            PositionComponent pos = default;
            if (_positionSystem.TryGetComponent(posId, ref pos))
            {
                pos.Position += _time.DeltaTime * c.Speed * c.Direction.normalized;
                _positionSystem.UpdateComponent(posId, pos);
            }
        }
    }
```

All Systems sould have System<struct> as their base class. Non struct components are not alowed.
If you need other components, use ***[EcsInject]*** attribute to inject other system.
Systems Require maxSize field to be specified in constructor to warmup components storage

Iterate method called for each component, registered in system.

If you want your system to work with components with one tick livetime
```C#
	public SomeSystem(int maxSize):base(maxSize){
		// Some initialization code
 		_isOneTick = true;
 	}
 ```
add this to you system's constructor

### Create the EcsWorld intance

```C#
_world = new EcsWorld();
_world.AddSystem(new PlayerInputSystem(1));
_world.AddSystem(new MoveSystem(time, GameSize));
_world.AddSystem(new PositionSystem(GameSize));
_world.Init();
```

**ATTERNTION! All systems should be added before EcsWorld.Init will be called!**

To iterate EcsWorld, use 
```C#
_world.Update();
```
method

### Entity creation

To create new entity use ***EcsWorld.NewEntity()*** method. It work with entity pool to prevent alocations on frequient entity creation and removing

```C#
_world.NewEntity()
    .With<PlayerInput>()
    .With<PositionComponent>()
    .With<MoveComponent>(new MoveComponent()
    {
        Speed = 5
    }).Create();
```

This method returns Entity builder, which allow you to add components before entity will be registered

### Components workflow

To work with components use EcsWorld instance
```C#
	// Component creation
	_world.AddComponent<DamageComponent>(entity, new DamageComponent{
			Amount = 10
		});
	
	// Component removement is implemented by component id. to get component id, call Entity.GetComponent<T> or Entity.GetComponents<T> for all components
	_world.RemoveComponent(entity, entity.GetComponent<InvincibleComponent>());	
	
	// Component data fetching	
	if (_world.TryGetComponent(entity, out var pos)){
		// do smtn with component data
	}
```

**COMPONENT MODIFICATION OUTSIZE OF SYSTEMS IS NOW IMPLEMENTED FOR NOW**